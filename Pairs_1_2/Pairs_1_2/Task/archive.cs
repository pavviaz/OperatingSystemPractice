using System;
using System.IO.Compression;

namespace Pairs_1_2.Task
{
    class archive
    {
        public static void FuncMain()
        {
            string sourceFile = "C://TestCatalog";
            string zipFile = "C://TestCatalog.zip"; 
            string targetFile = "C://TestCatalog2";

            ZipFile.CreateFromDirectory(sourceFile, zipFile);

            Console.WriteLine($"Папка {sourceFile} архивирована"); 

            ZipFile.ExtractToDirectory(zipFile, targetFile);

            Console.WriteLine($"Папка {sourceFile} разархивирована в {targetFile}");

        }

    }
}
