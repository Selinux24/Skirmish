using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache contour set
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="nconts">Number of contours</param>
    public readonly struct TileCacheContourSet(int nconts)
    {
        /// <summary>
        /// Number of contours
        /// </summary>
        private readonly int nconts = nconts;
        /// <summary>
        /// Contour list
        /// </summary>
        private readonly TileCacheContour[] conts = new TileCacheContour[nconts];

        /// <summary>
        /// Gets the contour at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly TileCacheContour GetContour(int index)
        {
            return conts[index];
        }
        /// <summary>
        /// Sets the contour value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="contour">Contout</param>
        public readonly void SetContour(int index, TileCacheContour contour)
        {
            conts[index] = contour;
        }
        /// <summary>
        /// Iterates over the contour list
        /// </summary>
        public readonly IEnumerable<(int i, TileCacheContour c)> IterateContours()
        {
            for (int i = 0; i < conts.Length; i++)
            {
                yield return (i, conts[i]);
            }
        }

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

            for (int i = 0; i < nconts; ++i)
            {
                var nverts = conts[i].GetVertexCount();

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
