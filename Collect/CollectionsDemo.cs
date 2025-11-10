using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpKnP321.Collect
{
    internal class CollectionsDemo
    {
        public void Run()
        {
            Console.WriteLine("Collections Demo");
            /* Масив(класично) - спосіб збереження даних, за якого однотипні дані розміщуються у
             * пам'яті послідовно і мають визначений розмір.
             * У C#.NET масив - об'єкт, який забезпечує управління класичним масивом.
             */
            String[] arr1 = new String[3];
            String[] arr2 = new String[3] { "1", "2", "3" };
            String[] arr3 = { "1", "2", "3" };
            String[] arr4 = [ "1", "2", "3" ];
            arr1[0] = new("Str 1");   // базовий синтаксис роботи з елементами масивів
            arr1[1] = arr2[0];        // забезпечується індексаторами на відміну від С++
                                      // де [n] - це розіменування зі зміщенням: a[n] == *(a + n)
                                      // Dereferencing (Posible null dereferencing -> NullReferenceExc)
            
            arr1[0] = "New Str 1";
            int x = arr1.Length;

            /* На відміну від масивів колекції:
             * дозволяють змінний розмір
             * дозволяють непослідовне збереження
             */
        }
    }
}
/* Garbage Collector
 * [obj1  obj2  obj3 ....]
 * [obj1  ----  obj3 ....] 
 * 
 * 
 *                pointer                      pointer
 *                   |                           |
 * GC: [obj1  ----  obj3 ....] --> [obj1 obj3 ........] 
 *                   |                    |
 *               reference             reference
 * 
 * 
 * [ arr1[0] arr1[1] ...        "Str1" ... "New Str 1" ]
 *      \_x______________________/           /
 *        \_________________________________/
 */
