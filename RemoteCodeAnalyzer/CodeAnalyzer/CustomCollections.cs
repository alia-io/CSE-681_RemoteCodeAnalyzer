using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

namespace CodeAnalyzer
{
    /* Thread-safe KeyedCollection for ProgramClassType - allows for quick retrieval of ProgramClassTypes by both index and key (name) */
    public class ProgramClassTypeCollection : SynchronizedKeyedCollection<string, ProgramClassType>
    {
        internal void NotifyNameChange(ProgramClassType programClassType, string newName) =>
            ChangeItemKey(programClassType, newName);

        protected override string GetKeyForItem(ProgramClassType item) => item.Name;

        protected override void InsertItem(int index, ProgramClassType item)
        {
            base.InsertItem(index, item);
            item.ProgramClassTypes = this;
        }

        protected override void SetItem(int index, ProgramClassType item)
        {
            base.SetItem(index, item);
            item.ProgramClassTypes = this;
        }

        protected override void RemoveItem(int index)
        {
            base[index].ProgramClassTypes = null;
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            ProgramClassType[] copy = new ProgramClassType[Count];
            try
            {
                CopyTo(copy, 0);
                foreach (ProgramClassType programClassType in copy) programClassType.ProgramClassTypes = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to copy ProgramClassTypeCollection to array: {0}", e.ToString());
            }
            base.ClearItems();
        }
    }

    /* A thread-safe queue that is size limited and blocks until it can enqueue or dequeue */
    public class SizeLimitedBlockingQueue<T> : ConcurrentQueue<T>
    {
        private readonly Semaphore producer;
        private readonly Semaphore consumer;

        public SizeLimitedBlockingQueue(int capacity) : base()
        {
            producer = new Semaphore(capacity, capacity);
            consumer = new Semaphore(0, capacity);
        }

        public new void Enqueue(T item)
        {
            producer.WaitOne();
            base.Enqueue(item);
            consumer.Release();
        }

        private new bool TryDequeue(out T result)
        {
            consumer.WaitOne();
            if (base.TryDequeue(out result))
            {
                producer.Release();
                return true;
            }

            return false;
        }

        public T Dequeue()
        {
            if (TryDequeue(out T result)) return result;
            return default;
        }
    }

    /* A thread-safe list that is alphabetically ordered by Name */
    public class ConcurrentOrderedList
    {
        private readonly List<ProgramType> list = new List<ProgramType>(); 
        private readonly object _lock = new object();

        public int Count
        {
            get
            {
                int count;
                lock (_lock) count = list.Count;
                return count;
            }
        }

        public ProgramType this[int index]
        {
            get
            {
                ProgramType item = null;
                lock (_lock)
                {
                    try
                    {
                        item = list[index];
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Cannot access ConcurrentOrderedList index: {0}", e.ToString());
                        Console.WriteLine(e.StackTrace);
                    }
                }
                return item;
            }
            private set
            {
                lock (_lock) list[index] = value;
            }
        }

        internal class NameComparer : IComparer<ProgramType>
        {
            public int Compare(ProgramType x, ProgramType y) => x.Name.CompareTo(y.Name);
        }

        public bool Add(ProgramType item)
        {
            bool result = false;
            lock (_lock)
            {
                int index = BinarySearch(item, 0, Count, new NameComparer());

                if (index >= 0 && index <= Count)
                {
                    Insert(index, item);
                    result = true;
                }
            }
            return result;
        }

        private void Insert(int index, ProgramType item)
        {
            if (index >= list.Count)
                list.Add(item);
            else
                list.Insert(index, item);
        }

        private int BinarySearch(ProgramType item, int low, int high, NameComparer comparer)
        {
            if (low == Count) return Count;

            if (high >= low)
            {
                int middle = low + ((high - low) / 2);
                int compare = comparer.Compare(item, this[middle]);

                if (compare == 0) return -1;

                if (high == low)
                {
                    if (compare < 0) return middle;
                    else if (compare > 0) return middle + 1;
                }

                if (compare < 0) return BinarySearch(item, low, middle, comparer);
                
                if (compare > 0) return BinarySearch(item, middle + 1, high, comparer);
            }
            return -1;
        }

        public bool Remove(ProgramType item)
        {
            lock (_lock)
            {
                int index = list.IndexOf(item);
                if (index >= 0)
                {
                    list.RemoveAt(index);
                    return true;
                }

                return false;
            }
        }

        public void RemoveAt(int index)
        {
            lock (_lock) list.RemoveAt(index);
        }

        public void Clear()
        {
            lock (_lock) list.Clear();
        }
    }
}
