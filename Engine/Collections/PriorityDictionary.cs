using System;
using System.Collections;
using System.Collections.Generic;

namespace Engine.Collections
{
    /// <summary>
    /// Priority queue
    /// </summary>
    /// <typeparam name="TValue">Object type</typeparam>
    /// <typeparam name="TPriority">Priority value type</typeparam>
    public class PriorityDictionary<TValue, TPriority> : ICollection, IEnumerable<PriorityDictionaryItem<TValue, TPriority>>
    {
        private const int DefaultCapacity = 16;

        private int capacity;
        private PriorityDictionaryItem<TValue, TPriority>[] items;
        private readonly Comparison<TPriority> compareFunc;

        /// <summary>
        /// Gets or sets queue capacity
        /// </summary>
        public int Capacity
        {
            get
            {
                return this.capacity;
            }
            set
            {
                int newCap = value;

                if (newCap < DefaultCapacity)
                {
                    newCap = DefaultCapacity;
                }

                if (newCap < this.Count)
                {
                    throw new ArgumentOutOfRangeException("value", "The new capacity is not enough for the elements currently in the queue");
                }

                this.capacity = newCap;

                if (items == null)
                {
                    items = new PriorityDictionaryItem<TValue, TPriority>[newCap];

                    return;
                }

                Array.Resize(ref items, newCap);
            }
        }
        /// <summary>
        /// Gets the number of elements currently in the queue.
        /// </summary>
        public int Count { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PriorityDictionary()
            : this(DefaultCapacity, Comparer<TPriority>.Default)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialCapacity">Initial capacity</param>
        public PriorityDictionary(int initialCapacity)
            : this(initialCapacity, Comparer<TPriority>.Default)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">An object that implements IComparer for elements of type TPriority</param>
        public PriorityDictionary(IComparer<TPriority> comparer)
            : this(DefaultCapacity, comparer)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparison">Comparison function</param>
        public PriorityDictionary(Comparison<TPriority> comparison)
            : this(DefaultCapacity, comparison)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialCapacity">Initial capacity</param>
        /// <param name="comparer">An object that implements IComparer for elements of type TPriority</param>
        public PriorityDictionary(int initialCapacity, IComparer<TPriority> comparer)
        {
            this.Count = 0;
            this.compareFunc = new Comparison<TPriority>(comparer.Compare);
            this.Capacity = initialCapacity;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialCapacity">Initial capacity</param>
        /// <param name="comparison">Comparison function</param>
        public PriorityDictionary(int initialCapacity, Comparison<TPriority> comparison)
        {
            this.Count = 0;
            this.compareFunc = comparison;
            this.Capacity = initialCapacity;
        }

        /// <summary>
        /// Adds an item to the queue with a priority value
        /// </summary>
        /// <param name="value">Value to store</param>
        /// <param name="priority">Priority</param>
        public void Enqueue(TValue value, TPriority priority)
        {
            if (this.Count == capacity)
            {
                //Increase capacity
                this.Capacity = (int)(Capacity * 1.5);
            }

            // Create the new item
            PriorityDictionaryItem<TValue, TPriority> newItem = new PriorityDictionaryItem<TValue, TPriority>(value, priority);

            int i = this.Count++;

            while ((i > 0) && (compareFunc(items[i / 2].Priority, newItem.Priority) > 0))
            {
                items[i] = items[i / 2];
                i /= 2;
            }

            items[i] = newItem;
        }
        /// <summary>
        /// Removes and returns the element with higher priority queue
        /// </summary>
        /// <returns>Returns the first element removed from the queue</returns>
        public PriorityDictionaryItem<TValue, TPriority> Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Empty queue");
            }

            return RemoveAt(0);
        }
        /// <summary>
        /// Removes the item with the specified value of the queue
        /// </summary>
        /// <param name="item">Item to remove</param>
        public void Remove(TValue item)
        {
            Remove(item, EqualityComparer<TValue>.Default);
        }
        /// <summary>
        /// Removes the item with the specified value of the queue
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <param name="comp">Object that implements IEqualityComparer for the type of item in the collection</param>
        public void Remove(TValue item, IEqualityComparer comparer)
        {
            for (int index = 0; index < this.Count; ++index)
            {
                if (comparer.Equals(item, items[index].Value))
                {
                    RemoveAt(index);
                    return;
                }
            }

            throw new InvalidOperationException("The specified item is not in the queue.");
        }
        /// <summary>
        /// Gets the highest priority queue without removing it
        /// </summary>
        /// <returns>Returns the object to the front of the queue</returns>
        public PriorityDictionaryItem<TValue, TPriority> Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Empty queue");
            }

