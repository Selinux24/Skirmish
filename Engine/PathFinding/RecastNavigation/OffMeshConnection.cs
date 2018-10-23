using SharpDX;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Defines an navigation mesh off-mesh connection within a dtMeshTile object.
    /// An off-mesh connection is a user defined traversable connection made up to two vertices.
    /// </summary>
    [Serializable]
    public class OffMeshConnection : ISerializable
    {
        /// <summary>
        /// The start endpoint of the connection. [(ax, ay, az)]
        /// </summary>
        public Vector3 start { get; set; }
        /// <summary>
        /// The end endpoint of the connection. [(bx, by, bz)]
        /// </summary>
        public Vector3 end { get; set; }
        /// <summary>
        /// The radius of the endpoints. [Limit: >= 0]
        /// </summary>
        public float rad { get; set; }
        /// <summary>
        /// The polygon reference of the connection within the tile.
        /// </summary>
        public int poly { get; set; }
        /// <summary>
        /// Link flags. 
        /// </summary>
        /// <remarks>
        /// These are not the connection's user defined flags. Those are assigned via the connection's dtPoly definition. These are link flags used for internal purposes.
        /// </remarks>
        public int flags { get; set; }
        /// <summary>
        /// End point side.
        /// </summary>
        public int side { get; set; }
        /// <summary>
        /// The id of the offmesh connection. (User assigned when the navigation mesh is built.)
        /// </summary>
        public int userId { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public OffMeshConnection()
        {

        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected OffMeshConnection(SerializationInfo info, StreamingContext context)
        {
            start = info.GetVector3("start");
            end = info.GetVector3("end");
            rad = info.GetSingle("rad");
            poly = info.GetInt32("poly");
            flags = info.GetInt32("flags");
            side = info.GetInt32("side");
            userId = info.GetInt32("userId");
        }
        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddVector3("start", start);
            info.AddVector3("end", end);
            info.AddValue("rad", rad);
            info.AddValue("poly", poly);
            info.AddValue("flags", flags);
            info.AddValue("side", side);
            info.AddValue("userId", userId);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("start: {0}; end: {1}; rad: {2}; poly: {3}; flags: {4}; side: {5}; userId: {6};",
                start, end,
                rad, poly,
                flags, side, userId);
        }
    }
}
