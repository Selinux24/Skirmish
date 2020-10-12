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
    class SceneNavmeshTest : Scene
    {
        private const int layerHUD = 99;

        private readonly string resourcesFolder = "navmeshtest/resources";

        private Player agent = null;

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

        }

        public override async Task Initialize()
        {
            await base.Initialize();

            GameEnvironment.Background = new Color4(0.09f, 0.09f, 0.09f, 1f);

            Game.VisibleMouse = true;
            Game.LockMouse = false;

            Camera.MovementDelta = 25f;

            await LoadResourcesAsync(
                InitializeText(),
                (resUi) =>
                {
                    if (!resUi.Completed)
                    {
                        resUi.ThrowExceptions();
                    }

                    InitializeLights();
                    InitializeAgent();

                    _ = LoadResourcesAsync(
                        new[]
                        {
                            InitializeNavmesh(),
                            InitializeDebug()
                        },
                        (res) =>
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

                            Task.WhenAll(UpdateNavigationGraph());

                            gameReady = true;
                        });
                });
        }
        private async Task InitializeText()
        {
            var title = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18, Color.White) }, layerHUD);
            title.Text = "Navigation Mesh Test Scene";
            title.SetPosition(Vector2.Zero);

            debug = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Green) }, layerHUD);
            debug.Text = null;
            debug.SetPosition(new Vector2(0, title.Top + title.Height + 3));

            help = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) }, layerHUD);
            help.Text = @"Camera: WASD+Mouse (Press right mouse in windowed mode to look). 
B: Change Build Mode (SHIFT reverse).
P: Change Partition Type (SHIFT reverse).
T: Toggle using Tile Cache.
F5: Saves the graph to a file.
F6: Loads the graph from a file.
Left Mouse: Update current tile (SHIFT remove).
Middle Mouse: Finds random point around circle (5 units).
Space: Finds random over navmesh";
            help.SetPosition(new Vector2(0, debug.Top + debug.Height + 3));
            help.Visible = false;

            var spDesc = new SpriteDescription()
            {
                Width = Game.Form.RenderWidth,
                Height = debug.Top + debug.Height + 3,
                BaseColor = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);
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
        private async Task InitializeNavmesh()
        {
            inputGeometry = await this.AddComponentModel(
                new ModelDescription()
                {
                    TextureIndex = 0,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(resourcesFolder, "modular_dungeon.xml"),
                },
                SceneObjectUsages.Ground);

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
                Name = "DEBUG++ Graph",
                Count = 50000,
            };
            graphDrawer = await this.AddComponentPrimitiveListDrawer(graphDrawerDesc);

            var volumesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 10000
            };
            volumesDrawer = await this.AddComponentPrimitiveListDrawer(volumesDrawerDesc);
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

            if (Game.Input.RightMouseButtonPressed)
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
        }
        private void UpdateNavmeshInput()
        {
            if (Game.Input.MiddleMouseButtonJustReleased)
            {
                var pRay = GetPickingRay();
                var rayPParams = RayPickingParams.FacingOnly | RayPickingParams.Perfect;

                if (PickNearest(pRay, rayPParams, out PickingResult<Triangle> r))
                {
                    DrawPoint(r.Position, 0.25f, Color.Red);
                    DrawTriangle(r.Item, Color.White);

                    float radius = 5;

                    DrawCircle(r.Position, radius, Color.Orange);

                    var pt = NavigationGraph.FindRandomPoint(agent, r.Position, radius);
                    if (pt.HasValue)
                    {
                        float dist = Vector3.Distance(r.Position, pt.Value);
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

            if (Game.Input.LeftMouseButtonJustReleased)
            {
                var pRay = GetPickingRay();
                var rayPParams = RayPickingParams.FacingOnly | RayPickingParams.Perfect;

                if (PickNearest(pRay, rayPParams, out PickingResult<Triangle> r))
                {
                    DrawPoint(r.Position, 0.25f, Color.Red);
                    DrawTriangle(r.Item, Color.White);

                    ToggleTile(r.Position);
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
            graphDrawer.Clear();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (!Game.Input.ShiftPressed)
            {
                NavigationGraph.CreateAt(tilePosition);
            }
            else
            {
                NavigationGraph.RemoveAt(tilePosition);
            }
            sw.Stop();
            lastElapsedSeconds = sw.ElapsedMilliseconds / 1000.0f;
        }
    }
}
