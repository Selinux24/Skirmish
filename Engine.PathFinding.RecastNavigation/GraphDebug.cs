using Engine.PathFinding.RecastNavigation.Detour;
using Engine.PathFinding.RecastNavigation.Detour.Tiles;
using Engine.PathFinding.RecastNavigation.Recast;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
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
                NavMesh.GetTileAtPosition(point, graph.Settings.TileCellSize, graph.Bounds, out var tx, out var ty);

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
                GraphDebugTypes.TileCacheLayersAreas => GetTileCacheLayer(buildData.TileCacheLayer, buildData.CellSize, buildData.CellHeight, true, false),
                GraphDebugTypes.TileCacheLayersRegions => GetTileCacheLayer(buildData.TileCacheLayer, buildData.CellSize, buildData.CellHeight, false, true),
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
        private readonly IEnumerable<GraphDebugDataCollection> GetNodes(bool separateNodes, bool showTriangles)
        {
            var nodes = graph.GetNodes(Agent).OfType<GraphNode>();
            if (!nodes.Any())
            {
                return [];
            }

            Color4 color = new Color(255, 255, 255, 32);

            if (showTriangles)
            {
                return
                [
                    .. GetBounds(graph.Bounds, color),
                    .. GetNodeTris(nodes, separateNodes),
                    .. GetNodeLines(nodes, separateNodes),
                ];
            }
            else
            {
                return
                [
                    .. GetBounds(graph.Bounds, color),
                    .. GetNodeTris(nodes, separateNodes),
                ];
            }
        }
        /// <summary>
        /// Gets the node triangles debug information
        /// </summary>
        private static IEnumerable<GraphDebugDataCollection> GetNodeTris(IEnumerable<GraphNode> nodes, bool separateNodes)
        {
            string nameTris = $"{GraphDebugTypes.Nodes}_Tris";

            if (separateNodes)
            {
                var tris = nodes
                    .GroupBy(n => Helper.IntToCol(n.Id, 128))
                    .ToDictionary(
                        keySelector => keySelector.Key,
                        elementSelector => elementSelector.SelectMany(gn => gn.Triangles.SelectMany(t => t.GetVertices())).AsEnumerable());

                yield return new(nameTris, Topology.TriangleList, tris);
            }
            else
            {
                Color4 colorTris = new Color(0, 192, 255, 128);
                Dictionary<Color4, IEnumerable<Vector3>> tris = new([new(colorTris, nodes.SelectMany(n => n.Triangles.SelectMany(t => t.GetVertices())).AsEnumerable())]);

                yield return new(nameTris, Topology.TriangleList, tris);
            }
        }
        /// <summary>
        /// Gets the node lines debug information
        /// </summary>
        private static IEnumerable<GraphDebugDataCollection> GetNodeLines(IEnumerable<GraphNode> nodes, bool separateNodes)
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

                yield return new(nameLines, Topology.LineList, lines);
            }
            else
            {
                Color4 colorLines = new Color(0, 192, 255, 128);
                Dictionary<Color4, IEnumerable<Vector3>> lines = new([new(colorLines, nodes.SelectMany(n => n.Triangles.SelectMany(t => t.GetEdgeSegments().SelectMany(s => new Vector3[] { s.Point1 + deltaY, s.Point2 + deltaY }))).AsEnumerable())]);

                yield return new(nameLines, Topology.LineList, lines);
            }
        }

        /// <summary>
        /// Gets the height field debug information
        /// </summary>
        private static List<GraphDebugDataCollection> GetHeightfield(Heightfield hf, bool walkable)
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
                return [new(name, Topology.TriangleList, data)];
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
                return [new(name, Topology.TriangleList, data)];
            }
        }

        /// <summary>
        /// Gets the contour debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        /// <param name="drawRaw">Draw raw contour set</param>
        private readonly List<GraphDebugDataCollection> GetContours(ContourSet cset, bool drawRaw)
        {
            if (cset == null)
            {
                return [];
            }

            var orig = cset.Bounds.Minimum;
            float cs = cset.CellSize;
            float ch = cset.CellHeight;

            Color4 color = new Color(255, 255, 255, 32);

            List<GraphDebugDataCollection> res =
            [
                .. GetBounds(graph.Bounds, color),
                .. GetContoursRegions(cset, orig, cs, ch),
            ];

            if (drawRaw)
            {
                res.AddRange(GetContourRawLines(cset, orig, cs, ch, 0.75f));
            }
            else
            {
                res.AddRange(GetContourBorders(cset, orig, cs, ch, 0.75f));
            }

            return res;
        }
        /// <summary>
        /// Gets the contour lines debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        private static IEnumerable<GraphDebugDataCollection> GetContourRawLines(ContourSet cset, Vector3 orig, float cs, float ch, float alpha)
        {
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
            yield return new($"{GraphDebugTypes.Contours}_RawLines", Topology.LineList, dict);
        }
        /// <summary>
        /// Gets the contour borders debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        private static IEnumerable<GraphDebugDataCollection> GetContourBorders(ContourSet cset, Vector3 orig, float cs, float ch, float alpha)
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
                    var col = Contour.IsAreaBorder(va.Flag) ? regColor : bColor;
                    var cva = ReadContourVertex(va, orig, cs, ch);
                    var cvb = ReadContourVertex(vb, orig, cs, ch);

                    lines.TryAdd(col, []);
                    lines[col].AddRange([cva, cvb]);

                    empty = false;
                }
            }

            if (empty)
            {
                yield break;
            }

            var dict = lines.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return new($"{GraphDebugTypes.Contours}_Borders", Topology.LineList, dict);
        }
        /// <summary>
        /// Gets the contour regions debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        private static IEnumerable<GraphDebugDataCollection> GetContoursRegions(ContourSet cset, Vector3 orig, float cs, float ch)
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
            yield return new($"{GraphDebugTypes.Contours}_RegionConnections", Topology.LineList, dict);
        }

        /// <summary>
        /// Gets the polygon mesh debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private readonly IEnumerable<GraphDebugDataCollection> GetPolyMesh(PolyMesh pm)
        {
            if (pm == null)
            {
                return [];
            }

            Color4 color = new Color(255, 255, 255, 32);
            float cs = pm.CellSize;
            float ch = pm.CellHeight;
            var orig = pm.Bounds.Minimum;

            return
                     [
                .. GetBounds(graph.Bounds, color),
                .. GetPolyMeshTris(pm, orig, cs, ch),
                .. GetPolyEdges(pm, orig, cs, ch),
                .. GetPolyBoundaries(pm, orig, cs, ch),
            ];
        }
        /// <summary>
        /// Gets the polygon mesh triangles debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<GraphDebugDataCollection> GetPolyMeshTris(PolyMesh pm, Vector3 orig, float cs, float ch)
        {
            Dictionary<Color4, List<Vector3>> tris = [];
            foreach (var (_, t, _, _, area) in pm.IteratePolyTriangles())
            {
                foreach (var (col, v) in ReadTris(t, area, orig, cs, ch))
                {
                    tris.TryAdd(col, []);

                    tris[col].Add(v);
                }
            }

            if (tris.Count <= 0)
            {
                yield break;
            }

            var dict = tris.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return new($"{GraphDebugTypes.PolyMesh}_Tris", Topology.TriangleList, dict);
        }
        /// <summary>
        /// Gets the polygon mesh edges debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<GraphDebugDataCollection> GetPolyEdges(PolyMesh pm, Vector3 orig, float cs, float ch)
        {
            List<Vector3> points = [];

            var pmVerts = pm.GetVertices();

            // Draw neighbours edges
            foreach (var (i0, i1, p) in pm.IteratePolySegments())
            {
                if (!p.AdjacencyIsNull(i0))
                {
                    continue;
                }

                int p0 = p.GetVertex(i0);
                int p1 = p.GetVertex(i1);

                var segVerts = ReadSegment(p0, p1, pmVerts, orig, cs, ch);

                points.AddRange(segVerts);
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
            yield return new($"{GraphDebugTypes.PolyMesh}_Edges", Topology.LineList, dict);
        }
        /// <summary>
        /// Gets the polygon mesh boundaries debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<GraphDebugDataCollection> GetPolyBoundaries(PolyMesh pm, Vector3 orig, float cs, float ch)
        {
            Dictionary<Color4, List<Vector3>> edges = [];

            var pmVerts = pm.GetVertices();

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

                var segVerts = ReadSegment(p0, p1, pmVerts, orig, cs, ch);

                edges[col].AddRange(segVerts);
            }

            if (edges.Count <= 0)
            {
                yield break;
            }

            var dict = edges.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return new($"{GraphDebugTypes.PolyMesh}_BEdges", Topology.LineList, dict);
        }

        /// <summary>
        /// Gets the polygon detail mesh debug information
        /// </summary>
        /// <param name="dm">Detail mesh</param>
        private readonly IEnumerable<GraphDebugDataCollection> GetDetailMesh(PolyMeshDetail dm)
        {
            if (dm == null)
            {
                return [];
            }

            Color4 color = new Color(255, 255, 255, 32);

            return
            [
                .. GetBounds(graph.Bounds, color),
                .. GetDetailMeshTris(dm),
                .. GetDetailMeshEdges(dm),
            ];
        }
        /// <summary>
        /// Gets the polygon detail mesh triangles debug information
        /// </summary>
        /// <param name="dm">Detail mesh</param>
        private static IEnumerable<GraphDebugDataCollection> GetDetailMeshTris(PolyMeshDetail dm)
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
            yield return new($"{GraphDebugTypes.DetailMesh}_Tris", Topology.TriangleList, dict);
        }
        /// <summary>
        /// Gets the polygon detail mesh edges debug information
        /// </summary>
        /// <param name="dm">Detail mesh</param>
        private static IEnumerable<GraphDebugDataCollection> GetDetailMeshEdges(PolyMeshDetail dm)
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
            yield return new($"{GraphDebugTypes.DetailMesh}_Edges", Topology.LineList, dict);
        }

        /// <summary>
        /// Gets the tile cache layer debug information
        /// </summary>
        /// <param name="layer">Tile cache layer</param>
        /// <param name="cs">Cell size</param>
        /// <param name="ch">Cell height</param>
        /// <param name="dAreas">Draw areas</param>
        /// <param name="dRegions">Draw regions</param>
        private readonly List<GraphDebugDataCollection> GetTileCacheLayer(TileCacheLayer layer, float cs, float ch, bool dAreas, bool dRegions)
        {
            Color4 color = new Color(255, 255, 255, 32);

            List<GraphDebugDataCollection> res =
            [
                .. GetBounds(graph.Bounds, color),
                .. GetTileCacheLayerBounds(layer, cs),
                .. GetTileCacheLayerPortals(layer, cs, ch),
            ];

            if (dAreas)
            {
                res.AddRange(GetTileCacheLayerAreas(layer, cs, ch));
            }

            if (dRegions)
            {
                res.AddRange(GetTileCacheLayerRegions(layer, cs, ch));
            }

            return res;
        }
        /// <summary>
        /// Gets the tile cache layer bounds
        /// </summary>
        /// <param name="layer">Tile cache layer</param>
        /// <param name="cs">Cell size</param>
        private static IEnumerable<GraphDebugDataCollection> GetTileCacheLayerBounds(TileCacheLayer layer, float cs)
        {
            int idx = layer.Header.TLayer;

            Color color = (Color)Helper.IntToCol(idx + 1, 128);

            var bmin = layer.Header.Bounds.Minimum;
            var bmax = layer.Header.Bounds.Maximum;

            // Layer bounds
            Vector3 lbmin = new();
            Vector3 lbmax = new();
            lbmin.X = bmin.X + layer.Header.MinX * cs;
            lbmin.Y = bmin.Y;
            lbmin.Z = bmin.Z + layer.Header.MinY * cs;
            lbmax.X = bmin.X + (layer.Header.MaxX + 1) * cs;
            lbmax.Y = bmax.Y;
            lbmax.Z = bmin.Z + (layer.Header.MaxY + 1) * cs;

            return GetBounds(new BoundingBox(lbmin, lbmax), color);
        }
        /// <summary>
        /// Gets the tile cache layer areas
        /// </summary>
        /// <param name="layer">Tile cache layer</param>
        /// <param name="cs">Cell size</param>
        /// <param name="ch">Cell height</param>
        private static IEnumerable<GraphDebugDataCollection> GetTileCacheLayerAreas(TileCacheLayer layer, float cs, float ch)
        {
            Dictionary<Color4, List<Vector3>> res = [];

            var bmin = layer.Header.Bounds.Minimum;

            int idx = layer.Header.TLayer;
            Color color = (Color)Helper.IntToCol(idx + 1, 255);

            foreach (var (lidx, x, y) in layer.IterateLayer())
            {
                int lh = layer.GetHeight(lidx);
                if (lh == 0xff) continue;

                var area = layer.GetArea(lidx);
                Color col;
                if (area == AreaTypes.RC_WALKABLE_AREA)
                {
                    col = Color.Lerp(color, new Color(0, 192, 255, 64), 32);
                }
                else if (area == AreaTypes.RC_NULL_AREA)
                {
                    col = Color.Lerp(color, new Color(0, 0, 0, 64), 32);
                }
                else
                {
                    col = Color.Lerp(color, (Color)AreaToCol(area), 32);
                }

                float fx = bmin.X + x * cs;
                float fy = bmin.Y + (lh + 1) * ch;
                float fz = bmin.Z + y * cs;

                Vector3 v0 = new(fx, fy, fz);
                Vector3 v1 = new(fx, fy, fz + cs);
                Vector3 v2 = new(fx + cs, fy, fz + cs);
                Vector3 v3 = new(fx + cs, fy, fz);

                res.TryAdd(col, []);
                res[col].AddRange([v0, v1, v2]);
                res[col].AddRange([v0, v2, v3]);
            }

            if (res.Count <= 0)
            {
                yield break;
            }

            var dict = res.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return new($"{GraphDebugTypes.TileCacheLayersAreas}", Topology.TriangleList, dict);
        }
        /// <summary>
        /// Gets the tile cache layer regions
        /// </summary>
        /// <param name="layer">Tile cache layer</param>
        /// <param name="cs">Cell size</param>
        /// <param name="ch">Cell height</param>
        private static IEnumerable<GraphDebugDataCollection> GetTileCacheLayerRegions(TileCacheLayer layer, float cs, float ch)
        {
            Dictionary<Color4, List<Vector3>> res = [];

            var bmin = layer.Header.Bounds.Minimum;

            int idx = layer.Header.TLayer;
            Color color = (Color)Helper.IntToCol(idx + 1, 255);

            foreach (var (lidx, x, y) in layer.IterateLayer())
            {
                int lh = layer.GetHeight(lidx);
                if (lh == 0xff) continue;

                int reg = layer.GetRegion(lidx);

                Color col = Color.Lerp(color, (Color)Helper.IntToCol(reg, 255), 192);

                float fx = bmin.X + x * cs;
                float fy = bmin.Y + (lh + 1) * ch;
                float fz = bmin.Z + y * cs;

                Vector3 v0 = new(fx, fy, fz);
                Vector3 v1 = new(fx, fy, fz + cs);
                Vector3 v2 = new(fx + cs, fy, fz + cs);
                Vector3 v3 = new(fx + cs, fy, fz);

                res.TryAdd(col, []);
                res[col].AddRange([v0, v1, v2]);
                res[col].AddRange([v0, v2, v3]);
            }

            if (res.Count <= 0)
            {
                yield break;
            }

            var dict = res.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return new($"{GraphDebugTypes.TileCacheLayersRegions}", Topology.TriangleList, dict);
        }
        /// <summary>
        /// Gets the tile cache layer portals
        /// </summary>
        /// <param name="layer">Tile cache layer</param>
        /// <param name="cs">Cell size</param>
        /// <param name="ch">Cell height</param>
        private static IEnumerable<GraphDebugDataCollection> GetTileCacheLayerPortals(TileCacheLayer layer, float cs, float ch)
        {
            Dictionary<Color4, List<Vector3>> res = [];

            var bmin = layer.Header.Bounds.Minimum;

            // Portals
            Color pcol = new(255, 255, 255, 255);

            int[][] segs = [[0, 0, 0, 1], [0, 1, 1, 1], [1, 1, 1, 0], [1, 0, 0, 0]];

            foreach (var (lidx, x, y) in layer.IterateLayer())
            {
                int lh = layer.GetHeight(lidx);
                if (lh == 0xff) continue;

                int con = layer.GetConnection(lidx);

                for (int dir = 0; dir < 4; ++dir)
                {
                    if ((con & (1 << (dir + 4))) == 0)
                    {
                        continue;
                    }

                    int[] seg = segs[dir];

                    float ax = bmin.X + (x + seg[0]) * cs;
                    float ay = bmin.Y + (lh + 2) * ch;
                    float az = bmin.Z + (y + seg[1]) * cs;

                    float bx = bmin.X + (x + seg[2]) * cs;
                    float by = bmin.Y + (lh + 2) * ch;
                    float bz = bmin.Z + (y + seg[3]) * cs;

                    res.TryAdd(pcol, []);
                    res[pcol].Add(new(ax, ay, az));
                    res[pcol].Add(new(bx, by, bz));
                }
            }

            if (res.Count <= 0)
            {
                yield break;
            }

            var dict = res.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return new($"{nameof(GetTileCacheLayerPortals)}", Topology.LineList, dict);
        }

        /// <summary>
        /// Gets the contour debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        /// <param name="drawRaw">Draw raw contour set</param>
        private readonly List<GraphDebugDataCollection> GetTileCacheContours(TileCacheContourSet cset, Vector3 orig, float cs, float ch)
        {
            Color4 color = new Color(255, 255, 255, 32);

            return
            [
                .. GetBounds(graph.Bounds, color),
                .. GetTileCacheContoursRegions(cset, orig, cs, ch),
                .. GetTileCacheContourBorders(cset, orig, cs, ch, 0.75f),
            ];
        }
        /// <summary>
        /// Gets the contour borders debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        private static IEnumerable<GraphDebugDataCollection> GetTileCacheContourBorders(TileCacheContourSet cset, Vector3 orig, float cs, float ch, float alpha)
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
                    var col = Contour.IsAreaBorder(va.Flag) ? regColor : bColor;
                    var cva = ReadContourVertex(va, orig, cs, ch);
                    var cvb = ReadContourVertex(vb, orig, cs, ch);

                    lines.TryAdd(col, []);
                    lines[col].AddRange([cva, cvb]);

                    empty = false;
                }
            }

            if (empty)
            {
                yield break;
            }

            var dict = lines.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return new($"{GraphDebugTypes.Contours}_Borders", Topology.LineList, dict);
        }
        /// <summary>
        /// Gets the contour regions debug information
        /// </summary>
        /// <param name="cset">Contour set</param>
        private static IEnumerable<GraphDebugDataCollection> GetTileCacheContoursRegions(TileCacheContourSet cset, Vector3 orig, float cs, float ch)
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
            yield return new($"{GraphDebugTypes.Contours}_RegionConnections", Topology.LineList, dict);
        }

        /// <summary>
        /// Gets the polygon mesh debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private readonly IEnumerable<GraphDebugDataCollection> GetTileCachePolyMesh(TileCachePolyMesh pm, Vector3 orig, float cs, float ch)
        {
            Color4 color = new Color(255, 255, 255, 32);

            return
            [
                .. GetBounds(graph.Bounds, color),
                .. GetTileCachePolyMeshTris(pm, orig, cs, ch),
                .. GetTileCachePolyEdges(pm, orig, cs, ch),
                .. GetTileCachePolyBoundaries(pm, orig, cs, ch),
            ];
        }
        /// <summary>
        /// Gets the polygon mesh triangles debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<GraphDebugDataCollection> GetTileCachePolyMeshTris(TileCachePolyMesh pm, Vector3 orig, float cs, float ch)
        {
            Dictionary<Color4, List<Vector3>> tris = [];
            foreach (var (_, t, _, area) in pm.IteratePolyTriangles())
            {
                foreach (var (col, v) in ReadTris(t, area, orig, cs, ch))
                {
                    tris.TryAdd(col, []);

                    tris[col].Add(v);
                }
            }

            if (tris.Count <= 0)
            {
                yield break;
            }

            var dict = tris.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return new($"{GraphDebugTypes.PolyMesh}_Tris", Topology.TriangleList, dict);
        }
        /// <summary>
        /// Gets the polygon mesh edges debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<GraphDebugDataCollection> GetTileCachePolyEdges(TileCachePolyMesh pm, Vector3 orig, float cs, float ch)
        {
            List<Vector3> points = [];

            var pmVerts = pm.GetVertices();

            // Draw neighbours edges
            foreach (var (i0, i1, p) in pm.IteratePolySegments())
            {
                if (!p.AdjacencyIsNull(i0))
                {
                    continue;
                }

                int p0 = p.GetVertex(i0);
                int p1 = p.GetVertex(i1);

                var segVerts = ReadSegment(p0, p1, pmVerts, orig, cs, ch);

                points.AddRange(segVerts);
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
            yield return new($"{GraphDebugTypes.PolyMesh}_Edges", Topology.LineList, dict);
        }
        /// <summary>
        /// Gets the polygon mesh boundaries debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<GraphDebugDataCollection> GetTileCachePolyBoundaries(TileCachePolyMesh pm, Vector3 orig, float cs, float ch)
        {
            Dictionary<Color4, List<Vector3>> edges = [];

            var pmVerts = pm.GetVertices();

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

                var segVerts = ReadSegment(p0, p1, pmVerts, orig, cs, ch);

                edges[col].AddRange(segVerts);
            }

            if (edges.Count <= 0)
            {
                yield break;
            }

            var dict = edges.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return new($"{GraphDebugTypes.PolyMesh}_BEdges", Topology.LineList, dict);
        }

        /// <summary>
        /// Gets the graph bounds
        /// </summary>
        private static IEnumerable<GraphDebugDataCollection> GetBounds(BoundingBox bounds, Color4 color)
        {
            var points = Line3D.CreateBox(bounds).SelectMany(a => new[] { a.Point1, a.Point2 });

            var dict = new Dictionary<Color4, IEnumerable<Vector3>>
            {
                { color, points }
            };
            yield return new($"{GetBounds}", Topology.LineList, dict);
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
        /// Reads a vertex from a contour vertex
        /// </summary>
        /// <param name="v">Contour vertex</param>
        /// <param name="orig">Origin</param>
        /// <param name="cs">Cell size</param>
        /// <param name="ch">Cell height</param>
        private static Vector3 ReadContourVertex(ContourVertex v, Vector3 orig, float cs, float ch)
        {
            float fx = orig.X + v.X * cs;
            float fy = orig.Y + v.Y * ch;
            float fz = orig.Z + v.Z * cs;

            return new(fx, fy, fz);
        }
        /// <summary>
        /// Reads a list of vertices from a indexed triangle definition
        /// </summary>
        /// <param name="t">Indexed triangle list</param>
        /// <param name="area">Area</param>
        /// <param name="orig">Origin</param>
        /// <param name="cs">Cell size</param>
        /// <param name="ch">Cell height</param>
        private static IEnumerable<(Color4, Vector3)> ReadTris(Int3[] t, SamplePolyAreas area, Vector3 orig, float cs, float ch)
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

            for (int k = 0; k < t.Length; ++k)
            {
                float x = orig.X + t[k].X * cs;
                float y = orig.Y + (t[k].Y + 1) * ch;
                float z = orig.Z + t[k].Z * cs;

                yield return (col, new(x, y, z));
            }
        }
        /// <summary>
        /// Reads a list of vertices from a indexed segment definition
        /// </summary>
        /// <param name="p0">First segment index</param>
        /// <param name="p1">Second segment index</param>
        /// <param name="verts">Real vertices</param>
        /// <param name="orig">Origin</param>
        /// <param name="cs">Cell size</param>
        /// <param name="ch">Cell height</param>
        private static IEnumerable<Vector3> ReadSegment(int p0, int p1, Int3[] verts, Vector3 orig, float cs, float ch)
        {
            int[] vi = [p0, p1];

            for (int k = 0; k < vi.Length; ++k)
            {
                var v = verts[vi[k]];
                float x = orig.X + v.X * cs;
                float y = orig.Y + (v.Y + 1) * ch + 0.1f;
                float z = orig.Z + v.Z * cs;

                yield return new(x, y, z);
            }
        }

        /// <summary>
        /// Converts the area value to color
        /// </summary>
        /// <param name="area">Area value</param>
        /// <param name="alpha">Alpha value</param>
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
        /// <summary>
        /// Converts the area value to color
        /// </summary>
        /// <param name="area">Area value</param>
        /// <returns></returns>
        private static Color4 AreaToCol(AreaTypes area)
        {
            if (area == AreaTypes.RC_NULL_AREA)
            {
                // Treat zero area type as default.
                return new Color(0, 192, 255, 255);
            }
            else
            {
                return Helper.IntToCol((int)area, 255);
            }
        }
    }
}
