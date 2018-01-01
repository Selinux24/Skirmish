using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.PathFinding.NavMesh2
{
    public class CompressedTile
    {
        public TileCacheLayerHeader Header;
        /// <summary>
        /// Counter describing modifications to the tile.
        /// </summary>
        public int Salt;
        public char[] Compressed;
        public int CompressedSize;
        public char[] Data;
        public int DataSize;
        public uint Flags;
        public CompressedTile Next;
    }
}
