using Microsoft.Data.SqlClient;
using SharpKnP321.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKnP321.Data
{
    internal class DataDemo
    {
        public void Run()
        {            
            DataAccessor dataAccessor = new();

            dataAccessor.MonthlySalesByManagersSql(year:2025, month: 1);
            Console.WriteLine("---------------");
            dataAccessor.MonthlySalesByManagersOrm(1).ForEach(Console.WriteLine);
            /*
             Д.З. Реалізувати вибірки статистики місячних продажів за товарами
            Назва товару -- кількість продажів
            Номер місяця (та рік) передавати параметрами до методу.
            Забезпечити реалізацію через ORM - створити модель (не використовувати
            модель, створену для менеджерів).
            Провести випробування, порівняти з безпосереднім виконанням SQL запиту
             */

            // Console.WriteLine(dataAccessor.RandomProduct());
            // Console.WriteLine(dataAccessor.RandomDepartment());
            // dataAccessor.GetProducts().ForEach(Console.WriteLine);

            // Console.Write("Кількість продажів за місяць (1-12): ");
            // String? input = Console.ReadLine();
            // // int value;
            // if(int.TryParse(input, out int value))   // side effect - зміна "оточення" - змінних поза тілом ф-ції
            // {
            //     try
            //     {
            //         Console.WriteLine(dataAccessor.GetSalesCountByMonth(value));
            //     }
            //     catch
            //     {
            //         Console.WriteLine("Введене значення не було оброблене");
            //     }
            // }
            // else
            // {
            //     Console.WriteLine("Введене значення не розпізнано як число");
            // }

            // dataAccessor.Install();
            // dataAccessor.Seed();
            // dataAccessor.FillSales();

            // List<Product> products = dataAccessor.GetProducts();

            // Вивести товари: назва -- ціна
            // - за зростанням ціни
            // - за спаданням ціни
            // - за абеткою
            // Вивести результати:
            //  - 3 найдорожчі товари
            //  - 3 найдешевші товари
            //  - 3 випадкові товари (з кожним запуском різні)

            // foreach(var p in products.OrderBy(p => p.Price))
            // {
            //     Console.WriteLine("{0} -- {1:F2}", p.Name, p.Price);
            // }

        }
        public void Run1()
        {
            Console.WriteLine("Data Demo");
            // Робота з БД проводиться у кілька етапів
            // І. Підключення.        raw string  
            String connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\samoylenko_d\Source\Repos\SharpKnP321\Database1.mdf;Integrated Security=True";
            
            // ADO.NET - інструментарій (технологія) доступу до даних у .NET
            SqlConnection connection = new(connectionString);
            // Особливість - утворення об'єкту не відкиває підключення
            try
            {
                connection.Open();   // підключення необхідно відкривати окремою командою
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Connection failed: " + ex.Message);
                return;
            }

            // II. Формування та виконання команди (SQL)
            String sql = "SELECT CURRENT_TIMESTAMP";
            // using (у даному контексті) - блок з автоматичним руйнуванням (AutoDisposable)
            using SqlCommand cmd = new(sql, connection);
            Object scalar;
            try
            {
                scalar = cmd.ExecuteScalar();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Command failed: {0}\n{1}", ex.Message, sql);
                return;
            }

            // III. Передача та оброблення даних від БД
            DateTime timestamp;
            timestamp = Convert.ToDateTime(scalar);
            Console.WriteLine("Res: {0}", timestamp);

            // IV. Закриття підключення, перевірка, що всі дані передані
            connection.Close();
        }
    }
}
/* Робота з даними на прикладі БД
 * БД зазвичай є відокремленим від проєкту сервісом, що вимагає окремого 
 * підключення та специфічної взаємодії. У Студії є спрощений сервіс БД
 * LocalDB  New Item -- Service Based DB -- Create
 * Знаходимо рядок підключення до БД через її властивості у Server Explorer
 * 
 * NuGet - система управління підключеними додатковими модулями (бібліотеками)
 *  проєкту C#.NET: Tools -- NuGet Package Manager -- Manage...
 * Знаходимо та встановлюємо Microsoft.Data.SqlClient - додаткові інструменти
 *  для взаємодії з СУБД MS SQLServer, у т.ч. LocalDB
 * 
 * ORM - Object Relation Mapping - відображення даних та їх зв'язків на об'єкти
 * (мови програмування) та їх зв'язки.
 * DTO - Data Transfer Object (Entity) - об'єкти (класи) для представлення даних
 * DAO - Data Access Object - об'єкти для оперування з DTO
 * 
 * 
 * Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\samoylenko_d\Source\Repos\SharpKnP321\Database1.mdf;Integrated Security=True
 */
