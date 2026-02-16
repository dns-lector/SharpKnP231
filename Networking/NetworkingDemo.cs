using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKnP321.Networking
{
    internal class NetworkingDemo
    {
        public void Run()
        {
            HttpClient client = new();
            Task<String> getRequest = client.GetStringAsync("https://itstep.org/");  // вилучає тіло
            Console.Write(DateTime.Now.Ticks / 10000 % 100000);
            Console.WriteLine(" Get start");
            String requestBody = getRequest.Result;
            Console.WriteLine(DateTime.Now.Ticks / 10000 % 100000);
            Console.WriteLine(requestBody);
        }
    }
}
/* Д.З. Реалізувати завантажувач кодів сайтів:
 * Користувач вводить адресу сайту,
 * здійснюється запит, виводиться HTML код сайту, а також
 * запускається браузер з переходом на даний сайт (асинхронно)
 */
/* Робота мережею Інтернет
 * Мережа - сукупність вузлів та зв'язків між ними (каналів зв'язку)
 * Вузол (Node) - активний учасник, що перетворює дані (ПК, принтер, телефон, виконавчий пристрій тощо)
 *  вузол у мережі відрізняється адресою та/або іменем
 * Зв'язок - спосіб передачі даних між вузлами (дріт, оптоволокно, радіоканал тощо)
 * НТТР - текстовий транспортий протокол
 * запит              відповідь
 * метод шлях         статус-код  фраза
 * заголовки - пари ключ: значення\r\n
 * тіло (довільна інформація), зокрема, JSON - текстовий протокол передачі даних
 * 
 * CONNECT  службові
 * TRACE
 * 
 * HEAD     технологічні
 * OPTIONS
 * 
 *          загальні CRUD - Create Read Update Delete
 * GET     одержання даних (читання, Read) -- без модифікації системи (без змін)
 * POST    створення нових елементів (Create)
 * DELETE  
 * PUT     заміна наявних даних на передані
 * PATCH   оновлення частини наявних даних
 * 
 *          галузеві стандарти
 * LINK
 * UNLINK
 * PURGE
 * MKCOL
 * 
 * 
 */
