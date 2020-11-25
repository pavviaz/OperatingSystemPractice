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
        internal Process _internalProcess;
        public ProcessPriorities Priority { get; set; }

        internal int _changablePrior;

        internal int _Counter;

        internal bool _Stopped = false;


        public Descriptor(Process process, ProcessPriorities priority = ProcessPriorities.AVERAGE)
        {
            _internalProcess = process;
            Priority = priority;
            _changablePrior = (int)Priority;
        }

        public Descriptor(int PID, ProcessPriorities priority = ProcessPriorities.AVERAGE)
        {
            _internalProcess = Process.GetProcessById(PID);
            Priority = priority;
            _changablePrior = (int)Priority;
        }

        public void SetPriority(ProcessPriorities priority)
        {
            Priority = priority;
        }

    }

    class ProScheduler
    {

        private List<Descriptor> _readyProcesses;
        private List<Descriptor> _readyProcessesUI = new List<Descriptor>();
        private List<Descriptor> _awaitingProcesses = new List<Descriptor>();
        private Descriptor _currentProcess;

        private int[] _priorityNumber;
        private const int TimeSlice = 20; //на самом деле минимальное нормальное время такта ~30мс
        private int ptr;
        private bool _allTasksEnded;

        public ProScheduler(params Descriptor[] parameters)
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

            foreach (var t in _readyProcesses)
            {
                t._internalProcess.Start();
                //t._internalProcess.PriorityClass = ProcessPriorityClass.High;
                if (t._internalProcess.ProcessName == string.Empty)
                    throw new Exception("NoProcessFound");
                ProcessExtension.Suspend(t._internalProcess);
            }

            Console.CursorVisible = false;
            KeyListener();
            Execute();

        }
        private void Execute()
        {
            foreach (var descriptor in _readyProcesses)
                _readyProcessesUI.Add(descriptor);
            
            while (true)
            {
                Stopwatch s = new Stopwatch();
                s.Start();

                if (_readyProcesses.Count != 0)
                {
                    _currentProcess = Scheduler();

                    _currentProcess._internalProcess.Resume();
                    Thread.Sleep(TimeSlice);
                    _currentProcess._internalProcess.Suspend();
                }
                    

                //s.Stop();
                //Console.WriteLine($"Elapsed time: {s.ElapsedMilliseconds}, name = {_currentProcess._internalProcess.ProcessName}, PID = {_currentProcess._internalProcess.Id}, IP = {_currentProcess.Priority}, CP = {_currentProcess._changablePrior}");

                UIUpdate(s.ElapsedMilliseconds);

                foreach (var process in _readyProcesses.ToArray())
                {
                    if (process._internalProcess.HasExited)
                        _readyProcesses.Remove(process);
                }

                if (_readyProcesses.Count == 0 && _awaitingProcesses.Count == 0)
                {
                    _allTasksEnded = true;
                    ProcessExtension.InterruptConsole(); //симуляция нажатия клавиши, чтобы прервать ReadKey()
                    break;
                }

                s.Stop();
                Console.WriteLine("\n\nReal Elapsed Time= " + s.ElapsedMilliseconds);
                Console.WriteLine("\n\nControls: Backspace - change process ; \\ - increment priority ; Enter - decrement priority");

            }

        }

        private void UIUpdate(long ElapsedTIme)
        {

            Clear(1, 1, Console.BufferWidth, 25);
            //Console.Write("t = " + ptr);

            int left = 1, top = 1;
            string genstr;
            string EXEC = " , EXEC |";


            for (int i = 0; i < _readyProcessesUI.ToArray().Length; i++)
            {


                genstr = $"| NAME : {_readyProcessesUI[i]._internalProcess.ProcessName} , PID : {_readyProcessesUI[i]._internalProcess.Id} , Pr. : {_readyProcessesUI[i].Priority} , Ch.Pr. : {_readyProcessesUI[i]._changablePrior} , CallCount : {_readyProcessesUI[i]._Counter}";




                //$"\t| {process._internalProcess.Id} |" +
                //$"\t| {process} |" +
                //$"\t| {} |" +
                //$"\t| {} |" +
                //$"\t| {} |");
                if (left + genstr.Length + EXEC.Length >= Console.WindowWidth)
                {
                    left = 1;
                    top += 4;
                }
                Console.SetCursorPosition(left, top);
                if (ptr == i)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(genstr);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.Write(genstr);
                }

                if (_currentProcess == _readyProcessesUI[i])
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(EXEC);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(EXEC);
                    Console.ForegroundColor = ConsoleColor.White;
                }

                left = Console.CursorLeft + 4;


                if(_readyProcessesUI[i]._Stopped == true)
                    Console.Write(" STOPPED!!!");
                //Thread.Sleep(1000);

            }
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("\n\nElapsed Time: " + ElapsedTIme);
            Console.ForegroundColor = ConsoleColor.White;
            //Console.SetCursorPosition(0, 0);

        }

        private async Task KeyListener()
        {
            await Task.Run(() =>
            {
                while (!_allTasksEnded)
                {
                    switch (Console.ReadKey().KeyChar)
                    {
                        case (char)ConsoleKey.Backspace:
                            if (ptr < _readyProcessesUI.Count - 1)
                                ptr++;
                            else
                                ptr = 0;
                            break;
                        case '\\': //повысить приор
                            switch (_readyProcessesUI[ptr].Priority)
                            {
                                case ProcessPriorities.LOW:
                                    _readyProcessesUI[ptr].Priority = ProcessPriorities.AVERAGE;
                                    break;
                                case ProcessPriorities.AVERAGE:
                                    _readyProcessesUI[ptr].Priority = ProcessPriorities.MAX;
                                    break;
                            }
                            break;
                        case (char)ConsoleKey.Enter: //понизить приор
                            switch (_readyProcessesUI[ptr].Priority)
                            {
                                case ProcessPriorities.MAX:
                                    _readyProcessesUI[ptr].Priority = ProcessPriorities.AVERAGE;
                                    break;
                                case ProcessPriorities.AVERAGE:
                                    _readyProcessesUI[ptr].Priority = ProcessPriorities.LOW;
                                    break;
                            }
                            break;
                        case (char)ConsoleKey.D0:
                            _readyProcessesUI[ptr]._internalProcess.Kill();
                            ptr--;
                            break;
                        case ' ':
                            try
                            {
                                if (_readyProcessesUI[ptr]._Stopped == false)
                                {
                                    _readyProcessesUI[ptr]._Stopped = !_readyProcessesUI[ptr]._Stopped;
                                    var temp = _readyProcessesUI[ptr];
                                    _readyProcesses.Remove(temp);
                                    _awaitingProcesses.Add(temp);
                                    //ptr--;
                                }
                                else
                                {
                                    _readyProcessesUI[ptr]._Stopped = !_readyProcessesUI[ptr]._Stopped;
                                    Descriptor temp;
                                    foreach (var descriptor in _awaitingProcesses)
                                    {
                                        if (descriptor == _readyProcessesUI[ptr])
                                        {
                                            _readyProcesses.Add(descriptor);
                                            _awaitingProcesses.Remove(descriptor);
                                            break;
                                        }
                                    }
                                }
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

        private Descriptor Scheduler()
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

                if (_readyProcesses[k].Priority == ProcessPriorities.MAX && _readyProcesses[k]._changablePrior < (int)ProcessPriorities.MAX)
                    _readyProcesses[k]._changablePrior++;

                if (_readyProcesses[k].Priority == ProcessPriorities.AVERAGE && _readyProcesses[k]._changablePrior < (int)ProcessPriorities.AVERAGE)
                    _readyProcesses[k]._changablePrior++;

            }

            #endregion

            _readyProcesses[i]._Counter++;
            return _readyProcesses[i];

        }
        
        private int RandNumber(int Low, int High)
        {
            return new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber)).Next(Low, High + 1);
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
        const int VK_RETURN = 0x0D;
        const int WM_KEYDOWN = 0x100;

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

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

        public static void InterruptConsole()
        {
            var hWnd = Process.GetCurrentProcess().MainWindowHandle;
            PostMessage(hWnd, WM_KEYDOWN, VK_RETURN, 0);
        }
    }

    class Program
    {
        private static int RandNumber(int Low, int High)
        {
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber));
            int rnd = rndNum.Next(Low, High);
            return rnd;
        }
        public static void Main(string[] args)
        {
            
            Process[] processes = new Process[5];
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
            processes[4].StartInfo.FileName =
                "C:\\Users\\Xiaomi\\Documents\\GitHub\\OperatingSystemPractice\\Pairs_3_4\\Pairs_3_4\\bin\\Debug\\Pairs_3_4.exe";

            Descriptor one = new Descriptor(processes[0], ProcessPriorities.MAX);
            Descriptor two = new Descriptor(processes[1], ProcessPriorities.AVERAGE);
            Descriptor thr = new Descriptor(processes[2], ProcessPriorities.MAX);
            Descriptor four = new Descriptor(processes[3], ProcessPriorities.LOW);
            Descriptor fif = new Descriptor(processes[4], ProcessPriorities.LOW);

            ProScheduler PS = new ProScheduler(one, two, four);
            PS.LaunchScheduler();

            //Process[] processes = new Process[5];
            //for (int i = 0; i < processes.Length; i++)
            //    processes[i] = new Process();

            //processes[0].StartInfo.FileName =
            //    "C:\\WINDOWS\\system32\\mspaint.exe";
            //processes[1].StartInfo.FileName =
            //    "C:\\WINDOWS\\system32\\mspaint.exe";
            //processes[2].StartInfo.FileName =
            //    "C:\\WINDOWS\\system32\\mspaint.exe";
            //processes[3].StartInfo.FileName =
            //    "C:\\WINDOWS\\system32\\mspaint.exe";
            //processes[4].StartInfo.FileName =
            //    "C:\\WINDOWS\\system32\\mspaint.exe";

            //Descriptor one = new Descriptor(processes[0], ProcessPriorities.MAX);
            //Descriptor two = new Descriptor(processes[1], ProcessPriorities.AVERAGE);
            //Descriptor thr = new Descriptor(processes[2], ProcessPriorities.MAX);
            //Descriptor four = new Descriptor(processes[3], ProcessPriorities.LOW);
            //Descriptor fif = new Descriptor(processes[4], ProcessPriorities.MAX);

            //ProScheduler PS = new ProScheduler(one, fif, thr);
            //PS.LaunchScheduler();

            int a = 11;
            int b = 2;
            int c = a + b;

            Console.WriteLine("\n\nEND");
            Console.ReadKey();
        }


    }
}
