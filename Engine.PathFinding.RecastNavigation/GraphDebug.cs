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
        public readonly Dictionary<Color4, IEnumerable<Triangle>> GetInfo(int id, Vector3 point)
        {
            var nm = graph.CreateAgentQuery(agent)?.GetAttachedNavMesh();
            if (nm == null)
            {
                return [];
            }

            var debug = (GraphDebugTypes)id;

            if (graph.Settings.BuildMode == BuildModes.Solo)
            {
                var buildData = nm.GetBuildData(0, 0);

                return debug switch
                {
                    GraphDebugTypes.NavMesh => GetNodes(false),
                    GraphDebugTypes.Nodes => GetNodes(true),
                    GraphDebugTypes.Heightfield => GetHeightfield(buildData.Heightfield, false),
                    GraphDebugTypes.WalkableHeightfield => GetHeightfield(buildData.Heightfield, true),
                    _ => []
                };
            }
            else if (graph.Settings.BuildMode == BuildModes.Tiled)
            {
                NavMesh.GetTileAtPosition(point, graph.Input, graph.Settings, out var tx, out var ty, out _);

                var buildData = nm.GetBuildData(tx, ty);

                return debug switch
                {
                    GraphDebugTypes.NavMesh => GetNodes(false),
                    GraphDebugTypes.Nodes => GetNodes(true),
                    GraphDebugTypes.Heightfield => GetHeightfield(buildData.Heightfield, false),
                    GraphDebugTypes.WalkableHeightfield => GetHeightfield(buildData.Heightfield, true),
                    _ => []
                };
            }

            return [];
        }

        /// <summary>
        /// Gets the nodes debug information
        /// </summary>
        private readonly Dictionary<Color4, IEnumerable<Triangle>> GetNodes(bool separateNodes)
        {
            var nodes = graph.GetNodes(Agent).OfType<GraphNode>();
            if (!nodes.Any())
            {
                return [];
            }

            if (separateNodes)
            {
                return nodes
                    .GroupBy(n => Helper.IntToCol(n.Id, 128))
                    .ToDictionary(keySelector => keySelector.Key, elementSelector => elementSelector.SelectMany(gn => gn.Triangles).AsEnumerable());
            }
            else
            {
                Color4 color = new Color(0, 192, 255, 255);

                return new([new(color, nodes.SelectMany(n => n.Triangles).AsEnumerable())]);
            }
        }
        /// <summary>
        /// Gets the height field debug information
        /// </summary>
        private static Dictionary<Color4, IEnumerable<Triangle>> GetHeightfield(Heightfield hf, bool walkable)
        {
            if (hf == null)
            {
                return [];
            }

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

                return new()
                {
                    { walkableColor, walkableTriangles.Where(t => t.Normal == Vector3.Up) },
                    { nullColor, nullTriangles.Where(t => t.Normal == Vector3.Up) },
                    { multiColor, multiTriangles.Where(t => t.Normal == Vector3.Up) },
                    { Color.White, [.. walkableTriangles.Where(t => t.Normal != Vector3.Up), .. nullTriangles.Where(t => t.Normal != Vector3.Up), .. multiTriangles.Where(t => t.Normal != Vector3.Up)] },
                };
            }
            else
            {
                return new()
                {
                    { Color.White, [.. walkableTriangles, ..nullTriangles, .. multiTriangles] },
                };
            }
        }

        /// <summary>
        /// Triangulates a box from a bounding box
        /// </summary>
        private static IEnumerable<Triangle> TriangulateBox(Vector3 min, Vector3 max)
        {
            var boxVertices = new[]
            {
                new Vector3(min.X, min.Y, min.Z),
                new Vector3(max.X, min.Y, min.Z),
                new Vector3(max.X, min.Y, max.Z),
                new Vector3(min.X, min.Y, max.Z),
                new Vector3(min.X, max.Y, min.Z),
                new Vector3(max.X, max.Y, min.Z),
                new Vector3(max.X, max.Y, max.Z),
                new Vector3(min.X, max.Y, max.Z),
            };

            var boxIndices = new uint[]
            {
                7, 6, 5, 4,
                0, 1, 2, 3,
                1, 5, 6, 2,
                3, 7, 4, 0,
                2, 6, 7, 3,
                0, 4, 5, 1,
            };

            int vIndex = 0;
            for (int i = 0; i < 6; i++)
            {
                //Quad
                var v0 = boxVertices[boxIndices[vIndex++]];
                var v1 = boxVertices[boxIndices[vIndex++]];
                var v2 = boxVertices[boxIndices[vIndex++]];
                var v3 = boxVertices[boxIndices[vIndex++]];

                //Triangles
                var t0 = new Triangle(v0, v1, v2);
                var t1 = new Triangle(v0, v2, v3);

                yield return t0;
                yield return t1;
            }
        }
    }
}
