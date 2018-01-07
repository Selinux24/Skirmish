
namespace Engine.PathFinding.NavMesh2
{
    public class LayerRegion
    {
        public const int MaxLayers = 63;
        public const int MaxNeighbors = 16;

        public byte[] layers;
        public byte[] neis;
        public ushort ymin;
        public ushort ymax;
        /// <summary>
        /// Layer ID
        /// </summary>
        public byte layerId;
        /// <summary>
        /// Layer count
        /// </summary>
        public byte nlayers;
        /// <summary>
        ///  Neighbour count
        /// </summary>
        public byte nneis;
        /// <summary>
        /// Flag indicating if the region is the base of merged regions.
        /// </summary>
        public bool isBase;
    }
}