using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Tasks
{
    public class Buffer<T> where T : class
    {
        private readonly object _mutex = new object();

        private readonly Queue<T> _queue = new Queue<T>();

        public T Dequeue()
        {
            lock (_mutex)
            {
                if (_queue.Count == 0)
                {
                    return null;
                }

                var message = _queue.Dequeue();
                Monitor.Pulse(_mutex);
                return message;
            }
        }

        public void Enqueue(T task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            lock (_mutex)
            {
                _queue.Enqueue(task);
                Monitor.Pulse(_mutex);
            }
        }
    }
}