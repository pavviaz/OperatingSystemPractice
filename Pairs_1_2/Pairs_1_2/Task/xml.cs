using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;

namespace Pairs_1_2.Task
{
    class PlayerData
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public int Damage { get; set; }
    }
    class xml
    {
        public static void Execute()
        {
            string xmlPath = @"C:\TestCatalog\playerData.xml";  //  Адрес хранения будущего XML-файла

            List<PlayerData> players = new List<PlayerData>();  //  Создаем список объектов типа PlayerData
            players.Add(new PlayerData { Name = "P1", Health = 100, Damage = 20 });
            players.Add(new PlayerData { Name = "P2", Health = 80, Damage = 25 });
            players.Add(new PlayerData { Name = "P3", Health = 120, Damage = 11 });

            XDocument xDoc = new XDocument(new XElement("players"));  //  Добавляем корневой элемент players
            foreach (PlayerData player in players)  //  Добавляем данные каждого объекта из списка в XML
            {
                xDoc.Root.Add(new XElement("player",
                    new XAttribute("name", player.Name), new XElement("health", player.Health), new XElement("damage", player.Damage)));
            }
            xDoc.Save(xmlPath);

            Console.WriteLine($"An XML file was generated at {xmlPath}!");

            foreach (XElement playerElement in xDoc.Root.Elements("player"))
            {
                XAttribute nameAttribute = playerElement.Attribute("name");
                XElement healthElement = playerElement.Element("health");
                XElement damageElement = playerElement.Element("damage");

                if (nameAttribute != null && healthElement != null && damageElement != null)
                {
                    Console.WriteLine($"{nameAttribute}");
                    Console.WriteLine($"\t{healthElement}");
                    Console.WriteLine($"\t{damageElement}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("\nPress any key to procceed and delete created .xml file...\n");
            Console.ReadKey();

            File.Delete(xmlPath);

            Console.WriteLine("Created files deleted successfully!\n");
        }
    }
    
}
