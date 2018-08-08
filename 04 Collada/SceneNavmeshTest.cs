using Engine;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using SharpDX;
using System;
using System.Diagnostics;

namespace Collada
{
    /// <summary>
    /// Navigation mesh test scene
    /// </summary>
    class SceneNavmeshTest : Scene
    {
        private const int layerHUD = 99;

        private Player agent = null;

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> help = null;
        private SceneObject<TextDrawer> debug = null;
        private SceneObject<Sprite> backPannel = null;

        private SceneObject<TriangleListDrawer> graphDrawer = null;

        private SceneObject<Model> inputGeometry = null;
        private BuildSettings nmsettings = BuildSettings.Default;

        private float? lastElapsedSeconds = null;

        public SceneNavmeshTest(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

#if DEBUG
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;
#else
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = true;
#endif

            this.Lights.KeyLight.CastShadow = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = false;

            this.InitializeText();
            this.InitializeAgent();
            this.InitializeNavmesh();
            this.InitializeDebug();
        }
        private void InitializeText()
        {
            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsageEnum.UI, layerHUD);
            this.title.Instance.Text = "Navigation Mesh Test Scene";
            this.title.Instance.Position = Vector2.Zero;

            this.help = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.help.Instance.Text = "Camera: WASD+Mouse. B: Change Build Mode. P: Change Partition Type. (SHIFT reverse). F5: Save. F6: Load. Space: Update current tile (SHIFT remove).";
            this.help.Instance.Position = new Vector2(0, 24);

            this.debug = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.debug.Instance.Text = null;
            this.debug.Instance.Position = new Vector2(0, 48);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.debug.Instance.Top + this.debug.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHUD - 1);
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
            this.Camera.MovementDelta = agent.Velocity;
            this.Camera.SlowMovementDelta = agent.VelocitySlow;
        }
        private void InitializeNavmesh()
        {
            //Rasterization
            nmsettings.CellSize = 0.20f;
            nmsettings.CellHeight = 0.15f;

            //Agents
            nmsettings.Agents = new[] { this.agent };

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypeEnum.Watershed;

            //Polygonization
            nmsettings.EdgeMaxError = 1.0f;

            //Tiling
            nmsettings.BuildMode = BuildModesEnum.Tiled;
            nmsettings.TileSize = 32;

            this.PathFinderDescription = new PathFinderDescription()
            {
                Settings = nmsettings,
            };

            this.inputGeometry = this.AddComponent<Model>(
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
                SceneObjectUsageEnum.Ground);

            this.SetGround(inputGeometry, true);
        }
        private void InitializeDebug()
        {
            var graphDrawerDesc = new TriangleListDrawerDescription()
            {
                Name = "DEBUG++ Graph",
                AlphaEnabled = true,
                Count = 50000,
            };
            this.graphDrawer = this.AddComponent<TriangleListDrawer>(graphDrawerDesc);
        }

        public override void Initialized()
        {
            base.Initialized();

            this.UpdateGraphNodes(this.agent);

            var bbox = inputGeometry.Instance.GetBoundingBox();
            var center = bbox.GetCenter();
            float maxD = Math.Max(Math.Max(bbox.GetX(), bbox.GetY()), bbox.GetZ());

            this.Camera.Interest = center;
            this.Camera.Position = center + new Vector3(1, 0.8f, -1) * maxD * 0.8f;
        }

        public override void UpdateNavigationGraph()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            base.UpdateNavigationGraph();
            sw.Stop();
            lastElapsedSeconds = sw.ElapsedMilliseconds / 1000.0f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            this.UpdateCamera(gameTime);

            this.UpdateGraph(gameTime);
        }
        private void UpdateCamera(GameTime gameTime)
        {
            bool slow = this.Game.Input.KeyPressed(Keys.LShiftKey);

            var prevPos = this.Camera.Position;

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

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
        }
        private void UpdateGraph(GameTime gameTime)
        {
            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.navigationGraph.Save(@"test.grf");
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                this.navigationGraph.Load(@"test.grf");
                this.UpdateGraphNodes(this.agent);
            }

            bool updateGraph = false;
            bool updateGraphDrawing = false;

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (!shift)
                {
                    ((Graph)this.navigationGraph).BuildTile(this.Camera.Position);
                }
                else
                {
                    ((Graph)this.navigationGraph).RemoveTile(this.Camera.Position);
                }
                sw.Stop();
                lastElapsedSeconds = sw.ElapsedMilliseconds / 1000.0f;

                updateGraphDrawing = true;
            }

            if (this.Game.Input.KeyJustReleased(Keys.B))
            {
                if (!shift)
                {
                    nmsettings.BuildMode = (BuildModesEnum)Helper.Next((int)nmsettings.BuildMode, 3);
                }
                else
                {
                    nmsettings.BuildMode = (BuildModesEnum)Helper.Prev((int)nmsettings.BuildMode, 3);
                }
                updateGraph = true;
                updateGraphDrawing = true;
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                if (!shift)
                {
                    nmsettings.PartitionType = (SamplePartitionTypeEnum)Helper.Next((int)nmsettings.PartitionType, 3);
                }
                else
                {
                    nmsettings.PartitionType = (SamplePartitionTypeEnum)Helper.Prev((int)nmsettings.PartitionType, 3);
                }
                updateGraph = true;
                updateGraphDrawing = true;
            }

            if (updateGraph)
            {
                this.UpdateNavigationGraph();
            }

            if (updateGraphDrawing)
            {
                this.UpdateGraphNodes(this.agent);
            }

            this.debug.Instance.Text = string.Format("Build Mode: {0}; Partition Type: {1}; Build Time: {2:0.00000} seconds", nmsettings.BuildMode, nmsettings.PartitionType, lastElapsedSeconds);
        }
        private void UpdateGraphNodes(AgentType agent)
        {
            var nodes = this.GetNodes(agent);
            if (nodes != null && nodes.Length > 0)
            {
                this.graphDrawer.Instance.Clear();

                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = (GraphNode)nodes[i];
                    var color = node.Color;
                    var tris = node.Triangles;

                    this.graphDrawer.Instance.AddTriangles(color, tris);
                }
            }
        }
    }
}
