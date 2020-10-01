using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pairs_3_4
{
    internal class ParallelTaskScheduler : TaskScheduler
    {
        [ThreadStatic]
        private static bool currentThreadIsProcessingItems;
        private int maxDegreeOfParallelism;
        private volatile int runningOrQueuedCount;
        private readonly LinkedList<Task> tasks = new LinkedList<Task>();

        public ParallelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1)
                throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");

            this.maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public ParallelTaskScheduler() : this(Environment.ProcessorCount) { }

        public override int MaximumConcurrencyLevel
        {
            get { return maxDegreeOfParallelism; }
        }

        protected override bool TryDequeue(Task task)
        {
            lock (tasks) return tasks.Remove(task);
        }

        protected override bool TryExecuteTaskInline(Task task,
            bool taskWasPreviouslyQueued)
        {
            if (!currentThreadIsProcessingItems) return false;

            if (taskWasPreviouslyQueued) TryDequeue(task);

            return base.TryExecuteTask(task);
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(tasks, ref lockTaken);
                if (lockTaken) return tasks.ToArray();
                else throw new NotSupportedException();
            }
            finally { if (lockTaken) Monitor.Exit(tasks); }
        }

        protected override void QueueTask(Task task)
        {
            lock (tasks) tasks.AddLast(task);

            if (runningOrQueuedCount < maxDegreeOfParallelism)
            {
                runningOrQueuedCount++;
                RunTasks();
            }
        }
        private void RunTasks()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                List<Task> taskList = new List<Task>();

                currentThreadIsProcessingItems = true;
                try
                {
                    while (true)
                    {
                        lock (tasks)
                        {
                            if (tasks.Count == 0)
                            {
                                runningOrQueuedCount--;
                                break;
                            }

                            var t = tasks.First.Value;
                            taskList.Add(t);
                            tasks.RemoveFirst();
                        }
                    }

                    if (taskList.Count == 1)
                    {
                        base.TryExecuteTask(taskList[0]);
                    }
                    else if (taskList.Count > 0)
                    {
                        var batches = taskList.GroupBy(
                            task => taskList.IndexOf(task) / maxDegreeOfParallelism);

                        foreach (var batch in batches)
                        {
                            batch.AsParallel().ForAll(task =>
                                base.TryExecuteTask(task));
                        }
                    }
                }
                finally { currentThreadIsProcessingItems = false; }

            }, null);
        }

    }
}
