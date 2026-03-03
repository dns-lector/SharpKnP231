using Dapper;
using Microsoft.Data.SqlClient;
using SharpKnP321.Services.Email;
using SharpKnP321.Services.Kdf;
using SharpKnP321.Users.Dal.Entities;
using SharpKnP321.Users.Models;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace SharpKnP321.Users.Dal
{
    internal class DataAccessor
    {
        private SqlConnection connection;
        private readonly Random random = new();
        private readonly IEmailService emailService = new GmailService();
        private readonly IKdfService kdfService = new PbKdfService();
        private const double tokenPeriodMinutes = 5.0;

        public DataAccessor() 
        {
            String settingsFilename = "appsettings.json";
            if (!File.Exists(settingsFilename))
            {
                throw new Exception("Не знайдено файл конфігурації. Прочитайте Readme");
            }
            var settings = JsonSerializer.Deserialize<JsonElement>(
                File.ReadAllText(settingsFilename)
            );
            String userDb;
            try
            {
                var csSection = settings.GetProperty("ConnectionStrings");
                userDb = csSection.GetProperty("UserDb").GetString()!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка визначення конфігурації: {ex.Message}");
            }
            connection = new(userDb);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка підключення БД: {ex.Message}");
            }
        }

        public async Task<DateTime> ProlongToken(Guid tokenId)
        {
            DateTime TokenExp = DateTime.Now.AddMinutes(tokenPeriodMinutes);
            await connection.ExecuteAsync("UPDATE AccessToken SET TokenExp = @TokenExp WHERE TokenId = @tokenId", new
            {
                TokenExp,
                tokenId
            });
            return TokenExp;
        }

        public async Task SignUp(UserData userData, String password)
        {
            if (userData.UserId == default)
            {
                userData.UserId = Guid.NewGuid();
            }
            userData.UserEmailCode = String.Join("", Enumerable.Range(0, 6).Select(_ => "0123456789"[random.Next(10)]));   // 6-значний код підтвердження
            // формуємо повідомлення на підтвердження пошти
            MailMessage mailMessage = new()
            {
                IsBodyHtml = true,
                Subject = "Message from Sharp",
                Body = @$"<html>
                    <h1>Шановний користувач!</h1>
                    <p>Вітаємо вас з реєстрацією на сайті<p>
                    <p>Для завершення підтвердження пошти введіть код <b style='font-size: large'>{userData.UserEmailCode}</b></p>
                </html>",
            };
            mailMessage.To.Add(new MailAddress(userData.UserEmail));
            Task emailTask = emailService.SendAsync(mailMessage);   // запускаємо, але не чекаємо

            // Зберігаємо нового користувача (у т.ч. код підтвердження) у БД
            Task dbTask = connection.ExecuteAsync(@"INSERT INTO UserData(UserId, UserName, UserEmail, UserEmailCode)
            VALUES(@UserId, @UserName, @UserEmail, @UserEmailCode)", userData);

            // Створюємо доступ для нового користувача
            String salt = Guid.NewGuid().ToString()[..16];
            String dk = kdfService.Dk(salt, password);
            Task accessTask = connection.ExecuteAsync(
            @"INSERT INTO UserAccess(AccessId, UserId, AccessLogin, AccessSalt, AccessDk)
            VALUES(@AccessId, @UserId, @AccessLogin, @AccessSalt, @AccessDk)", new
            {
                AccessId = Guid.NewGuid(),
                UserId = userData.UserId,
                AccessLogin = userData.UserEmail,
                AccessSalt = salt,
                AccessDk = dk
            });

            await Task.WhenAll(emailTask, dbTask, accessTask);
        }

        public async Task<SignInModel?> SignIn(String login, String password)
        {
            UserAccess? userAccess = await connection.QuerySingleOrDefaultAsync<UserAccess>(
                "SELECT * FROM UserAccess u WHERE u.AccessLogin = @AccessLogin", new
                {
                    AccessLogin = login
                });
            if (userAccess == null || 
                kdfService.Dk(userAccess.AccessSalt, password) != userAccess.AccessDk)
            {
                return null;
            }

            var userDataTask = connection.QuerySingleAsync<UserData>(
                "SELECT * FROM UserData u WHERE u.UserId = @UserId", new
                {
                    UserId = userAccess.UserId,
                });
            SignInModel ret = new()
            {
                UserAccess = userAccess
            };

            // формуємо токен
            AccessToken accessToken = new()
            {
                TokenId = Guid.NewGuid(),
                AccessId = userAccess.AccessId,
                TokenIat = DateTime.Now,
                TokenExp = DateTime.Now.AddMinutes(tokenPeriodMinutes),
            };

            ret.UserData = await userDataTask;

            // Вносимо токен до БД
            var accessTokenTask = connection.ExecuteAsync(
                "INSERT INTO AccessToken (TokenId,AccessId,TokenIat,TokenExp) " +
                "VALUES (@TokenId,@AccessId,@TokenIat,@TokenExp)",
                accessToken);

            ret.AccessToken = accessToken;
            await accessTokenTask;
            return ret;
        }
        /* Д.З. При автентифікації здійснювати перевірку чи є в користувача
         * активний токен. У такому разі подовжувати його дію, замість
         * генерування нового. За відсутності - генерувати новий.
         */

        public async Task<bool> ConfirmEmailCodeAsync(Guid userId, String code)
        {
            UserData userData = await connection.QuerySingleAsync<UserData>(
                "SELECT * FROM UserData u WHERE u.UserId = @UserId", new
                {
                    UserId = userId
                });
            bool isOk = userData.UserEmailCode == code;
            if(isOk)   // фіксуємо підтвердження - скидаємо до NULL код у БД
            {
                await connection.ExecuteAsync("UPDATE UserData SET UserEmailCode = NULL WHERE UserId = @UserId", new
                {
                    UserId = userId
                });
            }
            return isOk;
        }

        public void Install(bool isHard = false)
        {
            if(isHard)
            {
                connection.Execute("DROP TABLE IF EXISTS UserData");
            }
            connection.Execute(@"CREATE TABLE UserData (
                UserId        UNIQUEIDENTIFIER PRIMARY KEY,
                UserName      NVARCHAR(128)    NOT NULL,
                UserEmail     NVARCHAR(256)    NOT NULL,
                UserEmailCode VARCHAR(16)          NULL,
                UserDelAt     DATETIME2            NULL
            )");
            if (isHard)
            {
                connection.Execute("DROP TABLE IF EXISTS UserAccess");
            }
            connection.Execute(@"CREATE TABLE UserAccess (
                AccessId      UNIQUEIDENTIFIER PRIMARY KEY,
                UserId        UNIQUEIDENTIFIER NOT NULL,
                RoleId        UNIQUEIDENTIFIER     NULL,
                AccessLogin   NVARCHAR(64)     NOT NULL,
                AccessSalt    CHAR(16)         NOT NULL,
                AccessDk      CHAR(32)             NULL
            )");
            if (isHard)
            {
                connection.Execute("DROP TABLE IF EXISTS AccessToken");
            }
            connection.Execute(@"CREATE TABLE AccessToken (
                TokenId      UNIQUEIDENTIFIER PRIMARY KEY,
                AccessId     UNIQUEIDENTIFIER NOT NULL,
                TokenIat     DATETIME2        NOT NULL,
                TokenExp     DATETIME2            NULL
            )");
        }
    }
}
/* Д.З. Реалізувати сервіс формування ОТР (одноразових паролів)
 * який приймає параметри
 * - довжина паролю
 * - режим - цифри, літери, змішаний (*у змішаному режимі вилучаються символи однакового нарису - О/0 тощо)
 */