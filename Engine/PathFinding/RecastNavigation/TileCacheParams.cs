using SharpDX;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Tile cache parameters
    /// </summary>
    [Serializable]
    public class TileCacheParams : ISerializable
    {
        /// <summary>
        /// Origin
        /// </summary>
        public Vector3 Origin;
        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize;
        /// <summary>
        /// Cell height
        /// </summary>
        public float CellHeight;
        /// <summary>
        /// Width
        /// </summary>
        public int Width;
        /// <summary>
        /// Height
        /// </summary>
        public int Height;
        /// <summary>
        /// Walkable height
        /// </summary>
        public float WalkableHeight;
        /// <summary>
        /// Walkable radius
        /// </summary>
        public float WalkableRadius;
        /// <summary>
        /// Walkable climb
        /// </summary>
        public float WalkableClimb;
        /// <summary>
        /// Maximum simplification error
        /// </summary>
        public float MaxSimplificationError;
        /// <summary>
        /// Maximum tiles
        /// </summary>
        public int MaxTiles;
        /// <summary>
        /// Maximum obstacles
        /// </summary>
        public int MaxObstacles;

        /// <summary>
        /// Constructor
        /// </summary>
        public TileCacheParams()
        {

        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization information</param>
        /// <param name="context">Serializatio context</param>
        protected TileCacheParams(SerializationInfo info, StreamingContext context)
        {
            Origin = info.GetVector3("Origin");
            CellSize = info.GetSingle("CellSize");
            CellHeight = info.GetSingle("CellHeight");
            Width = info.GetInt32("Width");
            Height = info.GetInt32("Height");
            WalkableHeight = info.GetSingle("WalkableHeight");
            WalkableRadius = info.GetSingle("WalkableRadius");
            WalkableClimb = info.GetSingle("WalkableClimb");
            MaxSimplificationError = info.GetSingle("MaxSimplificationError");
            MaxTiles = info.GetInt32("MaxTiles");
            MaxObstacles = info.GetInt32("MaxObstacles");
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
            info.AddValue("CellSize", CellSize);
            info.AddValue("CellHeight", CellHeight);
            info.AddValue("Width", Width);
            info.AddValue("Height", Height);
            info.AddValue("WalkableHeight", WalkableHeight);
            info.AddValue("WalkableRadius", WalkableRadius);
            info.AddValue("WalkableClimb", WalkableClimb);
            info.AddValue("MaxSimplificationError", MaxSimplificationError);
            info.AddValue("MaxTiles", MaxTiles);
            info.AddValue("MaxObstacles", MaxObstacles);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Origin: {0}; CellSize: {1}; CellHeight: {2}; Width: {3}; Height: {4}; WalkableHeight: {5}; WalkableRadius: {6}; WalkableClimb: {7}; MaxSimplificationError: {8}; MaxTiles: {9}; MaxObstacles: {10};",
                Origin,
                CellSize, CellHeight,
                Width, Height,
                WalkableHeight, WalkableRadius, WalkableClimb,
                MaxSimplificationError, MaxTiles, MaxObstacles);
        }
    }
}
