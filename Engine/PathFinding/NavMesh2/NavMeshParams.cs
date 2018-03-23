using SharpDX;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Navigation mesh parameters
    /// </summary>
    [Serializable]
    public class NavMeshParams : ISerializable
    {
        public Vector3 Origin;
        public float TileWidth;
        public float TileHeight;
        public int MaxTiles;
        public int MaxPolys;

        public NavMeshParams()
        {

        }

        protected NavMeshParams(SerializationInfo info, StreamingContext context)
        {
            Origin = info.GetVector3("Origin");
            TileWidth = info.GetSingle("TileWidth");
            TileHeight = info.GetSingle("TileHeight");
            MaxTiles = info.GetInt32("MaxTiles");
            MaxPolys = info.GetInt32("MaxPolys");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddVector3("Origin", Origin);
            info.AddValue("TileWidth", TileWidth);
            info.AddValue("TileHeight", TileHeight);
            info.AddValue("MaxTiles", MaxTiles);
            info.AddValue("MaxPolys", MaxPolys);
        }
    }
}
