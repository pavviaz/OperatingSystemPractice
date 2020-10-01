using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pairs_5_6
{
    class Program
    {
        private const int producer = 3;
        private const int consumer = 2;
        public static List<int> queue = new List<int>(); //250ms
        public static Task[] PR = new Task[producer];
        public static Task[] CNS = new Task[consumer];

        public static CancellationTokenSource TokenSource2 = new CancellationTokenSource();
        public static CancellationToken Ct = TokenSource2.Token;

        public static CancellationTokenSource TokenSource3 = new CancellationTokenSource();
        public static CancellationToken Ct1 = TokenSource3.Token;

        public static bool Startup = true;
        public static bool Canceled;

        public static void CTupd_1()
        {
            TokenSource2 = new CancellationTokenSource();
            Ct = TokenSource2.Token;
        }
        public static async Task Main(string[] args)
        {

            FabricStart();
            ConsumerStart(); 
            Print();
            Exit();

            await Task.Run(() =>
            {
                while (true)
                {
                    if (Startup && queue.Count >= 80)
                        Startup = false;

                    if (!Startup)
                    {
                        if (queue.Count >= 100)
                        {
                            TokenSource2.Cancel();
                            Thread.Sleep(100);
                            CTupd_1();
                        }

                        if (queue.Count <= 80 && !Canceled)
                            FabricStart();

                        if (queue.Count == 0)
                        {
                            TokenSource2.Cancel();
                            TokenSource3.Cancel();
                            break;
                        }
                    }
                }

            });

            Console.ReadKey();

        }

        public static async Task Print()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    Console.WriteLine($"count = {queue.Count}");
                    if (queue.Count == 0)
                        break;
                }

            });
        }
        public static async Task Exit()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (Console.ReadKey().KeyChar == 'q')
                    {
                        TokenSource2.Cancel();
                        Canceled = true;
                    }
                        
                    if (queue.Count == 0)
                        break;
                }

            });
        }

        public static void FabricStart()
        {

            for (int t = 0; t < PR.Length; t++)
            {
                var capi = t;
                PR[capi] = Task.Run(() => FabricFunc(Ct));
            }
        }
        public static void ConsumerStart()
        {

            for (int t = 0; t < CNS.Length; t++)
            {
                var capi = t;
                CNS[capi] = Task.Run(() => ShopFunc(Ct1));
            }
        }

        public static void FabricFunc(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                queue.Add(new Random().Next(1, 100));
                Thread.Sleep(250);
            }


        }

        public static void ShopFunc(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                queue.RemoveAt(0);
                Thread.Sleep(250);
            }

        }
    }
}
