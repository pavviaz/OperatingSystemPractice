using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pairs_3_4
{
    class AsyncRecursFunc
    {
        private int i = 0;
        private string hash;
        char[] start;
        char[] exit;
        private static SHA256 sha256Hash;

        public AsyncRecursFunc(string hash, char[] start, char[] exit)
        {
            this.hash = hash;
            this.start = start;
            this.exit = exit;
        }

        public void Func()
        {
            while (true)
            {
                if (i != start.Length - 1)
                {
                    i++;
                    Func();
                }
                if (!Main_EXP.End && ComputeHash(start) == hash)
                {
                    Console.WriteLine("thread id " + Thread.CurrentThread.Name + " found key");
                    Main_EXP.DecodeString = new string(start);
                    Main_EXP.End = true;
                }

                if (start == exit || Main_EXP.End)
                    return;
                if (start[i] == 'z')
                {
                    start[i] = 'a';
                    i--;
                    return;
                }
                start[i]++;

            }
        }
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
