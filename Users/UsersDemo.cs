using SharpKnP321.Users.Dal;
using SharpKnP321.Users.Dal.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpKnP321.Users
{
    internal record MenuItem(char Key, String Title, Action? action)
    {
        public override string ToString()
        {
            return $"{Key} - {Title}";
        }
    };

    internal class UsersDemo
    {
        private DataAccessor _accessor = null!;

        private MenuItem[] menu => [
            new MenuItem('i', "Інсталювати таблиці БД", () => _accessor.Install()),
            new MenuItem('h', "ПереІнсталювати таблиці БД", () => _accessor.Install(isHard:true)),
            new MenuItem('1', "Реєстрація нового користувача", SignUp),
            new MenuItem('2', "Вхід до системи (автентифікація)", SignIn),
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

            MenuItem? selectedItem;
            do
            {
                foreach (var item in menu)
                {
                    Console.WriteLine(item);
                }
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                Console.WriteLine();
                selectedItem = menu.FirstOrDefault(item => item.Key == keyInfo.KeyChar);
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

        private void SignIn()
        {
            Console.WriteLine("SignIn");
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
                Console.WriteLine("Реєстрація успішна, перевірте пошту");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
