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

namespace Collada
{
    /// <summary>
    /// Navigation mesh test scene
    /// </summary>
    class SceneNavmeshTest : Scene
    {
        private const int layerHUD = 99;

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

            this.Game.VisibleMouse = true;
            this.Game.LockMouse = false;

            this.Camera.MovementDelta = 25f;

            await this.LoadResourcesAsync(
                this.InitializeText(),
                () =>
                {
                    this.InitializeLights();
                    this.InitializeAgent();

                    _ = this.LoadResourcesAsync(
                        new[]
                        {
                            this.InitializeNavmesh(),
                            this.InitializeDebug()
                        },
                        () =>
                        {
                            var bbox = inputGeometry.GetBoundingBox();
                            var center = bbox.GetCenter();
                            float maxD = Math.Max(Math.Max(bbox.GetX(), bbox.GetY()), bbox.GetZ());

                            this.Camera.Interest = center;
                            this.Camera.Position = center + new Vector3(1, 0.8f, -1) * maxD * 0.8f;

                            Task.WhenAll(this.UpdateNavigationGraph());

                            gameReady = true;
                        });
                });
        }
        private async Task InitializeText()
        {
            var title = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.Default("Tahoma", 18, Color.White) }, layerHUD);
            title.Text = "Navigation Mesh Test Scene";
            title.SetPosition(Vector2.Zero);

            this.debug = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.Default("Lucida Sans", 12, Color.Green) }, layerHUD);
            this.debug.Text = null;
            this.debug.SetPosition(new Vector2(0, title.Top + title.Height + 3));

            this.help = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.Default("Lucida Sans", 12, Color.Yellow) }, layerHUD);
            this.help.Text = @"Camera: WASD+Mouse (Press right mouse in windowed mode to look). 
