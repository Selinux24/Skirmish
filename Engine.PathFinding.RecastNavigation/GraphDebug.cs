using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Graph debug helper
    /// </summary>
    public struct GraphDebug : IGraphDebug
    {
        /// <inheritdoc/>
        public IGraph Graph { get; private set; }
        /// <inheritdoc/>
        public AgentType Agent { get; private set; }

        /// <summary>
        /// Navigation mesh build date
        /// </summary>
        private NavMeshBuildData buildData;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <param name="agent">Agent</param>
        /// <param name="buildData">Build data</param>
        public GraphDebug(Graph graph, AgentType agent)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            Agent = agent ?? throw new ArgumentNullException(nameof(agent));

            buildData = graph.CreateAgentQuery(agent)?.GetAttachedNavMesh()?.GetBuildData() ?? throw new ArgumentException("Invalid angent data", nameof(agent));
        }

        /// <inheritdoc/>
        public readonly IEnumerable<(int Id, string Information)> GetAvailableDebugInformation()
        {
            return Enum
                .GetValues<GraphDebugTypes>()
                .Except(new[] { GraphDebugTypes.None })
                .Select(v => ((int)v, v.ToString()));
        }
        /// <inheritdoc/>
        public readonly Dictionary<Color4, IEnumerable<Triangle>> GetInfo(int id)
        {
            return (GraphDebugTypes)id switch
            {
                GraphDebugTypes.Nodes => GetNodes(),
                GraphDebugTypes.Heightfield => GetHeightfield(false),
                GraphDebugTypes.WalkableHeightfield => GetHeightfield(true),
                _ => new()
            };
        }

        /// <summary>
        /// Gets the nodes debug information
        /// </summary>
        private readonly Dictionary<Color4, IEnumerable<Triangle>> GetNodes()
        {
            var nodes = Graph.GetNodes(Agent).OfType<GraphNode>();
            if (!nodes.Any())
            {
                return new();
            }

            return nodes
                .GroupBy(n => n.Color)
                .ToDictionary(keySelector => keySelector.Key, elementSelector => elementSelector.SelectMany(gn => gn.Triangles).AsEnumerable());
        }
        /// <summary>
        /// Gets the height field debug information
        /// </summary>
        private readonly Dictionary<Color4, IEnumerable<Triangle>> GetHeightfield(bool walkable)
        {
            var hf = buildData.Heightfield;
            if (hf == null)
            {
                return new();
            }

            var orig = hf.BoundingBox.Minimum;
            float cs = hf.CellSize;
            float ch = hf.CellHeight;

            List<Triangle> triangles = new();

            foreach (var (row, col, span) in hf.IterateSpans())
            {
                float fz = orig.Z + col * cs;
                float fx = orig.X + row * cs;

                var s = span;
                do
                {
                    if (walkable && s.Area != AreaTypes.RC_WALKABLE_AREA)
                    {
                        s = s.Next;

                        continue;
                    }

                    var min = new Vector3(fx, orig.Y + s.SMin * ch, fz);
                    var max = new Vector3(fx + cs, orig.Y + s.SMax * ch, fz + cs);

                    var boxTris = TriangulateBox(min, max);
                    triangles.AddRange(boxTris);

                    s = s.Next;
                }
                while (s != null);
            }

            return new()
            {
                { Color.White, triangles }
            };
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
