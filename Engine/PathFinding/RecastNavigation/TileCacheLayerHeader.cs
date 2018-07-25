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
        public int magic;
        /// <summary>
        /// Data version
        /// </summary>
        public int version;
        /// <summary>
        /// Tile x
        /// </summary>
        public int tx;
        /// <summary>
        /// Tile y
        /// </summary>
        public int ty;
        /// <summary>
        /// Tile layer
        /// </summary>
        public int tlayer;
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox b;
        /// <summary>
        /// Height min range
        /// </summary>
        public int hmin;
        /// <summary>
        /// Height max range
        /// </summary>
        public int hmax;
        /// <summary>
        /// Width of the layer.
        /// </summary>
        public int width;
        /// <summary>
        /// Height of the layer.
        /// </summary>
        public int height;
        /// <summary>
        /// Minx usable sub-region.
        /// </summary>
        public int minx;
        /// <summary>
        /// Maxx usable sub-region.
        /// </summary>
        public int maxx;
        /// <summary>
        /// Miny usable sub-region.
        /// </summary>
        public int miny;
        /// <summary>
        /// Maxy usable sub-region.
        /// </summary>
        public int maxy;

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        internal TileCacheLayerHeader(SerializationInfo info, StreamingContext context)
        {
            magic = info.GetInt32("magic");
            version = info.GetInt32("version");
            tx = info.GetInt32("tx");
            ty = info.GetInt32("ty");
            tlayer = info.GetInt32("tlayer");
            b = info.GetBoundingBox("b");
            hmin = info.GetInt32("hmin");
            hmax = info.GetInt32("hmax");
            width = info.GetInt32("width");
            height = info.GetInt32("height");
            minx = info.GetInt32("minx");
            maxx = info.GetInt32("maxx");
            miny = info.GetInt32("miny");
            maxy = info.GetInt32("maxy");
        }
        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("magic", magic);
            info.AddValue("version", version);
            info.AddValue("tx", tx);
            info.AddValue("ty", ty);
            info.AddValue("tlayer", tlayer);
            info.AddBoundingBox("b", b);
            info.AddValue("hmin", hmin);
            info.AddValue("hmax", hmax);
            info.AddValue("width", width);
            info.AddValue("height", height);
            info.AddValue("minx", minx);
            info.AddValue("maxx", maxx);
            info.AddValue("miny", miny);
            info.AddValue("maxy", maxy);
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (this.magic == 0 && this.version == 0)
            {
                return "Empty;";
            }

            if (this.magic != DetourTileCache.DT_TILECACHE_MAGIC)
            {
                return "Invalid;";
            }

            return string.Format("tx {0:000}; ty {1:000}; tlayer {2:000};",
                this.tx, this.ty, this.tlayer);
        }
    }
}
