using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTaskForVS
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "-file")
            {
                ProcessFile(args[1]);
            }
            else if (args.Length == 0)
            {
                RunInteractiveMode();
            }
            else
            {
                Console.WriteLine(@"Incorrect arguments.
                This program allows you to use following combinations of arguments:
                1) no arguments: run interactive mode, process expressions on by one.
                2) -file <filename>: process file with list of expressions");
            }
        }


        static void ProcessFile(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Error. File not exists: " + filename);
                return;
            }

            var outWriter = new StreamWriter(filename + ".out", false);

            var lines = File.ReadLines(filename); //note: ReadLines do not load whole file in memory
            foreach(var line in lines)
            {
                try
                {
                    outWriter.WriteLine(ProcessExpression(line));
                }
                catch (Exception e)
                {
                    outWriter.WriteLine("Error: " + e.Message);
                }
            }
            outWriter.Close();

            //по кодировке - только английские символы, кириллица не допускается
            //потоковое чтение и запись результата
        }

        static void RunInteractiveMode()
        {
            while (true)
            {
                Console.WriteLine("Please input an expression. If you want leave - press Ctrl+C");
                var expr = Console.ReadLine();
                try
                {
                    Console.WriteLine("Result: " + ProcessExpression(expr));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }

        static string ProcessExpression(string expr)
        {
            var e = new Expression(expr);
            var polynomial = e.Process();
            return polynomial.ToStandardForm();
        }

    }
}
