using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pairs_9_10_V2
{

    class MemoryControl
    {
        public const string RAM_Way = @"RAM.txt";
        public const string SWAP_Way = @"SWAP.txt";
        private const int MemoryBites = 65535;
        public const int SWAP_LIMIT = 524288;
        public static int SWAP_SIZE = 0;
        public static bool WriteInProcessRAM;
        public static bool WriteInProcessSWAP;

        public static List<int> FreeSegmentIndex = new List<int>();

        public static readonly List<string> _processes = new List<string>
        {
            "PROCESS_1",
            "PROCESS_2",
            "PROCESS_3"
        };

        public static readonly List<string> _programData = new List<string>
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

        private static void WritingToSWAP(int SegmentCount)
        {
            WriteInProcessSWAP = true;
            List<string> arr = new List<string>();
            List<char> arrChar = new List<char>();

            if (SWAP_SIZE + SegmentCount*4 > SWAP_LIMIT)
                throw new Exception("SWAP_FILE_OVERFLOW");

            for (int k = 0; k < SegmentCount; k++)
            {
                for (int i = k; i < k + 4; i++)
                {
                    arr.Add($"[{new string('0', SWAP_LIMIT.ToString().Length - SWAP_SIZE.ToString().Length) + SWAP_SIZE}]");
                    SWAP_SIZE++;
                }
                var tempSegm = new Segment();
                tempSegm.GenRandom();
                var SegmParams = tempSegm.Params();
                arr[arr.Count - 4] += SegmParams[0];
                arr[arr.Count - 3] += SegmParams[1];
                arr[arr.Count - 2] += SegmParams[2];
                arr[arr.Count - 1] += SegmParams[3];
            }

            for (int i = 0; i < arr.Count; i++)
            {
                if (!arr[i].Contains('\n'))
                    arr[i] += '\n';
            }

            foreach (var str in arr)
                foreach (var chr in str)
                    arrChar.Add(chr);

            byte[] array = Encoding.Default.GetBytes(arrChar.ToArray());

            using (FileStream fstream = new FileStream(SWAP_Way, FileMode.Append))
            {
                fstream.Write(array, 0, array.Length);
            }

            WriteInProcessSWAP = false;

        }

        public static void WritingToRAM(int SegmentCount)
        {
            WriteInProcessRAM = true;
            byte[] array;
            string[] textFromFile;
            List<char> arr = new List<char>();
            using (FileStream fstream = new FileStream(RAM_Way, FileMode.Open))
            {
                array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
                var a = Encoding.Default.GetString(array);
                a = a.Remove(a.LastIndexOf('\n'), 1);
                textFromFile = a.Split('\n');
            }
            FreeSegmentsUpdate();


            for (int i = 0; i < SegmentCount; i++)
            {

                if (FreeSegmentIndex.Count == 0)
                {
                    WritingToSWAP(SegmentCount - i);
                    break;
                }

                var tempSegm = new Segment();
                tempSegm.GenRandom();
                var SegmParams = tempSegm.Params();
                textFromFile[FreeSegmentIndex[0]] += SegmParams[0];
                textFromFile[FreeSegmentIndex[0] + 1] += SegmParams[1];
                textFromFile[FreeSegmentIndex[0] + 2] += SegmParams[2];
                textFromFile[FreeSegmentIndex[0] + 3] += SegmParams[3];
                FreeSegmentIndex.RemoveAt(0);

            }

            for(int i = 0 ; i < textFromFile.Length; i++)
            {
                if (!textFromFile[i].Contains('\n'))
                    textFromFile[i] += '\n';
            }

            File.WriteAllText(RAM_Way, string.Empty);

            foreach (var str in textFromFile)
                foreach (var chr in str)
                    arr.Add(chr);

            array = Encoding.Default.GetBytes(arr.ToArray());
            using (FileStream fstream = new FileStream(RAM_Way, FileMode.Open))
            {
                fstream.Write(array, 0, array.Length);
            }

            WriteInProcessRAM = false;

        }

        public static void DeleteProcessSegments(string ProcessErase)
        {

            #region RAM_ERASING

            WriteInProcessRAM = true;
            byte[] array;
            string[] textFromFile = {};
            List<char> arr = new List<char>();
            using (FileStream fstream = new FileStream(RAM_Way, FileMode.Open))
            {
                array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
                var a = Encoding.Default.GetString(array);
                if (a.Length != 0)
                {
                    a = a.Remove(a.LastIndexOf('\n'), 1);
                    textFromFile = a.Split('\n');
                }
            }

            if (textFromFile.Length != 0)
            {
                for (int i = 0; i < textFromFile.Length; i += 4)
                {
                    if (textFromFile[i].Remove(0, 7) == ProcessErase)
                    {
                        textFromFile[i] = textFromFile[i].Replace(ProcessErase, "");
                        textFromFile[i + 1] = textFromFile[i + 1].Remove(7, textFromFile[i + 1].Length - 7);
                        textFromFile[i + 2] = textFromFile[i + 2].Remove(7, textFromFile[i + 2].Length - 7);
                        textFromFile[i + 3] = textFromFile[i + 3].Remove(7, textFromFile[i + 3].Length - 7);
                    }
                }

                for (int i = 0; i < textFromFile.Length; i++)
                {
                    if (!textFromFile[i].Contains('\n'))
                        textFromFile[i] += '\n';
                }

                File.WriteAllText(RAM_Way, string.Empty);

                foreach (var str in textFromFile)
                foreach (var chr in str)
                    arr.Add(chr);

                array = Encoding.Default.GetBytes(arr.ToArray());
                using (FileStream fstream = new FileStream(RAM_Way, FileMode.Open))
                {
                    fstream.Write(array, 0, array.Length);
                }
            }

            WriteInProcessRAM = false;
            #endregion


            #region SWAP_ERASING

            WriteInProcessSWAP = true;
            List<string> SwapSegments = new List<string>();
            arr = new List<char>();
            using (FileStream fstream = new FileStream(SWAP_Way, FileMode.Open))
            {
                array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
                var a = Encoding.Default.GetString(array);
                if (a.Length != 0)
                {
                    a = a.Remove(a.LastIndexOf('\n'), 1);
                    SwapSegments = a.Split('\n').ToList();
                }
            }

            if (SwapSegments.Count != 0)
            {
                foreach (var segment in SwapSegments.ToArray())
                {
                    if (segment.Remove(0, 8) == ProcessErase)
                    {
                        var k = SwapSegments.IndexOf(segment);
                        SwapSegments.RemoveAt(k);
                        SwapSegments.RemoveAt(k);
                        SwapSegments.RemoveAt(k);
                        SwapSegments.RemoveAt(k);
                    }
                }

                SWAP_SIZE = 0;

                for (int i = 0; i < SwapSegments.Count; i++)
                {
                    SwapSegments[i] = SwapSegments[i].Remove(0, 8);
                    SwapSegments[i] = SwapSegments[i].Insert(0, $"[{new string('0', SWAP_LIMIT.ToString().Length - SWAP_SIZE.ToString().Length) + SWAP_SIZE}]");
                    SWAP_SIZE++;
                }

                for (int i = 0; i < SwapSegments.Count; i++)
                {
                    if (!SwapSegments[i].Contains('\n'))
                        SwapSegments[i] += '\n';
                }

                File.WriteAllText(SWAP_Way, string.Empty);

                foreach (var str in SwapSegments)
                    foreach (var chr in str)
                        arr.Add(chr);

                array = Encoding.Default.GetBytes(arr.ToArray());
                using (FileStream fstream = new FileStream(SWAP_Way, FileMode.Open))
                {
                    fstream.Write(array, 0, array.Length);
                }
            }

            WriteInProcessSWAP = false;
            #endregion

            FreeSegmentsUpdate();

        }

        public static void FreeSegmentsUpdate()
        {
            int k = 0;
            string[] textFromFile;
            FreeSegmentIndex.Clear();

            using (FileStream fstream = new FileStream(RAM_Way, FileMode.Open))
            {
                var array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
                var a = Encoding.Default.GetString(array);
                a = a.Remove(a.LastIndexOf('\n'), 1);
                textFromFile = a.Split('\n');
            }

            for (int i = 0; i < textFromFile.Length; i++)
            {
                if (textFromFile[i].Remove(0, 7) == string.Empty || textFromFile[i].Remove(0, 7) == "\n")
                    k++;
                else
                    k = 0;
                if (k == 4)
                {
                    k = 0;
                    FreeSegmentIndex.Add(i - 3);
                }
            }
            //FreeSegmentIndex.Sort();
        }


    }

    class Segment
    {
        public string Process;
        public string Command;
        public string Arg1;
        public string Arg2;

        public Segment()
        {

        }
        public Segment(string Process, string Command, string Arg1, string Arg2)
        {
            this.Process = Process;
            this.Command = Command;
            this.Arg1 = Arg1;
            this.Arg2 = Arg2;
        }

        public void GenRandom()
        {
            Process = MemoryControl._processes[RandNumber(0, MemoryControl._processes.Count - 1)];
            Command = MemoryControl._programData[RandNumber(0, MemoryControl._programData.Count - 1)];
            Arg1 = $"%{RandNumber(10, 99)} , %{RandNumber(10, 99)}";
            Arg2 = $"%{RandNumber(10, 99)} , %{RandNumber(10, 99)}";
        }

        public string[] Params()
        {
            string[] ret = new string[4];
            ret[0] = Process + '\n';
            ret[1] = Command + '\n';
            ret[2] = Arg1 + '\n';
            ret[3] = Arg2 + '\n';
            return ret;
        }

        private static int RandNumber(int Low, int High)
        {
            return new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber)).Next(Low, High + 1);
        }

    }

    class Program
    {

        public static int SegmentsToWrite = 4096;
        public static int ProcessPointer;
        private static bool Error;

        static async Task Main(string[] args)
        {
            Console.CursorVisible = false;
            MemoryControl.Initialization(); 
            MemoryControl.FreeSegmentsUpdate();
            KeyListener();
            await UIUpdate();


            Console.BackgroundColor = ConsoleColor.Blue;
            Clear(1, 1, Console.WindowWidth - 1, Console.WindowHeight);
            Console.SetCursorPosition(Console.WindowWidth / 4 , Console.WindowHeight / 2);
            Console.Write(":(   Your PC Ran into a Problem and Needs to Restart");

            Console.ReadKey();
        }

        public static async Task UIUpdate()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if(Error)
                        break;
                    Thread.Sleep(20);
                    Clear(1, 1, Console.WindowWidth, 25);
                    
                    Console.SetCursorPosition(1, 1);
                    Console.Write($"PRESS SPACE TO WRITE");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($" {SegmentsToWrite} ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("SEGMENTS");

                    Console.SetCursorPosition(1, 3);
                    Console.Write($"PRESS ENTER TO DELETE ALL");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($" {MemoryControl._processes[ProcessPointer]} ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("SEGMENTS FROM RAM AND SWAP");

                    Console.SetCursorPosition(1, 5);
                    Console.Write($"FREE SEGMENTS IN RAM:");
                    Console.ForegroundColor = MemoryControl.FreeSegmentIndex.Count != 0 ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
                    Console.Write( $" { MemoryControl.FreeSegmentIndex.Count}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" ; FREE SEGMENTS IN SWAP:");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write($" { MemoryControl.SWAP_LIMIT/4 - MemoryControl.SWAP_SIZE/4}");
                    Console.ForegroundColor = ConsoleColor.White;


                    Console.SetCursorPosition(1, 7);
                    if (MemoryControl.WriteInProcessRAM)
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("RAM_USING");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" ; ");
                    if (MemoryControl.WriteInProcessSWAP)
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("SWAP_USING");
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.SetCursorPosition(1, 9);
                    Console.Write("PRESS 0 TO INCREASE STR AMOUNT ; PRESS 9 TO DECREASE STR AMOUNT"); 
                    Console.SetCursorPosition(1, 11);
                    Console.Write("PRESS \\ TO CHANGE PROCESS SEGMENTS TO DELETE");

                }
            });
        }

        private static async Task KeyListener()
        {
            await Task.Run(() =>
            {
                while (!Error)
                {
                    switch (Console.ReadKey().KeyChar)
                    {
                        case (char)ConsoleKey.D9:
                            if (SegmentsToWrite - 4096 > 0)
                                SegmentsToWrite -= 4096;
                            break;
                        case (char)ConsoleKey.D0:
                            if (SegmentsToWrite + 4096 <= 16384)
                                SegmentsToWrite += 4096;
                            break;
                        case '\\':
                            if (ProcessPointer == 2)
                                ProcessPointer = 0;
                            else
                                ProcessPointer++;
                            break;
                        case (char)ConsoleKey.Enter:
                            try
                            {
                                MemoryControl.DeleteProcessSegments(MemoryControl._processes[ProcessPointer]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                            break;
                        case ' ':
                            try
                            {
                                MemoryControl.WritingToRAM(SegmentsToWrite);
                            }
                            catch (Exception e)
                            {
                                Error = true;
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
