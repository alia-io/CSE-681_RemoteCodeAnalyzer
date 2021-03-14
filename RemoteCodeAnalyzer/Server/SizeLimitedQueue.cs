using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server
{
    public class SizeLimitedQueue<T>
    {
        private readonly ConcurrentQueue<T> queue;
        private readonly Semaphore producer;
        private readonly Semaphore consumer;

        public SizeLimitedQueue(int capacity)
        {
            queue = new ConcurrentQueue<T>();
            producer = new Semaphore(0, capacity);
            consumer = new Semaphore(capacity, capacity);
        }

        public void Enqueue(T item)
        {
            producer.WaitOne();
            queue.Enqueue(item);
            consumer.Release();
        }

        public T Dequeue()
        {
            consumer.WaitOne();
            if (queue.TryDequeue(out T item))
            {
                producer.Release();
                return item;
            }
            return default(T);
        }

        public int Count() => queue.Count;
    }
}
