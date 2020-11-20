using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pairs_9_10
{

    class MemoryControl
    {
        public const string RAM_Way = @"RAM.txt";
        public const string SWAP_Way = @"SWAP.txt";
        private const int MemoryBites = 65535;
        private const int SWAP_LIMIT = 524288;

        public static bool WriteInExecution;
        public static string[] textFromFile;
        public static string index_RAM = "";
        public static string index_SWAP = "";

        public static string lastProcess_RAM;
        public static string lastProcess_SWAP;
        public static string lastCommand_RAM;
        public static string lastCommand_SWAP;
        public static string lastArguments_RAM;
        public static string lastArguments_SWAP;

        public static int SWAP_SIZE = 0;

        private static readonly List<string> _processes = new List<string>
        {
            "PROCESS_1",
            "PROCESS_2",
            "PROCESS_3"
        };

        private static readonly List<string> _programData = new List<string>
        {
            "mov",
            "psh",
            "pop",
            "LRA",
            "add",
            "sub",
            "inc",
            "dec"
        };

        public static void Initialization()
        {
            File.WriteAllText(RAM_Way, String.Empty);
            File.WriteAllText(SWAP_Way, String.Empty);


            using (FileStream fstream = new FileStream(RAM_Way, FileMode.OpenOrCreate))
            {
                for (int i = 0; i <= MemoryBites; i++)
                {
                    var array = Encoding.Default.GetBytes($"[{new string('0', MemoryBites.ToString().Length - i.ToString().Length) + i}]\n");
                    fstream.Write(array, 0, array.Length);
                }
            }

        }

        private static void Program_Loading_To_SWAP(int writeKB)
        {

            List<string> arr = new List<string>();
            List<char> arrChar = new List<char>();
            byte[] array;

            //if (SWAP_SIZE == 0)
            //    File.WriteAllText(SWAP_Way, String.Empty);

            if (SWAP_SIZE >= SWAP_LIMIT)
                throw new Exception("SWAP_FILE_OVERFLOW");

            for (int k = 0; k < writeKB; k += 4)
            {
                for (int i = k; i < k + 4; i++)
                {
                    arr.Add($"[{new string('0', SWAP_LIMIT.ToString().Length - SWAP_SIZE.ToString().Length) + SWAP_SIZE}]");
                    SWAP_SIZE++;
                }
                    
                arr[k] += _processes[RandNumber(0, _processes.Count - 1)];
                lastProcess_SWAP = arr[k].Remove(0, 8);
                arr[k + 1] += _programData[RandNumber(0, _programData.Count - 1)];
                lastCommand_SWAP = arr[k + 1].Remove(0, 8);
                arr[k + 2] += $"%{RandNumber(10, 99)} , %{RandNumber(10, 99)}";
                index_SWAP = arr[k + 3];
                arr[k + 3] += $"%{RandNumber(10, 99)} , %{RandNumber(10, 99)}";
                lastArguments_SWAP = arr[k + 2].Remove(0, 8) + " ; " + arr[k + 3].Remove(0, 8);
                //Thread.Sleep(1);
            }

            for (int i = 0; i < arr.Count; i++)
                arr[i] += '\n';

            foreach (var str in arr)
                foreach (var chr in str)
                    arrChar.Add(chr);

            array = Encoding.Default.GetBytes(arrChar.ToArray());

            using (FileStream fstream = new FileStream(SWAP_Way, FileMode.Append))
            {
                fstream.Write(array, 0, array.Length);
            }
            WriteInExecution = false;

        }

        public static void Program_Loading_To_RAM(int writeKB)
        {

            WriteInExecution = true;

            if (index_RAM.Length != 0 && Convert.ToInt32(index_RAM.Replace("[", "").Replace("]", "")) >= MemoryBites) //swapping
            {
                Program_Loading_To_SWAP(writeKB);
                return;
            }


            byte[] array;
            List<char> arr = new List<char>();
            int fromWrite = 0, toWrite = 0;

            using (FileStream fstream = new FileStream(RAM_Way, FileMode.Open))
            {
                array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
            }

            var a = Encoding.Default.GetString(array);
            a = a.Remove(a.LastIndexOf('\n'), 1);
            textFromFile = a.Split('\n');


            for (int i = 0; i < textFromFile.Length; i++)
            {
                if (textFromFile[i].Remove(0, 7) == string.Empty)
                {
                    fromWrite = i;
                    break;
                }
            }

            for (int k = fromWrite; k < fromWrite + writeKB; k += 4)
            {
                if (k + 1 >= textFromFile.Length || k + 2 >= textFromFile.Length || k + 3 >= textFromFile.Length)
                    break;
                textFromFile[k] += _processes[RandNumber(0, _processes.Count - 1)];
                lastProcess_RAM = textFromFile[k].Remove(0, 7);
                textFromFile[k + 1] += _programData[RandNumber(0, _programData.Count - 1)];
                lastCommand_RAM = textFromFile[k + 1].Remove(0, 7);
                textFromFile[k + 2] += $"%{RandNumber(10, 99)} , %{RandNumber(10, 99)}";
                index_RAM = textFromFile[k + 3];
                textFromFile[k + 3] += $"%{RandNumber(10, 99)} , %{RandNumber(10, 99)}";
                lastArguments_RAM = textFromFile[k + 2].Remove(0, 7) + " ; " + textFromFile[k + 3].Remove(0, 7);

                //Thread.Sleep(1);
            }

            for (int i = 0; i < textFromFile.Length; i++)
                textFromFile[i] += '\n';


            File.WriteAllText(RAM_Way, string.Empty);

            foreach (var str in textFromFile)
                foreach (var chr in str)
                    arr.Add(chr);

            array = Encoding.Default.GetBytes(arr.ToArray());
            using (FileStream fstream = new FileStream(RAM_Way, FileMode.Open))
            {
                fstream.Write(array, 0, array.Length);
            }

            WriteInExecution = false;
        }


        private static int RandNumber(int Low, int High)
        {
            return new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber)).Next(Low, High + 1);
        }

    }

    class Program
    {
        public static bool y = false;
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            //Task a = new Task(() => Update());
            //a.Start();
            KeyListener();
            MemoryControl.Initialization();

            //y = true;

            while (!y)
            {
                Update();
                Thread.Sleep(20);
            }




            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }

        public static void Update()
        {

            Clear(1, 1, Console.BufferWidth, 25);
            //Console.Write("t = " + ptr);
            Console.SetCursorPosition(1, 1);
            Console.Write("PRESS SPACE TO WRITE 16KB TO ");
            Console.Write(MemoryControl.index_RAM == "[65535]" ? "SWAP_FILE: " : "RAM: ");

            if (!MemoryControl.WriteInExecution)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("IDLE");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("WRITING...");
            }

            Console.ForegroundColor = ConsoleColor.White;

            Console.SetCursorPosition(1, 3);
            Console.Write($"LAST_COMMAND_WRITTEN_TO_RAM: ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(MemoryControl.lastCommand_RAM);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" WITH ARGUMENTS: ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(MemoryControl.lastArguments_RAM);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" IN ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(MemoryControl.lastProcess_RAM);


            Console.ForegroundColor = ConsoleColor.White;

            Console.SetCursorPosition(1, 5);
            if (MemoryControl.index_RAM == "[65535]")
                Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"LAST_CHANGED_RAM_CELL: {MemoryControl.index_RAM}");
            Console.ForegroundColor = ConsoleColor.White;

            Console.SetCursorPosition(1, 7);
            Console.Write($"SWAP_SIZE = {MemoryControl.SWAP_SIZE}");

            Console.SetCursorPosition(1, 9);
            Console.Write($"LAST_COMMAND_WRITTEN_TO_SWAP: ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(MemoryControl.lastCommand_SWAP);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" WITH ARGUMENTS: ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(MemoryControl.lastArguments_SWAP);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" IN ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(MemoryControl.lastProcess_SWAP);

            //Console.Write(File. + " , i = " + MemoryControl.i);



        }

        private static async Task KeyListener()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    switch (Console.ReadKey().KeyChar)
                    {
                        case (char)ConsoleKey.Backspace:
                            y = true;
                            break;
                        case '\\':

                            break;
                        case (char)ConsoleKey.Enter:

                            break;
                        case (char)ConsoleKey.D0:
                            Environment.Exit(0);
                            break;
                        case ' ':
                            try
                            {
                                MemoryControl.Program_Loading_To_RAM(32768);
                                //Console.BackgroundColor = ConsoleColor.Black;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                            break;
                    }
                }

            });
        }

        public static void Clear(int x, int y, int width, int height)
        {
            int curTop = Console.CursorTop;
            int curLeft = Console.CursorLeft;
            for (; height > 0;)
            {
                Console.SetCursorPosition(x, y + --height);
                Console.Write(new string(' ', width));
            }
            Console.SetCursorPosition(curLeft, curTop);
        }

    }
}
