///////////////////////////////////////////////////////////////////////////////////////////
///                                                                                     ///
///  CustomCollections.cs - Several collections customized for specific application     ///
///                 purposes, including thread safety                                   ///
///                                                                                     ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019                         ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10              ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:                        ///
///                 Software Modeling and Analysis, 2021                                ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                    ///
///                                                                                     ///
///////////////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   ProgramClassTypeCollection inherits C#'s SynchronizedKeyedCollection. It is implemented
 *   to provide a way to quickly retrieve elements by index and by key, and to define the key
 *   as the object's Name property. ProgramClassTypeCollection is used in this application to
 *   store the collection of classes and interfaces in the project, which require both efficient
 *   iteration and efficient lookup.
 *   
 *   SizeLimitedBlockingQueue inherits C#'s generic ConcurrentQueue. It requires a maximum
 *   queue capacity, and blocks threads on enqueue if the capacity is reached, and on dequeue
 *   if the queue is empty. A single enqueuing thread becomes unblocked when an item is dequeued,
 *   and a single dequeuing thread becomes unblocked when an item is enqueued.
 *   
 *   ConcurrentOrderedList provides a container for C#'s generic List, holding items of type
 *   ProgramType. It provides thread safety using C#'s lock mechanism. It also inserts items
 *   into the list in alphabetical order according to their Name property.
 * 
 *   Public Interface
 *   ----------------
 *   
 *   ProgramClassTypeCollection
 *   --------------------------
 *   ProgramClassTypeCollection programClassTypeCollection = new ProgramClassTypeCollection();
 *   programClassTypeCollection.NotifyNameChange((ProgramClassType) item, (string) newName);
 *   programClassTypeCollection.CopyTo((ProgramClassType[]) array, (int) index);
 *   programClassTypeCollection.Add((ProgramClassType) item);
 *   programClassTypeCollection.Insert((int) index, (ProgramClassType) item);
 *   programClassTypeCollection.RemoveAt((int) index);
 *   programClassTypeCollection.Clear();
 *   int count = programClassTypeCollection.Count;
 *   int index = programClassTypeCollection.IndexOf((ProgramClassType) item);
 *   bool contains = programClassTypeCollection.Contains((ProgramClassType) item);
 *   bool contains = programClassTypeCollection.Contains((string) key);
 *   bool remove = programClassTypeCollection.Remove((ProgramClassType) item);
 *   bool remove = programClassTypeCollection.Remove((string) key);
 *   IEnumerator<ProgramClassType> enumerator = programClassTypeCollection.GetEnumerator();
 *   object obj = programClassTypeCollection.SyncRoot;
 *   
 *   SizeLimitedBlockingQueue
 *   ------------------------
 *   SizeLimitedBlockingQueue<T> sizeLimitedBlockingQueue = new SizeLimitedBlockingQueue<T>((int) size);
 *   sizeLimitedBlockingQueue.CopyTo((T[]) array, (int) index);
 *   sizeLimitedBlockingQueue.Enqueue((T) item);
 *   int count = sizeLimitedBlockingQueue.Count;
 *   bool empty = sizeLimitedBlockingQueue.IsEmpty;
 *   bool peek = sizeLimitedBlockingQueue.TryPeek(out (T) result);
 *   bool dequeue = sizeLimitedBlockingQueue.TryDequeue(out (T) result);
 *   T item = sizeLimitedBlockingQueue.Dequeue();
 *   T[] array = sizeLimitedBlockingQueue.ToArray();
 *   IEnumerator<T> enumerator = sizeLimitedBlockingQueue.GetEnumerator();
 *   
 *   ConcurrentOrderedList
 *   ---------------------
 *   ConcurrentOrderedList concurrentOrderedList = new ConcurrentOrderedList();
 *   concurrentOrderedList.Add((ProgramType) item);
 *   concurrentOrderedList.RemoveAt((int) index);
 *   concurrentOrderedList.Clear();
 *   int count = concurrentOrderedList.Count;
 *   ProgramType item = concurrentOrderedList[(int) index];
 *   bool remove = concurrentOrderedList.Remove((ProgramType) item);
 */

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace CodeAnalyzer
{
    /* Thread-safe KeyedCollection for ProgramClassType - allows for quick retrieval of ProgramClassTypes by both index and key (name) */
    public class ProgramClassTypeCollection : SynchronizedKeyedCollection<string, ProgramClassType>
    {
        /* Changes the item's key in the collection if an existing item's name changes */
        internal void NotifyNameChange(ProgramClassType programClassType, string newName) =>
            ChangeItemKey(programClassType, newName);

        /* Returns an item's key */
        protected override string GetKeyForItem(ProgramClassType item) => item.Name;

        /* Inserts an item at the specified index */
        protected override void InsertItem(int index, ProgramClassType item)
        {
            base.InsertItem(index, item);
            item.ProgramClassTypes = this;
        }

        /* Sets the value of a specified index */
        protected override void SetItem(int index, ProgramClassType item)
        {
            base.SetItem(index, item);
            item.ProgramClassTypes = this;
        }

        /* Removes an item at the specified index */
        protected override void RemoveItem(int index)
        {
            base[index].ProgramClassTypes = null;
            base.RemoveItem(index);
        }

        /* Clears the collection */
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

        /* Enqueues an item; if queue is full, blocks until there is space for another item */
        public new void Enqueue(T item)
        {
            producer.WaitOne();
            base.Enqueue(item);
            consumer.Release();
        }

        /* Attempts to dequeue an item from the queue; returns false if queue is empty */
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

        /* Dequeues an item; if queue is empty, blocks until there is an item to dequeue */
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
                    }
                }
                return item;
            }
            private set
            {
                lock (_lock) list[index] = value;
            }
        }

        /* IComparer for ProgramType objects */
        internal class NameComparer : IComparer<ProgramType>
        {
            /* Compares ProgramType objects by their Name properties */
            public int Compare(ProgramType x, ProgramType y) => x.Name.CompareTo(y.Name);
        }

        /* Adds an item to the list */
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

        /* Inserts an item into the list at the specified index */
        private void Insert(int index, ProgramType item)
        {
            if (index >= list.Count)
                list.Add(item);
            else
                list.Insert(index, item);
        }

        /* Performs binary search on the list, used to obtain insertion index for a new item */
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

        /* Remove an item from the list */
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

        /* Remove the item at the specified index from the list */
        public void RemoveAt(int index)
        {
            lock (_lock) list.RemoveAt(index);
        }

        /* Clear the list */
        public void Clear()
        {
            lock (_lock) list.Clear();
        }
    }
}
