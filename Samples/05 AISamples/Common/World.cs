using AISamples.Common.Agents;
using AISamples.Common.Items;
using AISamples.Common.Markings;
using AISamples.Common.Persistence;
using AISamples.Common.Primitives;
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

namespace AISamples.Common
{
    class World(Graph graph, float height)
    {
        private static readonly Color4 roadColor = new(0.3f, 0.3f, 0.3f, 1f);
        private static readonly Color4 roadMarksColor = new(0.9f, 0.9f, 0.9f, 1f);
        private const float hLayer = 0.5f;
        private const float hDelta = 0.1f;
        private const string matDiffuseName = "diffuse";

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
        private readonly float buildingHeightMin = 50f;
        private readonly float buildingHeightMax = 80f;
        private readonly List<Building> buildings = [];
        private GeometryDrawer<VertexPositionNormalTexture> buildingDrawer = null;

        private readonly float treeScale = 0.333f;
        private readonly float treeRadius = 30;
        private readonly float treeHeight = 100;
        private readonly List<Tree> trees = [];
        private ModelInstanced treesDrawer = null;

        private readonly List<Segment2> laneGuides = [];
        private readonly List<Marking> markings = [];
        private GeometryDrawer<VertexPositionTexture> markingsDrawer2d = null;
        private GeometryDrawer<VertexPositionTexture> markingsDrawer3d = null;

        private readonly List<Car> cars = [];
        private Car bestCar = null;
        private readonly Color4 bestCarColor = new Color(252, 212, 32, 255);
        private readonly Color4 carColor = new Color(252, 222, 200, 255);

        public Graph Graph { get => graph; }
        public Guid Version { get; private set; } = Guid.NewGuid();
        public float RoadWidth { get; } = 30f;
        public bool Populated { get => graph.GetSegments().Length > 0; }

        public static WorldFile FromWorld(World world)
        {
            return new()
            {
                Graph = Graph.FromGraph(world.graph),
                Height = world.height,

                Envelopes = world.envelopes.Select(Envelope.FromEnvelope).ToArray(),
                RoadEnvelopes = world.roadEnvelopes.Select(Envelope.FromEnvelope).ToArray(),
                RoadBorders = world.roadBorders.Select(Segment2.FromSegment).ToArray(),
                Buildings = world.buildings.Select(Building.FromBuilding).ToArray(),
                Trees = world.trees.Select(Tree.FromTree).ToArray(),
                LaneGuides = world.GetLaneGuides().Select(Segment2.FromSegment).ToArray(),
                Markings = world.markings.Select(m => m.FromMarking()).ToArray(),

                Version = world.Version,
            };
        }
        public void LoadFromWorldFile(WorldFile file)
        {
            graph = Graph.FromGraphFile(file.Graph);
            height = file.Height;

            envelopes.Clear();
            roadEnvelopes.Clear();
            roadBorders.Clear();
            buildings.Clear();
            trees.Clear();
            laneGuides.Clear();
            markings.Clear();

            envelopes.AddRange(file.Envelopes.Select(Envelope.FromEnvelopeFile));
            roadEnvelopes.AddRange(file.RoadEnvelopes.Select(Envelope.FromEnvelopeFile));
            roadBorders.AddRange(file.RoadBorders.Select(Segment2.FromSegmentFile));
            buildings.AddRange(file.Buildings.Select(Building.FromBuildingFile));
            trees.AddRange(file.Trees.Select(Tree.FromTreeFile));
            laneGuides.AddRange(file.LaneGuides.Select(Segment2.FromSegmentFile));
            markings.AddRange(file.Markings.Select(m => m.FromMarkingFile()));

            Version = file.Version;
            graphVersion = graph.Version;

            DrawMarkings();
            DrawGraph();
            worldChanged = false;
        }
        public void GenerateFromGraph(Graph graph)
        {
            this.graph = graph;
            graphVersion = graph.Version;

            Generate();
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
            buildings.AddRange(buildingBases.Select(b => new Building(b, Helper.RandomGenerator.NextFloat(buildingHeightMin, buildingHeightMax))));
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
                Count = 500000,
                DepthEnabled = true,
                BlendMode = BlendModes.Opaque,
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
                (matDiffuseName, Constants.MarkingsTexture),
            ];

