using Engine;
using Engine.Animation;
using Engine.Collada;
using Engine.Common;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.PathFinding.RecastNavigation.Detour.Crowds;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TerrainSamples.SceneCrowds
{
    public class CrowdsScene : WalkableScene
    {
        private const int MaxGridDrawer = 10000;

        private const float near = 0.1f;
        private const float far = 1000f;

        private UITextArea title = null;
        private UITextArea help = null;
        private Sprite upperPanel = null;

        private Agent tankAgentType = null;
        private readonly List<GameAgent<SteerManipulatorController>> tankAgents = new();

        private Graph graph = null;
        private Crowd crowd = null;

        private Model tree = null;
        private ModelInstanced trees = null;

        private PrimitiveListDrawer<Line3D> lineDrawer = null;
        private PrimitiveListDrawer<Triangle> terrainGraphDrawer = null;

        private readonly Dictionary<string, AnimationPlan> animations = new();

        private bool objectsReady = false;
        private bool gameReady = false;

        public CrowdsScene(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await InitializeUIComponents();
        }

        private async Task InitializeUIComponents()
        {
            await LoadResourcesAsync(
                new[]
                {
                    InitializeCursor(),
                    InitializeUI()
                },
                InitializeUIComponentsComplete);
        }
        private async Task InitializeCursor()
        {
            var cursorDesc = UICursorDescription.Default(@"SceneCrowds/Resources/target.png", 15, 15, true);

            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", cursorDesc);
        }
        private async Task InitializeUI()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Tahoma", 18);
            var defaultFont12 = TextDrawerDescription.FromFamily("Tahoma", 12);
            var defaultFont10 = TextDrawerDescription.FromFamily("Tahoma", 10);
            defaultFont18.LineAdjust = true;
            defaultFont12.LineAdjust = true;
            defaultFont10.LineAdjust = true;

            var dTitle = new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White };
            var dHelp = new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow };

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", dTitle);
            help = await AddComponentUI<UITextArea, UITextAreaDescription>("Help", "Help", dHelp);

            upperPanel = await AddComponentUI<Sprite, SpriteDescription>("Upperpanel", "Upperpanel", SpriteDescription.Default(new Color4(0, 0, 0, 0.75f)), LayerUI - 1);
        }
        private void InitializeUIComponentsComplete(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            title.Text = "Crowds";
            help.Text = "Loading...";

            UpdateLayout();

            _ = Task.Run(InitializeSceneComponents);
        }

        private async Task InitializeSceneComponents()
        {
            await LoadResourcesAsync(
                new[]
                {
                    InitializeSkydom(),
                    InitializeTanks(),
                    InitializeTerrain(),
                    InitializeTrees(),
                    InitializeDebug()
                },
                InitializeSceneComponentsCompleted);
        }
        private async Task InitializeSkydom()
        {
            var desc = SkydomDescription.Sphere(@"SceneCrowds/Resources/sunset.dds", far);

            await AddComponentSky<Skydom, SkydomDescription>("Sky", "Sky", desc);
        }
        private async Task InitializeTanks()
        {
            var desc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile("SceneCrowds/Resources", "leopard.json"),
                Instances = 5,
            };
            var tanks = await AddComponent<ModelInstanced, ModelInstancedDescription>("Tanks", "Tanks", desc);

            tanks[0].Manipulator.SetScale(0.2f, true);
            var tankbbox = tanks[0].GetBoundingBox();

            tankAgentType = new Agent()
            {
                Height = tankbbox.Height,
                Radius = Math.Max(tankbbox.Width, tankbbox.Depth) * 0.5f,
                MaxClimb = tankbbox.Height * 0.55f,
            };

            for (int i = 0; i < tanks.InstanceCount; i++)
            {
                InitializeTank(tanks[i]);
            }
        }
        private void InitializeTank(ModelInstance tank)
        {
            tank.Manipulator.SetScale(0.2f, true);

            var tankController = new SteerManipulatorController()
            {
                MaximumForce = 0.5f,
                MaximumSpeed = 7.5f,
                ArrivingRadius = 7.5f,
            };

            var tankAgent = new GameAgent<SteerManipulatorController>(
                $"tankAgent.{tank.Id}",
                $"tankAgent",
                tankAgentType,
                tank,
                tankController);

            tankAgents.Add(tankAgent);

            Lights.AddRange(tankAgent.Lights);
        }
        private async Task InitializeTerrain()
        {
            await AddComponentGround<Scenery, GroundDescription>("Terrain", "Terrain", GroundDescription.FromFile("SceneCrowds/Resources", "terrain.json", 2));
        }
        private async Task InitializeTrees()
        {
            var desc1 = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.DefaultTransparent,
                PathFindingHull = PickingHullTypes.Hull,
                Content = ContentDescription.FromFile("SceneCrowds/resources/trees", "birch_a.json"),
            };
            tree = await AddComponentGround<Model, ModelDescription>("Lonely tree", "Lonely tree", desc1);

            var desc2 = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.DefaultTransparent,
                Instances = 10,
                PathFindingHull = PickingHullTypes.Hull,
                Content = ContentDescription.FromFile("SceneCrowds/resources/trees", "birch_b.json"),
            };
            trees = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Bunch of trees", "Bunch of trees", desc2);
        }
        private async Task InitializeDebug()
        {
            var lineDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 1000,
                StartsVisible = true,
            };
            lineDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("DEBUG++ Lines", "DEBUG++ Lines", lineDrawerDesc);

            var terrainGraphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = MaxGridDrawer,
                StartsVisible = false,
            };
            terrainGraphDrawer = await AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("DEBUG++ Terrain Graph", "DEBUG++ Terrain Graph", terrainGraphDrawerDesc);
        }
        private async Task InitializeSceneComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                help.Text = res.GetErrorMessage();

                return;
            }

            StartNodes();
            StartAnimations();
            StartTerrain();
            StartItems(out Vector3 cameraPosition, out int modelCount);

            objectsReady = true;

            cameraPosition /= modelCount;
            Camera.Goto(cameraPosition + new Vector3(-30, 30, -30));
            Camera.LookTo(cameraPosition + Vector3.Up);
            Camera.NearPlaneDistance = near;
            Camera.FarPlaneDistance = far;

            var nmsettings = BuildSettings.Default;
            nmsettings.CellSize = 0.5f;
            nmsettings.CellHeight = 1f;
            nmsettings.Agents = new[] { tankAgentType };
            nmsettings.PartitionType = SamplePartitionTypes.Layers;
            nmsettings.EdgeMaxError = 1.0f;
            nmsettings.BuildMode = BuildModes.Tiled;
            nmsettings.TileSize = 16;

            var nmInput = new InputGeometry(GetTrianglesForNavigationGraph);

            PathFinderDescription = new PathFinderDescription(nmsettings, nmInput);

            await UpdateNavigationGraph((progress) =>
            {
                help.Text = $"Loading navigation mesh {progress:0.0%}...";
            });

            await Task.Delay(100);

            help.Text = "Point & click over terrain to move the crowd. Press F1 to show the Navigation mesh.";

            gameReady = true;
        }
        private void StartNodes()
        {
            terrainGraphDrawer.Clear();

            var nodes = GetNodes(tankAgentType).OfType<GraphNode>();
            if (!nodes.Any())
            {
                return;
            }

            var clrRnd = Helper.NewGenerator(1);
            Color[] regions = new Color[nodes.Count()];
            for (int i = 0; i < nodes.Count(); i++)
            {
                regions[i] = new Color(clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), 0.55f);
            }

            foreach (var node in nodes)
            {
                terrainGraphDrawer.AddPrimitives(node.Color, node.Triangles);
            }
        }
        private void StartAnimations()
        {
            var ap = new AnimationPath();
            ap.AddLoop("roll");
            animations.Add("default", new AnimationPlan(ap));
        }
        private void StartTerrain()
        {
            if (FindTopGroundPosition<Triangle>(20, -20, out var treePos))
            {
                tree.Manipulator.SetTransform(treePos.Position, Quaternion.Identity, 0.5f);
            }

            for (int i = 0; i < trees.InstanceCount; i++)
            {
                if (FindTopGroundPosition<Triangle>((i * 10) - 35, 17, out var pos))
                {
                    trees[i].Manipulator.SetTransform(pos.Position, Quaternion.RotationAxis(Vector3.Up, i), 0.5f);
                }
            }
        }
        private void StartItems(out Vector3 cameraPosition, out int modelCount)
        {
            cameraPosition = Vector3.Zero;
            modelCount = 0;

            for (int i = 0; i < tankAgents.Count; i++)
            {
                if (FindTopGroundPosition<Triangle>((i * 10) - (tankAgents.Count * 10 / 2), 40, out var t1Pos))
                {
                    tankAgents[i].Manipulator.SetPosition(t1Pos.Position);
                    tankAgents[i].Manipulator.SetNormal(t1Pos.Primitive.Normal);
                    cameraPosition += t1Pos.Position;
                    modelCount++;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (!objectsReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            UpdateInputCamera(gameTime);

            if (!gameReady)
            {
                return;
            }

            UpdateInputMouse();
            UpdateInputDebug();

            UpdateAgents(gameTime);
            UpdateDebugProximityGridDrawer();
        }
        private void UpdateInputCamera(GameTime gameTime)
        {
#if DEBUG
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    gameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                gameTime,
                Game.Input.MouseXDelta,
                Game.Input.MouseYDelta);
#endif

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.C))
            {
                Camera.MoveDown(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.Space))
            {
                Camera.MoveUp(gameTime, Game.Input.ShiftPressed);
            }
        }
        private void UpdateInputMouse()
        {
            if (!Game.Input.MouseButtonJustReleased(MouseButtons.Left))
            {
                return;
            }

            var pRay = GetPickingRay(PickingHullTypes.Perfect);

            if (!this.PickNearest<Triangle>(pRay, SceneObjectUsages.None, out var r))
            {
                return;
            }

            var tri = Line3D.CreateWiredTriangle(r.PickingResult.Primitive);
            var cross = Line3D.CreateCross(r.PickingResult.Position, 0.25f);

            lineDrawer.SetPrimitives(Color.White, tri);
            lineDrawer.SetPrimitives(Color.Red, cross);

            if (Game.Input.ShiftPressed)
            {
                graph.RequestMoveAgent(crowd, tankAgents[0].CrowdAgent, tankAgentType, r.PickingResult.Position);
            }
            else
            {
                graph.RequestMoveCrowd(crowd, tankAgentType, r.PickingResult.Position);
            }
        }
        private void UpdateInputDebug()
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                terrainGraphDrawer.Visible = !terrainGraphDrawer.Visible;
            }
        }

        private void UpdateAgents(GameTime gameTime)
        {
            tankAgents.ForEach(a => a.Update(new UpdateContext() { GameTime = gameTime }));

            if (crowd == null)
            {
                return;
            }

            for (int i = 0; i < tankAgents.Count; i++)
            {
                var cag = tankAgents[i].CrowdAgent;
                var pPos = tankAgents[i].Manipulator.Position;

                if (Vector3.NearEqual(cag.NPos, pPos, new Vector3(0.001f)))
                {
                    continue;
                }

                var tDir = cag.NPos - pPos;
                tankAgents[i].Manipulator.SetPosition(cag.NPos);
                tankAgents[i].Manipulator.RotateTo(cag.NPos + tDir, Axis.Y, 0.1f);
            }
        }
        private void UpdateDebugProximityGridDrawer()
        {
            if (crowd == null)
            {
                return;
            }

            var lines = new List<Line3D>();

            var grid = crowd.GetGrid();

            var rect = grid.GetBounds();

            var c0 = new Vector2(rect.Left, rect.Top);
            var c1 = new Vector2(rect.Right, rect.Top);
            var c2 = new Vector2(rect.Right, rect.Bottom);
            var c3 = new Vector2(rect.Left, rect.Bottom);
            var ct = rect.Center;

            FindFirstGroundPosition<Triangle>(c0.X, c0.Y, out var r0);
            FindFirstGroundPosition<Triangle>(c1.X, c1.Y, out var r1);
            FindFirstGroundPosition<Triangle>(c2.X, c2.Y, out var r2);
            FindFirstGroundPosition<Triangle>(c3.X, c3.Y, out var r3);
            FindFirstGroundPosition<Triangle>(ct.X, ct.Y, out var rt);

            lines.AddRange(Line3D.CreateWiredSquare(new[] { r0.Position, r1.Position, r2.Position, r3.Position }));

            float r = Vector3.Distance(r0.Position, r2.Position) * 0.5f;
            grid.QueryItems(rt.Position, r, out var items);
            foreach (var item in items)
            {
                lines.AddRange(Line3D.CreateCircle(item.RealPosition, item.Radius, 32));
            }

            lines.AddRange(Line3D.CreateCircle(rt.Position, r, 64));

            lineDrawer.SetPrimitives(Color.Orange, lines);
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            help.SetPosition(new Vector2(0, title.Top + title.Height + 2));

            upperPanel.Width = Game.Form.RenderWidth;
            upperPanel.Height = help.Top + help.Height + 3;
        }

        public override void NavigationGraphUpdated()
        {
            UpdateGraphNodes(tankAgentType);

            if (NavigationGraph is not Graph graph)
            {
                return;
            }

            this.graph = graph;

            var settings = new CrowdParameters(tankAgentType, tankAgents.Count);

            crowd = graph.AddCrowd(settings);

            var par = new CrowdAgentParameters()
            {
                Radius = tankAgentType.Radius,
                Height = tankAgentType.Height,
                MaxAcceleration = 1f,
                MaxSpeed = 15f,
                CollisionQueryRange = tankAgentType.Radius * 12,
                PathOptimizationRange = tankAgentType.Radius * 30,
                UpdateFlags =
                    UpdateFlagTypes.DT_CROWD_OBSTACLE_AVOIDANCE |
                    UpdateFlagTypes.DT_CROWD_ANTICIPATE_TURNS,
                SeparationWeight = 3,
                ObstacleAvoidanceType = 0,
                QueryFilterTypeIndex = 0
            };

            for (int i = 0; i < tankAgents.Count; i++)
            {
                tankAgents[i].CrowdAgent = Graph.AddCrowdAgent(crowd, tankAgents[i].Manipulator.Position, par);

                graph.EnableDebugInfo(crowd, tankAgents[i].CrowdAgent);
            }
        }
        private void UpdateGraphNodes(Agent agent)
        {
            terrainGraphDrawer.Clear();

            var nodes = GetNodes(agent).OfType<GraphNode>();
            if (!nodes.Any())
            {
                return;
            }

            foreach (var node in nodes)
            {
                terrainGraphDrawer.AddPrimitives(node.Color, node.Triangles);
            }
        }
    }
}
