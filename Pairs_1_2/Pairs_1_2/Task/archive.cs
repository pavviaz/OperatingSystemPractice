using System;
using System.IO.Compression;

namespace Pairs_1_2.Task
{
    class archive
    {
        public static void FuncMain()
        {
            string sourceFile = "C://TestCatalog";
            string targetFile = "C://TestCatalog2/archive.zip";

            ZipFile.CreateFromDirectory(sourceFile, targetFile);

            Console.WriteLine($"Папка {sourceFile} архивирована"); 

            ZipFile.ExtractToDirectory(targetFile, @"C://");

            Console.WriteLine($"Папка {sourceFile} разархивирована в C://");

        }

    }
}
