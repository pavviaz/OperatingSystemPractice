using System;
using System.IO;

namespace Pairs_1_2.Task
{
    class Directories
    {
        public static void Func()
        {
            string dirName = "C:\\";
            string[] dirs = Directory.GetDirectories(dirName);
            string[] files;
            int choice;
            DirectoryInfo dirInfo;

            if (Directory.Exists(dirName))
            {
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
                    foreach (var file in files)
                    {
                        Console.WriteLine(file);
                    }
                }

                Console.WriteLine($"\nВыберете действие:\n1)Получить информацию о каталоге\n2)Создать каталог\n3)Удалить каталог (будьте аккуратны)");
                switch (Convert.ToInt32(Console.ReadLine()))
                {
                    case 1:
                        Console.WriteLine("\nПодкаталоги диска C: ");
                        for (int i = 0; i < dirs.Length; i++)
                            Console.WriteLine($"{i + 1}) {dirs[i]}");
                        Console.Write($"Выберете каталог, информацию о котором необходимо узнать (1 - {dirs.Length}): ");
                        choice = Convert.ToInt32(Console.ReadLine()) - 1; 
                        dirInfo = new DirectoryInfo(dirs[choice]);
                        Console.WriteLine($"\nИнформация о каталоге {dirs[choice]}: ");
                        Console.WriteLine($"Название: {dirInfo.Name}");
                        Console.WriteLine($"Полное название: {dirInfo.FullName}");
                        Console.WriteLine($"Время создания: {dirInfo.CreationTime}");
                        Console.WriteLine($"Корневой каталог: {dirInfo.Root}");
                        break;

                    case 2:
                        Console.WriteLine();
                        Console.WriteLine("\nПодкаталоги диска C: ");
                        for (int i = 0; i < dirs.Length; i++)
                            Console.WriteLine($"{i + 1}) {dirs[i]}");
                        Console.Write($"Выберете каталог, в котором вы хотите создать подкаталог (1 - {dirs.Length}): ");
                        choice = Convert.ToInt32(Console.ReadLine()) - 1;
                        dirInfo = new DirectoryInfo(dirs[choice]);
                        if(!dirInfo.Exists)
                            dirInfo.Create();
                        Console.WriteLine("Введите название папки: ");
                        dirInfo.CreateSubdirectory(Console.ReadLine());
                        Console.WriteLine("Папка успешно создана");
                        break;
                    case 3:
                        Console.WriteLine();
                        Console.WriteLine("\nПодкаталоги диска C: ");
                        for (int i = 0; i < dirs.Length; i++)
                            Console.WriteLine($"{i + 1}) {dirs[i]}");
                        Console.Write($"Выберете каталог, который вы хотите удалить (1 - {dirs.Length}): ");
                        choice = Convert.ToInt32(Console.ReadLine()) - 1;
                        try
                        {
                            dirInfo = new DirectoryInfo(dirs[choice]);
                            dirInfo.Delete(true);
                            Console.WriteLine("Каталог удален");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                        break;
                }


            }

        }



    }
}