            var materialB = MaterialBlinnPhongContent.Default;
            materialB.DiffuseTexture = matDiffuseName;
            materialB.IsTransparent = false;

            var descB = new GeometryDrawerDescription<VertexPositionNormalTexture>()
            {
                Count = 500000,
                DepthEnabled = true,
                BlendMode = BlendModes.Opaque,
                Topology = Topology.TriangleList,
                Images = images,
                Material = materialB,
                TintColor = Color.White,
            };

            buildingDrawer = await scene.AddComponentGround<GeometryDrawer<VertexPositionNormalTexture>, GeometryDrawerDescription<VertexPositionNormalTexture>>(
                nameof(buildingDrawer),
                nameof(buildingDrawer),
                descB);

            var material2d = MaterialBlinnPhongContent.Default;
            material2d.DiffuseTexture = matDiffuseName;
            material2d.IsTransparent = true;

            var descS2d = new GeometryDrawerDescription<VertexPositionTexture>()
            {
                Count = 500000,
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
            material3d.DiffuseTexture = matDiffuseName;
            material3d.IsTransparent = true;

            var descS3d = new GeometryDrawerDescription<VertexPositionTexture>()
            {
                Count = 500000,
                DepthEnabled = true,
                BlendMode = BlendModes.Opaque,
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

        public void Update(IGameTime gameTime, ModelInstanced carDrawer)
        {
            UpdateCars(gameTime, carDrawer);

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
        private void UpdateCars(IGameTime gameTime, ModelInstanced carDrawer)
        {
            int maxCarCount = carDrawer.InstanceCount;
            var road = roadBorders.ToArray();

            for (int i = 0; i < maxCarCount; i++)
            {
                if (i >= cars.Count || cars[i].Damaged)
                {
                    if (carDrawer[i].TintColor != Color.Black)
                    {
                        carDrawer[i].Manipulator.SetTransform(Matrix.Translation(Vector3.One * 10000000f));
                        carDrawer[i].TintColor = Color.Black;
                    }

                    continue;
                }

                var car = cars[i];

                car.Update(gameTime, road, [], false);

                carDrawer[i].Manipulator.SetTransform(car.GetTransform(height + hLayer));
                carDrawer[i].TintColor = car == bestCar ? bestCarColor : carColor;
            }

            bestCar = cars.Where(c => c.Damaged == false).MaxBy(c => c.FittnessValue);

            carDrawer.Visible = cars.Count > 0;
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
                if (segment.OneWay)
                {
                    DrawEnvelope(new Envelope(segment, 2, 1), height + hLayer + hDelta, roadMarksColor, roadMarksDrawer);

                    continue;
                }

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
        private static void DrawBuilding(Building building, float height, GeometryDrawer<VertexPositionNormalTexture> drawer)
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
        public (Vector2 start, Vector2 dir) GetStart()
        {
            var fStart = markings.OfType<Start>().FirstOrDefault();
            if (fStart == null)
            {
                return (Vector2.Zero, Vector2.UnitY);
            }

            return (fStart.Position, fStart.Direction);
        }
        public (Vector2 start, Vector2 dir) GetRandomStart()
        {
            var fStartList = markings.OfType<Start>();
            if (!fStartList.Any())
            {
                return (Vector2.Zero, Vector2.UnitY);
            }

            var fStart = fStartList.ElementAt(Helper.RandomGenerator.Next(0, fStartList.Count() - 1));

            return (fStart.Position, fStart.Direction);
        }

        public void AddCar(Car car)
        {
            if (cars.Contains(car))
            {
                return;
            }

            cars.Add(car);
        }
        public void RemoveCar(Car car)
        {
            cars.Remove(car);
        }
        public Car GetBestCar()
        {
            return bestCar;
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

            ClearTraffic();

            worldChanged = true;
            Version = Guid.NewGuid();
        }
        public void ClearTraffic()
        {
            cars.Clear();

            bestCar = null;
        }
    }
}
