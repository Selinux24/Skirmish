using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Link all nodes together. Store indices in hash map.
    /// </summary>
    public class NodePool
    {
        private List<Node> nodes;
        private Dictionary<int, Node> nodeDict;
        private int maxNodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodePool"/> class.
        /// </summary>
        /// <param name="maxNodes">The maximum number of nodes that can be stored</param>
        /// <param name="hashSize">The maximum number of elements in the hash table</param>
        public NodePool(int maxNodes)
        {
            this.maxNodes = maxNodes;

            nodes = new List<Node>(maxNodes);
            nodeDict = new Dictionary<int, Node>();
        }

        /// <summary>
        /// Reset all the data.
        /// </summary>
        public void Clear()
        {
            nodes.Clear();
            nodeDict.Clear();
        }
        /// <summary>
        /// Try to find a node.
        /// </summary>
        /// <param name="id">Node's id</param>
        /// <returns>The node, if found. Null, if otherwise.</returns>
        public Node FindNode(int id)
        {
            Node node;
            if (nodeDict.TryGetValue(id, out node))
            {
                return node;
            }

            return null;
        }
        /// <summary>
        /// Try to find the node. If it doesn't exist, create a new node.
        /// </summary>
        /// <param name="id">Node's id</param>
        /// <returns>The node</returns>
        public Node GetNode(int id)
        {
            Node node;
            if (nodeDict.TryGetValue(id, out node))
            {
                return node;
            }

            if (nodes.Count >= maxNodes)
            {
                return null;
            }

            Node newNode = new Node();
            newNode.ParentIdx = 0;
            newNode.cost = 0;
            newNode.total = 0;
            newNode.Id = id;
            newNode.Flags = 0;

            nodes.Add(newNode);
            nodeDict.Add(id, newNode);

            return newNode;
        }
        /// <summary>
        /// Gets the id of the node.
        /// </summary>
        /// <param name="node">The node</param>
        /// <returns>The id</returns>
        public int GetNodeIdx(Node node)
        {
            if (node == null)
            {
                return 0;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == node)
                {
                    return i + 1;
                }
            }

            return 0;
        }
        /// <summary>
        /// Return a node at a certain index. If index is out-of-bounds, return null.
        /// </summary>
        /// <param name="idx">Node index</param>
        /// <returns></returns>
        public Node GetNodeAtIdx(int idx)
        {
            if (idx <= 0 || idx > nodes.Count)
            {
                return null;
            }

            return nodes[idx - 1];
        }
    }
}
