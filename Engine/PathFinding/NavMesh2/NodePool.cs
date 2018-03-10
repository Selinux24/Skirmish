using System;

namespace Engine.PathFinding.NavMesh2
{
    public class NodePool : IDisposable
    {
        public int m_maxNodes;
        public int m_nodeCount;

        public Node[] m_nodes;
        public int[] m_first;
        public int[] m_next;
        public int m_hashSize;

        public NodePool(int maxNodes, int hashSize)
        {
            m_maxNodes = maxNodes;
            m_hashSize = hashSize;

            m_nodes = new Node[m_maxNodes];
            m_next = new int[m_maxNodes];
            m_first = new int[m_hashSize];
            m_nodeCount = 0;
        }

        public void Dispose()
        {
            Helper.Dispose(m_nodes);
            Helper.Dispose(m_next);
            Helper.Dispose(m_first);
        }

        public void Clear()
        {
            Helper.Dispose(m_first);
            m_first = new int[m_hashSize];
            m_nodeCount = 0;
        }

        public int GetMaxNodes()
        {
            return m_maxNodes;
        }
    }
}
