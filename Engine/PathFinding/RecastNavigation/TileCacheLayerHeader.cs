using SharpDX;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Tile cache header
    /// </summary>
    [Serializable]
    public struct TileCacheLayerHeader : ISerializable
    {
        /// <summary>
        /// Data magic
        /// </summary>
        public int Magic { get; set; }
        /// <summary>
        /// Data version
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// Tile x
        /// </summary>
        public int TX { get; set; }
        /// <summary>
        /// Tile y
        /// </summary>
        public int TY { get; set; }
        /// <summary>
        /// Tile layer
        /// </summary>
        public int TLayer { get; set; }
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BBox { get; set; }
        /// <summary>
        /// Height min range
        /// </summary>
        public int HMin { get; set; }
        /// <summary>
        /// Height max range
        /// </summary>
        public int HMax { get; set; }
        /// <summary>
        /// Width of the layer.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height of the layer.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Minx usable sub-region.
        /// </summary>
        public int MinX { get; set; }
        /// <summary>
        /// Maxx usable sub-region.
        /// </summary>
        public int MaxX { get; set; }
        /// <summary>
        /// Miny usable sub-region.
        /// </summary>
        public int MinY { get; set; }
        /// <summary>
        /// Maxy usable sub-region.
        /// </summary>
        public int MaxY { get; set; }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        internal TileCacheLayerHeader(SerializationInfo info, StreamingContext context)
        {
            Magic = info.GetInt32("magic");
            Version = info.GetInt32("version");
            TX = info.GetInt32("tx");
            TY = info.GetInt32("ty");
            TLayer = info.GetInt32("tlayer");
            BBox = info.GetBoundingBox("b");
            HMin = info.GetInt32("hmin");
            HMax = info.GetInt32("hmax");
            Width = info.GetInt32("width");
            Height = info.GetInt32("height");
            MinX = info.GetInt32("minx");
            MaxX = info.GetInt32("maxx");
            MinY = info.GetInt32("miny");
            MaxY = info.GetInt32("maxy");
        }
        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("magic", Magic);
            info.AddValue("version", Version);
            info.AddValue("tx", TX);
            info.AddValue("ty", TY);
            info.AddValue("tlayer", TLayer);
            info.AddBoundingBox("b", BBox);
            info.AddValue("hmin", HMin);
            info.AddValue("hmax", HMax);
            info.AddValue("width", Width);
            info.AddValue("height", Height);
            info.AddValue("minx", MinX);
            info.AddValue("maxx", MaxX);
            info.AddValue("miny", MinY);
            info.AddValue("maxy", MaxY);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (this.Magic == 0 && this.Version == 0)
            {
                return "Empty;";
            }

            if (this.Magic != DetourTileCache.DT_TILECACHE_MAGIC)
            {
                return "Invalid;";
            }

            return string.Format("tx {0:000}; ty {1:000}; tlayer {2:000};",
                this.TX, this.TY, this.TLayer);
        }
    }
}
