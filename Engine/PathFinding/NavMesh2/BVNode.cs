using SharpDX;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Bounding volume node.
    /// </summary>
    [Serializable]
    public class BVNode : ISerializable
    {
        /// <summary>
        /// Minimum bounds of the node's AABB. [(x, y, z)]
        /// </summary>
        public Int3 bmin;
        /// <summary>
        /// Maximum bounds of the node's AABB. [(x, y, z)]
        /// </summary>
        public Int3 bmax;
        /// <summary>
        /// The node's index. (Negative for escape sequence.)
        /// </summary>
        public int i;

        public BVNode()
        {

        }

        protected BVNode(SerializationInfo info, StreamingContext context)
        {
            bmin = info.GetInt3("bmin");
            bmax = info.GetInt3("bmax");
            i = info.GetInt32("i");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddInt3("bmin", bmin);
            info.AddInt3("bmax", bmax);
            info.AddValue("i", i);
        }
    };
}
