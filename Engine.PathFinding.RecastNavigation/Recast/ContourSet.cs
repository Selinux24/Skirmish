using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Represents a group of related contours.
    /// </summary>
    class ContourSet
    {
        /// <summary>
        /// Applied to the region id field of contour vertices in order to extract the region id.
        /// The region id field of a vertex may have several flags applied to it.  So the
        /// fields value can't be used directly.
        /// </summary>
        const int RC_CONTOUR_REG_MASK = 0xffff;
        /// <summary>
        /// Area border flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// the border of an area.
        /// (Used during the region and contour build process.)
        /// </summary>
        public const int RC_AREA_BORDER = 0x20000;
        /// <summary>
        /// Border vertex flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// a tile border. If a contour vertex's region ID has this bit set, the 
        /// vertex will later be removed in order to match the segments and vertices 
        /// at tile boundaries.
        /// (Used during the build process.)
        /// </summary>
        public const int RC_BORDER_VERTEX = 0x10000;

        /// <summary>
        /// An array of the contours in the set. [Size: #nconts]
        /// </summary>
        public Contour[] Conts { get; set; }
        /// <summary>
        /// The number of contours in the set.
        /// </summary>
        public int NConts { get; set; }
        /// <summary>
        /// The bounds in world space.
        /// </summary>
        public BoundingBox Bounds { get; set; }
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float CellSize { get; set; }
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float CellHeight { get; set; }
        /// <summary>
        /// The width of the set. (Along the x-axis in cell units.) 
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height of the set. (Along the z-axis in cell units.) 
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// The AABB border size used to generate the source data from which the contours were derived.
        /// </summary>
        public int BorderSize { get; set; }
        /// <summary>
        /// The max edge error that this contour set was simplified with.
        /// </summary>
        public float MaxError { get; set; }

        /// <summary>
        /// Builds a new contour set
        /// </summary>
        /// <param name="chf">Compact heightfield</param>
        /// <param name="maxError">Maximum error value</param>
        /// <param name="maxEdgeLen">Maximum edge length</param>
        /// <param name="buildFlags">Build flags</param>
        /// <returns>Returns the new contour</returns>
        public static ContourSet Build(CompactHeightfield chf, float maxError, int maxEdgeLen, BuildContoursFlagTypes buildFlags)
        {
            int w = chf.Width;
            int h = chf.Height;
            int borderSize = chf.BorderSize;
            float cellSize = chf.CellSize;
            float cellHeight = chf.CellHeight;
            int maxRegions = chf.MaxRegions;
            int maxContours = Math.Max(maxRegions, 8);
            var bounds = chf.GetBoundsWithBorder();

            var cset = new ContourSet
            {
                Bounds = bounds,
                CellSize = cellSize,
                CellHeight = cellHeight,
                Width = w - borderSize * 2,
                Height = h - borderSize * 2,
                BorderSize = borderSize,
                MaxError = maxError,
                Conts = new Contour[maxContours],
                NConts = 0
            };

            int[] flags = chf.InitializeFlags();

            List<(int Reg, AreaTypes Area, ContourVertex[] RawVerts)> cells = new();
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    cells.AddRange(chf.BuildCompactCells(x, y, flags));
                }
            }

            foreach (var (Reg, Area, RawVerts) in cells)
            {
                var cont = SimplifyContour(RawVerts, maxError, maxEdgeLen, buildFlags);
                if (cont.Length < 3)
                {
                    continue;
                }

                // Store region->contour remap info.
                cset.AddContour(Reg, Area, RawVerts, cont, maxContours, borderSize);
            }

            // Merge holes if needed.
            cset.MergeHoles(maxRegions + 1);

            return cset;
        }
        /// <summary>
        /// Simplifies the contour
        /// </summary>
        /// <param name="points">Contour points</param>
        /// <param name="maxError">Max error</param>
        /// <param name="maxEdgeLen">Max edge length</param>
        /// <param name="buildFlags">Build flags</param>
        /// <returns>Returns the simplified contour</returns>
        public static ContourVertex[] SimplifyContour(ContourVertex[] points, float maxError, int maxEdgeLen, BuildContoursFlagTypes buildFlags)
        {
            // Add initial points.
            var simplified = Initialize(points);

            // Add points until all raw points are within
            // error tolerance to the simplified shape.
            simplified = AddPoints(points, simplified, maxError);

            // Split too long edges.
            simplified = SplitLongEdges(points, simplified, maxEdgeLen, buildFlags);

            simplified = UpdateNeighbors(points, simplified);

            simplified = RemoveDegenerateSegments(simplified);

            return simplified.ToArray();
        }
        /// <summary>
        /// Add initial points.
        /// </summary>
        private static ContourVertex[] Initialize(ContourVertex[] points)
        {
            var simplified = new List<ContourVertex>();

            bool hasConnections = PointsHasConnections(points);
            if (hasConnections)
            {
                // The contour has some portals to other regions.
                // Add a new point to every location where the region changes.
                var changePoints = GetChangePoints(points);
                simplified.AddRange(changePoints);
            }

            if (simplified.Count == 0)
            {
                // If there is no connections at all,
                // create some initial points for the simplification process.
                // Find lower-left and upper-right vertices of the contour.
                var initialPoints = CreateInitialPoints(points);
                simplified.AddRange(initialPoints);
            }

            return simplified.ToArray();
        }
        /// <summary>
        /// Gets whether at least, one of the point of the list has connections
        /// </summary>
        /// <param name="points">Point list</param>
        private static bool PointsHasConnections(ContourVertex[] points)
        {
            bool hasConnections = false;
            for (int i = 0; i < points.Length; i++)
            {
                if ((points[i].Flag & RC_CONTOUR_REG_MASK) != 0)
                {
                    hasConnections = true;
                    break;
                }
            }
            return hasConnections;
        }
        /// <summary>
        /// Add a new point to every location where the region changes.
        /// </summary>
        private static ContourVertex[] GetChangePoints(ContourVertex[] points)
        {
            var changes = new List<ContourVertex>();

            for (int i = 0, ni = points.Length; i < ni; ++i)
            {
                int ii = (i + 1) % ni;
                var pi = points[i];
                var pii = points[ii];

                bool differentRegs = (pi.Flag & RC_CONTOUR_REG_MASK) != (pii.Flag & RC_CONTOUR_REG_MASK);
                bool areaBorders = (pi.Flag & RC_AREA_BORDER) != (pii.Flag & RC_AREA_BORDER);
                if (differentRegs || areaBorders)
                {
                    changes.Add(new(pi.X, pi.Y, pi.Z, i));
                }
            }

            return changes.ToArray();
        }
        /// <summary>
        /// Find lower-left and upper-right vertices of the contour.
        /// </summary>
        private static ContourVertex[] CreateInitialPoints(ContourVertex[] points)
        {
            var initialPoints = new List<ContourVertex>();

            int llx = points[0].X;
            int lly = points[0].Y;
            int llz = points[0].Z;
            int lli = 0;
            int urx = points[0].X;
            int ury = points[0].Y;
            int urz = points[0].Z;
            int uri = 0;
            for (int i = 0; i < points.Length; i++)
            {
                int x = points[i].X;
                int y = points[i].Y;
                int z = points[i].Z;
                if (x < llx || (x == llx && z < llz))
                {
                    llx = x;
                    lly = y;
                    llz = z;
                    lli = i;
                }
                if (x > urx || (x == urx && z > urz))
                {
                    urx = x;
                    ury = y;
                    urz = z;
                    uri = i;
                }
            }
            initialPoints.Add(new(llx, lly, llz, lli));
            initialPoints.Add(new(urx, ury, urz, uri));

            return initialPoints.ToArray();
        }
        /// <summary>
        /// Adds the point list to de point array
        /// </summary>
        /// <param name="points">Point list to add</param>
        /// <param name="list">Point list</param>
        /// <param name="maxError">Max error</param>
        /// <returns>Returns the updated list</returns>
        private static ContourVertex[] AddPoints(ContourVertex[] points, ContourVertex[] list, float maxError)
        {
            var simplified = new List<ContourVertex>(list);

            int pn = points.Length;
            for (int i = 0; i < simplified.Count;)
            {
                int ii = (i + 1) % simplified.Count;

                int ax = simplified[i].X;
                int az = simplified[i].Z;
                int ai = simplified[i].Flag;

                int bx = simplified[ii].X;
                int bz = simplified[ii].Z;
                int bi = simplified[ii].Flag;

                // Find maximum deviation from the segment.
                float maxd = 0;
                int maxi = -1;
                int ci, cinc, endi;

                // Traverse the segment in lexilogical order so that the
                // max deviation is calculated similarly when traversing
                // opposite segments.
                if (bx > ax || (bx == ax && bz > az))
                {
                    cinc = 1;
                    ci = (ai + cinc) % pn;
                    endi = bi;
                }
                else
                {
                    cinc = pn - 1;
                    ci = (bi + cinc) % pn;
                    endi = ai;
                    Helper.Swap(ref ax, ref bx);
                    Helper.Swap(ref az, ref bz);
                }

                // Tessellate only outer edges or edges between areas.
                if ((points[ci].Flag & RC_CONTOUR_REG_MASK) == 0 ||
                    (points[ci].Flag & RC_AREA_BORDER) != 0)
                {
                    while (ci != endi)
                    {
                        float d = Utils.DistancePtSegSqr2D(points[ci].X, points[ci].Z, ax, az, bx, bz);
                        if (d > maxd)
                        {
                            maxd = d;
                            maxi = ci;
                        }
                        ci = (ci + cinc) % pn;
                    }
                }

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > (maxError * maxError))
                {
                    // Add the point.
                    simplified.Insert(i + 1, new(points[maxi].X, points[maxi].Y, points[maxi].Z, maxi));
                }
                else
                {
                    ++i;
                }
            }

            return simplified.ToArray();
        }
        /// <summary>
        /// Split long edgest
        /// </summary>
        /// <param name="points">Point list to add</param>
        /// <param name="list">Point list</param>
        /// <param name="maxEdgeLen">Max edge length</param>
        /// <param name="buildFlags">Build flags</param>
        /// <returns>Returns the updated list</returns>
        private static ContourVertex[] SplitLongEdges(ContourVertex[] points, ContourVertex[] list, int maxEdgeLen, BuildContoursFlagTypes buildFlags)
        {
            bool tesselate = maxEdgeLen > 0 && (buildFlags & (BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES | BuildContoursFlagTypes.RC_CONTOUR_TESS_AREA_EDGES)) != 0;
            if (!tesselate)
            {
                return list.ToArray();
            }

            var simplified = new List<ContourVertex>(list);

            int pn = points.Length;
            for (int i = 0; i < simplified.Count;)
            {
                int ii = (i + 1) % simplified.Count;

                int ax = simplified[i].X;
                int az = simplified[i].Z;
                int ai = simplified[i].Flag;

                int bx = simplified[ii].X;
                int bz = simplified[ii].Z;
                int bi = simplified[ii].Flag;

                // Find maximum deviation from the segment.
                int maxi = -1;
                int ci = (ai + 1) % pn;

                // Tessellate only outer edges or edges between areas.
                bool tess = false;

                // Wall edges.
                if ((buildFlags & BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES) != 0 &&
                    (points[ci].Flag & RC_CONTOUR_REG_MASK) == 0)
                {
                    tess = true;
                }

                // Edges between areas.
                if ((buildFlags & BuildContoursFlagTypes.RC_CONTOUR_TESS_AREA_EDGES) != 0 &&
                    (points[ci].Flag & RC_AREA_BORDER) != 0)
                {
                    tess = true;
                }

                if (tess)
                {
                    int dx = bx - ax;
                    int dz = bz - az;
                    if (dx * dx + dz * dz > maxEdgeLen * maxEdgeLen)
                    {
                        // Round based on the segments in lexilogical order so that the
                        // max tesselation is consistent regardles in which direction
                        // segments are traversed.
                        int n = bi < ai ? (bi + pn - ai) : (bi - ai);
                        if (n > 1)
                        {
                            if (bx > ax || (bx == ax && bz > az))
                            {
                                maxi = (ai + n / 2) % pn;
                            }
                            else
                            {
                                maxi = (ai + (n + 1) / 2) % pn;
                            }
                        }
                    }
                }

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1)
                {
                    // Add the point.
                    simplified.Insert(i + 1, new(points[maxi].X, points[maxi].Y, points[maxi].Z, maxi));
                }
                else
                {
                    ++i;
                }
            }

            return simplified.ToArray();
        }
        /// <summary>
        /// Update neighbors
        /// </summary>
        /// <param name="points">Point list to add</param>
        /// <param name="list">Point list</param>
        /// <returns>Returns the updated list</returns>
        private static ContourVertex[] UpdateNeighbors(ContourVertex[] points, ContourVertex[] list)
        {
            var simplified = new List<ContourVertex>(list);

            int pn = points.Length;
            for (int i = 0; i < simplified.Count; ++i)
            {
                // The edge vertex flag is take from the current raw point,
                // and the neighbour region is take from the next raw point.
                var sv = simplified[i];
                int ai = (sv.Flag + 1) % pn;
                int bi = sv.Flag;
                sv.Flag = (points[ai].Flag & (RC_CONTOUR_REG_MASK | RC_AREA_BORDER)) | (points[bi].Flag & RC_BORDER_VERTEX);
                simplified[i] = sv;
            }

            return simplified.ToArray();
        }
        /// <summary>
        /// Removes degenerate segments
        /// </summary>
        /// <param name="list">Point list</param>
        /// <returns>Returns the updated list</returns>
        private static ContourVertex[] RemoveDegenerateSegments(ContourVertex[] list)
        {
            var simplified = new List<ContourVertex>(list);

            // Remove adjacent vertices which are equal on xz-plane,
            // or else the triangulator will get confused.
            int npts = simplified.Count;
            for (int i = 0; i < npts; ++i)
            {
                int ni = Utils.Next(i, npts);

                if (!Utils.VEqual2D(simplified[i].Coords, simplified[ni].Coords))
                {
                    continue;
                }

                // Degenerate segment, remove.
                simplified.RemoveAt(i);
                npts = simplified.Count;
            }

            return simplified.ToArray();
        }

        /// <summary>
        /// Gets the geometry configuration of the contour set
        /// </summary>
        /// <param name="maxVertices">Maximum vertices</param>
        /// <param name="maxPolys">Maximum polygons</param>
        /// <param name="maxVertsPerCont">Maximum vertices per contour</param>
        public void GetGeometryConfiguration(out int maxVertices, out int maxPolys, out int maxVertsPerCont)
        {
            maxVertices = 0;
            maxPolys = 0;
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
                maxPolys += nverts - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, nverts);
            }
        }
        /// <summary>
        /// Adds the contour
        /// </summary>
        /// <param name="reg">Region</param>
        /// <param name="area">Area type</param>
        /// <param name="rawVerts">Raw vertices</param>
        /// <param name="verts">Contour vertices</param>
        /// <param name="maxContours">Maximum number of contours</param>
        /// <param name="borderSize">Border size</param>
        public void AddContour(int reg, AreaTypes area, ContourVertex[] rawVerts, ContourVertex[] verts, int maxContours, int borderSize)
        {
            if (NConts >= maxContours)
            {
                // Allocate more contours.
                // This happens when a region has holes.
                Contour[] newConts = new Contour[maxContours * 2];
                for (int j = 0; j < NConts; ++j)
                {
                    newConts[j] = Conts[j];
                }
                Conts = newConts;
            }

            var cont = new Contour
            {
                NVertices = verts.Length,
                Vertices = verts.ToArray(),
                NRawVertices = rawVerts.Length,
                RawVertices = rawVerts.ToArray(),
                RegionId = reg,
                Area = area
            };

            if (borderSize > 0)
            {
                // If the heightfield was build with bordersize, remove the offset.
                for (int j = 0; j < cont.NVertices; ++j)
                {
                    var v = cont.Vertices[j];
                    v.X -= borderSize;
                    v.Z -= borderSize;
                    cont.Vertices[j] = v;
                }

                // If the heightfield was build with bordersize, remove the offset.
                for (int j = 0; j < cont.NRawVertices; ++j)
                {
                    var v = cont.RawVertices[j];
                    v.X -= borderSize;
                    v.Z -= borderSize;
                    cont.RawVertices[j] = v;
                }
            }

            Conts[NConts++] = cont;
        }
        /// <summary>
        /// Merge holes
        /// </summary>
        /// <param name="nregions">Number of regions</param>
        private void MergeHoles(int nregions)
        {
            if (NConts <= 0)
            {
                return;
            }

            // Calculate winding of all polygons.
            if (!CalculateWindings(out var winding))
            {
                return;
            }

            // Collect outline contour and holes contours per region.
            // We assume that there is one outline and multiple holes.
            var regions = Helper.CreateArray(nregions, () => { return new ContourRegion(); });
            var holes = Helper.CreateArray(NConts, () => { return new ContourHole(); });

            for (int i = 0; i < NConts; ++i)
            {
                var cont = Conts[i];
                // Positively would contours are outlines, negative holes.
                if (winding[i] > 0)
                {
                    if (regions[cont.RegionId].Outline != null)
                    {
                        Logger.WriteWarning(nameof(ContourSet), $"Multiple outlines for region {cont.RegionId}");
                    }
                    regions[cont.RegionId].Outline = cont;
                }
                else
                {
                    regions[cont.RegionId].NHoles++;
                }
            }

            int index = 0;
            for (int i = 0; i < nregions; i++)
            {
                if (regions[i].NHoles > 0)
                {
                    regions[i].Holes = new ContourHole[regions[i].NHoles];
                    Array.Copy(holes, index, regions[i].Holes, 0, regions[i].NHoles);
                    index += regions[i].NHoles;
                    regions[i].NHoles = 0;
                }
            }

            for (int i = 0; i < NConts; ++i)
            {
                var cont = Conts[i];
                var reg = regions[cont.RegionId];
                if (winding[i] < 0)
                {
                    reg.Holes[reg.NHoles++].Contour = cont;
                }
            }

            // Finally merge each regions holes into the outline.
            for (int i = 0; i < nregions; i++)
            {
                var reg = regions[i];
                if (reg.NHoles == 0)
                {
                    continue;
                }

                if (reg.Outline != null)
                {
                    reg.MergeRegionHoles();
                }
                else
                {
                    // The region does not have an outline.
                    // This can happen if the contour becames selfoverlapping because of too aggressive simplification settings.
                    Logger.WriteWarning(nameof(ContourSet), $"Bad outline for region {i}, contour simplification is likely too aggressive.");
                }
            }
        }
        /// <summary>
        /// Calculate windings
        /// </summary>
        /// <param name="winding">Resulting winding list</param>
        private bool CalculateWindings(out int[] winding)
        {
            winding = new int[NConts];
            int nholes = 0;
            for (int i = 0; i < NConts; ++i)
            {
                var cont = Conts[i];
                // If the contour is wound backwards, it is a hole.
                winding[i] = cont.CalcAreaOfPolygon2D() < 0 ? -1 : 1;
                if (winding[i] < 0)
                {
                    nholes++;
                }
            }

            if (nholes <= 0)
            {
                return false;
            }

            return true;
        }
    }
}
