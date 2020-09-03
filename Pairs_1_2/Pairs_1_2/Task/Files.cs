using System;
using System.IO;

namespace Pairs_1_2.Task
{
    class Files
    {

        public static void Func()
        {
            string[] dirs = Directory.GetDirectories(@"C:\\");
            int choice;
            string[] files;
            FileInfo fileinf;

            Console.WriteLine("Подкаталоги: ");
            for (int i = 0; i < dirs.Length; i++)
                Console.WriteLine($"{i + 1}) {dirs[i]}");

            Console.Write($"Выберете каталог, в котором необходимо просмотреть файлы (1 - {dirs.Length}): ");
            choice = Convert.ToInt32(Console.ReadLine()) - 1;
            Console.WriteLine($"\nФайлы в каталоге {dirs[choice]}: ");
            try
            {
                files = Directory.GetFiles(dirs[choice]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            if (files.Length == 0)
                Console.WriteLine("Папка пуста!");
            else
            {
                for (int i = 0; i < files.Length; i++)
                {
                    Console.WriteLine($"{i + 1}) {files[i]}");
                }
            }


            Console.WriteLine($"Выберете файл с которым будет работа (1 - {files.Length}): ");
            choice = Convert.ToInt32(Console.ReadLine()) - 1;
            Console.WriteLine("Выберете действие:\n1) Получить информацию о файле\n2) Удалить файл\n3) Переместить файл\n4) Копировать файл");

            switch (Convert.ToInt32(Console.ReadLine()))
            {
                case 1: 
                    fileinf = new FileInfo(files[choice]);
                    if (fileinf.Exists)
                    {
                        Console.WriteLine($"Имя: {fileinf.Name}");
                        Console.WriteLine($"Время создания: {fileinf.CreationTime}");
                        Console.WriteLine($"Размер: {fileinf.Length}");
                    }
                    break;
                case 2:
                    fileinf = new FileInfo(files[choice]);
                    if (fileinf.Exists)
                    {
                        fileinf.Delete();
                        Console.WriteLine("Файл удален");
                    }
                    break;
                case 3:
                    //string newpath = @"C:\\TestCatalog2\";
                    fileinf = new FileInfo(files[choice]);
                    if (fileinf.Exists)
                    {
                        fileinf.MoveTo(@"C:\\TestCatalog2\"); // не работает
                        Console.WriteLine("Файл перемещен");
                    }
                    break;
                case 4:
                    string newpath = @"C:\TestCatalog2\Test2.txt";
                    fileinf = new FileInfo(files[choice]);
                    if (fileinf.Exists)
                    {
                        fileinf.CopyTo(newpath, true); // не работает
                        Console.WriteLine("Файл скопирован");
                    }
                    break;
            }




        }
    }
}
