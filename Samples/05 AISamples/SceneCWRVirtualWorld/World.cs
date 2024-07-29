using AISamples.SceneCWRVirtualWorld.Markings;
using AISamples.SceneCWRVirtualWorld.Primitives;
using Engine;
using Engine.BuiltIn.Components.Primitives;
using Engine.BuiltIn.Primitives;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AISamples.SceneCWRVirtualWorld
{
    class World
    {
        private static readonly Color4 roadColor = new(0.6f, 0.6f, 0.6f, 1f);
        private static readonly Color4 roadMarksColor = new(0.9f, 0.9f, 0.9f, 1f);
        private static readonly Color4 buildingColor = new(0.3f, 0.3f, 1f, 0.5f);
        private static readonly Color4 treeColor = new(0.3f, 1f, 0.3f, 0.5f);
        private const float hDelta = 0.1f;

        public Graph Graph { get; }

        private readonly float height;
        private Guid graphVersion;
        private readonly List<Envelope> envelopes = [];

        public float RoadWidth { get; } = 30f;
        private readonly int roadRoundness = 16;
        private readonly List<Envelope> roadEnvelopes = [];
        private readonly List<Segment2> roadBorders = [];
        private GeometryColorDrawer<Triangle> roadDrawer = null;
        private GeometryColorDrawer<Triangle> roadMarksDrawer = null;

        private readonly float buildingWidth = 100f;
        private readonly float buildingMinLenth = 100f;
        private readonly float buildingSpacing = 25f;
        private readonly List<Envelope> buildingEnvelopes = [];
        private readonly List<Segment2> buildingGuides = [];
        private readonly List<Segment2> buildingSupports = [];
        private readonly List<Polygon> buildingBases = [];
        private GeometryColorDrawer<Triangle> buildingDrawer = null;

        private readonly List<Vector2> trees = [];
        private readonly float treeRadius = 30;

        private readonly List<Segment2> laneGuides = [];

        private GeometryDrawer<VertexPositionTexture> markingsDrawer = null;
        private readonly List<Marking> markings = [];

        public World(Graph graph, float height)
        {
            Graph = graph;
            this.height = height;

            graphVersion = graph.Version;

            Generate();
        }
        private void Generate()
        {
            var segments = Graph.GetSegments();

            GenerateRoads(segments);

            GenerateBuildings(segments);

            GenerateTrees(segments);

            GenerateLaneGuides(segments);
        }
        private void GenerateRoads(Segment2[] segments)
        {
            envelopes.Clear();

            for (int i = 0; i < segments.Length; i++)
            {
                envelopes.Add(new(segments[i], RoadWidth, roadRoundness));
            }

            roadEnvelopes.Clear();
            roadEnvelopes.AddRange(envelopes.Select(e => e.Scale(1.2f)));

            roadBorders.Clear();
            roadBorders.AddRange(Polygon.Union(envelopes.Select(e => e.GetPolygon()).ToArray()));
        }
        private void GenerateBuildings(Segment2[] segments)
        {
            buildingEnvelopes.Clear();

            float width = RoadWidth + buildingWidth + buildingSpacing * 2;
            for (int i = 0; i < segments.Length; i++)
            {
                buildingEnvelopes.Add(new(segments[i], width, roadRoundness));
            }

            buildingGuides.Clear();
            buildingGuides.AddRange(Polygon.Union(buildingEnvelopes.Select(e => e.GetPolygon()).ToArray()));
            buildingGuides.RemoveAll(e => e.Length < buildingMinLenth);

            buildingSupports.Clear();
            foreach (var guide in buildingGuides)
            {
                float len = guide.Length + buildingSpacing;
                int buildingCount = (int)MathF.Floor(len / (buildingMinLenth + buildingSpacing));
                float buildingLength = len / buildingCount - buildingSpacing;

                var dir = guide.Direction;
                var q1 = guide.P1;
                var q2 = Vector2.Add(q1, dir * buildingLength);
                buildingSupports.Add(new(q1, q2));

                for (int i = 2; i <= buildingCount; i++)
                {
                    q1 = Vector2.Add(q2, dir * buildingSpacing);
                    q2 = Vector2.Add(q1, dir * buildingLength);
                    buildingSupports.Add(new(q1, q2));
                }
            }

            buildingBases.Clear();
            var bases = buildingSupports.Select(seg => new Envelope(seg, buildingWidth, 1).GetPolygon());
            buildingBases.AddRange(bases);

            buildingBases.RemoveAll(b =>
            {
                foreach (var otherB in buildingBases)
                {
                    if (otherB == b)
                    {
                        continue;
                    }

                    if (b.IntersectsPolygonSegments(otherB))
                    {
                        return true;
                    }

                    if (b.DistanceToPolygon(otherB) < buildingSpacing - 0.001f)
                    {
                        return true;
                    }
                }

                return false;
            });
        }
        private void GenerateTrees(Segment2[] segments)
        {
            trees.Clear();

            if (segments.Length == 0)
            {
                return;
            }

            var rnd = Helper.NewGenerator(1);

            Polygon[] illegalPolys =
            [
                .. buildingBases,
                .. envelopes.Select(e => e.GetPolygon())
            ];

            var areaSize = GetWorldSize();

            int treeCount = 0;
            while (treeCount < 100)
            {
                var x = rnd.NextFloat(areaSize.Left, areaSize.Right);
                var y = rnd.NextFloat(areaSize.Top, areaSize.Bottom);
                var p = new Vector2(x, y);

                bool keep = true;
                foreach (var poly in illegalPolys)
                {
                    if (poly.ContainsPoint(p) || poly.DistanceToPoint(p) < treeRadius)
                    {
                        keep = false;
                        break;
                    }
                }

                if (keep)
                {
                    keep = !trees.Exists(t => Vector2.Distance(t, p) < treeRadius * 2);
                }

                if (keep)
                {
                    keep = Array.Exists(illegalPolys, poly => poly.DistanceToPoint(p) < treeRadius * 4);
                }

                if (keep)
                {
                    trees.Add(p);
                    treeCount = 0;
                }
                treeCount++;
            }
        }
        private void GenerateLaneGuides(Segment2[] segments)
        {
            laneGuides.Clear();

            if (segments.Length == 0)
            {
                return;
            }

            Envelope[] tmpEnvelopes = new Envelope[segments.Length];
            for (int i = 0; i < segments.Length; i++)
            {
                tmpEnvelopes[i] = new(segments[i], RoadWidth * 0.5f, roadRoundness);
            }

            laneGuides.AddRange(Polygon.Union(tmpEnvelopes.Select(e => e.GetPolygon()).ToArray()));
        }
        private RectangleF GetWorldSize()
        {
            Vector2[] points =
            [
                .. roadBorders.SelectMany(b => new Vector2[] { b.P1, b.P2 }),
                .. buildingBases.SelectMany(b => b.Vertices)
            ];

            Vector2 min = new(float.MaxValue);
            Vector2 max = new(float.MinValue);
            foreach (var p in points)
            {
                min.X = Math.Min(min.X, p.X);
                min.Y = Math.Min(min.Y, p.Y);
                max.X = Math.Max(max.X, p.X);
                max.Y = Math.Max(max.Y, p.Y);
            }

            return new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        public async Task Initialize(Scene scene)
        {
            var descT = new GeometryColorDrawerDescription<Triangle>()
            {
                Count = 20000,
                DepthEnabled = false,
                BlendMode = BlendModes.Alpha,
            };

            buildingDrawer = await scene.AddComponentEffect<GeometryColorDrawer<Triangle>, GeometryColorDrawerDescription<Triangle>>(
                nameof(buildingDrawer),
                nameof(buildingDrawer),
                descT,
                Scene.LayerEffects);

            roadDrawer = await scene.AddComponentEffect<GeometryColorDrawer<Triangle>, GeometryColorDrawerDescription<Triangle>>(
                nameof(roadDrawer),
                nameof(roadDrawer),
                descT,
                Scene.LayerEffects + 1);

            roadMarksDrawer = await scene.AddComponentEffect<GeometryColorDrawer<Triangle>, GeometryColorDrawerDescription<Triangle>>(
                nameof(roadMarksDrawer),
                nameof(roadMarksDrawer),
                descT,
                Scene.LayerEffects + 2);


            (string, string)[] images =
            [
                ("diffuse", Constants.MarkingsTexture),
            ];

            var stopMaterial = MaterialBlinnPhongContent.Default;
            stopMaterial.DiffuseTexture = "diffuse";
            stopMaterial.IsTransparent = true;

            var descS = new GeometryDrawerDescription<VertexPositionTexture>()
            {
                Count = 20000,
                DepthEnabled = false,
                BlendMode = BlendModes.Transparent,
                Topology = Topology.TriangleList,
                Images = images,
                Material = stopMaterial,
                TintColor = roadMarksColor,
            };

            markingsDrawer = await scene.AddComponentEffect<GeometryDrawer<VertexPositionTexture>, GeometryDrawerDescription<VertexPositionTexture>>(
                nameof(markingsDrawer),
                nameof(markingsDrawer),
                descS,
                Scene.LayerEffects + 3);
        }

        public void Update()
        {
            DrawMarkings();

            if (graphVersion == Graph.Version)
            {
                return;
            }

            graphVersion = Graph.Version;

            Generate();

            DrawGraph();
        }
        private void DrawMarkings()
        {
            markingsDrawer.Clear();

            foreach (var marking in markings)
            {
                markingsDrawer.AddPrimitives(marking.Draw(height));
            }
        }
        private void DrawGraph()
        {
            buildingDrawer.Clear();
            roadDrawer.Clear();
            roadMarksDrawer.Clear();

            // Draw building areas
            foreach (var support in buildingBases)
            {
                DrawPolygon(support, height, buildingColor, buildingDrawer);
            }

            // Draw trees
            foreach (var tree in trees)
            {
                DrawCircle(tree, treeRadius, height, treeColor, buildingDrawer);
            }

            // Draw road base
            foreach (var roadEnvelope in roadEnvelopes)
            {
                DrawEnvelope(roadEnvelope, height, roadColor, roadDrawer);
            }

            // Draw road marks
            foreach (var segment in Graph.GetSegments())
            {
                var dashes = Utils.Divide(segment, 5, 5);

                foreach (var dash in dashes)
                {
                    DrawEnvelope(new Envelope(dash, 2, 1), height + hDelta, roadMarksColor, roadMarksDrawer);
                }
            }

            // Draw road borders
            foreach (var border in roadBorders)
            {
                DrawEnvelope(new Envelope(border, 2, 3), height + hDelta, roadMarksColor, roadMarksDrawer);
            }
        }
        private static void DrawEnvelope(Envelope envelope, float height, Color4 color, GeometryColorDrawer<Triangle> drawer)
        {
            var vertices = envelope
                .GetPolygonVertices()
                .Select(v => new Vector3(v.X, height, v.Y));

            var t = GeometryUtil.CreatePolygonTriangleList(vertices, true);
            drawer.AddPrimitives(color, Triangle.ComputeTriangleList(t));
        }
        private static void DrawPolygon(Polygon polygon, float height, Color4 color, GeometryColorDrawer<Triangle> drawer)
        {
            var vertices = polygon
                .Vertices
                .Select(v => new Vector3(v.X, height, v.Y));

            var t = GeometryUtil.CreatePolygonTriangleList(vertices, true);
            drawer.AddPrimitives(color, Triangle.ComputeTriangleList(t));
        }
        private static void DrawCircle(Vector2 point, float radius, float height, Color4 color, GeometryColorDrawer<Triangle> drawer)
        {
            var v = new Vector3(point.X, height, point.Y);

            var t = GeometryUtil.CreateCircle(Topology.TriangleList, v, radius, 32);
            drawer.AddPrimitives(color, Triangle.ComputeTriangleList(t));
        }

        public Segment2[] GetLaneGuides()
        {
            return [.. laneGuides];
        }

        public void AddMarking(Marking marking)
        {
            if (marking == null)
            {
                return;
            }

            if (markings.Contains(marking))
            {
                return;
            }

            markings.Add(marking);
        }
        public Marking GetMarkingAtPoint(Vector2 point)
        {
            foreach (var marking in markings)
            {
                if (marking.ContainsPoint(point))
                {
                    return marking;
                }
            }

            return null;
        }
        public void RemoveMarking(Marking marking)
        {
            if (marking == null)
            {
                return;
            }

            markings.Remove(marking);
        }

        public void Clear()
        {
            Graph.Clear();
            markings.Clear();
        }
    }
}
