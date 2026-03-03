using SharpKnP321.Users.Dal;
using SharpKnP321.Users.Dal.Entities;
using SharpKnP321.Users.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharpKnP321.Users
{
    internal record MenuItem(char Key, String Title, Action? action, bool IsAuthorized = false)
    {
        public override string ToString()
        {
            return $"{Key} - {Title}";
        }
    };

    internal class UsersDemo
    {
        private const String savedFilename = "saved.model";   // azure.spd111.od.0@ukr.net   123

        private DataAccessor _accessor = null!;
        private SignInModel? signInModel;

        private MenuItem[] menu => [
            new MenuItem('i', "Інсталювати таблиці БД", () => _accessor.Install()),
            new MenuItem('h', "ПереІнсталювати таблиці БД", () => _accessor.Install(isHard:true)),
            new MenuItem('1', "Реєстрація нового користувача", SignUp),
            new MenuItem('2', "Вхід до системи (автентифікація)", SignIn),
            new MenuItem('3', "Одержати персональні дані (авторизація)", GetPersonal, IsAuthorized:true),
            new MenuItem('4', "Вийти з авторизованого режиму (Sign Out)", SignOut, IsAuthorized:true),
            new MenuItem('0', "Вихід", null),
        ];

        public void Run()
        {
            try
            {
                _accessor = new();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            // перевіряємо чи є збережені дані автентифікації (запам'ятати мене)
            if (File.Exists(savedFilename))
            {
                signInModel = JsonSerializer.Deserialize<SignInModel>(
                    File.ReadAllText(savedFilename)
                )!;
                // Перевіряємо термін придатності збереженого токена
                if(signInModel.AccessToken.TokenExp == null || signInModel.AccessToken.TokenExp.Value > DateTime.Now)
                {
                    // Токен придатний - оновлюємо його термін на новий період
                    var task = _accessor.ProlongToken(signInModel.AccessToken.TokenId);
                    Console.WriteLine($"Вітаємо, {signInModel.UserData.UserName}, ваш доступ відновлено");
                    signInModel.AccessToken.TokenExp = task.Result;
                }
                else
                {
                    Console.WriteLine("Збережені дані добігли терміну придатності. Необхідний новий вхід");
                    signInModel = null;
                    File.Delete(savedFilename);
                }
            }

            MenuItem? selectedItem;
            do
            {
                foreach (var item in menu)
                {
                    if (!item.IsAuthorized || signInModel != null)
                    {
                        Console.WriteLine(item);
                    }
                }
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                Console.WriteLine();
                // selectedItem = menu.FirstOrDefault(item => item.Key == keyInfo.KeyChar);
                // Коригуємо пошук меню з урахуванням обмежень з авторизації
                selectedItem = menu.FirstOrDefault(item => 
                    item.Key == keyInfo.KeyChar && 
                    (!item.IsAuthorized || signInModel != null));
                if (selectedItem is null)
                {
                    Console.WriteLine("Нерозпізнаний вибір");
                }
                else
                {
                    selectedItem.action?.Invoke();
                }
            } while (selectedItem == null || selectedItem.action != null);
        }

        private void SignOut()
        {
            Console.Write("Підтверджуєте вихід (y/...)? ");
            ConsoleKeyInfo keyInfo = Console.ReadKey(false);
            Console.WriteLine();
            if (keyInfo.KeyChar == 'y' || keyInfo.KeyChar == 'Y')
            {
                signInModel = null;
                File.Delete(savedFilename);
                Console.WriteLine("Переведено до неавторизованого режиму");
            }
            else
            {
                Console.WriteLine("Вихід скасовано");
            }
        }

        private void GetPersonal()
        {
            if (signInModel == null) return;
            Console.WriteLine($"Name: {signInModel.UserData.UserName}, Email: {signInModel.UserData.UserEmail}, TokenExp: {signInModel.AccessToken.TokenExp}");
        }

        private void SignIn()
        {
            Console.WriteLine("Автентифікація у системі");
            String UserEmail;
            String password;
            bool isEntryCorrect;
            do
            {
                Console.Write("E-mail: ");
                UserEmail = Console.ReadLine()!.Trim();
                if (UserEmail == String.Empty) return;
                isEntryCorrect = Regex.IsMatch(UserEmail, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
                if (!isEntryCorrect)
                {
                    Console.WriteLine("Не відповідає формату, відкоригуйте");
                }
            } while (!isEntryCorrect);
                
            Console.Write("Password: ");
            password = Console.ReadLine()!.Trim();
            
            signInModel = _accessor.SignIn(UserEmail, password).Result;
            if(signInModel == null)
            {
                Console.WriteLine("У вході відмовлено");
                return;
            }
            Console.WriteLine($"Вітання, {signInModel.UserData.UserName}, Вам видано токен {signInModel.AccessToken.TokenId}");
            // Перевірити чи у користувача підтверджена пошта (за наявністю коду у БД)
            // якщо ні, то запропонувати введення коду 
            Console.Write("Запам'ятати мене (y/...)? ");
            ConsoleKeyInfo keyInfo = Console.ReadKey(false);
            Console.WriteLine();
            if(keyInfo.KeyChar == 'y' || keyInfo.KeyChar == 'Y')
            {
                File.WriteAllText(savedFilename, JsonSerializer.Serialize(signInModel));
                Console.WriteLine("Дані збережено");
            }  
        }

        private void SignUp()
        {
            UserData userData = new();
            String password;
            bool isEntryCorrect;
            Console.WriteLine("Реєстрація нового користувача (порожній ввід - вихід)");
            do
            {
                Console.Write("Повне Ім'я: ");
                userData.UserName = Console.ReadLine()!;
                if (userData.UserName == String.Empty) return;
                isEntryCorrect = userData.UserName.Length >= 2;
                if (!isEntryCorrect)
                {
                    Console.WriteLine("Занадто коротке, відкоригуйте");
                }
            } while(!isEntryCorrect);
            do
            {
                Console.Write("E-mail: ");
                userData.UserEmail = Console.ReadLine()!.Trim();
                if (userData.UserEmail == String.Empty) return;
                isEntryCorrect = Regex.IsMatch(userData.UserEmail, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"); 
                if (!isEntryCorrect)
                {
                    Console.WriteLine("Не відповідає формату, відкоригуйте");
                }
            } while(!isEntryCorrect);
            do
            {
                Console.Write("Password: ");
                password = Console.ReadLine()!.Trim();
                if (password == String.Empty) return;
                isEntryCorrect = password.Length > 2;  // Regex.IsMatch(password, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"); 
                if (!isEntryCorrect)
                {
                    Console.WriteLine("Не відповідає формату, відкоригуйте");
                }
            } while(!isEntryCorrect);

            try
            {
                _accessor.SignUp(userData, password).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("Реєстрація успішна, перевірте пошту");
            int cnt = 3;
            bool isConfirmed;
            do
            {
                Console.Write("Код підтвердження з пошти: ");
                String code = Console.ReadLine()!;
                isConfirmed = _accessor.ConfirmEmailCodeAsync(userData.UserId, code).Result;
                cnt--;
                if (!isConfirmed)
                {
                    Console.WriteLine("Код не прийнято");
                }
            } while (cnt > 0 && !isConfirmed);
            if (isConfirmed)
            {
                Console.WriteLine("Пошту успішно підтверджено");
            }
            else
            {
                Console.WriteLine("Пошту не підтверджено, код можна буде ввести після автентифікації");
            }
        }
    }
}
/* Робота з користувачами: реєстрація, автентифікація, авторизація
 * 
 * UserData          UserAccess        AccessTokens
 *  UserId            AccessId          TokenId
 *  UserName          AccessLogin       AccessId
 *  UserEmail         AccessSalt        TokenIat
 *  UserDelAt         AccessDk          TokenExp 
 *  UserEmailCode     UserId
 *                    RoleId ---------> Roles
 *                                       RoleId
 *                                       ....
 *  
 * SELECT * FROM  UserAccess ua JOIN UserData ud ON ua.UserId = ud.UserId
 * UserId | AccessId | AccessLogin | AccessSalt | AccessDk || UserId | UserName | UserEmail | UserDelAt
 * 
 * ! Одна з традицій іменування полів таблиць БД:
 *  - якщо у вибірці є однакові імена (через поєднання таблиць), то їх значення мають бути однаковими
 *     (НЕ так, що в кожній з таблиць є ID та їх значення різні у сукупній вибірці)
 *  - поєднання таблиць здійснюється за однаковими іменами полів (... ON ua.UserId = ud.UserId)  
 *     (НЕ ... ON ua.UserId = ud.Id)
 */
/* Д.З. Реалізувати валідацію паролю при реєстрації нового користувача.
 * Перевіряти введений пароль на "міцність"
 * - довжина не менша 6 символів
 * - містить щонайменше одну цифру, 
 * - містить щонайменше один спецсимвол (не літера, не цифра), 
 * - містить щонайменше одну літеру нижнього реєстру (малу)
 * - містить щонайменше одну літеру верхнього реєстру (велику)
 * Якщо пароль не відповідає принаймні одному критерію, то виводити повідомлення
 * про ті критерії, що порушені, та повертатись до повторного введення паролю.
 */