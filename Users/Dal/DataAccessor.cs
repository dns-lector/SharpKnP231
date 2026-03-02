using Dapper;
using Microsoft.Data.SqlClient;
using SharpKnP321.Services.Email;
using SharpKnP321.Services.Kdf;
using SharpKnP321.Users.Dal.Entities;
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