B: Change Build Mode (SHIFT reverse).
P: Change Partition Type (SHIFT reverse).
T: Toggle using Tile Cache.
F5: Saves the graph to a file.
F6: Loads the graph from a file.
Left Mouse: Update current tile (SHIFT remove).
Middle Mouse: Finds random point around circle (5 units).
Space: Finds random over navmesh";
            this.help.SetPosition(new Vector2(0, debug.Top + debug.Height + 3));
            this.help.Visible = false;

            var spDesc = new SpriteDescription()
            {
                Width = this.Game.Form.RenderWidth,
                Height = this.debug.Top + this.debug.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private void InitializeLights()
        {
            this.Lights.KeyLight.CastShadow = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = false;
        }
        private void InitializeAgent()
        {
            this.agent = new Player()
            {
                Name = "Player",
                Height = 0.2f,
                Radius = 0.1f,
                MaxClimb = 0.5f,
                MaxSlope = 50f,
                Velocity = 3f,
                VelocitySlow = 1f,
            };

            this.Camera.NearPlaneDistance = 0.01f;
            this.Camera.FarPlaneDistance *= 2;
        }
        private async Task InitializeNavmesh()
        {
            this.inputGeometry = await this.AddComponentModel(
                new ModelDescription()
                {
                    TextureIndex = 0,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/SceneNavmeshTest",
                        ModelContentFilename = "modular_dungeon.xml",
                    }
                },
                SceneObjectUsages.Ground);

            this.SetGround(inputGeometry, true);

            //Rasterization
            nmsettings.CellSize = 0.20f;
            nmsettings.CellHeight = 0.15f;

            //Agents
            nmsettings.Agents = new[] { this.agent };

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypes.Monotone;

            //Polygonization
            nmsettings.EdgeMaxError = 1.0f;

            //Tiling
            nmsettings.BuildMode = BuildModes.Tiled;
            nmsettings.TileSize = 64;

            var nminput = new InputGeometry(GetTrianglesForNavigationGraph);

            this.PathFinderDescription = new PathFinderDescription(nmsettings, nminput);
        }
        private async Task InitializeDebug()
        {
            var graphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "DEBUG++ Graph",
                Count = 50000,
            };
            this.graphDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(graphDrawerDesc);

            var volumesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 10000
            };
            this.volumesDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(volumesDrawerDesc);
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
            this.DrawGraphNodes(this.agent);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.help.Visible = !this.help.Visible;
            }

            this.UpdateCameraInput();
            this.UpdateNavmeshInput();
            this.UpdateGraphInput(shift);
            this.UpdateFilesInput();
        }
        private void UpdateCameraInput()
        {
            bool slow = this.Game.Input.KeyPressed(Keys.LShiftKey);

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(this.Game.GameTime, slow);
            }

            if (this.Game.Input.RightMouseButtonPressed)
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
        }
        private void UpdateNavmeshInput()
        {
            if (this.Game.Input.MiddleMouseButtonJustReleased)
            {
                var pRay = this.GetPickingRay();
                var rayPParams = RayPickingParams.FacingOnly | RayPickingParams.Perfect;

                if (this.PickNearest(pRay, rayPParams, out PickingResult<Triangle> r))
                {
                    DrawPoint(r.Position, 0.25f, Color.Red);
                    DrawTriangle(r.Item, Color.White);

                    float radius = 5;

                    DrawCircle(r.Position, radius, Color.Orange);

                    var pt = this.NavigationGraph.FindRandomPoint(agent, r.Position, radius);
                    if (pt.HasValue)
                    {
                        float dist = Vector3.Distance(r.Position, pt.Value);
                        Color color = dist < radius ? Color.LightGreen : Color.Pink;
                        DrawPoint(pt.Value, 2.5f, color);
                    }
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                var pt = this.NavigationGraph.FindRandomPoint(agent);
                if (pt.HasValue)
                {
                    DrawPoint(pt.Value, 2.5f, Color.LightGreen);
                }
            }
        }
        private void UpdateGraphInput(bool shift)
        {
            bool updateGraph = false;

            if (this.Game.Input.LeftMouseButtonJustReleased)
            {
                var pRay = this.GetPickingRay();
                var rayPParams = RayPickingParams.FacingOnly | RayPickingParams.Perfect;

                if (this.PickNearest(pRay, rayPParams, out PickingResult<Triangle> r))
                {
                    DrawPoint(r.Position, 0.25f, Color.Red);
                    DrawTriangle(r.Item, Color.White);

                    ToggleTile(shift, r.Position);
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.B))
            {
                var buildModes = Enum.GetNames(typeof(BuildModes)).Length;

                if (!shift)
                {
                    nmsettings.BuildMode = (BuildModes)Helper.Next((int)nmsettings.BuildMode, buildModes);
                }
                else
                {
                    nmsettings.BuildMode = (BuildModes)Helper.Prev((int)nmsettings.BuildMode, buildModes);
                }
                updateGraph = true;
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                var sampleTypes = Enum.GetNames(typeof(SamplePartitionTypes)).Length;

                if (!shift)
                {
                    nmsettings.PartitionType = (SamplePartitionTypes)Helper.Next((int)nmsettings.PartitionType, sampleTypes);
                }
                else
                {
                    nmsettings.PartitionType = (SamplePartitionTypes)Helper.Prev((int)nmsettings.PartitionType, sampleTypes);
                }
                updateGraph = true;
            }

            if (this.Game.Input.KeyJustReleased(Keys.T))
            {
                nmsettings.UseTileCache = !nmsettings.UseTileCache;

                updateGraph = true;
            }

            if (updateGraph)
            {
                _ = this.UpdateNavigationGraph();
            }

            this.debug.Text = string.Format("Build Mode: {0}; Partition Type: {1}; Build Time: {2:0.00000} seconds", nmsettings.BuildMode, nmsettings.PartitionType, lastElapsedSeconds);
        }
        private void UpdateFilesInput()
        {
            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                using (var dlg = new System.Windows.Forms.SaveFileDialog())
                {
                    dlg.FileName = @"test.grf";

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Task.Run(() => this.PathFinderDescription.Save(dlg.FileName, this.NavigationGraph));
                    }
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                using (var dlg = new System.Windows.Forms.OpenFileDialog())
                {
                    dlg.FileName = @"test.grf";

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var graphTask = Task.Run(() => this.PathFinderDescription.Load(dlg.FileName));
                        this.SetNavigationGraph(graphTask.Result);
                    }
                }
            }
        }

        private void DrawPoint(Vector3 position, float size, Color color)
        {
            var cross = Line3D.CreateCross(position, size);
            this.volumesDrawer.SetPrimitives(color, cross);
        }
        private void DrawTriangle(Triangle triangle, Color color)
        {
            var tri = Line3D.CreateWiredTriangle(triangle);
            this.volumesDrawer.SetPrimitives(color, tri);
        }
        private void DrawCircle(Vector3 position, float radius, Color color)
        {
            var circle = Line3D.CreateCircle(position, radius, 12);
            this.volumesDrawer.SetPrimitives(color, circle);
        }
        private void DrawGraphNodes(AgentType agent)
        {
            var nodes = this.GetNodes(agent).OfType<GraphNode>();
            if (nodes.Any())
            {
                this.graphDrawer.Clear();

                foreach (var node in nodes)
                {
                    this.graphDrawer.AddPrimitives(node.Color, node.Triangles);
                }
            }
        }

        private void ToggleTile(bool shift, Vector3 tilePosition)
        {
            this.graphDrawer.Clear();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (!shift)
            {
                this.NavigationGraph.CreateAt(tilePosition);
            }
            else
            {
                this.NavigationGraph.RemoveAt(tilePosition);
            }
            sw.Stop();
            lastElapsedSeconds = sw.ElapsedMilliseconds / 1000.0f;
        }
    }
}
