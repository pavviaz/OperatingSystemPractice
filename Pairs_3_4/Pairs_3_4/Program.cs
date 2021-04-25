using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pairs_3_4
{
    class Program
    {

        private const int LatAlpLettCount = 26;
        private static Stopwatch stopwatch = new Stopwatch();
        private static SHA256 sha256Hash;
        private static int choice;

        static void Main(string[] args)
        {
            List<string> hashes = new List<string>
            {
                "1115dd800feaacefdf481f1f9070374a2a81e27880f187396db67958b207cbad",
                "3a7bd3e2360a3d29eea436fcfb7e44c735d117c42d1c1835420b6b9942dd4f1b",
                "74e1bb62f8dabb8125a58852b63bdf6eaef667cb56ac7f7cdba6d7305c50a22f"
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
                        choice = Convert.ToInt32(Console.ReadLine()) - 1;
                        stopwatch.Start();
                        Console.WriteLine($"\nResult: {NonAsyncBruteForce(hashes[choice])}. Elapsed time: {Convert.ToDouble(stopwatch.ElapsedMilliseconds) / 1000} seconds");
                        stopwatch.Stop();
                        stopwatch.Reset();
                        break;
                    case 3:
                        Console.Write($"Choose hash (1 - {hashes.Count}): ");
                        choice = Convert.ToInt32(Console.ReadLine()) - 1;
                        Console.Write("Enter amount of parallel threads(1 - 7): ");
                        int p = Convert.ToInt32(Console.ReadLine());
                        stopwatch.Start();
                        Console.WriteLine($"\nResult: {AsyncBruteForce(hashes[choice], p).Result}. Elapsed time: {Convert.ToDouble(stopwatch.ElapsedMilliseconds) / 1000} seconds");
                        stopwatch.Stop();
                        stopwatch.Reset();
                        break;
                    case 4:
                        Environment.Exit(0);
                        break;
                }

                Console.ReadKey();
            }


        }

         

        public static async Task<string> AsyncBruteForce(string hash, int p)
        {

            bool[] br;
            bool end = false;
            string result = string.Empty;

            var tokenSource2 = new CancellationTokenSource();
            CancellationToken ct = tokenSource2.Token;

            List<char[]> pulls = new List<char[]>();
            List<char> exitChars = new List<char>();
            Task[] tasks;

            if (LatAlpLettCount % p == 0)
            {
                tasks = new Task[p];
                br = new bool[p];
            }
            else
            {
                tasks = new Task[p + 1];
                br = new bool[p + 1];
            }
            for (int i = 0; i < p; i++)
            {
                pulls.Add(new[] { Convert.ToChar(97 + i * (LatAlpLettCount / p)), 'a', 'a', 'a', 'a', 'a' });
                exitChars.Add(Convert.ToChar(97 + (i + 1) * (LatAlpLettCount / p) - 1));
            }
            if (exitChars[exitChars.Count - 1] != 'z')
            {
                pulls.Add(new[] { Convert.ToChar(Convert.ToInt32(exitChars[exitChars.Count - 1]) + 1), 'a', 'a', 'a', 'a', 'a' });
                exitChars.Add('z');
            }
            for (int t = 0; t < tasks.Length; t++)
            {
                var capi = t;
                tasks[capi] = Task.Run(() => AsyncCoreRecursFunc(0, hash, exitChars[capi], pulls[capi], ct, ref br[capi], ref end));
            }

            await Task.Run(() =>
            {
                while (true)
                {
                    if (end || br.All(u => u))
                        break;
                }

            });
            tokenSource2.Cancel();
            foreach (var vr in pulls)
            {
                if (ComputeHash(vr) == hash)
                {
                    result = new string(vr);
                    break;
                }
            }
            if (result != string.Empty)
                return result;
            return "No password has been found";
        }

        public static void AsyncCoreRecursFunc(int i, string hash, char exit, char[] chars, CancellationToken token, ref bool br, ref bool end)
        {
            while (true)
            {
                if (i != 5)
                    AsyncCoreRecursFunc(i + 1, hash, exit, chars, token, ref br, ref end);
                if (!end && ComputeHash(chars) == hash)
                    end = true;
                if (chars[1] == 'z' && chars[2] == 'z' && chars[3] == 'z' && chars[4] == 'z' && chars[5] == 'z')
                    if (chars[0] == exit)
                        br = true;
                if (chars[i] == 'z' || br || token.IsCancellationRequested || end)
                    break;

                //Console.WriteLine(new string(chars));

                chars[i]++;

            }
            if (br || end)
                return;
            chars[i] = 'a';
        }

        public static string NonAsyncBruteForce(string hash)
        {
            bool br = false;
            char[] res = { 'a', 'a', 'a', 'a', 'a' };
            CoreRecursFunc(0, hash, res, ref br);
            if (new string(res) == "aaaaa")
                return "No password has been found";
            return new string(res);
        }

        public static void CoreRecursFunc(int i, string hash, char[] chars, ref bool br)
        {
            while (true)
            {
                if (i != 4)
                    CoreRecursFunc(i + 1, hash, chars, ref br);
                if (!br && ComputeHash(chars) == hash)
                    br = true;
                if (chars[i] == 'z' || br)
                    break;
                chars[i]++;
            }
            if (br)
                return;
            chars[i] = 'a';
        }

        //public static void CoreRecursFunc(string hash, char[] chars, ref bool br)
        //{
        //    int lastchar = chars.Length - 1;
        //    int back = lastchar;
        //    while (true) //chars != new []{'z' , 'z', 'z' , 'z', 'z'}
        //    {

        //        while (chars[lastchar] != 122)
        //        {
        //            if (ComputeHash(chars) == hash)
        //            {
        //                br = true;
        //                break;
        //            }
        //            chars[lastchar]++;
        //        }
        //        if (br)
        //            return;
        //        while (chars[back] == 122)
        //        {
        //            chars[back] = 'a';
        //            back--;

        //        }
        //        chars[back]++;
        //        back = lastchar;
        //    }



        //}

        //public static void CoreRecursFunc(string hash, char[] chars, ref bool br)
        //{
        //    int lastchar = chars.Length - 1;
        //    int back = lastchar;
        //    while (true)
        //    {
        //        for (int k = 97; k < 123; k++)
        //        {
        //            chars[lastchar] = (char)k;
        //            if (ComputeHash(chars) == hash)
        //            {
        //                br = true;
        //                break;
        //            }
        //        }
        //        if (br)
        //            return;
        //        while (chars[back] == 122)
        //        {
        //            chars[back] = 'a';
        //            back--;
        //        }
        //        chars[back]++;
        //        back = lastchar;
        //    }
        //}

        public static string ComputeHash(char[] input)
        {
            sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.ASCII.GetBytes(input));
            StringBuilder builder = new StringBuilder();
            foreach (var t in bytes)
                builder.Append(t.ToString("x2"));
            return builder.ToString();
        }


    }
}
