using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: tee <outputFile>");
            return;
        }

        string outputFile = args[0];

        using (StreamWriter writer = new StreamWriter(outputFile))
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                Console.WriteLine(line);
                writer.WriteLine(line);
            }
        }
    }
}