using Engine;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.UI;
using SharpDX;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Collada.NavmeshTest
{
    /// <summary>
    /// Navigation mesh test scene
    /// </summary>
    class SceneNavmeshTest : WalkableScene
    {
        private readonly string resourcesFolder = "navmeshtest/resources";

        private Player agent = null;

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea debug = null;
        private UITextArea help = null;

        private PrimitiveListDrawer<Triangle> graphDrawer = null;
        private PrimitiveListDrawer<Line3D> volumesDrawer = null;

        private Model inputGeometry = null;
        private readonly BuildSettings nmsettings = BuildSettings.Default;

        private float? lastElapsedSeconds = null;

        private bool gameReady = false;

        public SceneNavmeshTest(Game game) : base(game)
        {
            GameEnvironment.Background = new Color4(0.09f, 0.09f, 0.09f, 1f);

            Game.VisibleMouse = true;
            Game.LockMouse = false;

            Camera.MovementDelta = 25f;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            LoadResourcesAsync(
                InitializeText(),
                InitializeComponentsCompleted);
        }
        private async Task InitializeText()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Tahoma", 18);
            var defaultFont12 = TextDrawerDescription.FromFamily("Tahoma", 12);
            defaultFont18.LineAdjust = true;
            defaultFont12.LineAdjust = true;

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            title.Text = "Navigation Mesh Test Scene";

            debug = await AddComponentUI<UITextArea, UITextAreaDescription>("Debug", "Debug", new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Green });
            debug.Text = null;

            help = await AddComponentUI<UITextArea, UITextAreaDescription>("Help", "Help", new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow });
            help.Text = @"Camera: WASD+Mouse (Press right mouse in windowed mode to look). 
