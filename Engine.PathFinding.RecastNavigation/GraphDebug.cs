using Engine.PathFinding.RecastNavigation.Detour;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
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
        internal GraphDebug(IGraph graph, AgentType agent, NavMeshBuildData buildData)
        {
            Graph = graph;
            Agent = agent;

            this.buildData = buildData;
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
            var debugType = (GraphDebugTypes)id;

            if (debugType == GraphDebugTypes.Nodes)
            {
                return GetNodes();
            }

            if (debugType == GraphDebugTypes.Heightfield)
            {
                return GetHeightfield();
            }

            return new();
        }

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

        private readonly Dictionary<Color4, IEnumerable<Triangle>> GetHeightfield()
        {
            var hf = buildData.Heightfield;
            if (hf == null)
            {
                return new();
            }

            var orig = hf.BoundingBox.Minimum;
            float cs = hf.CellSize;
            float ch = hf.CellHeight;

            int w = hf.Width;
            int h = hf.Height;

            Dictionary<Color4, IEnumerable<Triangle>> result = new();

            float delta = 1f / (w * 2);

            for (int col = 0; col < h; ++col)
            {
                for (int row = 0; row < w; ++row)
                {
                    float v = 1f - (row * delta);
                    Color4 rowColor = new(v, v, v, 0.5f);

                    float fx = orig.X + row * cs;
                    float fz = orig.Z + col * cs;
                    var s = hf.Spans[row + col * w];
                    while (s != null)
                    {
                        var boxTris = AppendBox(fx, orig.Y + s.SMin * ch, fz, fx + cs, orig.Y + s.SMax * ch, fz + cs);

                        if (!result.ContainsKey(rowColor))
                        {
                            result.Add(rowColor, boxTris);
                        }
                        else
                        {
                            result[rowColor] = result[rowColor].Concat(boxTris);
                        }

                        s = s.Next;
                    }
                }
            }

            return result;
        }
        private static IEnumerable<Triangle> AppendBox(float minx, float miny, float minz, float maxx, float maxy, float maxz)
        {
            var boxVertices = new[]
            {
                new Vector3(minx, miny, minz),
                new Vector3(maxx, miny, minz),
                new Vector3(maxx, miny, maxz),
                new Vector3(minx, miny, maxz),
                new Vector3(minx, maxy, minz),
                new Vector3(maxx, maxy, minz),
                new Vector3(maxx, maxy, maxz),
                new Vector3(minx, maxy, maxz),
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
