using SharpDX;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Defines an navigation mesh off-mesh connection within a dtMeshTile object.
    /// An off-mesh connection is a user defined traversable connection made up to two vertices.
    /// </summary>
    [Serializable]
    public class OffMeshConnection : ISerializable
    {
        /// <summary>
        /// The endpoints of the connection. [(ax, ay, az, bx, by, bz)]
        /// </summary>
        public Vector3[] pos = new Vector3[2];
        /// <summary>
        /// The radius of the endpoints. [Limit: >= 0]
        /// </summary>
        public float rad;
        /// <summary>
        /// The polygon reference of the connection within the tile.
        /// </summary>
        public int poly;
        /// <summary>
        /// Link flags. 
        /// </summary>
        /// <remarks>
        /// These are not the connection's user defined flags. Those are assigned via the connection's dtPoly definition. These are link flags used for internal purposes.
        /// </remarks>
        public int flags;
        /// <summary>
        /// End point side.
        /// </summary>
        public int side;
        /// <summary>
        /// The id of the offmesh connection. (User assigned when the navigation mesh is built.)
        /// </summary>
        public int userId;

        public OffMeshConnection()
        {

        }

        protected OffMeshConnection(SerializationInfo info, StreamingContext context)
        {
            pos = new[]
            {
                info.GetVector3("pos0"),
                info.GetVector3("pos1"),
            };
            rad = info.GetSingle("rad");
            poly = info.GetInt32("poly");
            flags = info.GetInt32("flags");
            side = info.GetInt32("side");
            userId = info.GetInt32("userId");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddVector3("pos0", pos[0]);
            info.AddVector3("pos1", pos[1]);
            info.AddValue("rad", rad);
            info.AddValue("poly", poly);
            info.AddValue("flags", flags);
            info.AddValue("side", side);
            info.AddValue("userId", userId);
        }
    }
}
