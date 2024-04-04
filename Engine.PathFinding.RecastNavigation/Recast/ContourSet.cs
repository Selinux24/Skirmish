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
        /// An array of the contours in the set. [Size: #nconts]
        /// </summary>
        private Contour[] conts;
        /// <summary>
        /// The number of contours in the set.
        /// </summary>
        private int nconts;

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
        /// Gets whether the contour set has contours
        /// </summary>
        public bool HasContours()
        {
            return nconts > 0;
        }

        /// <summary>
        /// Iterates over the contour list
        /// </summary>
        public IEnumerable<(int i, Contour c)> IterateContours()
        {
            for (int i = 0; i < nconts; i++)
            {
                yield return (i, conts[i]);
            }
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
                conts = new Contour[maxContours],
                nconts = 0
            };

            int[] flags = chf.InitializeFlags();

            var cells = GridUtils.Iterate(w, h).SelectMany((item) => chf.BuildCompactCells(item.row, item.col, flags));

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

            return [.. simplified];
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

            return [.. simplified];
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
                if (Contour.IsRegion(points[i].Flag))
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

                bool differentRegs = (pi.Flag & Contour.RC_CONTOUR_REG_MASK) != (pii.Flag & Contour.RC_CONTOUR_REG_MASK);
                bool areaBorders = (pi.Flag & Contour.RC_AREA_BORDER) != (pii.Flag & Contour.RC_AREA_BORDER);
                if (differentRegs || areaBorders)
                {
                    changes.Add(new(pi.X, pi.Y, pi.Z, i));
                }
            }

            return [.. changes];
        }
        /// <summary>
        /// Find lower-left and upper-right vertices of the contour.
        /// </summary>
        private static ContourVertex[] CreateInitialPoints(ContourVertex[] points)
        {
            var ll = points[0];
            var ur = points[0];
            for (int i = 1; i < points.Length; i++)
            {
                var p = points[i];
                if (p.X < ll.X || (p.X == ll.X && p.Z < ll.Z))
                {
                    ll = p;
                    ll.Flag = i;
                }

                if (p.X > ur.X || (p.X == ur.X && p.Z > ur.Z))
                {
                    ur = p;
                    ur.Flag = i;
                }
            }

            return [ll, ur];
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

            float error = maxError * maxError;

            int pn = points.Length;
            for (int i = 0; i < simplified.Count;)
            {
                int ii = (i + 1) % simplified.Count;

                // Find maximum deviation from the segment.
                var (maxd, maxi) = FindMaximumDeviationFromSegment(simplified[i], simplified[ii], points, pn);

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > error)
                {
                    // Add the point.
                    var maxPoint = points[maxi];

                    simplified.Insert(i + 1, new(maxPoint.X, maxPoint.Y, maxPoint.Z, maxi));
                }
                else
                {
                    ++i;
                }
            }

            return [.. simplified];
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
                return [.. list];
            }

            var simplified = new List<ContourVertex>(list);

            int pn = points.Length;
            for (int i = 0; i < simplified.Count;)
            {
                int ii = (i + 1) % simplified.Count;

                // Find maximum deviation from the segment.
                int maxi = FindMaximumDeviationFromSegment(simplified[i], simplified[ii], maxEdgeLen, buildFlags, points, pn);

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1)
                {
                    // Add the point.
                    var maxPoint = points[maxi];

                    simplified.Insert(i + 1, new(maxPoint.X, maxPoint.Y, maxPoint.Z, maxi));
                }
                else
                {
                    ++i;
                }
            }

            return [.. simplified];
        }
        /// <summary>
        /// Finds the maximum deviation distance point from segment
        /// </summary>
        /// <param name="pA">Segment point A</param>
        /// <param name="pB">Segment point B</param>
        /// <param name="points">Point list to test</param>
        /// <param name="npoints">Number of points in the list</param>
        /// <returns>Returns the maximum distance and the point index</returns>
        private static (float MaxD, int MaxI) FindMaximumDeviationFromSegment(ContourVertex pA, ContourVertex pB, ContourVertex[] points, int npoints)
        {
            int ax = pA.X;
            int az = pA.Z;
            int ai = pA.Flag;

            int bx = pB.X;
            int bz = pB.Z;
            int bi = pB.Flag;

            int ci;
            int cinc;
            int endi;

            // Traverse the segment in lexilogical order so that the
            // max deviation is calculated similarly when traversing
            // opposite segments.
            if (bx > ax || (bx == ax && bz > az))
            {
                cinc = 1;
                ci = (ai + cinc) % npoints;
                endi = bi;
            }
            else
            {
                cinc = npoints - 1;
                ci = (bi + cinc) % npoints;
                endi = ai;
                Helper.Swap(ref ax, ref bx);
                Helper.Swap(ref az, ref bz);
            }

            // Tessellate only outer edges or edges between areas.
            if (Contour.IsRegion(points[ci].Flag) && !Contour.IsAreaBorder(points[ci].Flag))
            {
                return (0, -1);
            }

            float maxd = 0;
            int maxi = -1;

            while (ci != endi)
            {
                var p = points[ci];

                float d = Utils.DistancePtSegSqr2D(p.X, p.Z, ax, az, bx, bz);
                if (d > maxd)
                {
                    maxd = d;
                    maxi = ci;
                }
                ci = (ci + cinc) % npoints;
            }

            return (maxd, maxi);
        }
        /// <summary>
        /// Finds the maximum deviation distance point from segment
        /// </summary>
        /// <param name="pA">Segment point A</param>
        /// <param name="pB">Segment point B</param>
        /// <param name="maxEdgeLen">Maximum edge length</param>
        /// <param name="buildFlags">Build flags</param>
        /// <param name="points">Point list to test</param>
        /// <param name="npoints">Number of points in the list</param>
        /// <returns>Returns the point index</returns>
        private static int FindMaximumDeviationFromSegment(ContourVertex pA, ContourVertex pB, float maxEdgeLen, BuildContoursFlagTypes buildFlags, ContourVertex[] points, int npoints)
        {
            int ai = pA.Flag;
            int bi = pB.Flag;
            int ci = (ai + 1) % npoints;

            // Tessellate only outer edges or edges between areas.

            // Wall edges.
            if ((buildFlags & BuildContoursFlagTypes.RC_CONTOUR_TESS_WALL_EDGES) == 0 || Contour.IsRegion(points[ci].Flag))
            {
                return -1;
            }

            // Edges between areas.
            if ((buildFlags & BuildContoursFlagTypes.RC_CONTOUR_TESS_AREA_EDGES) == 0 || !Contour.IsBorderVertex(points[ci].Flag))
            {
                return -1;
            }

            int ax = pA.X;
            int az = pA.Z;
            int bx = pB.X;
            int bz = pB.Z;

            int dx = bx - ax;
            int dz = bz - az;
            if (dx * dx + dz * dz <= maxEdgeLen * maxEdgeLen)
            {
                return -1;
            }

            int n = bi < ai ? (bi + npoints - ai) : (bi - ai);
            if (n <= 1)
            {
                return -1;
            }

            // Round based on the segments in lexilogical order so that the
            // max tesselation is consistent regardles in which direction
            // segments are traversed.
            if (bx > ax || (bx == ax && bz > az))
            {
                return (ai + n / 2) % npoints;
            }
            else
            {
                return (ai + (n + 1) / 2) % npoints;
            }
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
                sv.Flag = (points[ai].Flag & (Contour.RC_CONTOUR_REG_MASK | Contour.RC_AREA_BORDER)) | (points[bi].Flag & Contour.RC_BORDER_VERTEX);
                simplified[i] = sv;
            }

            return [.. simplified];
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
                int ni = ArrayUtils.Next(i, npts);

                if (!Utils.VEqual2D(simplified[i].Coords, simplified[ni].Coords))
                {
                    continue;
                }

                // Degenerate segment, remove.
                simplified.RemoveAt(i);
                npts = simplified.Count;
            }

            return [.. simplified];
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

            for (int i = 0; i < nconts; ++i)
            {
                var nverts = conts[i].GetVertexCount();

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
        /// Finds the contour by region id
        /// </summary>
        /// <param name="reg">Region id</param>
        public Contour FindContour(int reg)
        {
            for (int i = 0; i < nconts; ++i)
            {
                if (conts[i].RegionId == reg)
                {
                    return conts[i];
                }
            }

            return null;
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
            if (nconts >= maxContours)
            {
                // Allocate more contours.
                // This happens when a region has holes.
                Contour[] newConts = new Contour[maxContours * 2];
                for (int j = 0; j < nconts; ++j)
                {
                    newConts[j] = conts[j];
                }
                conts = newConts;
            }

            Contour cont = new(reg, area, rawVerts, verts);

            cont.RemoveBorderSize(borderSize);

            conts[nconts++] = cont;
        }
        /// <summary>
        /// Merge holes
        /// </summary>
        /// <param name="nregions">Number of regions</param>
        private void MergeHoles(int nregions)
        {
            if (nconts <= 0)
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
            var regions = CollectOutlinesAndHolesPerRegion(nregions, winding);

            // Finally merge each regions holes into the outline.
            MergeIntoOutline(regions, nregions);
        }
        /// <summary>
        /// Collect outline contour and holes contours per region
        /// </summary>
        /// <param name="nregions">Number of regions</param>
        /// <param name="winding">Winding of all polygons</param>
        /// <returns>Returns the contour per region list</returns>
        /// <remarks>We assume that there is one outline and multiple holes</remarks>
        private ContourRegion[] CollectOutlinesAndHolesPerRegion(int nregions, int[] winding)
        {
            var regions = Helper.CreateArray(nregions, () => { return new ContourRegion(); });
            var holes = Helper.CreateArray(nconts, () => { return new ContourHole(); });

            for (int i = 0; i < nconts; ++i)
            {
                var cont = conts[i];

                // Positively would contours are outlines, negative holes.
                if (winding[i] <= 0)
                {
                    regions[cont.RegionId].NHoles++;

                    continue;
                }

                if (regions[cont.RegionId].Outline != null)
                {
                    Logger.WriteWarning(this, $"Multiple outlines for region {cont.RegionId}");
                }

                regions[cont.RegionId].Outline = cont;
            }

            int index = 0;
            for (int i = 0; i < nregions; i++)
            {
                if (regions[i].NHoles <= 0)
                {
                    continue;
                }

                regions[i].Holes = new ContourHole[regions[i].NHoles];
                Array.Copy(holes, index, regions[i].Holes, 0, regions[i].NHoles);
                index += regions[i].NHoles;
                regions[i].NHoles = 0;
            }

            for (int i = 0; i < nconts; ++i)
            {
                if (winding[i] >= 0)
                {
                    continue;
                }

                var cont = conts[i];
                var reg = regions[cont.RegionId];
                reg.Holes[reg.NHoles++].Contour = cont;
            }

            return regions;
        }
        /// <summary>
        /// Merges each region hole into the outline
        /// </summary>
        /// <param name="regions">Region list</param>
        /// <param name="nregions">Number of regions in the list</param>
        private void MergeIntoOutline(ContourRegion[] regions, int nregions)
        {
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
                    Logger.WriteWarning(this, $"Bad outline for region {i}, contour simplification is likely too aggressive.");
                }
            }
        }
        /// <summary>
        /// Calculate windings
        /// </summary>
        /// <param name="winding">Resulting winding list</param>
        private bool CalculateWindings(out int[] winding)
        {
            winding = new int[nconts];
            int nholes = 0;
            for (int i = 0; i < nconts; ++i)
            {
                var cont = conts[i];
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
