using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {


            var cv = MyConfig.GetConfigValues();
            Console.WriteLine(cv.datetime);
            Console.WriteLine(cv.dir1);
            Console.WriteLine(cv.filename);



            cv = MyConfig.GetConfigValues();
            Console.WriteLine(cv.datetime);
            Console.WriteLine(cv.dir1);
            Console.WriteLine(cv.filename);
           
            Console.ReadKey();

        }


       
    }
}
