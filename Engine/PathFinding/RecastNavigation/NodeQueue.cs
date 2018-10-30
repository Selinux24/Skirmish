using System;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Node queue
    /// </summary>
    public class NodeQueue : IDisposable
    {
        private readonly int m_capacity;
        private int m_size;
        private Node[] m_heap;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="n">Node capacity</param>
        public NodeQueue(int n)
        {
            m_capacity = n;
            m_heap = new Node[m_capacity + 1];
            m_size = 0;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~NodeQueue()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_heap = null;
            }
        }

        public void Clear()
        {
            m_size = 0;
        }

        public Node Top()
        {
            return m_heap[0];
        }

        public Node Pop()
        {
            Node result = m_heap[0];
            m_size--;
            TrickleDown(0, m_heap[m_size]);
            return result;
        }

        public void Push(Node node)
        {
            m_size++;
            BubbleUp(m_size - 1, node);
        }

        public void Modify(Node node)
        {
            for (int i = 0; i < m_size; ++i)
            {
                if (m_heap[i] == node)
                {
                    BubbleUp(i, node);
                    return;
                }
            }
        }

        public bool Empty()
        {
            return m_size == 0;
        }

        public int GetMemUsed()
        {
            return Marshal.SizeOf(this) + Marshal.SizeOf(typeof(Node)) * (m_capacity + 1);
        }

        public int GetCapacity()
        {
            return m_capacity;
        }

        private void BubbleUp(int i, Node node)
        {
            int parent = (i - 1) / 2;
            // note: (index > 0) means there is a parent
            while ((i > 0) && (m_heap[parent].Total > node.Total))
            {
                m_heap[i] = m_heap[parent];
                i = parent;
                parent = (i - 1) / 2;
            }
            m_heap[i] = node;
        }

        private void TrickleDown(int i, Node node)
        {
            int child = (i * 2) + 1;
            while (child < m_size)
            {
                if (((child + 1) < m_size) &&
                    (m_heap[child].Total > m_heap[child + 1].Total))
                {
                    child++;
                }
                m_heap[i] = m_heap[child];
                i = child;
                child = (i * 2) + 1;
            }
            BubbleUp(i, node);
        }
    }
}
