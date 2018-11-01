using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public struct TileCachePolyMesh
    {
        public int NVP { get; set; }
        /// <summary>
        /// Number of vertices.
        /// </summary>
        public int NVerts { get; set; }
        /// <summary>
        /// Number of polygons.
        /// </summary>
        public int NPolys { get; set; }
        /// <summary>
        /// Vertices of the mesh, 3 elements per vertex.
        /// </summary>
        public Int3[] Verts { get; set; }
        /// <summary>
        /// Polygons of the mesh, nvp*2 elements per polygon.
        /// </summary>
        public Polygoni[] Polys { get; set; }
        /// <summary>
        /// Per polygon flags.
        /// </summary>
        public SamplePolyFlagTypes[] Flags { get; set; }
        /// <summary>
        /// Area ID of polygons.
        /// </summary>
        public SamplePolyAreas[] Areas { get; set; }
    }
}
