using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh2
{
    public class NodeQueue : IDisposable
    {
        private List<Node> m_heap;
        private int m_capacity;
        private int m_size;

        public NodeQueue(int n)
        {
            m_capacity = n;
            m_heap = new List<Node>(m_capacity + 1);
            m_size = 0;
        }

        public void Dispose()
        {
            Helper.Dispose(m_heap);
        }

        public void Clear()
        {
            m_size = 0;
        }

        public int GetCapacity()
        {
            return m_capacity;
        }
    }
}
