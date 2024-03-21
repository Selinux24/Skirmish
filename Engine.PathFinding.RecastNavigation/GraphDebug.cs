using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;
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

            var debug = (GraphDebugTypes)id;

            if (graph.Settings.BuildMode == BuildModes.Solo)
            {
                var buildData = nm.GetBuildData(0, 0);

                var data = debug switch
                {
                    GraphDebugTypes.NavMesh => GetNodes(false, false),
                    GraphDebugTypes.Nodes => GetNodes(true, false),
                    GraphDebugTypes.NodesWithLinks => GetNodes(true, true),
                    GraphDebugTypes.Heightfield => GetHeightfield(buildData.Heightfield, false),
                    GraphDebugTypes.WalkableHeightfield => GetHeightfield(buildData.Heightfield, true),
                    GraphDebugTypes.PolyMesh => GetPolyMesh(buildData.PolyMesh),
                    GraphDebugTypes.DetailMesh => GetDetailMesh(buildData.PolyMeshDetail),
                    _ => []
                };
                return new GraphDebugData(data);
            }
            else if (graph.Settings.BuildMode == BuildModes.Tiled)
            {
                NavMesh.GetTileAtPosition(point, graph.Input, graph.Settings, out var tx, out var ty, out _);

                var buildData = nm.GetBuildData(tx, ty);

                var data = debug switch
                {
                    GraphDebugTypes.NavMesh => GetNodes(false, false),
                    GraphDebugTypes.Nodes => GetNodes(true, false),
                    GraphDebugTypes.NodesWithLinks => GetNodes(true, true),
                    GraphDebugTypes.Heightfield => GetHeightfield(buildData.Heightfield, false),
                    GraphDebugTypes.WalkableHeightfield => GetHeightfield(buildData.Heightfield, true),
                    GraphDebugTypes.PolyMesh => GetPolyMesh(buildData.PolyMesh),
                    GraphDebugTypes.DetailMesh => GetDetailMesh(buildData.PolyMeshDetail),
                    _ => []
                };
                return new GraphDebugData(data);
            }

            return new GraphDebugData([]);
        }

        /// <summary>
        /// Gets the nodes debug information
        /// </summary>
        private readonly IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetNodes(bool separateNodes, bool showTriangles)
        {
            var nodes = graph.GetNodes(Agent).OfType<GraphNode>();
            if (!nodes.Any())
            {
                yield break;
            }

            const string nameTris = $"{nameof(GetNodes)}_TRIS";
            const string nameLines = $"{nameof(GetNodes)}_LINES";
            Vector3 deltaY = new(0f, 0.01f, 0f);

            if (separateNodes)
            {
                var tris = nodes
                    .GroupBy(n => Helper.IntToCol(n.Id, 128))
                    .ToDictionary(
                        keySelector => keySelector.Key,
                        elementSelector => elementSelector.SelectMany(gn => gn.Triangles.SelectMany(t => t.GetVertices())).AsEnumerable());

                yield return (nameTris, Topology.TriangleList, tris);

                if (!showTriangles)
                {
                    yield break;
                }

                var lines = nodes
                    .GroupBy(n => Helper.IntToCol(n.Id, 128))
                    .ToDictionary(
                        keySelector => keySelector.Key,
                        elementSelector => elementSelector.SelectMany(gn => gn.Triangles.SelectMany(t => t.GetEdgeSegments().SelectMany(s => new Vector3[] { s.Point1 + deltaY, s.Point2 + deltaY }))).AsEnumerable());

                yield return (nameLines, Topology.LineList, lines);
            }
            else
            {
                Color4 colorTris = new Color(0, 192, 255, 255);
                Dictionary<Color4, IEnumerable<Vector3>> tris = new([new(colorTris, nodes.SelectMany(n => n.Triangles.SelectMany(t => t.GetVertices())).AsEnumerable())]);

                yield return (nameTris, Topology.TriangleList, tris);

                if (!showTriangles)
                {
                    yield break;
                }

                Color4 colorLines = new Color(255, 255, 255, 255);
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

            var orig = hf.BoundingBox.Minimum;
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
                    var min = new Vector3(fx, orig.Y + s.SMin * ch, fz);
                    var max = new Vector3(fx + cs, orig.Y + s.SMax * ch, fz + cs);

                    var boxTris = TriangulateBox(min, max);

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
        /// Gets the polygon mesh debug information
        /// </summary>
        /// <param name="pm">Polygon mesh</param>
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetPolyMesh(PolyMesh pm)
        {
            if (pm == null)
            {
                return [];
            }

            return
            [
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
            foreach (var (p, poly, i0, i1, i2) in pm.IteratePolyTriangles())
            {
                var area = pm.Areas[p];

                Color4 col;
                if (area == SamplePolyAreas.Ground)
                {
                    col = new Color(0, 192, 255, 16);
                }
                else if (area == SamplePolyAreas.None)
                {
                    col = new Color(0, 0, 0, 16);
                }
                else
                {
                    col = AreaToCol(area);
                }

                tris.TryAdd(col, []);

                int p0 = poly.GetVertex(i0);
                int p1 = poly.GetVertex(i1);
                int p2 = poly.GetVertex(i2);
                int[] vi = [p0, p1, p2];

                for (int k = 0; k < vi.Length; ++k)
                {
                    var v = pm.Verts[vi[k]];

                    float x = orig.X + v.X * cs;
                    float y = orig.Y + (v.Y + 1) * ch;
                    float z = orig.Z + v.Z * cs;

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

            // Draw neighbours edges
            Color4 col = new Color(0, 48, 64, 255);
            Dictionary<Color4, List<Vector3>> edges = [];
            edges.Add(col, []);
            foreach (var (p, i0, i1) in pm.IteratePolySegments())
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
                    var v = pm.Verts[vi[k]];
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

            // Draw boundary edges
            Dictionary<Color4, List<Vector3>> edges = [];
            foreach (var (p, i0, i1) in pm.IteratePolySegments())
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
                    var v = pm.Verts[vi[k]];
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
        private static IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetDetailMesh(PolyMeshDetail dm)
        {
            if (dm == null)
            {
                return [];
            }

            return
            [
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
            if (dm == null)
            {
                yield break;
            }

            Dictionary<Color4, List<Vector3>> res = [];

            foreach (var (meshIndex, p0, p1, p2) in dm.IterateMeshTriangleVertices())
            {
                Color4 color = Helper.IntToCol(meshIndex, 192);

                res.TryAdd(color, []);

                res[color].Add(p0);
                res[color].Add(p1);
                res[color].Add(p2);
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
            Dictionary<Color4, List<Vector3>> res = [];
            Vector3 delta = Vector3.Up * 0.001f;

            for (int i = 0; i < dm.Meshes.Count; i++)
            {
                var m = dm.Meshes[i];
                int bverts = m.VertBase;
                int btris = m.TriBase;
                int ntris = m.TriCount;
                var verts = dm.Vertices.Skip(bverts).ToArray();
                var tris = dm.Triangles.Skip(btris).ToArray();

                for (int j = 0; j < ntris; ++j)
                {
                    var t = tris[j];

                    for (int k = 0, kp = 2; k < 3; kp = k++)
                    {
                        Color4 color;

                        var ef = t.GetDetailTriEdgeFlags(kp);
                        if (ef == DetailTriEdgeFlagTypes.Boundary)
                        {
                            // Ext edge
                            color = new Color(128, 128, 128, 220);
                        }
                        else
                        {
                            if (t[kp] >= t[k])
                            {
                                continue;
                            }

                            // Internal edge
                            color = new Color(0, 0, 0, 64);
                        }

                        res.TryAdd(color, []);
                        res[color].Add(verts[t[kp]] + delta);
                        res[color].Add(verts[t[k]] + delta);
                    }
                }
            }

            if (res.Count <= 0)
            {
                yield break;
            }

            var dict = res.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
            yield return ($"{GraphDebugTypes.DetailMesh}_Edges", Topology.LineList, dict);
        }

        /// <summary>
        /// Triangulates a box from a bounding box
        /// </summary>
        private static IEnumerable<Triangle> TriangulateBox(Vector3 min, Vector3 max)
        {
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
        private static Color4 AreaToCol(SamplePolyAreas area)
        {
            if (area == SamplePolyAreas.None)
            {
                // Treat zero area type as default.
                return new Color(0, 192, 255, 255);
            }
            else
            {
                return Helper.IntToCol((int)area, 16);
            }
        }
    }
}
