using SharpDX;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation.Detour
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
        public Int3 BMin { get; set; }
        /// <summary>
        /// Maximum bounds of the node's AABB. [(x, y, z)]
        /// </summary>
        public Int3 BMax { get; set; }
        /// <summary>
        /// The node's index. (Negative for escape sequence.)
        /// </summary>
        public int I { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BVNode()
        {

        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization information</param>
        /// <param name="context">Serializatio context</param>
        protected BVNode(SerializationInfo info, StreamingContext context)
        {
            BMin = info.GetInt3("bmin");
            BMax = info.GetInt3("bmax");
            I = info.GetInt32("i");
        }
        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddInt3("bmin", BMin);
            info.AddInt3("bmax", BMax);
            info.AddValue("i", I);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("bmin: {0}; bmax: {1}; i: {2};", BMin, BMax, I);
        }
    }
}
