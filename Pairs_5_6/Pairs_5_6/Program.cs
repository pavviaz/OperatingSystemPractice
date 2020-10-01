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

        public static CancellationTokenSource tokenSource2 = new CancellationTokenSource();
        public static CancellationToken ct = tokenSource2.Token;

        public static CancellationTokenSource tokenSource3 = new CancellationTokenSource();
        public static CancellationToken ct1 = tokenSource3.Token;

        public static void CTupd_1()
        {
            tokenSource2 = new CancellationTokenSource();
            ct = tokenSource2.Token;
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

                    if (queue.Count >= 100)
                    {
                        tokenSource2.Cancel();
                        Thread.Sleep(100);
                        CTupd_1();
                    }

                    if (queue.Count <= 80)
                        FabricStart();

                    if (queue.Count == 0)
                    {
                        tokenSource2.Cancel();
                        tokenSource3.Cancel();
                        break;
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
                        tokenSource2.Cancel();
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
                PR[capi] = Task.Run(() => FabricFunc(ct));
            }
        }
        public static void ConsumerStart()
        {

            for (int t = 0; t < CNS.Length; t++)
            {
                var capi = t;
                CNS[capi] = Task.Run(() => Magazin(ct1));
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

        public static void Magazin(CancellationToken cancellationToken)
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
