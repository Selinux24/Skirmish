using SharpDX;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Navigation mesh parameters
    /// </summary>
    [Serializable]
    public class NavMeshParams : ISerializable
    {
        /// <summary>
        /// Origin
        /// </summary>
        public Vector3 Origin;
        /// <summary>
        /// Tile width
        /// </summary>
        public float TileWidth;
        /// <summary>
        /// Tile height
        /// </summary>
        public float TileHeight;
        /// <summary>
        /// Maximum tiles
        /// </summary>
        public int MaxTiles;
        /// <summary>
        /// Maximum polygons
        /// </summary>
        public int MaxPolys;

        /// <summary>
        /// Constructor
        /// </summary>
        public NavMeshParams()
        {

        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization information</param>
        /// <param name="context">Serializatio context</param>
        protected NavMeshParams(SerializationInfo info, StreamingContext context)
        {
            Origin = info.GetVector3("Origin");
            TileWidth = info.GetSingle("TileWidth");
            TileHeight = info.GetSingle("TileHeight");
            MaxTiles = info.GetInt32("MaxTiles");
            MaxPolys = info.GetInt32("MaxPolys");
        }
        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddVector3("Origin", Origin);
            info.AddValue("TileWidth", TileWidth);
            info.AddValue("TileHeight", TileHeight);
            info.AddValue("MaxTiles", MaxTiles);
            info.AddValue("MaxPolys", MaxPolys);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Origin: {0}; TileWidth: {1}; TileHeight: {2}; MaxTiles: {3}; MaxPolys: {4};",
                Origin,
                TileWidth, TileHeight,
                MaxTiles, MaxPolys);
        }
    }
}
