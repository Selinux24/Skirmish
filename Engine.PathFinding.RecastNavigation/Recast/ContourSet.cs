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
        /// The value returned by #rcGetCon if the specified direction is not connected
        /// to another span. (Has no neighbor.)
        /// </summary>
        public const int RC_NOT_CONNECTED = 0x3f;
        /// <summary>
        /// Heighfield border flag.
        /// If a heightfield region ID has this bit set, then the region is a border 
        /// region and its spans are considered unwalkable.
        /// (Used during the region and contour build process.)
        /// </summary>
        public const int RC_BORDER_REG = 0x8000;
        /// <summary>
        /// Applied to the region id field of contour vertices in order to extract the region id.
        /// The region id field of a vertex may have several flags applied to it.  So the
        /// fields value can't be used directly.
        /// </summary>
        public const int RC_CONTOUR_REG_MASK = 0xffff;
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

        private static readonly int[] OffsetsX = new[] { -1, 0, 1, 0, };
        private static readonly int[] OffsetsY = new[] { 0, 1, 0, -1 };
        private static readonly int[] OffsetsDir = new[] { 3, 0, -1, 2, 1 };

        /// <summary>
        /// An array of the contours in the set. [Size: #nconts]
        /// </summary>
        public Contour[] Conts { get; set; }
        /// <summary>
        /// The number of contours in the set.
        /// </summary>
        public int NConts { get; set; }
        /// <summary>
        /// The minimum bounds in world space. [(x, y, z)]
        /// </summary>
        public Vector3 BMin { get; set; }
        /// <summary>
        /// The maximum bounds in world space. [(x, y, z)]
        /// </summary>
        public Vector3 BMax { get; set; }
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

        public static int GetDirOffsetX(int dir)
        {
            return OffsetsX[dir & 0x03];
        }
        public static int GetDirOffsetY(int dir)
        {
            return OffsetsY[dir & 0x03];
        }
        public static int GetDirForOffset(int x, int y)
        {
            return OffsetsDir[((y + 1) << 1) + x];
        }
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
            var cset = CreateContourSet(chf, maxError, maxEdgeLen, buildFlags);

            // Merge holes if needed.
            cset.MergeHoles(chf);

            return cset;
        }
        private static ContourSet CreateContourSet(CompactHeightfield chf, float maxError, int maxEdgeLen, BuildContoursFlagTypes buildFlags)
        {
            int w = chf.Width;
            int h = chf.Height;
            int borderSize = chf.BorderSize;
            int maxContours = Math.Max(chf.MaxRegions, 8);

            var bmin = chf.BoundingBox.Minimum;
            var bmax = chf.BoundingBox.Maximum;
            if (borderSize > 0)
            {
                // If the heightfield was build with bordersize, remove the offset.
                float pad = borderSize * chf.CellSize;
                bmin.X += pad;
                bmin.Z += pad;
                bmax.X -= pad;
                bmax.Z -= pad;
            }

            var cset = new ContourSet
            {
                BMin = bmin,
                BMax = bmax,
                CellSize = chf.CellSize,
                CellHeight = chf.CellHeight,
                Width = chf.Width - chf.BorderSize * 2,
                Height = chf.Height - chf.BorderSize * 2,
                BorderSize = chf.BorderSize,
                MaxError = maxError,
                Conts = new Contour[maxContours],
                NConts = 0
            };

            int[] flags = InitializeFlags(chf);

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    cset.AddCompactCell(x, y, chf, maxError, maxEdgeLen, buildFlags, ref flags);
                }
            }

            return cset;
        }
        private static int[] InitializeFlags(CompactHeightfield chf)
        {
            int[] flags = new int[chf.SpanCount];

            int w = chf.Width;
            int h = chf.Height;

            // Mark boundaries.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    InitializeCellFlags(x, y, chf, ref flags);
                }
            }

            return flags;
        }
        private static void InitializeCellFlags(int x, int y, CompactHeightfield chf, ref int[] flags)
        {
            int w = chf.Width;

            var c = chf.Cells[x + y * w];
            for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
            {
                if (chf.Spans[i].Reg == 0 || (chf.Spans[i].Reg & RC_BORDER_REG) != 0)
                {
                    flags[i] = 0;
                    continue;
                }

                int res = 0;
                var s = chf.Spans[i];
                for (int dir = 0; dir < 4; ++dir)
                {
                    int r = 0;
                    if (s.GetCon(dir) != RC_NOT_CONNECTED)
                    {
                        int ax = x + GetDirOffsetX(dir);
                        int ay = y + GetDirOffsetY(dir);
                        int ai = chf.Cells[ax + ay * w].Index + s.GetCon(dir);
                        r = chf.Spans[ai].Reg;
                    }
                    if (r == chf.Spans[i].Reg)
                    {
                        res |= (1 << dir);
                    }
                }
                flags[i] = res ^ 0xf; // Inverse, mark non connected edges.
            }
        }

        private static IEnumerable<Int4> SimplifyContour(IEnumerable<Int4> points, float maxError, int maxEdgeLen, BuildContoursFlagTypes buildFlags)
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

            return simplified;
        }
        /// <summary>
        /// Add initial points.
        /// </summary>
        private static IEnumerable<Int4> Initialize(IEnumerable<Int4> points)
        {
            var simplified = new List<Int4>();

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

            return simplified;
        }
        private static bool PointsHasConnections(IEnumerable<Int4> points)
        {
            bool hasConnections = false;
            for (int i = 0; i < points.Count(); i++)
            {
                if ((points.ElementAt(i).W & RC_CONTOUR_REG_MASK) != 0)
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
        private static IEnumerable<Int4> GetChangePoints(IEnumerable<Int4> points)
        {
            var changes = new List<Int4>();

            for (int i = 0, ni = points.Count(); i < ni; ++i)
            {
                int ii = (i + 1) % ni;
                bool differentRegs = (points.ElementAt(i).W & RC_CONTOUR_REG_MASK) != (points.ElementAt(ii).W & RC_CONTOUR_REG_MASK);
                bool areaBorders = (points.ElementAt(i).W & RC_AREA_BORDER) != (points.ElementAt(ii).W & RC_AREA_BORDER);
                if (differentRegs || areaBorders)
                {
                    changes.Add(new Int4(points.ElementAt(i).X, points.ElementAt(i).Y, points.ElementAt(i).Z, i));
                }
            }

            return changes;
        }
        /// <summary>
        /// Find lower-left and upper-right vertices of the contour.
        /// </summary>
        private static IEnumerable<Int4> CreateInitialPoints(IEnumerable<Int4> points)
        {
            var initialPoints = new List<Int4>();

            int llx = points.First().X;
            int lly = points.First().Y;
            int llz = points.First().Z;
            int lli = 0;
            int urx = points.First().X;
            int ury = points.First().Y;
            int urz = points.First().Z;
            int uri = 0;
            for (int i = 0; i < points.Count(); i++)
            {
                int x = points.ElementAt(i).X;
                int y = points.ElementAt(i).Y;
                int z = points.ElementAt(i).Z;
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
            initialPoints.Add(new Int4(llx, lly, llz, lli));
            initialPoints.Add(new Int4(urx, ury, urz, uri));

            return initialPoints;
        }
        private static IEnumerable<Int4> AddPoints(IEnumerable<Int4> points, IEnumerable<Int4> list, float maxError)
        {
            var simplified = new List<Int4>(list);

            int pn = points.Count();
            for (int i = 0; i < simplified.Count;)
            {
                int ii = (i + 1) % simplified.Count;

                int ax = simplified[i].X;
                int az = simplified[i].Z;
                int ai = simplified[i].W;

                int bx = simplified[ii].X;
                int bz = simplified[ii].Z;
                int bi = simplified[ii].W;

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
                if ((points.ElementAt(ci).W & RC_CONTOUR_REG_MASK) == 0 ||
                    (points.ElementAt(ci).W & RC_AREA_BORDER) != 0)
                {
                    while (ci != endi)
                    {
                        float d = Utils.DistancePtSeg2D(points.ElementAt(ci).X, points.ElementAt(ci).Z, ax, az, bx, bz);
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
                    simplified.Insert(i + 1, new Int4(points.ElementAt(maxi).X, points.ElementAt(maxi).Y, points.ElementAt(maxi).Z, maxi));
                }
                else
                {
                    ++i;
                }
            }

            return simplified;
        }
        private static IEnumerable<Int4> SplitLongEdges(IEnumerable<Int4> points, IEnumerable<Int4> list, int maxEdgeLen, BuildContoursFlagTypes buildFlags)
        {
            bool tesselate = maxEdgeLen > 0 && (buildFlags & (BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES | BuildContoursFlagTypes.RC_CONTOUR_TESS_AREA_EDGES)) != 0;
            if (!tesselate)
            {
                return list.ToArray();
            }

            var simplified = new List<Int4>(list);

            int pn = points.Count();
            for (int i = 0; i < simplified.Count;)
            {
                int ii = (i + 1) % simplified.Count;

                int ax = simplified[i].X;
                int az = simplified[i].Z;
                int ai = simplified[i].W;

                int bx = simplified[ii].X;
                int bz = simplified[ii].Z;
                int bi = simplified[ii].W;

                // Find maximum deviation from the segment.
                int maxi = -1;
                int ci = (ai + 1) % pn;

                // Tessellate only outer edges or edges between areas.
                bool tess = false;

                // Wall edges.
                if ((buildFlags & BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES) != 0 &&
                    (points.ElementAt(ci).W & RC_CONTOUR_REG_MASK) == 0)
                {
                    tess = true;
                }

                // Edges between areas.
                if ((buildFlags & BuildContoursFlagTypes.RC_CONTOUR_TESS_AREA_EDGES) != 0 &&
                    (points.ElementAt(ci).W & RC_AREA_BORDER) != 0)
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
                    simplified.Insert(i + 1, new Int4(points.ElementAt(maxi).X, points.ElementAt(maxi).Y, points.ElementAt(maxi).Z, maxi));
                }
                else
                {
                    ++i;
                }
            }

            return simplified;
        }
        private static IEnumerable<Int4> UpdateNeighbors(IEnumerable<Int4> points, IEnumerable<Int4> list)
        {
            var simplified = new List<Int4>(list);

            int pn = points.Count();
            for (int i = 0; i < simplified.Count; ++i)
            {
                // The edge vertex flag is take from the current raw point,
                // and the neighbour region is take from the next raw point.
                var sv = simplified[i];
                int ai = (sv.W + 1) % pn;
                int bi = sv.W;
                sv.W = (points.ElementAt(ai).W & (RC_CONTOUR_REG_MASK | RC_AREA_BORDER)) | (points.ElementAt(bi).W & RC_BORDER_VERTEX);
                simplified[i] = sv;
            }

            return simplified;
        }
        private static IEnumerable<Int4> RemoveDegenerateSegments(IEnumerable<Int4> list)
        {
            var simplified = new List<Int4>(list);

            // Remove adjacent vertices which are equal on xz-plane,
            // or else the triangulator will get confused.
            int npts = simplified.Count;
            for (int i = 0; i < npts; ++i)
            {
                int ni = Utils.Next(i, npts);

                if (!VEqualXZ(simplified[i], simplified[ni]))
                {
                    continue;
                }

                // Degenerate segment, remove.
                simplified.RemoveAt(i);
                npts = simplified.Count;
            }

            return simplified;
        }
        private static bool VEqualXZ(Int4 a, Int4 b)
        {
            return a.X == b.X && a.Z == b.Z;
        }

        private void AddContour(int reg, AreaTypes area, IEnumerable<Int4> verts, IEnumerable<Int4> simplified, int maxContours, int borderSize)
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
                NVertices = simplified.Count(),
                Vertices = simplified.ToArray(),
                NRawVertices = verts.Count(),
                RawVertices = verts.ToArray(),
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
        private void AddCompactCell(int x, int y, CompactHeightfield chf, float maxError, int maxEdgeLen, BuildContoursFlagTypes buildFlags, ref int[] flags)
        {
            int w = chf.Width;
            int borderSize = chf.BorderSize;
            int maxContours = Math.Max(chf.MaxRegions, 8);

            var c = chf.Cells[x + y * w];
            for (int i = c.Index, ni = c.Index + c.Count; i < ni; ++i)
            {
                if (flags[i] == 0 || flags[i] == 0xf)
                {
                    flags[i] = 0;
                    continue;
                }

                int reg = chf.Spans[i].Reg;
                if (reg == 0 || (reg & RC_BORDER_REG) != 0)
                {
                    continue;
                }

                var area = chf.Areas[i];
                var verts = chf.WalkContour(x, y, i, ref flags);
                var simplified = SimplifyContour(verts, maxError, maxEdgeLen, buildFlags);

                if (simplified.Count() < 3)
                {
                    continue;
                }

                // Store region->contour remap info.
                // Create contour.
                AddContour(reg, area, verts, simplified, maxContours, borderSize);
            }
        }
        private void MergeHoles(CompactHeightfield chf)
        {
            if (NConts <= 0)
            {
                return;
            }

            // Calculate winding of all polygons.
            int[] winding = new int[NConts];
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
                return;
            }

            // Collect outline contour and holes contours per region.
            // We assume that there is one outline and multiple holes.
            int nregions = chf.MaxRegions + 1;
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
    };
}
