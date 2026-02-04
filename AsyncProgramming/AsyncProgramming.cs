using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKnP321.AsyncProgramming
{
    internal class AsyncProgramming
    {
        public void Run()
        {
            // Console.WriteLine(Directory.GetCurrentDirectory());
            ConsoleKeyInfo keyInfo;
            do {
                Console.WriteLine("Async Programming: select an action");
                Console.WriteLine("1. Processes list");
                Console.WriteLine("2. Start notepad");
                Console.WriteLine("3. Edit demo file");
                Console.WriteLine("4. Thread demo");
                Console.WriteLine("5. Multi Thread demo (percent)");
                Console.WriteLine("0. Exit program");
                keyInfo = Console.ReadKey();
                Console.WriteLine();
                switch (keyInfo.KeyChar)
                {
                    case '0': return;
                    case '1': ProcessesDemo(); break;
                    case '2': ProcessControlDemo(); break;
                    case '3': ProcessWithParam(); break;
                    case '4': ThreadsDemo(); break;
                    case '5': MultiThread(); break;
                    default: Console.WriteLine("Wrong choice"); break;
                }
            } while (true);
        }

        private void MultiThread()
        {
            /* Багатопоточність - поділ завдань на кілька потоків-виконавців
             * 1. Дослідження задачі на можливість такого поділу
             * 2. Виділення частин, які мають бути послідовні, та 
             *    ті, які можуть виконуватись паралельно.
             * 3. Проєктування схеми виконання
             *       ------  
             *  ----<         >----
             *       --------
             *       
             * Приклад: розрахунок річної інфляції - НБУ дає відомості
             * про місячні коефіцієнти інфляцї (у відсотках). Задача -
             * запитати 12 коеф. та розрахувати річний показник.
             * Запит коефіцієнту здійснюється через АРІ з непостійним часом
             * відповіді: порядок запитів не обов'язково збігається з
             * порядком відповідей
             * 
             * 1. Дослідження. Питання - чи однаковий результат буде
             * при різній послідовності врахування відсотків:
             * (100 + 10%) + 20% =?= (100 + 20%) + 10%
             * (100 * 1.1) * 1.2 =!= (100 * 1.2) * 1.1
             * Порядок врахування не грає ролі у підсумку 
             * Висновок: можлива повна паралельність роботи потоків
             */
            sum = 100.0;
            for (int i = 0; i < 12; i++)
            {
                new Thread(CalcMonth).Start(i + 1);
            }
        }
        private double sum;  // спосіб обміну даними між потоками
        private void CalcMonth(Object? month)
        {
            int m = (int)month!;
            double res = sum;
            Console.WriteLine($"Request sent for month {m}");
            Thread.Sleep(1000);   // імітація АРІ-запиту
            double percent = m;
            res = res * (1.0 + percent / 100.0);
            sum = res;
            Console.WriteLine($"Response got for month {m} sum = {sum}");
            /* Одна з головних проблем багатопоточності - конкуренція
             * яка полягає в одночасному доступі до спільного ресурсу (sum)
             * Суть - розділені у часі операції читання та запису
             * sum = 100                             110
             * 
             * 1  res = sum(100) ............... sum=110
             * 2               res=sum(100) ............... sum=110
             */
        }

        private void ThreadsDemo()
        {
            // Потоки - системні об'єкти, складові частини процесів
            // Процес має принаймні один потік (основний), завершення
            // усіх потоків процесу = завершення процесу.
            // Потік будується на методі (функції) до якої є 
            // вимоги: тип повернення void, параметри: або відсутні,
            // або один узагальненого типу object?
            // Якщо потрібно кілька параметрів, то їх поєднують у клас

            Console.WriteLine("Thread created");
            var t = new Thread(ThreadActivity);   // створення потоку 
            // не запускає його, тільки створює об'єкт

            Console.WriteLine("Thread start");
            t.Start();  // запуск потоку
        }

        private void ThreadActivity()  // потоковий метод без параметрів
        {
            Console.WriteLine("ThreadActivity start");
            Thread.Sleep(5000);
            Console.WriteLine("ThreadActivity stop");
        }

        private void ProcessWithParam()
        {
            // запуск програми з параметрами вимагає правильного розрізнення
            // імен програми та параметрів
            try
            {
                var p = Process.Start(new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "AsyncProgramming",
                        "demo.txt"
                    ),
                    UseShellExecute = true,
                   // WorkingDirectory = Directory.GetCurrentDirectory(),
                });
                Console.WriteLine("Press a key");
                Console.ReadKey();
                p!.CloseMainWindow();
                p.Kill();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ProcessControlDemo()
        {
            try
            {
                Console.WriteLine("Press a key");
                Console.ReadKey();
                Process process = Process.Start("notepad.exe");

                Console.WriteLine("Press a key");
                Console.ReadKey();
                if (!process.HasExited)
                {
                    process.CloseMainWindow();
                    process.Kill(true);
                    process.WaitForExit();
                    process.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Виникла помилка: {ex.Message}");
            }
        }
        /* Д.З. Реалізувати запуск процесів з передачею до них аргументів
         * - блокнот (або інший редактор) з відкриттям заданого файлу
         * - браузер (* з пошуком наявного) з відкриттям заданої адреси
         * * музичний або відеопрогравач з заданим ресурсом
         */

        private void ProcessesDemo()
        {
            // System.Diagnostics.Process - клас, що
            // а) відповідає за роботу з процесом
            // б) містить статичні методи для управління процесами
            Process[] processes = Process.GetProcesses();
            Dictionary<String, int> proc = [];
            foreach (var process in processes)
            {
                if (proc.ContainsKey(process.ProcessName))
                {
                    proc[process.ProcessName]++;
                }
                else
                {
                    proc[process.ProcessName] = 1;
                }
            }
            // задача: вивести назву процеса та кількість процесів з однаковою назвою
            // задача: вивести впорядковано спочатку з більшою кількістю, 
            //   при однаковій кількості - за абеткою
            foreach (var pair in proc.OrderByDescending(p => p.Value).ThenBy(p => p.Key))
            {
                Console.WriteLine($"{pair.Key} ({pair.Value})");
            }
        }
    }
}
/* Розвиток техніки супроводжується збільшенням кількості процесорів, що
 * вимагає перегляду принципів програмування з розподілом виконавців по
 * різних процесорах.
 * 
 * Синхронне виконання коду - послідовне у часі, один за іншим
 * ---- - робота 1
 * ==== - робота 2
 * Синхронне виконання: -------=========
 * 
 * Асинхронність - будь-яке відхилення від синхронності.
 * --------      - - -       ----     -----  
 * ========       = = =         =======
 * 
 * Способи реалізації асинхронності
 * - мережеві технології
 *    = network - розподіл роботи по вузлах мережі
 *    = grid - суперкомп'ютери, складені з дискретних виконавців
 * - багатопроцесність
 * - багатопоточність
 * - багатозадачність
 * 
 * Процес - термін операційної системи - виконання програми
 *  іншими словами: запустити програму = створити процес
 *  процес обслуговується процесором
 *  
 * Потік (thread, потік коду) [не плутати з stream - потоком даних]
 *  частини процесів, обслуговуються ядрами процесора
 *  
 * Задача - поняття рівня мови програмування (платформи)
 *  зазвичай об'єкт, що відповідає за асинхронне виконання коду
 */
