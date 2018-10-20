﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Engine.Collections
{
    /// <summary>
    /// Use a priority queue (heap) to determine which node is more important.
    /// </summary>
    /// <typeparam name="T">
    /// A type that has a cost for each instance via the <see cref="IValueWithCost"/> interface.
    /// </typeparam>
    public sealed class PriorityQueue<T> : ICollection<T> where T : class, IValueWithCost
    {
        private readonly T[] heap;

        /// <summary>
        /// Gets the number of elements in the priority queue.
        /// </summary>
        public int Count { get; private set; }
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
        /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> class with a given capacity of size n.
        /// </summary>
        /// <param name="n">The maximum number of nodes that can be stored.</param>
        public PriorityQueue(int n)
        {
            Count = 0;
            heap = new T[n + 1];
        }

        /// <summary>
        /// Remove all the elements from the priority queue.
        /// </summary>
        public void Clear()
        {
            Count = 0;
        }
        /// <summary>
        /// Determines whether the priority queue is empty
        /// </summary>
        /// <returns>True if empty, false if not</returns>
        public bool Empty()
        {
            return Count == 0;
        }
        /// <summary>
        /// Return the node at the top of the heap.
        /// </summary>
        /// <returns>Top node in heap</returns>
        public T Top()
        {
            return (Count > 0) ? heap[0] : null;
        }
        /// <summary>
        /// Remove the node at the top of the heap. Then, move the bottommost node to the top and trickle down
        /// until the nodes are in order.
        /// </summary>
        /// <returns>Node with lowest value in heap</returns>
        public T Pop()
        {
            if (Count == 0)
                return null;

            T result = heap[0];
            Count--;
            TrickleDown(0, heap[Count]);
            return result;
        }
        /// <summary>
        /// Add the node at the bottom of the heap and move it up until the nodes ae in order.
        /// </summary>
        /// <param name="node">The node to add</param>
        public void Push(T node)
        {
            Count++;
            BubbleUp(Count - 1, node);
        }
        /// <summary>
        /// Returns whether the given item exists in the heap. 
        /// </summary>
        /// <param name="item">Item to look for</param>
        /// <returns>True or False</returns>
        public bool Contains(T item)
        {
            for (int c = 0; c < Count; c++)
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
            for (int i = 0; i < Count; i++)
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
        IEnumerator IEnumerable.GetEnumerator()
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

            while ((i > 0) && (heap[parent].TotalCost > node.TotalCost))
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

            while (child < Count)
            {
                //determine which child element has a smaller cost 
                if (((child + 1) < Count) && (heap[child].TotalCost > heap[child + 1].TotalCost))
                    child++;

                heap[i] = heap[child];
                i = child;
                child = (i * 2) + 1;
            }

            BubbleUp(i, node);
        }
    }
}
