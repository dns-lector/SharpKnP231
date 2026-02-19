using SharpKnP321.Networking.Api;
using SharpKnP321.Networking.Orm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharpKnP321.Networking
{
    internal class NetworkingDemo
    {
        public async Task Run()
        {
            // Курси валют НБУ, робота з JSON

            using HttpClient client = new();
            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json"),
            };
            Task<HttpResponseMessage> responseTask = 
                client.SendAsync(request);

            Console.WriteLine("Курси валют НБУ, робота з JSON");
            Console.Write(DateTime.Now.Ticks / 10000 % 100000); Console.WriteLine(" request start");

            HttpResponseMessage response = await responseTask;
            Task<String> contentTask = response.Content.ReadAsStringAsync();

            Console.Write(DateTime.Now.Ticks / 10000 % 100000); Console.WriteLine(" request finish");
            String jsonString = await contentTask;

            List<NbuRate> rates = NbuApi.ListFromJsonString(jsonString);

            foreach (NbuRate rate in rates)
            {
                Console.WriteLine(rate);
            }
        }

        public async Task RunStep()
        {
            using HttpClient client = new();
            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://itstep.org/"),
            };
            Task<HttpResponseMessage> responseTask = client.SendAsync(request);

            Console.WriteLine("HTTP requests and responses");
            Console.Write(DateTime.Now.Ticks / 10000 % 100000); Console.WriteLine(" request start");

            HttpResponseMessage response = await responseTask;
            Task<String> contentTask = response.Content.ReadAsStringAsync();

            Console.Write(DateTime.Now.Ticks / 10000 % 100000); Console.WriteLine(" request finish");

            Console.WriteLine($"HTTP/{response.Version} {((int)response.StatusCode)} {response.ReasonPhrase}");
            foreach (var header in response.Headers)
            {
                Console.WriteLine("{0}: {1}", header.Key, String.Join(',', header.Value));
            }
            Console.WriteLine();
            Console.WriteLine(await contentTask);
        }

        public void RunBody()
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
/* Д.З. Реалізувати відображення курсів валют на задану дану,
 * що її вводить користувач з клавіатури.
 * Забезпечити перевірку валідності дати, а також те, що вона
 * належить минулому
 * https://bank.gov.ua/ua/open-data/api-dev
 */
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
