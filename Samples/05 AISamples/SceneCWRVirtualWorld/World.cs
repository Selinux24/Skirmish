﻿using AISamples.Common;
using AISamples.SceneCWRVirtualWorld.Content;
using AISamples.SceneCWRVirtualWorld.Items;
using AISamples.SceneCWRVirtualWorld.Markings;
using AISamples.SceneCWRVirtualWorld.Primitives;
using Engine;
using Engine.BuiltIn.Components.Models;
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
    class World(Graph graph, float height)
    {
        private static readonly Color4 roadColor = new(0.6f, 0.6f, 0.6f, 1f);
        private static readonly Color4 roadMarksColor = new(0.9f, 0.9f, 0.9f, 1f);
        private const float hLayer = 0.5f;
        private const float hDelta = 0.1f;

        private float height = height;
        private Graph graph = graph;
        private Guid graphVersion = graph.Version;
        private bool worldChanged = false;

        private readonly List<Envelope> envelopes = [];

        private readonly int roadRoundness = 16;
        private readonly List<Envelope> roadEnvelopes = [];
        private readonly List<Segment2> roadBorders = [];
        private GeometryColorDrawer<Triangle> roadDrawer = null;
        private GeometryColorDrawer<Triangle> roadMarksDrawer = null;

        private readonly float buildingWidth = 100f;
        private readonly float buildingMinLenth = 100f;
        private readonly float buildingSpacing = 25f;
        private readonly float buildingHeight = 50f;
        private readonly List<Building> buildings = [];
        private GeometryDrawer<VertexPositionTexture> buildingDrawer = null;

        private readonly float treeScale = 0.333f;
        private readonly float treeRadius = 30;
        private readonly float treeHeight = 100;
        private readonly List<Tree> trees = [];
        private ModelInstanced treesDrawer = null;

        private readonly List<Segment2> laneGuides = [];
        private readonly List<Marking> markings = [];
        private GeometryDrawer<VertexPositionTexture> markingsDrawer2d = null;
        private GeometryDrawer<VertexPositionTexture> markingsDrawer3d = null;

        public Graph Graph { get => graph; }
        public Guid Version { get; private set; } = Guid.NewGuid();
        public float RoadWidth { get; } = 30f;

        public static WorldFile FromWorld(World world)
        {
            var graph = Graph.FromGraph(world.graph);
            var height = world.height;
            var envelopes = world.envelopes.Select(Envelope.FromEnvelope).ToArray();
            var roadEnvelopes = world.roadEnvelopes.Select(Envelope.FromEnvelope).ToArray();
            var roadBorders = world.roadBorders.Select(Segment2.FromSegment).ToArray();
            var buildings = world.buildings.Select(Building.FromBuilding).ToArray();
            var trees = world.trees.Select(Tree.FromTree).ToArray();
            var laneGuides = world.GetLaneGuides().Select(Segment2.FromSegment).ToArray();
            var markings = world.markings.Select(m => m.FromMarking()).ToArray();
            var version = world.Version;

            return new()
            {
                Graph = graph,
                Height = height,

                Envelopes = envelopes,
                RoadEnvelopes = roadEnvelopes,
                RoadBorders = roadBorders,
                Buildings = buildings,
                Trees = trees,
                LaneGuides = laneGuides,
                Markings = markings,

                Version = version,
            };
        }
        public void LoadFromWorldFile(WorldFile file)
        {
            var graph = Graph.FromGraphFile(file.Graph);
            var height = file.Height;
            var envelopes = file.Envelopes.Select(Envelope.FromEnvelopeFile).ToList();
            var roadEnvelopes = file.RoadEnvelopes.Select(Envelope.FromEnvelopeFile).ToList();
            var roadBorders = file.RoadBorders.Select(Segment2.FromSegmentFile).ToList();
            var buildings = file.Buildings.Select(Building.FromBuildingFile).ToList();
            var trees = file.Trees.Select(Tree.FromTreeFile).ToList();
            var laneGuides = file.LaneGuides.Select(Segment2.FromSegmentFile).ToList();
            var markings = file.Markings.Select(m => m.FromMarkingFile()).ToList();
            var version = file.Version;

            this.graph = graph;
            this.height = height;

            this.envelopes.Clear();
            this.roadEnvelopes.Clear();
            this.roadBorders.Clear();
            this.buildings.Clear();
            this.trees.Clear();
            this.laneGuides.Clear();
            this.markings.Clear();

            this.envelopes.AddRange(envelopes);
            this.roadEnvelopes.AddRange(roadEnvelopes);
            this.roadBorders.AddRange(roadBorders);
            this.buildings.AddRange(buildings);
            this.trees.AddRange(trees);
            this.laneGuides.AddRange(laneGuides);
            this.markings.AddRange(markings);

            Version = version;

            worldChanged = true;
        }

        public void Generate()
        {
            var segments = graph.GetSegments();

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
            List<Envelope> buildingEnvelopes = [];
            float width = RoadWidth + buildingWidth + buildingSpacing * 2;
            for (int i = 0; i < segments.Length; i++)
            {
                buildingEnvelopes.Add(new(segments[i], width, roadRoundness));
            }

            List<Segment2> buildingGuides = [];
            buildingGuides.AddRange(Polygon.Union(buildingEnvelopes.Select(e => e.GetPolygon()).ToArray()));
            buildingGuides.RemoveAll(e => e.Length < buildingMinLenth);

            List<Segment2> buildingSupports = [];
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

            List<Polygon> buildingBases = [];
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

            buildings.Clear();
            buildings.AddRange(buildingBases.Select(b => new Building(b, buildingHeight)));
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
                .. buildings.Select(b => b.Polygon),
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
                    keep = !trees.Exists(t => t.DistanceToPoint(p) < treeRadius * 2);
                }

                if (keep)
                {
                    keep = Array.Exists(illegalPolys, poly => poly.DistanceToPoint(p) < treeRadius * 4);
                }

                if (keep)
                {
                    trees.Add(new(new(p.X, height, p.Y), treeRadius, treeHeight));
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
                .. buildings.SelectMany(b => b.Polygon.GetVertices())
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
                DepthEnabled = true,
                BlendMode = BlendModes.Alpha,
            };

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

            var tDesc = new ModelInstancedDescription()
            {
                Content = ContentDescription.FromFile(Constants.TreesResourcesFolder, Constants.TreesModel),
                Instances = 1000,
                Optimize = true,
                PickingHull = PickingHullTypes.None,
                CastShadow = ShadowCastingAlgorihtms.None,
                StartsVisible = false,
            };

            treesDrawer = await scene.AddComponent<ModelInstanced, ModelInstancedDescription>(
                nameof(treesDrawer),
                nameof(treesDrawer),
                tDesc,
                SceneObjectUsages.Agent);

            (string, string)[] images =
            [
                ("diffuse", Constants.MarkingsTexture),
            ];

            var materialB = MaterialBlinnPhongContent.Default;
            materialB.DiffuseTexture = "diffuse";
            materialB.IsTransparent = false;

            var descB = new GeometryDrawerDescription<VertexPositionTexture>()
            {
                Count = 100000,
                DepthEnabled = true,
                BlendMode = BlendModes.Opaque,
                Topology = Topology.TriangleList,
                Images = images,
                Material = materialB,
                TintColor = Color.White,
            };

            buildingDrawer = await scene.AddComponentGround<GeometryDrawer<VertexPositionTexture>, GeometryDrawerDescription<VertexPositionTexture>>(
                nameof(buildingDrawer),
                nameof(buildingDrawer),
                descB);

            var material2d = MaterialBlinnPhongContent.Default;
            material2d.DiffuseTexture = "diffuse";
            material2d.IsTransparent = true;

            var descS2d = new GeometryDrawerDescription<VertexPositionTexture>()
            {
                Count = 100000,
                DepthEnabled = true,
                BlendMode = BlendModes.Transparent,
                Topology = Topology.TriangleList,
                Images = images,
                Material = material2d,
                TintColor = roadMarksColor,
            };

            markingsDrawer2d = await scene.AddComponentEffect<GeometryDrawer<VertexPositionTexture>, GeometryDrawerDescription<VertexPositionTexture>>(
                nameof(markingsDrawer2d),
                nameof(markingsDrawer2d),
                descS2d,
                Scene.LayerEffects + 3);

            var material3d = MaterialBlinnPhongContent.Default;
            material3d.DiffuseTexture = "diffuse";
            material3d.IsTransparent = true;

            var descS3d = new GeometryDrawerDescription<VertexPositionTexture>()
            {
                Count = 100000,
                DepthEnabled = true,
                BlendMode = BlendModes.Alpha,
                Topology = Topology.TriangleList,
                Images = images,
                Material = material3d,
                TintColor = Color4.White,
            };

            markingsDrawer3d = await scene.AddComponentEffect<GeometryDrawer<VertexPositionTexture>, GeometryDrawerDescription<VertexPositionTexture>>(
                nameof(markingsDrawer3d),
                nameof(markingsDrawer3d),
                descS3d,
                Scene.LayerEffects + 4);
        }

        public void Update(IGameTime gameTime)
        {
            foreach (var marking in markings)
            {
                worldChanged = marking.Update(gameTime) || worldChanged;
            }

            if (worldChanged)
            {
                worldChanged = false;

                DrawMarkings();
            }

            if (graphVersion != graph.Version)
            {
                graphVersion = graph.Version;
                Version = Guid.NewGuid();

                Generate();

                DrawGraph();
            }
        }
        private void DrawMarkings()
        {
            markingsDrawer2d.Clear();
            markingsDrawer3d.Clear();

            foreach (var marking in markings)
            {
                var vlist = marking.Draw(height + hLayer + hDelta);
                if (marking.Is3D)
                {
                    markingsDrawer3d.AddPrimitives(vlist);
                }
                else
                {
                    markingsDrawer2d.AddPrimitives(vlist);
                }
            }
        }
        private void DrawGraph()
        {
            buildingDrawer.Clear();
            roadDrawer.Clear();
            roadMarksDrawer.Clear();

            // Draw building areas
            foreach (var building in buildings)
            {
                DrawBuilding(building, height, buildingDrawer);
            }

            // Draw trees
            DrawTrees(trees, treeScale, treesDrawer);

            // Draw road base
            foreach (var roadEnvelope in roadEnvelopes)
            {
                DrawEnvelope(roadEnvelope, height + hLayer, roadColor, roadDrawer);
            }

            // Draw road marks
            foreach (var segment in graph.GetSegments())
            {
                var dashes = Utils.Divide(segment, 5, 5);

                foreach (var dash in dashes)
                {
                    DrawEnvelope(new Envelope(dash, 2, 1), height + hLayer + hDelta, roadMarksColor, roadMarksDrawer);
                }
            }

            // Draw road borders
            foreach (var border in roadBorders)
            {
                DrawEnvelope(new Envelope(border, 2, 3), height + hLayer + hDelta, roadMarksColor, roadMarksDrawer);
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
        private static void DrawTrees(IEnumerable<Tree> trees, float baseScale, ModelInstanced drawer)
        {
            for (int i = 0; i < drawer.InstanceCount; i++)
            {
                var tree = trees.ElementAtOrDefault(i);
                if (tree == null)
                {
                    drawer[i].Manipulator.SetTransform(Matrix.Identity);
                    drawer[i].Visible = false;
                    continue;
                }

                var scale = Matrix.Scaling(tree.Radius * baseScale);
                var rot = Matrix.RotationY(i * 0.1f);
                var translation = Matrix.Translation(tree.Position);
                var transform = scale * rot * translation;

                drawer[i].Manipulator.SetTransform(transform);
                drawer[i].Visible = true;
            }

            drawer.Visible = trees.Any();
        }
        private static void DrawBuilding(Building building, float height, GeometryDrawer<VertexPositionTexture> drawer)
        {
            var vlist = building.CreateBuilding(height);

            drawer.AddPrimitives(vlist);
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

            worldChanged = true;
            Version = Guid.NewGuid();
        }
        public Marking GetMarkingAtPoint(Vector2 point)
        {
            return markings.Find(m => m.ContainsPoint(point));
        }
        public void RemoveMarking(Marking marking)
        {
            if (marking == null)
            {
                return;
            }

            markings.Remove(marking);

            worldChanged = true;
            Version = Guid.NewGuid();
        }

        public void Clear()
        {
            graph.Clear();
            envelopes.Clear();
            roadEnvelopes.Clear();
            roadBorders.Clear();
            buildings.Clear();
            trees.Clear();
            laneGuides.Clear();
            markings.Clear();

            worldChanged = true;
            Version = Guid.NewGuid();
        }
    }
}
