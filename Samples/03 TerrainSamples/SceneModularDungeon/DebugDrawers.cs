using Engine;
using Engine.Common;
using Engine.Modular;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TerrainSamples.SceneModularDungeon
{
    internal class DebugDrawers(Scene scene, string id, string name) : BaseSceneObject<SceneObjectDescription>(scene, id, name)
    {
        private PrimitiveListDrawer<Line3D> bboxesDrawer = null;
        private PrimitiveListDrawer<Line3D> modelDrawer = null;
        private PrimitiveListDrawer<Triangle> graphDrawer = null;
        private PrimitiveListDrawer<Triangle> obstacleDrawer = null;
        private PrimitiveListDrawer<Line3D> connectionDrawer = null;

        private readonly Color connectionColor = new(Color.LightBlue.ToColor3(), 1f);
        private readonly Color obstacleColor = new(Color.Pink.ToColor3(), 0.5f);

        private Player[] agents;
        private int currentAgentIndex = 0;

        public async Task Initialize(Player[] agents)
        {
            this.agents = agents;

            var graphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = 50000,
            };
            graphDrawer = await Scene.AddComponentUI<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("db1", "DEBUG++ Graph", graphDrawerDesc);
            graphDrawer.Visible = false;

            var bboxesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Color = new Color4(1.0f, 0.0f, 0.0f, 0.25f),
                Count = 10000,
            };
            bboxesDrawer = await Scene.AddComponentUI<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("db2", "DEBUG++ Bounding volumes", bboxesDrawerDesc);
            bboxesDrawer.Visible = false;

            var ratDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Color = new Color4(0.0f, 1.0f, 1.0f, 0.25f),
                Count = 10000,
            };
            modelDrawer = await Scene.AddComponentUI<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("db3", "DEBUG++ Rat", ratDrawerDesc);
            modelDrawer.Visible = false;

            var obstacleDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                DepthEnabled = false,
                Count = 10000,
            };
            obstacleDrawer = await Scene.AddComponentUI<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("db4", "DEBUG++ Obstacles", obstacleDrawerDesc);
            obstacleDrawer.Visible = false;

            var connectionDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Color = connectionColor,
                Count = 10000,
            };
            connectionDrawer = await Scene.AddComponentUI<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("db5", "DEBUG++ Connections", connectionDrawerDesc);
            connectionDrawer.Visible = false;
        }

        public void Clear()
        {
            bboxesDrawer.Clear();
            modelDrawer.Clear();
            graphDrawer.Clear();
            obstacleDrawer.Clear();
            connectionDrawer.Clear();
        }

        public void SetNextAgentIndex()
        {
            currentAgentIndex++;
            currentAgentIndex %= agents.Length;
        }

        public void DrawScenery(ModularScenery scenery)
        {
            //Graph
            bboxesDrawer.Clear();

            //Objects
            DrawBoundingBoxes(scenery.GetObjectsByType(ObjectTypes.Entrance).Select(o => o.Instance), Color.PaleVioletRed);
            DrawBoundingBoxes(scenery.GetObjectsByType(ObjectTypes.Exit).Select(o => o.Instance), Color.ForestGreen);
            DrawBoundingBoxes(scenery.GetObjectsByType(ObjectTypes.Trigger).Select(o => o.Instance), Color.Cyan);
            DrawBoundingBoxes(scenery.GetObjectsByType(ObjectTypes.Door).Select(o => o.Instance), Color.LightYellow);
            DrawBoundingBoxes(scenery.GetObjectsByType(ObjectTypes.Light).Select(o => o.Instance), Color.MediumPurple);
        }
        private void DrawBoundingBoxes(IEnumerable<ModelInstance> items, Color color)
        {
            if (!items.Any())
            {
                return;
            }

            var boxes = items.Select(i => i.GetBoundingBox());
            bboxesDrawer.SetPrimitives(color, Line3D.CreateBoxes(boxes));
        }

        public void DrawCamera(Camera camera)
        {
            var frustum = Line3D.CreateFrustum(camera.Frustum);

            bboxesDrawer.SetPrimitives(Color.White, frustum);
        }
        public void DrawObstacles(IEnumerable<ObstacleInfo> obstacles)
        {
            obstacleDrawer.Clear(obstacleColor);

            foreach (var obstacle in obstacles.Select(o => o.Obstacle))
            {
                IEnumerable<Triangle> obstacleTris = null;

                if (obstacle is BoundingCylinder bc)
                {
                    obstacleTris = Triangle.ComputeTriangleList(bc, 32);
                }
                else if (obstacle is BoundingBox bbox)
                {
                    obstacleTris = Triangle.ComputeTriangleList(bbox);
                }
                else if (obstacle is OrientedBoundingBox obb)
                {
                    obstacleTris = Triangle.ComputeTriangleList(obb);
                }

                if (obstacleTris?.Any() == true)
                {
                    obstacleDrawer.AddPrimitives(obstacleColor, obstacleTris);
                }
            }
        }
        public void DrawConnections(IEnumerable<IGraphConnection> conns)
        {
            connectionDrawer.Clear(connectionColor);

            foreach (var conn in conns)
            {
                var arclines = Line3D.CreateArc(conn.Start, conn.End, 0.25f, 8);
                connectionDrawer.AddPrimitives(connectionColor, arclines);

                var cirlinesF = Line3D.CreateCircle(conn.Start, conn.Radius, 32);
                connectionDrawer.AddPrimitives(connectionColor, cirlinesF);

                if (conn.BiDirectional)
                {
                    var cirlinesT = Line3D.CreateCircle(conn.End, conn.Radius, 32);
                    connectionDrawer.AddPrimitives(connectionColor, cirlinesT);
                }
            }
        }
        public void DrawGraph()
        {
            if (Scene is not WalkableScene walkableScene)
            {
                return;
            }

            var nodes = BuildGraphNodeDebugAreas(walkableScene, agents[currentAgentIndex]);

            graphDrawer.Clear();
            graphDrawer.SetPrimitives(nodes);
        }
        private static Dictionary<Color4, IEnumerable<Triangle>> BuildGraphNodeDebugAreas(WalkableScene scene, AgentType agent)
        {
            var nodes = scene.GetNodes(agent).OfType<GraphNode>();
            if (!nodes.Any())
            {
                return [];
            }

            Dictionary<Color4, IEnumerable<Triangle>> res = [];

            foreach (var node in nodes)
            {
                var color = Helper.IntToCol(node.Id, 128);
                var tris = node.Triangles;

                if (!res.TryGetValue(color, out var value))
                {
                    value = new List<Triangle>(tris);
                    res.Add(color, value);
                }
                else
                {
                    ((List<Triangle>)value).AddRange(tris);
                }
            }

            return res;
        }
        public void DrawModel(Model model)
        {
            if (!modelDrawer.Visible)
            {
                return;
            }

            var bbox = model.GetBoundingBox();

            modelDrawer.SetPrimitives(Color.White, Line3D.CreateBox(bbox));
        }
        public void DrawPath(PathFindingPath path)
        {
            if (!modelDrawer.Visible)
            {
                return;
            }

            modelDrawer.SetPrimitives(Color.Red, Line3D.CreateLineList(path.Positions));
        }

        public void ToggleConnections()
        {
            graphDrawer.Visible = !graphDrawer.Visible;
            connectionDrawer.Visible = graphDrawer.Visible;
        }
        public void ToggleBoundingBoxes()
        {
            bboxesDrawer.Visible = !bboxesDrawer.Visible;
        }
        public void ToggleObstacles()
        {
            obstacleDrawer.Visible = !obstacleDrawer.Visible;
        }
        public void ToggleRat()
        {
            modelDrawer.Visible = !modelDrawer.Visible;
        }
    }
}
