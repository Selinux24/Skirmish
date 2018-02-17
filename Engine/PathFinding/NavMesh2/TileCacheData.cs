﻿
namespace Engine.PathFinding.NavMesh2
{
    public struct TileCacheData
    {
        public TileCacheLayerHeader Header;
        public byte[] Data;
        public int DataSize;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("{0}; Data: {1}", this.Header, this.Data?.Join(","));
        }
    }
}
