using System;
using System.Collections.Generic;
using System.Text;

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
        private MenuItem[] menu => [
            new MenuItem('1', "Реєстрація нового користувача", SignUp),
            new MenuItem('2', "Вхід до системи (автентифікація)", SignIn),
            new MenuItem('0', "Вихід", null),
        ];

        public void Run()
        {
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
            Console.WriteLine("SignUp");
        }
    }
}
/* Робота з користувачами: реєстрація, автентифікація, авторизація
 * 
 * UserData        UserAccess        AccessTokens
 *  UserId          AccessId          TokenId
 *  UserName        AccessLogin       AccessId
 *  UserEmail       AccessSalt        TokenIat
 *  UserDelAt       AccessDk          TokenExp 
 * 
 */
