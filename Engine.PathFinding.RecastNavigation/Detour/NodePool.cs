using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Node pool
    /// </summary>
    public class NodePool : IDisposable
    {
        /// <summary>
        /// A value that indicates the entity does not references to anything.
        /// </summary>
        const int DT_NULL_IDX = -1;
        /// <summary>
        /// Parent node bits
        /// </summary>
        const int DT_NODE_PARENT_BITS = 24;

        /// <summary>
        /// Hash size
        /// </summary>
        private readonly int hashSize;
        /// <summary>
        /// Node count
        /// </summary>
        private int nodeCount;
        /// <summary>
        /// Node list
        /// </summary>
        private Node[] nodes;
        /// <summary>
        /// First list
        /// </summary>
        private int[] first;
        /// <summary>
        /// Next list
        /// </summary>
        private int[] next;

        /// <summary>
        /// Gets the maximum number of nodes in the pool
        /// </summary>
        public int MaxNodes { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxNodes">Maximum nodes in pool</param>
        /// <param name="hashSize">Hash size</param>
        public NodePool(int maxNodes, int hashSize)
        {
            if (maxNodes > (1 << DT_NODE_PARENT_BITS) - 1)
            {
                throw new ArgumentException("Invalid maximum nodes value.", nameof(maxNodes));
            }

            MaxNodes = maxNodes;
            this.hashSize = hashSize;
            nodeCount = 0;
            nodes = new Node[maxNodes];
            first = Helper.CreateArray(hashSize, DT_NULL_IDX);
            next = Helper.CreateArray(maxNodes, DT_NULL_IDX);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~NodePool()
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
                nodes = null;
                first = null;
                next = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        /// <remarks>
        /// From Thomas Wang, https://gist.github.com/badboy/6267743
        /// </remarks>
        static int HashRef(int a)
        {
            a += ~(a << 15);
            a ^= a >> 10;
            a += a << 3;
            a ^= a >> 6;
            a += ~(a << 11);
            a ^= a >> 16;
            return a;
        }

        /// <summary>
        /// Clears the node pool
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < hashSize; i++)
            {
                first[i] = DT_NULL_IDX;
            }
            nodeCount = 0;
        }

        /// <summary>
        /// Gets the node by reference and state from the pool, and initialises a new node if not exists
        /// </summary>
        /// <param name="nodeRef">Node reference</param>
        /// <param name="state">State</param>
        public Node AllocateNode(int nodeRef, int state)
        {
            var node = FindNode(nodeRef, state);
            if (node != null)
            {
                return node;
            }

            if (nodeCount >= MaxNodes)
            {
                return null;
            }

            int i = nodeCount++;

            // Init node
            nodes[i] = new()
            {
                PIdx = 0,
                Cost = 0,
                Total = 0,
                Ref = nodeRef,
                State = state,
                Flags = 0
            };

            int bucket = HashRef(nodeRef) & (hashSize - 1);
            next[i] = first[bucket];
            first[bucket] = i;

            return nodes[i];
        }
        /// <summary>
        /// Finds the node by reference and state from the pool
        /// </summary>
        /// <param name="nodeRef">Node reference</param>
        /// <param name="state">State</param>
        public Node FindNode(int nodeRef, int state)
        {
            int bucket = HashRef(nodeRef) & (hashSize - 1);
            int i = first[bucket];
            while (i != DT_NULL_IDX)
            {
                if (nodes[i]?.Ref == nodeRef && nodes[i]?.State == state)
                {
                    return nodes[i];
                }
                i = next[i];
            }

            return null;
        }
        /// <summary>
        /// Finds all the nodes with the specified reference
        /// </summary>
        /// <param name="nodeRef">Node reference</param>
        /// <param name="maxNodes">Maximum number of results</param>
        /// <returns>Returns the node list and the found node count</returns>
        public (Node[] nodes, int n) FindNodes(int nodeRef, int maxNodes)
        {
            Node[] res = new Node[maxNodes];

            int n = 0;
            int bucket = HashRef(nodeRef) & (hashSize - 1);
            int i = first[bucket];
            while (i != DT_NULL_IDX)
            {
                if (nodes[i]?.Ref == nodeRef)
                {
                    if (n >= maxNodes)
                    {
                        break;
                    }
                    res[n++] = nodes[i];
                }
                i = next[i];
            }

            return (res, n);
        }

        /// <summary>
        /// Gets the node index in the pool
        /// </summary>
        /// <param name="node">Node</param>
        public int GetNodeIdx(Node node)
        {
            if (node == null) return 0;

            return Array.IndexOf(nodes, node) + 1;
        }
        /// <summary>
        /// Gets the node by index from the pool
        /// </summary>
        /// <param name="idx">Node index</param>
        public Node GetNodeAtIdx(int idx)
        {
            if (idx == 0) return null;

            return nodes[idx - 1];
        }

        /// <summary>
        ///  Returns true if the polygon reference is in the closed list. 
        /// </summary>
        /// <param name="r">The reference id of the polygon to check.</param>
        /// <param name="maxNodes">Maximum number of results</param>
        /// <returns>True if the polygon is in closed list.</returns>
        public bool IsInClosedList(int r, int maxNodes)
        {
            var (nList, n) = FindNodes(r, maxNodes);

            for (int i = 0; i < n; i++)
            {
                if (nList[i].IsClosed)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