B: Change Build Mode (SHIFT reverse).
P: Change Partition Type (SHIFT reverse).
T: Toggle using Tile Cache.
F5: Saves the graph to a file.
F6: Loads the graph from a file.
Left Mouse: Update current tile (SHIFT remove, CTRL add).
Middle Mouse: Finds random point around circle (5 units).
Space: Finds random over navmesh";
            help.Visible = false;

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Backpanel", "Backpanel", spDesc, LayerUI - 1);
        }
        private void InitializeComponentsCompleted(LoadResourcesResult resUi)
        {
            if (!resUi.Completed)
            {
                resUi.ThrowExceptions();
            }

            UpdateLayout();
            InitializeLights();
            InitializeAgent();

            InitializeMapData();
        }
        private void InitializeLights()
        {
            Lights.KeyLight.CastShadow = false;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = false;
        }
        private void InitializeAgent()
        {
            agent = new Player()
            {
                Name = "Player",
                Height = 0.2f,
                Radius = 0.1f,
                MaxClimb = 0.5f,
                MaxSlope = 50f,
                Velocity = 3f,
                VelocitySlow = 1f,
            };

            Camera.NearPlaneDistance = 0.01f;
            Camera.FarPlaneDistance *= 2;
        }

        private void InitializeMapData()
        {
            LoadResourcesAsync(
                new[]
                {
                    InitializeNavmesh(),
                    InitializeDebug()
                },
                InitializeMapDataCompleted);
        }
        private async Task InitializeNavmesh()
        {
            var contentDesc = ContentDescription.FromFile(resourcesFolder, "modular_dungeon.json");
            var desc = new ModelDescription()
            {
                TextureIndex = 0,
                CastShadow = true,
                UseAnisotropicFiltering = true,
                Content = contentDesc,
            };

            inputGeometry = await AddComponentGround<Model, ModelDescription>("NavMesh", "NavMesh", desc);

            SetGround(inputGeometry, true);

            //Rasterization
            nmsettings.CellSize = 0.20f;
            nmsettings.CellHeight = 0.15f;

            //Agents
            nmsettings.Agents = new[] { agent };

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypes.Monotone;

            //Polygonization
            nmsettings.EdgeMaxError = 1.0f;

            //Tiling
            nmsettings.BuildMode = BuildModes.Tiled;
            nmsettings.TileSize = 64;

            var nminput = new InputGeometry(GetTrianglesForNavigationGraph);

            PathFinderDescription = new PathFinderDescription(nmsettings, nminput);
        }
        private async Task InitializeDebug()
        {
            var graphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = 50000,
            };
            graphDrawer = await AddComponent<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("DEBUG++ Graph", "DEBUG++ Graph", graphDrawerDesc);

            var volumesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 10000
            };
            volumesDrawer = await AddComponent<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("DEBUG++ Volumes", "DEBUG++ Volumes", volumesDrawerDesc);
        }
        private async Task InitializeMapDataCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            var bbox = inputGeometry.GetBoundingBox();
            var center = bbox.GetCenter();
            float maxD = Math.Max(Math.Max(bbox.Width, bbox.Height), bbox.Depth);

            Camera.Interest = center;
            Camera.Position = center + new Vector3(1, 0.8f, -1) * maxD * 0.8f;

            await UpdateNavigationGraph();

            gameReady = true;
        }

        public override async Task UpdateNavigationGraph()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await base.UpdateNavigationGraph();
            sw.Stop();
            lastElapsedSeconds = sw.ElapsedMilliseconds / 1000.0f;
        }
        public override void NavigationGraphUpdated()
        {
            DrawGraphNodes(agent);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<Start.SceneStart>();
            }

            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                help.Visible = !help.Visible;
            }

            UpdateCameraInput();
            UpdateNavmeshInput();
            UpdateGraphInput();
            UpdateFilesInput();
        }
        private void UpdateCameraInput()
        {
            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
        }
        private void UpdateNavmeshInput()
        {
            if (Game.Input.MouseButtonJustReleased(MouseButtons.Middle))
            {
                var pRay = GetPickingRay(RayPickingParams.Perfect);

                if (this.PickNearest(pRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r))
                {
                    DrawPoint(r.PickingResult.Position, 0.25f, Color.Red);
                    DrawTriangle(r.PickingResult.Primitive, Color.White);

                    float radius = 5;

                    DrawCircle(r.PickingResult.Position, radius, Color.Orange);

                    var pt = NavigationGraph.FindRandomPoint(agent, r.PickingResult.Position, radius);
                    if (pt.HasValue)
                    {
                        float dist = Vector3.Distance(r.PickingResult.Position, pt.Value);
                        Color color = dist < radius ? Color.LightGreen : Color.Pink;
                        DrawPoint(pt.Value, 2.5f, color);
                    }
                }
            }

            if (Game.Input.KeyJustReleased(Keys.Space))
            {
                var pt = NavigationGraph.FindRandomPoint(agent);
                if (pt.HasValue)
                {
                    DrawPoint(pt.Value, 2.5f, Color.LightGreen);
                }
            }
        }
        private void UpdateGraphInput()
        {
            bool updateGraph = false;

            if (Game.Input.MouseButtonJustReleased(MouseButtons.Left))
            {
                var pRay = GetPickingRay(RayPickingParams.Perfect);

                if (this.PickNearest(pRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r))
                {
                    volumesDrawer.Clear(Color.Red);
                    volumesDrawer.Clear(Color.Green);
                    volumesDrawer.Clear(Color.Gray);
                    volumesDrawer.Clear(Color.White);

                    var pColor = Color.Red;
                    if (IsWalkable(agent, r.PickingResult.Position, out var nearest))
                    {
                        pColor = Color.Green;
                    }

                    if (nearest.HasValue)
                    {
                        DrawPoint(nearest.Value + new Vector3(0.02f), 0.45f, Color.Gray);
                    }

                    DrawPoint(r.PickingResult.Position, 0.25f, pColor);
                    DrawTriangle(r.PickingResult.Primitive, Color.White);

                    ToggleTile(r.PickingResult.Position);
                }
            }

            if (Game.Input.KeyJustReleased(Keys.B))
            {
                var buildModes = Enum.GetNames(typeof(BuildModes)).Length;

                if (!Game.Input.ShiftPressed)
                {
                    nmsettings.BuildMode = (BuildModes)Helper.Next((int)nmsettings.BuildMode, buildModes);
                }
                else
                {
                    nmsettings.BuildMode = (BuildModes)Helper.Prev((int)nmsettings.BuildMode, buildModes);
                }
                updateGraph = true;
            }

            if (Game.Input.KeyJustReleased(Keys.P))
            {
                var sampleTypes = Enum.GetNames(typeof(SamplePartitionTypes)).Length;

                if (!Game.Input.ShiftPressed)
                {
                    nmsettings.PartitionType = (SamplePartitionTypes)Helper.Next((int)nmsettings.PartitionType, sampleTypes);
                }
                else
                {
                    nmsettings.PartitionType = (SamplePartitionTypes)Helper.Prev((int)nmsettings.PartitionType, sampleTypes);
                }
                updateGraph = true;
            }

            if (Game.Input.KeyJustReleased(Keys.T))
            {
                nmsettings.UseTileCache = !nmsettings.UseTileCache;

                updateGraph = true;
            }

            if (updateGraph)
            {
                _ = UpdateNavigationGraph();
            }

            debug.Text = string.Format("Build Mode: {0}; Partition Type: {1}; Build Time: {2:0.00000} seconds", nmsettings.BuildMode, nmsettings.PartitionType, lastElapsedSeconds);
        }
        private void UpdateFilesInput()
        {
            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                using (var dlg = new System.Windows.Forms.SaveFileDialog())
                {
                    dlg.FileName = @"test.grf";

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Task.Run(() => PathFinderDescription.Save(dlg.FileName, NavigationGraph));
                    }
                }
            }

            if (Game.Input.KeyJustReleased(Keys.F6))
            {
                using (var dlg = new System.Windows.Forms.OpenFileDialog())
                {
                    dlg.FileName = @"test.grf";

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var graphTask = Task.Run(() => PathFinderDescription.Load(dlg.FileName));
                        SetNavigationGraph(graphTask.Result);
                    }
                }
            }
        }

        private void DrawPoint(Vector3 position, float size, Color color)
        {
            var cross = Line3D.CreateCross(position, size);
            volumesDrawer.SetPrimitives(color, cross);
        }
        private void DrawTriangle(Triangle triangle, Color color)
        {
            var tri = Line3D.CreateWiredTriangle(triangle);
            volumesDrawer.SetPrimitives(color, tri);
        }
        private void DrawCircle(Vector3 position, float radius, Color color)
        {
            var circle = Line3D.CreateCircle(position, radius, 12);
            volumesDrawer.SetPrimitives(color, circle);
        }
        private void DrawGraphNodes(AgentType agent)
        {
            var nodes = GetNodes(agent).OfType<GraphNode>();
            if (nodes.Any())
            {
                graphDrawer.Clear();

                foreach (var node in nodes)
                {
                    graphDrawer.AddPrimitives(node.Color, node.Triangles);
                }
            }
        }

        private void ToggleTile(Vector3 tilePosition)
        {
            bool remove = Game.Input.ShiftPressed;
            bool create = Game.Input.ControlPressed;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                if (create)
                {
                    NavigationGraph.CreateAt(tilePosition);
                    return;
                }

                if (remove)
                {
                    NavigationGraph.RemoveAt(tilePosition);
                    return;
                }

                NavigationGraph.UpdateAt(tilePosition);
            }
            finally
            {
                sw.Stop();
                lastElapsedSeconds = sw.ElapsedMilliseconds / 1000.0f;
            }
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();
            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            debug.SetPosition(new Vector2(0, title.Top + title.Height + 3));
            help.SetPosition(new Vector2(0, debug.Top + debug.Height + 3));
            panel.Width = Game.Form.RenderWidth;
            panel.Height = debug.Top + debug.Height + 3;
        }
    }
}
