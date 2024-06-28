using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Fixed capacity stack
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedStack<T>
    {
        /// <summary>
        /// Internal stack
        /// </summary>
        private readonly Stack<T> stack;
        /// <summary>
        /// Stack capacity
        /// </summary>
        private readonly int capacity;

        /// <summary>
        /// Gets the stack capacity
        /// </summary>
        public int Capacity
        {
            get { return capacity; }
        }
        /// <summary>
        /// Gets the stack count
        /// </summary>
        public int Count
        {
            get { return stack.Count; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Stack capacity</param>
        public FixedStack(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

            this.capacity = capacity;
            stack = new(capacity);
        }

        /// <summary>
        /// Clears the stack
        /// </summary>
        public void Clear()
        {
            stack.Clear();
        }
        /// <summary>
        /// Gets the top object on the stack without removing it.
        /// </summary>
        /// <remarks>If the stack is empty, Peek throws an InvalidOperationException.</remarks>
        public T Peek()
        {
            return stack.Peek();
        }
        /// <summary>
        /// Try to get the top object on the stack without removing it.
        /// </summary>
        /// <param name="result">Returns the top object of the stack</param>
        /// <returns>Returns true if there is an object at the top of the stack</returns>
        public bool TryPeek(out T result)
        {
            return stack.TryPeek(out result);
        }
        /// <summary>
        /// Pops an item from the top of the stack.
        /// </summary>
        /// <remarks>If the stack is empty, Peek throws an InvalidOperationException.</remarks>
        public T Pop()
        {
            return stack.Pop();
        }
        /// <summary>
        /// Try to pop an item from the top of the stack.
        /// </summary>
        /// <param name="result">Returns the top object of the stack</param>
        /// <returns>Returns true if there is an object at the top of the stack</returns>
        public bool TryPop(out T result)
        {
            return stack.TryPop(out result);
        }
        /// <summary>
        /// Inserts an object at the top of the stack.
        /// </summary>
        /// <param name="item">Object to push</param>
        /// <returns>Returns true if ther is enough capacity in the stack.</returns>
        public bool Push(T item)
        {
            if (stack.Count < capacity)
            {
                stack.Push(item);

                return true;
            }

            return false;
        }
        /// <summary>
        /// Copies the Stack to an array, in the same order Pop would return the items.
        /// </summary>
        public T[] ToArray()
        {
            return [.. stack.ToArray()];
        }
    }
}
