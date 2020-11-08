using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pairs_7_8
{

    public enum ProcessPriorities
    {
        LOW = 1,
        AVERAGE,
        MAX
    }

    class Descriptor
    {
        //protected internal static List<Descriptor> descriptors; //Есть мысль сюда добавлять новосозданные экземпляры Дескрипторов, и потом загружать в планировщике процессы отсюда

        internal Process _internalProcess;
        public int PID { get; }

        public ProcessPriorities Priority { get; private set; }

        internal int _changablePrior;

        public Descriptor(Process process, ProcessPriorities priority = ProcessPriorities.AVERAGE)
        {
            _internalProcess = process;
            //if (process.ProcessName == string.Empty)
            //    throw new Exception("NoSuchProcessException");
            //PID = _internalProcess.Id;
            Priority = priority;
            _changablePrior = (int) Priority;
            //descriptors.Add(this);
        }

        public Descriptor(int PID, ProcessPriorities priority = ProcessPriorities.AVERAGE)
        {
            _internalProcess = Process.GetProcessById(PID);
            //if (_internalProcess.ProcessName == string.Empty)
            //    throw new Exception("NoProcessWithSuchPIDException");
            //PID = _internalProcess.Id;
            Priority = priority;
            _changablePrior = (int) Priority;
            //descriptors.Add(this);
        }

        public void SetPriority(ProcessPriorities priority)
        {
            Priority = priority;
        }

    }

    class ProcessScheduler
    {
        private enum TimeSlice
        {
            LOW = 15,
            AVERAGE = 50,
            MAX = 80
        }

        private List<Descriptor> _readyProcesses;
        //private List<Descriptor> _awaitingProcesses;
        //private Dictionary<Descriptor, TimeSlice> _currentProcess;
        private (Descriptor, TimeSlice) _currentProcess;
        private int[] _priorityNumber;

        public ProcessScheduler(params Descriptor[] parameters)
        {
            _readyProcesses = new List<Descriptor>();
            foreach (var process in parameters)
                _readyProcesses.Add(process);
        }

        public void ReadyProcessesFlush()
        {
            _readyProcesses.Clear();
        }

        public void AddToReadyProcess(Descriptor process)
        {
            _readyProcesses.Add(process);
        }

        public void LaunchScheduler()
        {
            if (_readyProcesses.Count == 0)
                throw new Exception("NoProcessesCreated");
            //_priorityNumber = new int[_readyProcesses.Count];
            //for (int i = 0; i < _priorityNumber.Length; i++)
            //{
            //    switch (_readyProcesses[i].Priority)
            //    {
            //        case ProcessPriorities.LOW:
            //            _priorityNumber[i] = (int)ProcessPriorities.LOW;
            //            break;
            //        case ProcessPriorities.AVERAGE:
            //            _priorityNumber[i] = (int)ProcessPriorities.AVERAGE;
            //            break;
            //        case ProcessPriorities.MAX:
            //            _priorityNumber[i] = (int)ProcessPriorities.MAX;
            //            break;
            //    }
            //}
            foreach (var t in _readyProcesses)
            {
                t._internalProcess.Start();
                //t._internalProcess.PriorityClass = ProcessPriorityClass.High;
            }
                
            foreach (var t in _readyProcesses)
                ProcessExtension.Suspend(t._internalProcess);

            Execute();
        }
        private void Execute()
        {
            while (true)
            {
                Stopwatch s = new Stopwatch();
                s.Start();

                _currentProcess = Scheduler();
                ProcessExtension.Resume(_currentProcess.Item1._internalProcess);
                Thread.Sleep((int)_currentProcess.Item2);
                ProcessExtension.Suspend(_currentProcess.Item1._internalProcess);

                s.Stop();
                Console.WriteLine($"Elapsed time: {s.ElapsedMilliseconds}, name = {_currentProcess.Item1._internalProcess.ProcessName}, PID = {_currentProcess.Item1._internalProcess.Id}");
            }
            
        }

        private (Descriptor, TimeSlice) Scheduler()
        {
            #region INDEX_FINDER

            _priorityNumber = new int[_readyProcesses.Count];
            for (int t = 0; t < _readyProcesses.Count; t++)
                _priorityNumber[t] = _readyProcesses[t]._changablePrior;

            int rnd = RandNumber(1, _priorityNumber.Sum()), sum = 0, i;
            for (i = 0; i < _priorityNumber.Length; i++)
            {
                sum += _priorityNumber[i];
                if (sum - rnd >= 0)
                    break;
            }
            if (_readyProcesses[i]._changablePrior != 1)
                _readyProcesses[i]._changablePrior--;

            #endregion


            #region UPDATE

            for (int k = 0; k < _priorityNumber.Length; k++)
            {
                if (k == i)
                    continue;
                
                if(_readyProcesses[k].Priority == ProcessPriorities.MAX && _readyProcesses[k]._changablePrior < (int) ProcessPriorities.MAX)
                    _readyProcesses[k]._changablePrior++;

                if(_readyProcesses[k].Priority == ProcessPriorities.AVERAGE && _readyProcesses[k]._changablePrior < (int) ProcessPriorities.AVERAGE)
                    _readyProcesses[k]._changablePrior++;

            }

            #endregion


            switch (_readyProcesses[i].Priority)
            {
                case ProcessPriorities.LOW:
                    return (_readyProcesses[i], TimeSlice.LOW);
                case ProcessPriorities.AVERAGE:
                    return (_readyProcesses[i], TimeSlice.AVERAGE);
                case ProcessPriorities.MAX:
                    return (_readyProcesses[i], TimeSlice.MAX);
            }

            throw new Exception();
        }
        private static int RandNumber(int Low, int High)
        {
            return new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber)).Next(Low, High + 1);
        }

    }

    public static class ProcessExtension
    {

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        public static void Suspend(this Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    break;
                }
                SuspendThread(pOpenThread);
            }
        }
        public static void Resume(this Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    break;
                }
                ResumeThread(pOpenThread);
            }
        }
    }

    class Program
    {
        private static int RandNumber(int Low, int High) //кастомная функция рандома (родной класс Random почему-то очень плохо работает)
        {
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber));
            int rnd = rndNum.Next(Low, High);
            return rnd;
        }
        public static async Task Main(string[] args)
        {

            Process[] processes = new Process[4];
            for (int i = 0; i < processes.Length; i++)
                processes[i] = new Process();



            processes[0].StartInfo.FileName =
                "C:\\Users\\Xiaomi\\Documents\\GitHub\\OperatingSystemPractice\\Pairs_3_4\\Pairs_3_4\\bin\\Debug\\Pairs_3_4.exe";
            processes[1].StartInfo.FileName =
                "C:\\Users\\Xiaomi\\Documents\\GitHub\\OperatingSystemPractice\\Pairs_3_4\\Pairs_3_4\\bin\\Debug\\Pairs_3_4.exe";
            processes[2].StartInfo.FileName =
                "C:\\Users\\Xiaomi\\Documents\\GitHub\\OperatingSystemPractice\\Pairs_3_4\\Pairs_3_4\\bin\\Debug\\Pairs_3_4.exe";
            processes[3].StartInfo.FileName =
                "C:\\Users\\Xiaomi\\Documents\\GitHub\\OperatingSystemPractice\\Pairs_3_4\\Pairs_3_4\\bin\\Debug\\Pairs_3_4.exe";

            Descriptor one = new Descriptor(processes[0], ProcessPriorities.MAX);
            Descriptor two = new Descriptor(processes[1], ProcessPriorities.MAX);
            Descriptor thr = new Descriptor(processes[2], ProcessPriorities.MAX);
            Descriptor four = new Descriptor(processes[3], ProcessPriorities.LOW);

            ProcessScheduler PS = new ProcessScheduler(one, two, thr, four);
            PS.LaunchScheduler();



            int[] PriorityNumber = new[] { 3, 3, 3, 1 };
            char[] pr = new[] {'M', 'M', 'M', 'L'};


            while (true)
            {
                //Thread.Sleep(1000);


                int a = RandNumber(1, PriorityNumber.Sum() + 1);
                int sum = 0;
                int i;
                //Console.WriteLine();
                //foreach (var VARIABLE in PriorityNumber)
                //{
                //    Console.Write(VARIABLE + " ");
                //}
                //Console.WriteLine();
                for (i = 0; i < PriorityNumber.Length; i++)
                {
                    sum += PriorityNumber[i];
                    if (sum - a >= 0)
                        break;
                }
                //Console.WriteLine($"a = {a} , sum = {sum} , i = {i}");
                if (PriorityNumber[i] != 1)
                    PriorityNumber[i]--;

                //Console.WriteLine("---------------------\nBEFORE UPD: ");
                //foreach (var VARIABLE in PriorityNumber)
                //{
                //    Console.Write(VARIABLE + " ");
                //}
                //Console.WriteLine();
                for (int k = 0; k < PriorityNumber.Length; k++)
                {
                    if (k == i)
                        continue;
                    if (pr[k] == 'M')
                    {
                        if (PriorityNumber[k] < 3)
                            PriorityNumber[k]++;
                    }

                    if (pr[k] == 'A')
                    {
                        if (PriorityNumber[k] < 2)
                            PriorityNumber[k]++;
                    }

                }
                //Console.WriteLine("AFTER UPD: ");
                //foreach (var VARIABLE in PriorityNumber)
                //{
                //    Console.Write(VARIABLE + " ");
                //}
                //Console.WriteLine();


                //Console.ReadKey();

            }


            //foreach (var pr in processes)
            //    pr.Start();





            //await Task.Run(() =>
            //{
            //    while (true)
            //    {
            //        //Console.Clear();
            //        Console.WriteLine(processes[0].TotalProcessorTime);
            //        //if (Console.ReadKey().KeyChar == 'q')
            //        //{
            //        //    break;
            //        //}
            //    }

            //});

            Console.WriteLine(processes[0].Id);


            Thread.Sleep(5000);

            ProcessExtension.Suspend(processes[0]);

            Thread.Sleep(5000);

            ProcessExtension.Resume(processes[0]);



            //foreach (var pr in processes)
            //    pr.WaitForExit();

            Console.WriteLine("END");
            Console.ReadKey();
        }


    }
}
