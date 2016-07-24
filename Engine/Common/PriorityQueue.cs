using Engine.PathFinding;
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
    public class PriorityQueue<TValue, TPriority> : ICollection, IEnumerable<PriorityQueueItem<TValue, TPriority>>
    {
        private const Int32 DefaultCapacity = 16;

        private Int32 capacity;
        private PriorityQueueItem<TValue, TPriority>[] items;
        private Comparison<TPriority> compareFunc;

        /// <summary>
        /// Gets or sets queue capacity
        /// </summary>
        public int Capacity
        {
            get
            {
                return items.Length;
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
                    throw new ArgumentOutOfRangeException("newCapacity", "The new capacity is not enough for the elements currently in the queue");
                }

                this.capacity = newCap;
                if (items == null)
                {
                    items = new PriorityQueueItem<TValue, TPriority>[newCap];

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
        public PriorityQueue()
            : this(DefaultCapacity, Comparer<TPriority>.Default)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialCapacity">Initial capacity</param>
        public PriorityQueue(Int32 initialCapacity)
            : this(initialCapacity, Comparer<TPriority>.Default)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparer">An object that implements IComparer for elements of type TPriority</param>
        public PriorityQueue(IComparer<TPriority> comparer)
            : this(DefaultCapacity, comparer)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comparison">Comparison function</param>
        public PriorityQueue(Comparison<TPriority> comparison)
            : this(DefaultCapacity, comparison)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialCapacity">Initial capacity</param>
        /// <param name="comparer">An object that implements IComparer for elements of type TPriority</param>
        public PriorityQueue(int initialCapacity, IComparer<TPriority> comparer)
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
        public PriorityQueue(int initialCapacity, Comparison<TPriority> comparison)
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
            PriorityQueueItem<TValue, TPriority> newItem = new PriorityQueueItem<TValue, TPriority>(value, priority);

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
        public PriorityQueueItem<TValue, TPriority> Dequeue()
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

            throw new ApplicationException("The specified item is not in the queue.");
        }
        /// <summary>
        /// Gets the highest priority queue without removing it
        /// </summary>
        /// <returns>Returns the object to the front of the queue</returns>
        public PriorityQueueItem<TValue, TPriority> Peek()
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
            foreach (PriorityQueueItem<TValue, TPriority> qItem in items)
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
        public void CopyTo(PriorityQueueItem<TValue, TPriority>[] array, int arrayIndex)
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
        public PriorityQueueItem<TValue, TPriority>[] ToArray()
        {
            PriorityQueueItem<TValue, TPriority>[] newItems = new PriorityQueueItem<TValue, TPriority>[this.Count];

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
        private PriorityQueueItem<TValue, TPriority> RemoveAt(Int32 index)
        {
            PriorityQueueItem<TValue, TPriority> o = items[index];
            PriorityQueueItem<TValue, TPriority> tmp = items[this.Count - 1];

            items[--this.Count] = default(PriorityQueueItem<TValue, TPriority>);
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
            this.CopyTo((PriorityQueueItem<TValue, TPriority>[])array, index);
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
        public IEnumerator<PriorityQueueItem<TValue, TPriority>> GetEnumerator()
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

    /// <summary>
    /// Use a priority queue (heap) to determine which node is more important.
    /// </summary>
    /// <typeparam name="T">
    /// A type that has a cost for each instance via the <see cref="IValueWithCost"/> interface.
    /// </typeparam>
    public class PriorityQueue<T> : ICollection<T>
        where T : class, IValueWithCost
    {
        private T[] heap;
        private int capacity;
        private int size;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> class with a given capacity of size n.
        /// </summary>
        /// <param name="n">The maximum number of nodes that can be stored.</param>
        public PriorityQueue(int n)
        {
            capacity = n;
            size = 0;
            heap = new T[capacity + 1];
        }

        /// <summary>
        /// Gets the number of elements in the priority queue.
        /// </summary>
        public int Count
        {
            get
            {
                return size;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only. For <see cref="PriorityQueue{T}"/>, this is
        /// always <c>true</c>.
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Remove all the elements from the priority queue.
        /// </summary>
        public void Clear()
        {
            size = 0;
        }

        /// <summary>
        /// Determines whether the priority queue is empty
        /// </summary>
        /// <returns>True if empty, false if not</returns>
        public bool Empty()
        {
            return size == 0;
        }

        /// <summary>
        /// Return the node at the top of the heap.
        /// </summary>
        /// <returns>Top node in heap</returns>
        public T Top()
        {
            return (size > 0) ? heap[0] : null;
        }

        /// <summary>
        /// Remove the node at the top of the heap. Then, move the bottommost node to the top and trickle down
        /// until the nodes are in order.
        /// </summary>
        /// <returns>Node with lowest value in heap</returns>
        public T Pop()
        {
            if (size == 0)
                return null;

            T result = heap[0];
            size--;
            TrickleDown(0, heap[size]);
            return result;
        }

        /// <summary>
        /// Add the node at the bottom of the heap and move it up until the nodes ae in order.
        /// </summary>
        /// <param name="node">The node to add</param>
        public void Push(T node)
        {
            size++;
            BubbleUp(size - 1, node);
        }

        /// <summary>
        /// Returns whether the given item exists in the heap. 
        /// </summary>
        /// <param name="item">Item to look for</param>
        /// <returns>True or False</returns>
        public bool Contains(T item)
        {
            for (int c = 0; c < size; c++)
                if (heap[c] == item)
                    return true;

            return false;
        }

        /// <summary>
        /// Change the value of the node, which may involve some swapping of elements to maintain heap order.
        /// </summary>
        /// <param name="node">The node to modify</param>
        public void Modify(T node)
        {
            for (int i = 0; i < size; i++)
            {
                if (heap[i] == node)
                {
                    BubbleUp(i, node);
                    return;
                }
            }
        }

        /// <summary>
        /// Copies the contents of the <see cref="PriorityQueue{T}"/> to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index within the array to start copying to.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex + heap.Length > array.Length)
                throw new ArgumentException("Array not large enough to hold priority queue", "array");

            Array.Copy(heap, 0, array, arrayIndex, heap.Length);
        }

        /// <summary>
        /// Gets the <see cref="PriorityQueue"/>'s enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)heap).GetEnumerator();
        }

        /// <summary>
        /// Calls <see cref="Push"/>.
        /// </summary>
        /// <param name="item">The item to add.</param>
        void ICollection<T>.Add(T item)
        {
            Push(item);
        }

        /// <summary>
        /// Unsupported, but necessary to implement <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="item">An item.</param>
        /// <returns>Nothing. This method will always throw <see cref="InvalidOperationException"/>.</returns>
        /// <exception cref="InvalidOperationException">Will always be thrown. This is not a valid operation.</exception>
        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException("This priority queue implementation only allows elements to be popped off the top, not removed.");
        }

        /// <summary>
        /// The non-generic version of <see cref="GetEnumerator"/>.
        /// </summary>
        /// <returns>A non-generic enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// While going up a priority queue, keep swapping elements until the element reaches the top.
        /// </summary>
        /// <param name="i">Index of current node</param>
        /// <param name="node">The node itself</param>
        private void BubbleUp(int i, T node)
        {
            int parent = (i - 1) / 2;

            while ((i > 0) && (heap[parent].Cost > node.Cost))
            {
                heap[i] = heap[parent];
                i = parent;
                parent = (i - 1) / 2;
            }

            heap[i] = node;
        }

        /// <summary>
        /// While moving down the priority queue, keep swapping elements.
        /// </summary>
        /// <param name="i">Index of current node</param>
        /// <param name="node">The node itself</param>
        private void TrickleDown(int i, T node)
        {
            int child = (i * 2) + 1;

            while (child < size)
            {
                //determine which child element has a smaller cost 
                if (((child + 1) < size) && (heap[child].Cost > heap[child + 1].Cost))
                    child++;

                heap[i] = heap[child];
                i = child;
                child = (i * 2) + 1;
            }

            BubbleUp(i, node);
        }
    }
}