            return items[0];
        }
        /// <summary>
        /// Removes all objects from the queue
        /// </summary>
        public void Clear()
        {
            this.Count = 0;

            this.TrimExcess();
        }
        /// <summary>
        /// Sets the capacity to the actual number of elements in the queue if the number is less than 90% of current capacity
        /// </summary>
        public void TrimExcess()
        {
            if (this.Count < (0.9 * this.capacity))
            {
                this.Capacity = this.Count;
            }
        }
        /// <summary>
        /// Gets whether the element is in the queue
        /// </summary>
        /// <param name="o">Object to find</param>
        /// <returns>Returns true if the element is in the queue. False in all other cases</returns>
        public bool Contains(TValue item)
        {
            foreach (PriorityDictionaryItem<TValue, TPriority> qItem in items)
            {
                if (qItem.Value.Equals(item))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Copies the elements in a queue, starting from the specified index
        /// </summary>
        /// <param name="array">Destination array</param>
        /// <param name="arrayIndex">Index from which to start copying the elements</param>
        public void CopyTo(PriorityDictionaryItem<TValue, TPriority>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0");
            if (array.Rank > 1)
                throw new ArgumentException("the array is multidimensional");
            if (arrayIndex >= array.Length)
                throw new ArgumentException("arrayIndex is greater or equal to the number of array elements");
            if (this.Count > (array.Length - arrayIndex))
                throw new ArgumentException("The number of items in the collection destination is greater than the available space from arrayIndex detonation to the end of array");

            Array.Copy(items, 0, array, arrayIndex, this.Count);
        }
        /// <summary>
        /// Copies the elements to an array 
        /// </summary>
        /// <returns>Returns an array with the list of items in the queue</returns>
        public PriorityDictionaryItem<TValue, TPriority>[] ToArray()
        {
            PriorityDictionaryItem<TValue, TPriority>[] newItems = new PriorityDictionaryItem<TValue, TPriority>[this.Count];

            Array.Copy(items, newItems, this.Count);

            return newItems;
        }
        /// <summary>
        /// Gets whether access to the ICollection is safe for multithreaded (Thread Safe)
        /// </summary>
        protected bool IsSynchronized
        {
            get { return false; }
        }
        /// <summary>
        /// Gets a value that can be used to synchronize access to the ICollection
        /// </summary>
        protected object SyncRoot
        {
            get { return items.SyncRoot; }
        }
        /// <summary>
        /// Removes a node at the specified position of the tail
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the node removed</returns>
        private PriorityDictionaryItem<TValue, TPriority> RemoveAt(int index)
        {
            PriorityDictionaryItem<TValue, TPriority> o = items[index];
            PriorityDictionaryItem<TValue, TPriority> tmp = items[this.Count - 1];

            items[--this.Count] = default;
            if (this.Count > 0)
            {
                int i = index;
                int j = i + 1;
                while (i < Count / 2)
                {
                    if ((j < Count - 1) && (compareFunc(items[j].Priority, items[j + 1].Priority) > 0))
                    {
                        j++;
                    }
                    if (compareFunc(items[j].Priority, tmp.Priority) >= 0)
                    {
                        break;
                    }
                    items[i] = items[j];
                    i = j;
                    j *= 2;
                }
                items[i] = tmp;
            }

            return o;
        }

        /// <summary>
        /// Copies the elements in a queue, starting from the specified index
        /// </summary>
        /// <param name="array">Array copy destination</param>
        /// <param name="arrayIndex">Index from which to start copying the elements</param>
        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo((PriorityDictionaryItem<TValue, TPriority>[])array, index);
        }
        /// <summary>
        /// Gets whether access to the ICollection is safe for multithreaded (Thread Safe)
        /// </summary>
        bool ICollection.IsSynchronized
        {
            get { return this.IsSynchronized; }
        }
        /// <summary>
        /// Gets a value that can be used to synchronize access to the ICollection
        /// </summary>
        object ICollection.SyncRoot
        {
            get { return this.SyncRoot; }
        }
        /// <summary>
        /// Returns an enumerator that iterates through a collection
        /// </summary>
        /// <returns>Returns an enumerator that iterates through a collection</returns>
        public IEnumerator<PriorityDictionaryItem<TValue, TPriority>> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
            {
                yield return items[i];
            }
        }
        /// <summary>
        /// Returns an enumerator that iterates through a collection
        /// </summary>
        /// <returns>Returns an enumerator that iterates through a collection</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
