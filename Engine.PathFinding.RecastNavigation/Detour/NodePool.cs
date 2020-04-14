using System;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Node pool
    /// </summary>
    public class NodePool : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        /// <remarks>
        /// From Thomas Wang, https://gist.github.com/badboy/6267743
        /// </remarks>
        private static int HashRef(int a)
        {
            a += ~(a << 15);
            a ^= (a >> 10);
            a += (a << 3);
            a ^= (a >> 6);
            a += ~(a << 11);
            a ^= (a >> 16);
            return a;
        }

        public int MaxNodes { get; set; }
        public int NodeCount { get; set; }

        public Node[] Nodes { get; set; }
        public int[] First { get; set; }
        public int[] Next { get; set; }
        public int HashSize { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxNodes">Maximum nodes in pool</param>
        /// <param name="hashSize">Hash size</param>
        public NodePool(int maxNodes, int hashSize)
        {
            MaxNodes = maxNodes;
            HashSize = hashSize;

            Nodes = new Node[MaxNodes];
            Next = Helper.CreateArray(MaxNodes, DetourUtils.DT_NULL_IDX);
            First = Helper.CreateArray(HashSize, DetourUtils.DT_NULL_IDX);
            NodeCount = 0;
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
                this.Nodes = null;
                this.Next = null;
                this.First = null;
            }
        }

        public void Clear()
        {
            First = Helper.CreateArray(HashSize, DetourUtils.DT_NULL_IDX);
            NodeCount = 0;
        }
        public Node GetNode(int id, int state)
        {
            int bucket = HashRef(id) & (HashSize - 1);
            int i = First[bucket];
            while (i != DetourUtils.DT_NULL_IDX)
            {
                if (Nodes[i] != null && Nodes[i].Id == id && Nodes[i].State == state)
                {
                    return Nodes[i];
                }
                i = Next[i];
            }

            if (NodeCount >= MaxNodes)
            {
                return null;
            }

            i = NodeCount;
            NodeCount++;

            // Init node
            Nodes[i] = new Node
            {
                PIdx = 0,
                Cost = 0,
                Total = 0,
                Id = id,
                State = state,
                Flags = 0
            };

            Next[i] = First[bucket];
            First[bucket] = i;

            return Nodes[i];
        }
        public Node FindNode(int id, int state)
        {
            int bucket = HashRef(id) & (HashSize - 1);
            int i = First[bucket];
            while (i != DetourUtils.DT_NULL_IDX)
            {
                if (Nodes[i].Id == id && Nodes[i].State == state)
                {
                    return Nodes[i];
                }
                i = Next[i];
            }
            return null;
        }
        public int FindNodes(int id, int maxNodes, out Node[] nodes)
        {
            nodes = new Node[maxNodes];

            int n = 0;
            int bucket = HashRef(id) & (HashSize - 1);
            int i = First[bucket];
            while (i != DetourUtils.DT_NULL_IDX)
            {
                if (Nodes[i].Id == id)
                {
                    if (n >= maxNodes)
                    {
                        return n;
                    }
                    nodes[n++] = Nodes[i];
                }
                i = Next[i];
            }

            return n;
        }
        public int GetNodeIdx(Node node)
        {
            if (node == null) return 0;

            return Array.IndexOf(Nodes, node) + 1;
        }
        public Node GetNodeAtIdx(int idx)
        {
            if (idx == 0) return null;
            return Nodes[idx - 1];
        }

        public int GetMemUsed()
        {
            return
                Marshal.SizeOf(this) +
                Marshal.SizeOf(typeof(Node)) * MaxNodes +
                Marshal.SizeOf(typeof(int)) * MaxNodes +
                Marshal.SizeOf(typeof(int)) * HashSize;
        }

        public int GetMaxNodes()
        {
            return MaxNodes;
        }
        public int GetHashSize()
        {
            return HashSize;
        }
        public int GetFirst(int bucket)
        {
            return First[bucket];
        }
        public int GetNext(int i)
        {
            return Next[i];
        }
        public int GetNodeCount()
        {
            return NodeCount;
        }
    }
}
