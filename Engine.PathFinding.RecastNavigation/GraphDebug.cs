using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;
    using Engine.PathFinding.RecastNavigation.Recast;

    /// <summary>
    /// Graph debug helper
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="graph">Graph</param>
    /// <param name="agent">Agent</param>
    public struct GraphDebug(Graph graph, AgentType agent) : IGraphDebug
    {
        /// <summary>
        /// Internal graph
        /// </summary>
        private readonly Graph graph = graph ?? throw new ArgumentNullException(nameof(graph));

        /// <inheritdoc/>
        public readonly IGraph Graph { get { return graph; } }
        /// <inheritdoc/>
        public AgentType Agent { get; private set; } = agent ?? throw new ArgumentNullException(nameof(agent));

        /// <inheritdoc/>
        public readonly IEnumerable<(int Id, string Information)> GetAvailableDebugInformation()
        {
            return Enum
                .GetValues<GraphDebugTypes>()
                .Except([GraphDebugTypes.None])
                .Select(v => ((int)v, v.ToString()));
        }
        /// <inheritdoc/>
        public readonly IGraphDebugData GetInfo(int id, Vector3 point)
        {
            var nm = graph.CreateAgentQuery(agent)?.GetAttachedNavMesh();
            if (nm == null)
            {
                return new GraphDebugData([]);
            }

            BuildData buildData;
            if (graph.Settings.BuildMode == BuildModes.Solo)
            {
                buildData = nm.GetSoloBuildData();
            }
            else if (graph.Settings.BuildMode == BuildModes.Tiled)
            {
                NavMesh.GetTileAtPosition(point, graph.Settings.TileCellSize, graph.Bounds, out var tx, out var ty, out _);

                buildData = nm.GetTiledBuildData(tx, ty);
            }
            else
            {
                buildData = default;
            }

            var debug = (GraphDebugTypes)id;
            var data = debug switch
            {
                GraphDebugTypes.NavMesh => GetNodes(false, false),
                GraphDebugTypes.Nodes => GetNodes(true, false),
                GraphDebugTypes.NodesWithLinks => GetNodes(true, true),
                GraphDebugTypes.Heightfield => GetHeightfield(buildData.Heightfield, false),
                GraphDebugTypes.WalkableHeightfield => GetHeightfield(buildData.Heightfield, true),
                GraphDebugTypes.RawContours => GetContours(buildData.ContourSet, true),
                GraphDebugTypes.Contours => GetContours(buildData.ContourSet, false),
                GraphDebugTypes.PolyMesh => GetPolyMesh(buildData.PolyMesh),
                GraphDebugTypes.DetailMesh => GetDetailMesh(buildData.PolyMeshDetail),
                GraphDebugTypes.TileCacheContours => GetTileCacheContours(buildData.TileCacheContourSet, buildData.Origin, buildData.CellSize, buildData.CellHeight),
                GraphDebugTypes.TileCachePolyMesh => GetTileCachePolyMesh(buildData.TileCachePolyMesh, buildData.Origin, buildData.CellSize, buildData.CellHeight),
                _ => []
            };
            return new GraphDebugData(data);
        }

        /// <summary>
        /// Gets the nodes debug information
        /// </summary>
        /// <param name="separateNodes">Separate nodes</param>
        /// <param name="showTriangles">Show triangles</param>
        private readonly IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetNodes(bool separateNodes, bool showTriangles)
        {
            var nodes = graph.GetNodes(Agent).OfType<GraphNode>();
            if (!nodes.Any())
            {
                return [];
            }

            if (showTriangles)
            {
                return
                [
                    .. GetBounds(),
                    .. GetNodeTris(nodes, separateNodes),
                    .. GetNodeLines(nodes, separateNodes),
                ];
            }
            else
            {
                return
                [
                    .. GetBounds(),
                    .. GetNodeTris(nodes, separateNodes),
                ];
            }
        }
        /// <summary>
        /// Gets the node triangles debug information
        /// </summary>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetNodeTris(IEnumerable<GraphNode> nodes, bool separateNodes)
        {
            string nameTris = $"{GraphDebugTypes.Nodes}_Tris";

            if (separateNodes)
            {
                var tris = nodes
                    .GroupBy(n => Helper.IntToCol(n.Id, 128))
                    .ToDictionary(
                        keySelector => keySelector.Key,
                        elementSelector => elementSelector.SelectMany(gn => gn.Triangles.SelectMany(t => t.GetVertices())).AsEnumerable());

                yield return (nameTris, Topology.TriangleList, tris);
            }
            else
            {
                Color4 colorTris = new Color(0, 192, 255, 128);
                Dictionary<Color4, IEnumerable<Vector3>> tris = new([new(colorTris, nodes.SelectMany(n => n.Triangles.SelectMany(t => t.GetVertices())).AsEnumerable())]);

                yield return (nameTris, Topology.TriangleList, tris);
            }
        }
        /// <summary>
        /// Gets the node lines debug information
        /// </summary>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetNodeLines(IEnumerable<GraphNode> nodes, bool separateNodes)
        {
            string nameLines = $"{GraphDebugTypes.Nodes}_Lines";

            Vector3 deltaY = new(0f, 0.01f, 0f);

            if (separateNodes)
            {
                var lines = nodes
                    .GroupBy(n => Helper.IntToCol(n.Id, 128))
                    .ToDictionary(
                        keySelector => keySelector.Key,
                        elementSelector => elementSelector.SelectMany(gn => gn.Triangles.SelectMany(t => t.GetEdgeSegments().SelectMany(s => new Vector3[] { s.Point1 + deltaY, s.Point2 + deltaY }))).AsEnumerable());

                yield return (nameLines, Topology.LineList, lines);
            }
            else
            {
                Color4 colorLines = new Color(0, 192, 255, 128);
                Dictionary<Color4, IEnumerable<Vector3>> lines = new([new(colorLines, nodes.SelectMany(n => n.Triangles.SelectMany(t => t.GetEdgeSegments().SelectMany(s => new Vector3[] { s.Point1 + deltaY, s.Point2 + deltaY }))).AsEnumerable())]);

                yield return (nameLines, Topology.LineList, lines);
            }
        }

        /// <summary>
        /// Gets the height field debug information
        /// </summary>
        private static List<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetHeightfield(Heightfield hf, bool walkable)
        {
            if (hf == null)
            {
                return [];
            }

            const string name = nameof(GetHeightfield);

            var orig = hf.Bounds.Minimum;
            float cs = hf.CellSize;
            float ch = hf.CellHeight;

            List<Triangle> walkableTriangles = [];
            List<Triangle> nullTriangles = [];
            List<Triangle> multiTriangles = [];

            foreach (var (row, col, span) in hf.IterateSpans())
            {
                float fz = orig.Z + col * cs;
                float fx = orig.X + row * cs;

                var s = span;
                do
                {
                    var min = new Vector3(fx, orig.Y + s.Min * ch, fz);
                    var max = new Vector3(fx + cs, orig.Y + s.Max * ch, fz + cs);

                    var boxTris = TriangulateBox(min, max, 0.95f);

                    if (s.Area == AreaTypes.RC_WALKABLE_AREA)
                    {
                        walkableTriangles.AddRange(boxTris);
                    }
                    else if (s.Area == AreaTypes.RC_NULL_AREA)
                    {
                        nullTriangles.AddRange(boxTris);
                    }
                    else
                    {
                        multiTriangles.AddRange(boxTris);
                    }

                    s = s.Next;
                }
                while (s != null);
            }

            if (walkable)
            {
                Color4 walkableColor = new Color(64, 128, 160, 255);
                Color4 nullColor = new Color(64, 64, 64, 255);
                Color4 multiColor = new Color(0, 192, 255, 255);

                Dictionary<Color4, IEnumerable<Vector3>> data = new()
                {
                    { walkableColor, walkableTriangles.Where(t => t.Normal == Vector3.Up).SelectMany(t=>t.GetVertices()) },
                    { nullColor, nullTriangles.Where(t => t.Normal == Vector3.Up).SelectMany(t=>t.GetVertices()) },
                    { multiColor, multiTriangles.Where(t => t.Normal == Vector3.Up).SelectMany(t=>t.GetVertices()) },
                    { Color.White, [.. walkableTriangles.Where(t => t.Normal != Vector3.Up).SelectMany(t => t.GetVertices()), .. nullTriangles.Where(t => t.Normal != Vector3.Up).SelectMany(t => t.GetVertices()), .. multiTriangles.Where(t => t.Normal != Vector3.Up).SelectMany(t => t.GetVertices())] },
                };
                return [(name, Topology.TriangleList, data)];
            }
            else
            {
                var walkableVerts = walkableTriangles.SelectMany(t => t.GetVertices());
                var nullVerts = nullTriangles.SelectMany(t => t.GetVertices());
                var multiVerts = multiTriangles.SelectMany(t => t.GetVertices());

                Dictionary<Color4, IEnumerable<Vector3>> data = new()
                {
                    { Color.White, [.. walkableVerts, .. nullVerts, .. multiVerts] },
                };
                return [(name, Topology.TriangleList, data)];
            }
        }

        /// <summary>
        /// Gets the contour debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        /// <param name="drawRaw">Draw raw contour set</param>
        private readonly List<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetContours(ContourSet cset, bool drawRaw)
        {
            if (cset == null)
            {
                return [];
            }

            List<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> res =
            [
                .. GetBounds(),
                .. GetContoursRegions(cset),
            ];

            if (drawRaw)
            {
                res.AddRange(GetContourRawLines(cset, 0.75f));
            }
            else
            {
                res.AddRange(GetContourBorders(cset, 0.75f));
            }

            return res;
        }
        /// <summary>
        /// Gets the contour lines debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetContourRawLines(ContourSet cset, float alpha)
        {
            var orig = cset.Bounds.Minimum;
            float cs = cset.CellSize;
            float ch = cset.CellHeight;

            int a = (int)(alpha * 255.0f);

            Dictionary<Color4, List<Vector3>> lines = [];

            bool empty = true;
            foreach (var (i, c) in cset.IterateContours())
            {
                if (!c.HasRawVertices())
                {
                    continue;
                }

                Color4 regColor = Helper.IntToCol(c.RegionId, a);
                lines.TryAdd(regColor, []);

                foreach (var (j, v) in c.IterateRawVertices())
                {
                    float fx = orig.X + v.X * cs;
                    float fy = orig.Y + (v.Y + 1 + (i & 1)) * ch;
                    float fz = orig.Z + v.Z * cs;

                    lines[regColor].Add(new(fx, fy, fz));
                    if (j > 0)
                    {
                        lines[regColor].Add(new(fx, fy, fz));
                    }
                }

                // Loop last segment.
                var v0 = c.GetRawVertex(0);
                float f0x = orig.X + v0.X * cs;
                float f0y = orig.Y + (v0.Y + 1 + (i & 1)) * ch;
                float f0z = orig.Z + v0.Z * cs;
                lines[regColor].Add(new(f0x, f0y, f0z));
                empty = false;
            }

            if (empty)
            {
                yield break;
            }

            var dict = lines.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return ($"{GraphDebugTypes.Contours}_RawLines", Topology.LineList, dict);
        }
        /// <summary>
        /// Gets the contour borders debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetContourBorders(ContourSet cset, float alpha)
        {
            var orig = cset.Bounds.Minimum;
            float cs = cset.CellSize;
            float ch = cset.CellHeight;

            int a = (int)(alpha * 255.0f);
            Color4 bseColor = new Color(255, 255, 255, a);

            Dictionary<Color4, List<Vector3>> lines = [];

            bool empty = true;
            foreach (var (i, c) in cset.IterateContours())
            {
                if (!c.HasVertices())
                {
                    continue;
                }

                var regColor = Helper.IntToCol(c.RegionId, a);
                var bColor = Color4.Lerp(regColor, bseColor, 0.5f);

                foreach (var (va, vb) in c.IterateSegments())
                {
                    var colol = Contour.IsAreaBorder(va.Flag) ? regColor : bColor;
                    lines.TryAdd(colol, []);

                    float fx, fy, fz;
                    fx = orig.X + va.X * cs;
                    fy = orig.Y + (va.Y + 1 + (i & 1)) * ch;
                    fz = orig.Z + va.Z * cs;

                    lines[colol].Add(new(fx, fy, fz));

                    fx = orig.X + vb.X * cs;
                    fy = orig.Y + (vb.Y + 1 + (i & 1)) * ch;
                    fz = orig.Z + vb.Z * cs;

                    lines[colol].Add(new(fx, fy, fz));

                    empty = false;
                }
            }

            if (empty)
            {
                yield break;
            }

            var dict = lines.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return ($"{GraphDebugTypes.Contours}_Borders", Topology.LineList, dict);
        }
        /// <summary>
        /// Gets the contour regions debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetContoursRegions(ContourSet cset)
        {
            var orig = cset.Bounds.Minimum;
            float cs = cset.CellSize;
            float ch = cset.CellHeight;

            List<Vector3> lines = [];

            // Draw centers
            foreach (var (i, cont1) in cset.IterateContours())
            {
                var pos1 = cont1.GetContourCenter(orig, cs, ch);

                foreach (var (j, v) in cont1.IterateVertices())
                {
                    var r = (int)(uint)v.Flag;

                    if (v.Flag == 0 || r < cont1.RegionId)
                    {
                        continue;
                    }

                    var cont2 = cset.FindContour(r);
                    if (cont2 != null)
                    {
                        var pos2 = cont2.GetContourCenter(orig, cs, ch);

                        var arcPoints = Line3D.CreateArc(pos1, pos2, 0.25f, 8).SelectMany(a => new[] { a.Point1, a.Point2 });

                        lines.AddRange(arcPoints);
                    }
                }
            }

            if (lines.Count <= 0)
            {
                yield break;
            }

            Color4 color = new Color(32, 128, 128, 196);

            var dict = new Dictionary<Color4, IEnumerable<Vector3>>
            {
                { color, lines }
            };
            yield return ($"{GraphDebugTypes.Contours}_RegionConnections", Topology.LineList, dict);
        }

        /// <summary>
        /// Gets the contour debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        /// <param name="drawRaw">Draw raw contour set</param>
        private readonly List<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetTileCacheContours(TileCacheContourSet cset, Vector3 orig, float cs, float ch)
        {
            return
            [
                .. GetBounds(),
                .. GetTileCacheContoursRegions(cset, orig, cs, ch),
                .. GetTileCacheContourBorders(cset, orig, cs, ch, 0.75f),
            ];
        }
        /// <summary>
        /// Gets the contour borders debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetTileCacheContourBorders(TileCacheContourSet cset, Vector3 orig, float cs, float ch, float alpha)
        {
            int a = (int)(alpha * 255.0f);
            Color4 bseColor = new Color(255, 255, 255, a);

            Dictionary<Color4, List<Vector3>> lines = [];

            bool empty = true;
            foreach (var (i, c) in cset.IterateContours())
            {
                if (!c.HasVertices())
                {
                    continue;
                }

                var regColor = Helper.IntToCol(c.RegionId, a);
                var bColor = Color4.Lerp(regColor, bseColor, 0.5f);

                foreach (var (va, vb) in c.IterateSegments())
                {
                    var colol = Contour.IsAreaBorder(va.Flag) ? regColor : bColor;
                    lines.TryAdd(colol, []);

                    float fx, fy, fz;
                    fx = orig.X + va.X * cs;
                    fy = orig.Y + (va.Y + 1 + (i & 1)) * ch;
                    fz = orig.Z + va.Z * cs;

                    lines[colol].Add(new(fx, fy, fz));

                    fx = orig.X + vb.X * cs;
                    fy = orig.Y + (vb.Y + 1 + (i & 1)) * ch;
                    fz = orig.Z + vb.Z * cs;

                    lines[colol].Add(new(fx, fy, fz));

                    empty = false;
                }
            }

            if (empty)
            {
                yield break;
            }

            var dict = lines.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return ($"{GraphDebugTypes.Contours}_Borders", Topology.LineList, dict);
        }
        /// <summary>
        /// Gets the contour regions debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetTileCacheContoursRegions(TileCacheContourSet cset, Vector3 orig, float cs, float ch)
        {
            List<Vector3> lines = [];

            // Draw centers
            foreach (var (i, cont1) in cset.IterateContours())
            {
                var pos1 = cont1.GetContourCenter(orig, cs, ch);

                foreach (var (j, v) in cont1.IterateVertices())
                {
                    var r = (int)(uint)v.Flag;

                    if (v.Flag == 0 || r < cont1.RegionId)
                    {
                        continue;
                    }

                    var cont2 = cset.FindContour(r);
                    if (cont2 != null)
                    {
                        var pos2 = cont2.Value.GetContourCenter(orig, cs, ch);

                        var arcPoints = Line3D.CreateArc(pos1, pos2, 0.25f, 8).SelectMany(a => new[] { a.Point1, a.Point2 });

                        lines.AddRange(arcPoints);
                    }
                }
            }

            if (lines.Count <= 0)
            {
                yield break;
            }

            Color4 color = new Color(32, 128, 128, 196);

            var dict = new Dictionary<Color4, IEnumerable<Vector3>>
            {
                { color, lines }
            };
            yield return ($"{GraphDebugTypes.Contours}_RegionConnections", Topology.LineList, dict);
        }

        /// <summary>
        /// Gets the polygon mesh debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private readonly IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetPolyMesh(PolyMesh pm)
        {
            if (pm == null)
            {
                return [];
            }

            return
            [
                .. GetBounds(),
                .. GetPolyMeshTris(pm),
                .. GetPolyEdges(pm),
                .. GetPolyBoundaries(pm),
            ];
        }
        /// <summary>
        /// Gets the polygon mesh triangles debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetPolyMeshTris(PolyMesh pm)
        {
            float cs = pm.CellSize;
            float ch = pm.CellHeight;
            var orig = pm.Bounds.Minimum;

            Dictionary<Color4, List<Vector3>> tris = [];
            foreach (var (p, t, poly, _, area) in pm.IteratePolyTriangles())
            {
                Color4 col;
                if (area == SamplePolyAreas.Ground)
                {
                    col = new Color(0, 192, 255, 128);
                }
                else if (area == SamplePolyAreas.None)
                {
                    col = new Color(0, 0, 0, 128);
                }
                else
                {
                    col = AreaToCol(area, 128);
                }

                tris.TryAdd(col, []);

                for (int k = 0; k < t.Length; ++k)
                {
                    float x = orig.X + t[k].X * cs;
                    float y = orig.Y + (t[k].Y + 1) * ch;
                    float z = orig.Z + t[k].Z * cs;

                    tris[col].Add(new(x, y, z));
                }
            }

            if (tris.Count <= 0)
            {
                yield break;
            }

            var dict = tris.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return ($"{GraphDebugTypes.PolyMesh}_Tris", Topology.TriangleList, dict);
        }
        /// <summary>
        /// Gets the polygon mesh edges debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetPolyEdges(PolyMesh pm)
        {
            float cs = pm.CellSize;
            float ch = pm.CellHeight;
            var orig = pm.Bounds.Minimum;

            List<Vector3> points = [];

            // Draw neighbours edges
            foreach (var (i0, i1, p) in pm.IteratePolySegments())
            {
                if (!p.AdjacencyIsNull(i0))
                {
                    continue;
                }

                int p0 = p.GetVertex(i0);
                int p1 = p.GetVertex(i1);
                int[] vi = [p0, p1];

                for (int k = 0; k < vi.Length; ++k)
                {
                    var v = pm.GetVertex(vi[k]);
                    float x = orig.X + v.X * cs;
                    float y = orig.Y + (v.Y + 1) * ch + 0.1f;
                    float z = orig.Z + v.Z * cs;

                    points.Add(new(x, y, z));
                }
            }

            if (points.Count <= 0)
            {
                yield break;
            }

            Color4 color = new Color(0, 48, 64, 255);

            var dict = new Dictionary<Color4, IEnumerable<Vector3>>
            {
                { color, points }
            };
            yield return ($"{GraphDebugTypes.PolyMesh}_Edges", Topology.LineList, dict);
        }
        /// <summary>
        /// Gets the polygon mesh boundaries debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetPolyBoundaries(PolyMesh pm)
        {
            float cs = pm.CellSize;
            float ch = pm.CellHeight;
            var orig = pm.Bounds.Minimum;

            Dictionary<Color4, List<Vector3>> edges = [];

            // Draw boundary edges
            foreach (var (i0, i1, p) in pm.IteratePolySegments())
            {
                if (p.AdjacencyIsNull(i0))
                {
                    continue;
                }

                Color4 col = new Color(0, 48, 64, 255);
                if (p.IsExternalLink(i0))
                {
                    col = new Color(255, 255, 255, 255);
                }

                edges.TryAdd(col, []);

                int p0 = p.GetVertex(i0);
                int p1 = p.GetVertex(i1);
                int[] vi = [p0, p1];

                for (int k = 0; k < vi.Length; ++k)
                {
                    var v = pm.GetVertex(vi[k]);
                    float x = orig.X + v.X * cs;
                    float y = orig.Y + (v.Y + 1) * ch + 0.1f;
                    float z = orig.Z + v.Z * cs;

                    edges[col].Add(new(x, y, z));
                }
            }

            if (edges.Count <= 0)
            {
                yield break;
            }

            var dict = edges.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return ($"{GraphDebugTypes.PolyMesh}_BEdges", Topology.LineList, dict);
        }

        /// <summary>
        /// Gets the polygon mesh debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private readonly IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetTileCachePolyMesh(TileCachePolyMesh pm, Vector3 orig, float cs, float ch)
        {
            return
            [
                .. GetBounds(),
                .. GetTileCachePolyMeshTris(pm, orig, cs, ch),
                .. GetTileCachePolyEdges(pm, orig, cs, ch),
                .. GetTileCachePolyBoundaries(pm, orig, cs, ch),
            ];
        }
        /// <summary>
        /// Gets the polygon mesh triangles debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetTileCachePolyMeshTris(TileCachePolyMesh pm, Vector3 orig, float cs, float ch)
        {
            Dictionary<Color4, List<Vector3>> tris = [];
            foreach (var (p, t, poly, area) in pm.IteratePolyTriangles())
            {
                Color4 col;
                if (area == SamplePolyAreas.Ground)
                {
                    col = new Color(0, 192, 255, 128);
                }
                else if (area == SamplePolyAreas.None)
                {
                    col = new Color(0, 0, 0, 128);
                }
                else
                {
                    col = AreaToCol(area, 128);
                }

                tris.TryAdd(col, []);

                for (int k = 0; k < t.Length; ++k)
                {
                    float x = orig.X + t[k].X * cs;
                    float y = orig.Y + (t[k].Y + 1) * ch;
                    float z = orig.Z + t[k].Z * cs;

                    tris[col].Add(new(x, y, z));
                }
            }

            if (tris.Count <= 0)
            {
                yield break;
            }

            var dict = tris.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return ($"{GraphDebugTypes.PolyMesh}_Tris", Topology.TriangleList, dict);
        }
        /// <summary>
        /// Gets the polygon mesh edges debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetTileCachePolyEdges(TileCachePolyMesh pm, Vector3 orig, float cs, float ch)
        {
            List<Vector3> points = [];

            // Draw neighbours edges
            foreach (var (i0, i1, p) in pm.IteratePolySegments())
            {
                if (!p.AdjacencyIsNull(i0))
                {
                    continue;
                }

                int p0 = p.GetVertex(i0);
                int p1 = p.GetVertex(i1);
                int[] vi = [p0, p1];

                for (int k = 0; k < vi.Length; ++k)
                {
                    var v = pm.GetVertex(vi[k]);
                    float x = orig.X + v.X * cs;
                    float y = orig.Y + (v.Y + 1) * ch + 0.1f;
                    float z = orig.Z + v.Z * cs;

                    points.Add(new(x, y, z));
                }
            }

            if (points.Count <= 0)
            {
                yield break;
            }

            Color4 color = new Color(0, 48, 64, 255);

            var dict = new Dictionary<Color4, IEnumerable<Vector3>>
            {
                { color, points }
            };
            yield return ($"{GraphDebugTypes.PolyMesh}_Edges", Topology.LineList, dict);
        }
        /// <summary>
        /// Gets the polygon mesh boundaries debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetTileCachePolyBoundaries(TileCachePolyMesh pm, Vector3 orig, float cs, float ch)
        {
            Dictionary<Color4, List<Vector3>> edges = [];

            // Draw boundary edges
            foreach (var (i0, i1, p) in pm.IteratePolySegments())
            {
                if (p.AdjacencyIsNull(i0))
                {
                    continue;
                }

                Color4 col = new Color(0, 48, 64, 255);
                if (p.IsExternalLink(i0))
                {
                    col = new Color(255, 255, 255, 255);
                }

                edges.TryAdd(col, []);

                int p0 = p.GetVertex(i0);
                int p1 = p.GetVertex(i1);
                int[] vi = [p0, p1];

                for (int k = 0; k < vi.Length; ++k)
                {
                    var v = pm.GetVertex(vi[k]);
                    float x = orig.X + v.X * cs;
                    float y = orig.Y + (v.Y + 1) * ch + 0.1f;
                    float z = orig.Z + v.Z * cs;

                    edges[col].Add(new(x, y, z));
                }
            }

            if (edges.Count <= 0)
            {
                yield break;
            }

            var dict = edges.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return ($"{GraphDebugTypes.PolyMesh}_BEdges", Topology.LineList, dict);
        }

        /// <summary>
        /// Gets the polygon detail mesh debug information
        /// </summary>
        /// <param name="dm">Detail mesh</param>
        private readonly IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetDetailMesh(PolyMeshDetail dm)
        {
            if (dm == null)
            {
                return [];
            }

            return
            [
                .. GetBounds(),
                .. GetDetailMeshTris(dm),
                .. GetDetailMeshEdges(dm),
            ];
        }
        /// <summary>
        /// Gets the polygon detail mesh triangles debug information
        /// </summary>
        /// <param name="dm">Detail mesh</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetDetailMeshTris(PolyMeshDetail dm)
        {
            Dictionary<Color4, List<Vector3>> res = [];

            foreach (var (meshIndex, p0, p1, p2) in dm.IterateMeshTriangles())
            {
                Color4 color = Helper.IntToCol(meshIndex, 192);

                res.TryAdd(color, []);
                res[color].AddRange([p0, p1, p2]);
            }

            if (res.Count <= 0)
            {
                yield break;
            }

            var dict = res.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return ($"{GraphDebugTypes.DetailMesh}_Tris", Topology.TriangleList, dict);
        }
        /// <summary>
        /// Gets the polygon detail mesh edges debug information
        /// </summary>
        /// <param name="dm">Detail mesh</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetDetailMeshEdges(PolyMeshDetail dm)
        {
            Vector3 delta = Vector3.Up * 0.001f;

            Dictionary<Color4, List<Vector3>> res = [];

            foreach (var (a, b, flag, isInternal) in dm.IterateMeshEdges())
            {
                Color4 color;

                if (flag == DetailTriEdgeFlagTypes.Boundary)
                {
                    // Ext edge
                    color = new Color(128, 128, 128, 220);
                }
                else
                {
                    if (isInternal)
                    {
                        continue;
                    }

                    // Internal edge
                    color = new Color(0, 0, 0, 64);
                }

                res.TryAdd(color, []);
                res[color].AddRange([a + delta, b + delta]);
            }

            if (res.Count <= 0)
            {
                yield break;
            }

            var dict = res.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return ($"{GraphDebugTypes.DetailMesh}_Edges", Topology.LineList, dict);
        }

        /// <summary>
        /// Gets the graph bounds
        /// </summary>
        private readonly IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetBounds()
        {
            var bounds = graph.Bounds;

            var points = Line3D.CreateBox(bounds).SelectMany(a => new[] { a.Point1, a.Point2 });

            Color4 color = new Color(255, 255, 255, 32);

            var dict = new Dictionary<Color4, IEnumerable<Vector3>>
            {
                { color, points }
            };
            yield return ($"{GetBounds}", Topology.LineList, dict);
        }

        /// <summary>
        /// Triangulates a box from a bounding box
        /// </summary>
        /// <param name="min">Box minimum point</param>
        /// <param name="max">Box maximum point</param>
        /// <param name="scale">Scale</param>
        private static IEnumerable<Triangle> TriangulateBox(Vector3 min, Vector3 max, float scale = 1f)
        {
            var extents = (max - min) * 0.5f * (1f - scale);
            min += extents;
            max -= extents;

            Vector3[] boxVertices =
            [
                new(min.X, min.Y, min.Z),
                new(max.X, min.Y, min.Z),
                new(max.X, min.Y, max.Z),
                new(min.X, min.Y, max.Z),
                new(min.X, max.Y, min.Z),
                new(max.X, max.Y, min.Z),
                new(max.X, max.Y, max.Z),
                new(min.X, max.Y, max.Z),
            ];

            uint[] boxIndices =
            [
                7, 6, 5, 4,
                0, 1, 2, 3,
                1, 5, 6, 2,
                3, 7, 4, 0,
                2, 6, 7, 3,
                0, 4, 5, 1,
            ];

            int vIndex = 0;
            for (int i = 0; i < 6; i++)
            {
                //Quad
                var v0 = boxVertices[boxIndices[vIndex++]];
                var v1 = boxVertices[boxIndices[vIndex++]];
                var v2 = boxVertices[boxIndices[vIndex++]];
                var v3 = boxVertices[boxIndices[vIndex++]];

                //Triangles
                yield return new(v0, v1, v2);
                yield return new(v0, v2, v3);
            }
        }
        /// <summary>
        /// Converts the area value to color
        /// </summary>
        /// <param name="area">Area value</param>
        private static Color4 AreaToCol(SamplePolyAreas area, int alpha)
        {
            if (area == SamplePolyAreas.None)
            {
                // Treat zero area type as default.
                return new Color(0, 192, 255, 255);
            }
            else
            {
                return Helper.IntToCol((int)area, alpha);
            }
        }
    }
}
