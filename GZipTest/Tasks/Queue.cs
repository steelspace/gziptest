using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Tasks
{
    public class Queue
    {
        private readonly List<Thread> _threads;

        public readonly Buffer<MyTask> CompressTasks = new Buffer<MyTask>();
        public readonly Buffer<MyTask> WritingTasks = new Buffer<MyTask>();

        private void CompressConsumer()
        {
            while (true)
            {
                var task = CompressTasks.Dequeue();

                if (task == null)
                {
                    break;
                }

                task.Action.Invoke();
            }
        }

        private void WriterConsumer()
        {
            while (true)
            {
                var task = WritingTasks.Dequeue();

                if (task == null)
                {
                    break;
                }

                task.Action.Invoke();
            }
        }

        public void StartThreads()
        {
            foreach (var thread in _threads)
            {
                thread.Start();
            }

            foreach (var thread in _threads)
            {
                thread.Join();
            }
        }

        public void ExecuteHandler(Action action, Action<Exception> exceptionHandler)
        {
            try
            {
                action.Invoke();
            }

            catch (Exception e)
            {
                if (!(e is ThreadAbortException))
                {
                    exceptionHandler?.Invoke(e);
                }

                foreach (var thread in _threads)
                { 
                    thread.Abort();
                }
            }
        }

        public Queue(Action<Exception> exceptionHandler)
        {
            // initialize queue with 1 writer consumer and multiple coppressing consumers
            _threads = new List<Thread> { new Thread(() => ExecuteHandler(WriterConsumer, exceptionHandler)) };

            for (var i = 0; i < Environment.ProcessorCount - 1; i++)
            {
                _threads.Add(new Thread(() => ExecuteHandler(CompressConsumer, exceptionHandler)));
            }
        }
    }
}