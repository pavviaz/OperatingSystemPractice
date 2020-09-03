using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pairs_1_2.Task
{
    class Person
    {
        public string Name { get; set; }
        public  int Age { get; set; }
    }

    class jsn
    {
        public static void Func()
        {
            //для корректной работы программы необходимо добавить пакет Newtonsoft Json через Nuget

            Person tom = new Person() { Name = "Tom", Age = 35 };
            Person UnserilizedTom;

            //сериализация
            File.WriteAllText(@"C:\TestCatalog\jsn.json", JsonConvert.SerializeObject(tom));
            Console.WriteLine("The object has been serilized");

            //десериализация
            UnserilizedTom = JsonConvert.DeserializeObject<Person>(File.ReadAllText(@"C:\TestCatalog\jsn.json"));
            Console.WriteLine($"Name = {UnserilizedTom.Name}\nAge = {UnserilizedTom.Age}");

        }
    }
}
