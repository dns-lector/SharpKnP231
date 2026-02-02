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
            // ProcessesDemo();
            ProcessControlDemo();
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
                if (!process.HasExited) process.Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Виникла помилка: {ex.Message}");
            }
        }
        /* Д.З. Реалізувати запуск процесів
         * - блокнот (або інший редактор)
         * - браузер (* з пошуком наявного)
         * * калькулятор (за наявності)
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
