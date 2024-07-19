using Engine;
using Engine.BuiltIn.Components.Primitives;
using Engine.Common;
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
        private const float hDelta = 0.1f;

        private readonly Graph graph;
        private readonly float height;
        private readonly float roadWidth = 30f;
        private readonly int roadRoundness = 16;
        private readonly List<Envelope> envelopes = [];
        private readonly List<Segment2> roadBorders = [];

        private PrimitiveListDrawer<Triangle> roadDrawer = null;
        private PrimitiveListDrawer<Triangle> roadMarksDrawer = null;
        private Guid graphVersion = Guid.Empty;

        public World(Graph graph, float height)
        {
            this.graph = graph;
            this.height = height;

            graphVersion = graph.Version;

            Generate();
        }
        private void Generate()
        {
            envelopes.Clear();

            var segments = graph.GetSegments();
            for (int i = 0; i < segments.Length; i++)
            {
                envelopes.Add(new(segments[i], roadWidth, roadRoundness));
            }

            roadBorders.Clear();
            roadBorders.AddRange(Polygon.Union(envelopes.Select(e => e.GetPolygon()).ToArray()));
        }

        public async Task Initialize(Scene scene)
        {
            var descT = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = 20000,
                DepthEnabled = false,
                BlendMode = BlendModes.Alpha,
            };

            roadDrawer = await scene.AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>(
                nameof(roadDrawer),
                nameof(roadDrawer),
                descT);

            roadMarksDrawer = await scene.AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>(
                nameof(roadMarksDrawer),
                nameof(roadMarksDrawer),
                descT,
                Scene.LayerEffects + 1);
        }

        public void Update()
        {
            if (graphVersion == graph.Version)
            {
                return;
            }

            graphVersion = graph.Version;

            Generate();

            Draw();
        }
        private void Draw()
        {
            roadDrawer.Clear();
            roadMarksDrawer.Clear();

            foreach (var envelope in envelopes)
            {
                DrawEnvelope(envelope, height, roadColor, roadDrawer);
            }

            foreach (var segment in graph.GetSegments())
            {
                var dashes = Segment2.Divide(segment, 5, 5);

                foreach (var dash in dashes)
                {
                    DrawEnvelope(new Envelope(dash, 2, 1), height + hDelta, roadMarksColor, roadMarksDrawer);
                }
            }

            foreach (var border in roadBorders)
            {
                DrawEnvelope(new Envelope(border, 2, 1), height + hDelta, roadMarksColor, roadMarksDrawer);
            }
        }
        private static void DrawEnvelope(Envelope envelope, float height, Color4 envColor, PrimitiveListDrawer<Triangle> drawer)
        {
            var vertices = envelope
                .GetPolygonVertices()
                .Select(v => new Vector3(v.X, height, v.Y));

            var t = GeometryUtil.CreatePolygonTriangleList(vertices, true);
            drawer.AddPrimitives(envColor, Triangle.ComputeTriangleList(t));
        }
    }
}
