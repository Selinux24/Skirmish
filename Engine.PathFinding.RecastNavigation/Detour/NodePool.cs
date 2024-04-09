using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Node pool
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="maxNodes">Maximum nodes in pool</param>
    /// <param name="hashSize">Hash size</param>
    public class NodePool(int maxNodes, int hashSize)
    {
        /// <summary>
        /// A value that indicates the entity does not references to anything.
        /// </summary>
        const int DT_NULL_IDX = -1;

        /// <summary>
        /// Hash size
        /// </summary>
        private readonly int hashSize = hashSize;
        /// <summary>
        /// Node count
        /// </summary>
        private int nodeCount = 0;
        /// <summary>
        /// Node list
        /// </summary>
        private readonly Node[] nodes = new Node[maxNodes];
        /// <summary>
        /// First list
        /// </summary>
        private readonly int[] first = Helper.CreateArray(hashSize, DT_NULL_IDX);
        /// <summary>
        /// Next list
        /// </summary>
        private readonly int[] next = Helper.CreateArray(maxNodes, DT_NULL_IDX);

        /// <summary>
        /// Gets the maximum number of nodes in the pool
        /// </summary>
        public int MaxNodes { get; private set; } = maxNodes;

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
    }
}
