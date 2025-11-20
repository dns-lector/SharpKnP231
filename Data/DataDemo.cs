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
            dataAccessor.Install();
            dataAccessor.Seed();

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
