using SharpDX;
using System;
using System.Collections.Generic;

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

            ContourSet cset = new ContourSet
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

            int[] flags = new int[chf.SpanCount];

            // Mark boundaries.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
                    {
                        int res = 0;
                        var s = chf.Spans[i];
                        if (chf.Spans[i].Reg == 0 || (chf.Spans[i].Reg & RC_BORDER_REG) != 0)
                        {
                            flags[i] = 0;
                            continue;
                        }
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            int r = 0;
                            if (s.GetCon(dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastUtils.GetDirOffsetX(dir);
                                int ay = y + RecastUtils.GetDirOffsetY(dir);
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
            }

            List<Int4> verts = new List<Int4>();
            List<Int4> simplified = new List<Int4>();

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var c = chf.Cells[x + y * w];
                    for (int i = c.Index, ni = (c.Index + c.Count); i < ni; ++i)
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

                        verts.Clear();
                        simplified.Clear();

                        verts.AddRange(chf.WalkContour(x, y, i, flags));

                        SimplifyContour(verts, simplified, maxError, maxEdgeLen, buildFlags);
                        RemoveDegenerateSegments(simplified);

                        // Store region->contour remap info.
                        // Create contour.
                        if (simplified.Count >= 3)
                        {
                            if (cset.NConts >= maxContours)
                            {
                                // Allocate more contours.
                                // This happens when a region has holes.
                                Contour[] newConts = new Contour[maxContours * 2];
                                for (int j = 0; j < cset.NConts; ++j)
                                {
                                    newConts[j] = cset.Conts[j];
                                }
                                cset.Conts = newConts;
                            }

                            var cont = new Contour
                            {
                                NVertices = simplified.Count,
                                Vertices = simplified.ToArray(),
                                NRawVertices = verts.Count,
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

                            cset.Conts[cset.NConts++] = cont;
                        }
                    }
                }
            }

            // Merge holes if needed.
            if (cset.NConts > 0)
            {
                // Calculate winding of all polygons.
                int[] winding = new int[cset.NConts];
                int nholes = 0;
                for (int i = 0; i < cset.NConts; ++i)
                {
                    var cont = cset.Conts[i];
                    // If the contour is wound backwards, it is a hole.
                    winding[i] = RecastUtils.CalcAreaOfPolygon2D(cont.Vertices, cont.NVertices) < 0 ? -1 : 1;
                    if (winding[i] < 0)
                    {
                        nholes++;
                    }
                }

                if (nholes > 0)
                {
                    // Collect outline contour and holes contours per region.
                    // We assume that there is one outline and multiple holes.
                    int nregions = chf.MaxRegions + 1;
                    var regions = Helper.CreateArray(nregions, () => { return new ContourRegion(); });
                    var holes = Helper.CreateArray(cset.NConts, () => { return new ContourHole(); });

                    for (int i = 0; i < cset.NConts; ++i)
                    {
                        var cont = cset.Conts[i];
                        // Positively would contours are outlines, negative holes.
                        if (winding[i] > 0)
                        {
                            if (regions[cont.RegionId].Outline != null)
                            {
                                Logger.WriteWarning($"Multiple outlines for region {cont.RegionId}");
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
                    for (int i = 0; i < cset.NConts; ++i)
                    {
                        var cont = cset.Conts[i];
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
                            Logger.WriteWarning($"Bad outline for region {i}, contour simplification is likely too aggressive.");
                        }
                    }
                }

            }

            return cset;
        }
        private static void SimplifyContour(List<Int4> points, List<Int4> simplified, float maxError, int maxEdgeLen, BuildContoursFlagTypes buildFlags)
        {
            // Add initial points.
            bool hasConnections = false;
            for (int i = 0; i < points.Count; i++)
            {
                if ((points[i].W & RC_CONTOUR_REG_MASK) != 0)
                {
                    hasConnections = true;
                    break;
                }
            }

            if (hasConnections)
            {
                // The contour has some portals to other regions.
                // Add a new point to every location where the region changes.
                for (int i = 0, ni = points.Count; i < ni; ++i)
                {
                    int ii = (i + 1) % ni;
                    bool differentRegs = (points[i].W & RC_CONTOUR_REG_MASK) != (points[ii].W & RC_CONTOUR_REG_MASK);
                    bool areaBorders = (points[i].W & RC_AREA_BORDER) != (points[ii].W & RC_AREA_BORDER);
                    if (differentRegs || areaBorders)
                    {
                        simplified.Add(new Int4(points[i].X, points[i].Y, points[i].Z, i));
                    }
                }
            }

            if (simplified.Count == 0)
            {
                // If there is no connections at all,
                // create some initial points for the simplification process.
                // Find lower-left and upper-right vertices of the contour.
                int llx = points[0].X;
                int lly = points[0].Y;
                int llz = points[0].Z;
                int lli = 0;
                int urx = points[0].X;
                int ury = points[0].Y;
                int urz = points[0].Z;
                int uri = 0;
                for (int i = 0; i < points.Count; i++)
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
                simplified.Add(new Int4(llx, lly, llz, lli));
                simplified.Add(new Int4(urx, ury, urz, uri));
            }

            // Add points until all raw points are within
            // error tolerance to the simplified shape.
            int pn = points.Count;
            for (int i = 0; i < simplified.Count;)
            {
                int ii = (i + 1) % (simplified.Count);

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
                if ((points[ci].W & RC_CONTOUR_REG_MASK) == 0 ||
                    (points[ci].W & RC_AREA_BORDER) != 0)
                {
                    while (ci != endi)
                    {
                        float d = RecastUtils.DistancePtSeg2D(points[ci].X, points[ci].Z, ax, az, bx, bz);
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
                    simplified.Insert(i + 1, new Int4(points[maxi].X, points[maxi].Y, points[maxi].Z, maxi));
                }
                else
                {
                    ++i;
                }
            }

            // Split too long edges.
            if (maxEdgeLen > 0 && (buildFlags & (BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES | BuildContoursFlagTypes.RC_CONTOUR_TESS_AREA_EDGES)) != 0)
            {
                for (int i = 0; i < simplified.Count;)
                {
                    int ii = (i + 1) % (simplified.Count);

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
                        (points[ci].W & RC_CONTOUR_REG_MASK) == 0)
                    {
                        tess = true;
                    }
                    // Edges between areas.
                    if ((buildFlags & BuildContoursFlagTypes.RC_CONTOUR_TESS_AREA_EDGES) != 0 &&
                        (points[ci].W & RC_AREA_BORDER) != 0)
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
                        simplified.Insert(i + 1, new Int4(points[maxi].X, points[maxi].Y, points[maxi].Z, maxi));
                    }
                    else
                    {
                        ++i;
                    }
                }
            }

            for (int i = 0; i < simplified.Count; ++i)
            {
                // The edge vertex flag is take from the current raw point,
                // and the neighbour region is take from the next raw point.
                var sv = simplified[i];
                int ai = (sv.W + 1) % pn;
                int bi = sv.W;
                sv.W = (points[ai].W & (RC_CONTOUR_REG_MASK | RC_AREA_BORDER)) | (points[bi].W & RC_BORDER_VERTEX);
                simplified[i] = sv;
            }
        }
        private static void RemoveDegenerateSegments(List<Int4> simplified)
        {
            // Remove adjacent vertices which are equal on xz-plane,
            // or else the triangulator will get confused.
            int npts = simplified.Count;
            for (int i = 0; i < npts; ++i)
            {
                int ni = RecastUtils.Next(i, npts);

                if (simplified[i] == simplified[ni])
                {
                    // Degenerate segment, remove.
                    for (int j = i; j < simplified.Count - 1; ++j)
                    {
                        simplified[j] = simplified[(j + 1)];
                    }
                    simplified.Clear();
                    npts--;
                }
            }
        }

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
    };
}
