using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pairs_3_4
{
    class Main_EXP
    {
        public static bool End = false;
        public static String DecodeString;
        const int LatAlpLettCount = 26;

        static Thread[] threads;
        static List<char[]> start = new List<char[]>();
        static List<char[]> exit = new List<char[]>();
        private static Stopwatch stopwatch = new Stopwatch();

        static void Main(string[] args)
        {
            List<string> hashes = new List<string>
            {
                "1115dd800feaacefdf481f1f9070374a2a81e27880f187396db67958b207cbad",
                "3a7bd3e2360a3d29eea436fcfb7e44c735d117c42d1c1835420b6b9942dd4f1b",
                "74e1bb62f8dabb8125a58852b63bdf6eaef667cb56ac7f7cdba6d7305c50a22f",
                "71703167e89a7ff6c26383f8041f6e19dbcdbab278c311536997b761d07c3def"
            };


            while (true)
            {
                Console.WriteLine("Avaliable keys: ");
                for (int i = 0; i < hashes.Count; i++)
                    Console.WriteLine($"{i + 1}) {hashes[i]}");

                Console.WriteLine("\nMenu:\n1) Add key\n2) BruteForce the existing key (NonAsyncAlgorithm)\n3) BruteForce the existing key (Task's)\n4) Exit");

                switch (Convert.ToInt32(Console.ReadLine()))
                {
                    case 1:
                        Console.WriteLine("\nEnter SHA-256 hash: ");
                        hashes.Add(Console.ReadLine());
                        Console.WriteLine("Hash has been added successfully\n");
                        break;
                    case 2:
                        Console.Write($"Choose hash (1 - {hashes.Count}): ");
                        AsyncRecursFunc asyncRecurs = new AsyncRecursFunc(hashes[Convert.ToInt32(Console.ReadLine()) - 1], new char[] {'a', 'a', 'a', 'a', 'a', 'a'}, new char[] {'z', 'z', 'z', 'z', 'z', 'z'});
                        stopwatch.Start();
                        Thread thr = new Thread(new ThreadStart(asyncRecurs.Func));
                        //Thread thr = new Thread(new AsyncRecursFunc(hashes[Convert.ToInt32(Console.ReadLine()) - 1], new char[] { 'a', 'a', 'a', 'a', 'a', 'a' }, new char[] { 'z', 'z', 'z', 'z', 'z', 'z' }).Func);
                        thr.Start();
                        thr.Join();
                        Console.WriteLine("Result - " + DecodeString + $". Elapsed time: {Convert.ToDouble(stopwatch.ElapsedMilliseconds) / 1000} seconds");
                        break;
                    case 3:
                        start = new List<char[]>();
                        exit = new List<char[]>();
                        Console.Write($"Choose hash (1 - {hashes.Count}): ");
                        int hash = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Enter amount of parallel threads(1 - 8): ");
                        int p = Convert.ToInt32(Console.ReadLine());
                        stopwatch.Start();
                        threads = LatAlpLettCount % p == 0 ? new Thread[p] : new Thread[p + 1];
                        for (int i = 0; i < p; i++)
                        {
                            start.Add(new char[] { (char)(97 + i * (LatAlpLettCount / p)), 'a', 'a', 'a', 'a', 'a' });
                            exit.Add(new char[] { (char)(97 + (i + 1) * (LatAlpLettCount / p) - 1), 'z', 'z', 'z', 'z', 'z' });
                        }
                        if (exit[exit.Count - 1][0] != 'z')
                        {
                            start.Add(new char[] { (char)((int)(exit[exit.Count - 1][0]) + 1), 'a', 'a', 'a', 'a', 'a' });
                            exit.Add(new char[] { 'z', 'z', 'z', 'z', 'z', 'z' });
                        }

                        for (int i = 0; i < threads.Length; i++)
                        {
                            AsyncRecursFunc temp = new AsyncRecursFunc(hashes[hash - 1], start[i], exit[i]);
                            threads[i] = new Thread(temp.Func);
                            threads[i].Start();
                        }
                        foreach (var thread in threads)
                        {
                            thread.Join();
                        }

                        foreach (var strt in start)
                        {
                            if (AsyncRecursFunc.ComputeHash(strt) == hashes[hash - 1])
                            {
                                DecodeString = new string(strt);
                                break;
                            }
                        }
                        Console.WriteLine("Result - " + DecodeString + $". Elapsed time: {Convert.ToDouble(stopwatch.ElapsedMilliseconds) / 1000} seconds");
                        break;
                    case 4:
                        Environment.Exit(0);
                        break;
                }
                End = false;
                stopwatch.Stop();
                stopwatch.Reset();
                Console.ReadKey();
            }
        }

    }
}
