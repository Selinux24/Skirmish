using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache contour set
    /// </summary>
    public struct TileCacheContourSet
    {
        /// <summary>
        /// Number of contours
        /// </summary>
        public int NConts { get; set; }
        /// <summary>
        /// Contour list
        /// </summary>
        public TileCacheContour[] Conts { get; set; }

        /// <summary>
        /// Gets the geometry configuration of the contour set
        /// </summary>
        /// <param name="maxVertices">Maximum vertices</param>
        /// <param name="maxTris">Maximum triangles</param>
        /// <param name="maxVertsPerCont">Maximum vertices per contour</param>
        public readonly void GetGeometryConfiguration(out int maxVertices, out int maxTris, out int maxVertsPerCont)
        {
            maxVertices = 0;
            maxTris = 0;
            maxVertsPerCont = 0;

            for (int i = 0; i < NConts; ++i)
            {
                var nverts = Conts[i].NVertices;

                // Skip null contours.
                if (nverts < 3)
                {
                    continue;
                }

                maxVertices += nverts;
                maxTris += nverts - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, nverts);
            }
        }
    }
}
