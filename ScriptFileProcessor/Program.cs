using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ScriptFileProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileText;
            using (var fileStream = File.OpenText(args[0]))
            {
                // READ
                fileText = fileStream.ReadToEnd();
            }
            
            // REPLACE TEXT
            fileText = Regex.Replace(fileText, @"namespace [a-zA-Z.0-9_@]+\s*\r?\n\s*{", string.Empty);
            fileText = Regex.Replace(fileText, @"\}\s*$", string.Empty);

            //Console.WriteLine("File Content: {0}", fileText);

            using (var newTask = new StreamWriter(args[0], false))
            {
                // OVERWRITE
                newTask.WriteLine(fileText);
            }

            Console.WriteLine("File Content Replaced: {0}", args[0]);
        }
    }
}
