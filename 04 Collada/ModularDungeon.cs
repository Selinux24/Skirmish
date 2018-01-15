using Engine;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.NavMesh;
using SharpDX;
using System;

namespace Collada
{
    public class ModularDungeon : Scene
    {
        private const int layerHUD = 99;
        private const int layerEffects = 98;

        private const float maxDistance = 35;

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> fps = null;
        private SceneObject<TextDrawer> picks = null;
        private SceneObject<Sprite> backPannel = null;

        private Player agent = null;
        //private Player2 agent = null;

        private SceneObject<ModularScenery> scenery = null;

        private SceneObject<TriangleListDrawer> graphDrawer = null;
        private SceneObject<LineListDrawer> bboxesDrawer = null;

        public ModularDungeon(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.InitializeUI();
            this.InitializeModularScenery();
            this.InitializeEnvironment();
            this.InitializeDebug();
            this.InitializeCamera();
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.Black;

            this.Lights.KeyLight.Enabled = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = true;
            this.Lights.FillLight.Direction = Vector3.Down;
            this.Lights.FillLight.Brightness *= 0.25f;

            this.Lights.BaseFogColor = Color.Black;
            this.Lights.FogRange = 10f;
            this.Lights.FogStart = maxDistance - 20f;
        }
        private void InitializeUI()
        {
            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsageEnum.UI, layerHUD);
            this.title.Instance.Text = "Collada Modular Dungeon Scene";
            this.title.Instance.Position = Vector2.Zero;

            this.fps = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.fps.Instance.Text = null;
            this.fps.Instance.Position = new Vector2(0, 24);

            this.picks = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.picks.Instance.Text = null;
            this.picks.Instance.Position = new Vector2(0, 48);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.picks.Instance.Top + this.picks.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHUD - 1);
        }
        private void InitializeModularScenery()
        {
            var desc = new ModularSceneryDescription()
            {
                Name = "Dungeon",
                UseAnisotropic = true,
                CastShadow = true,
                AlphaEnabled = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "Resources/ModularDungeon",
                    ModelContentFilename = "assets.xml",
                },
                AssetsConfigurationFile = "assetsmap.xml",
            };

            this.scenery = this.AddComponent<ModularScenery>(desc, SceneObjectUsageEnum.Ground);

            this.SetGround(this.scenery, true);

            this.agent = new Player()
            {
                Name = "Player",
                Height = 1.5f,
                MaxClimb = 0.8f,
                MaxSlope = 45f,
                Radius = 0.5f,
                Velocity = 4f,
                VelocitySlow = 1f,
            };

            this.PathFinderDescription = new PathFinderDescription()
            {
                Settings = new NavigationMeshGenerationSettings()
                {
                    Agents = new[] { agent },
                    CellSize = 0.1f,
                    CellHeight = 0.1f,
                    ContourFlags = ContourBuildFlags.TessellateAreaEdges,
                }
            };

            /*
            this.agent = new Player2()
            {
                Name = "Player",
                Height = 1.5f,
                MaxClimb = 0.8f,
                MaxSlope = 45f,
                Radius = 0.5f,
                Velocity = 4f,
                VelocitySlow = 1f,
            };

            var nmsettings = Engine.PathFinding.NavMesh2.BuildSettings.Default;
            nmsettings.Agents = new[] { this.agent };

            this.PathFinderDescription = new PathFinderDescription()
            {
                Settings = nmsettings
            };
            */
        }
        private void InitializeDebug()
        {
            var graphDrawerDesc = new TriangleListDrawerDescription()
            {
                Name = "DEBUG++ Graph",
                AlphaEnabled = true,
                Count = 10000,
            };
            this.graphDrawer = this.AddComponent<TriangleListDrawer>(graphDrawerDesc);
            this.graphDrawer.Visible = false;

            var bboxesDrawerDesc = new LineListDrawerDescription()
            {
                Name = "DEBUG++ Bounding volumes",
                AlphaEnabled = true,
                Color = new Color4(1.0f, 0.0f, 0.0f, 0.55f),
                Count = 10000,
            };
            this.bboxesDrawer = this.AddComponent<LineListDrawer>(bboxesDrawerDesc);
            this.bboxesDrawer.Visible = false;
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = maxDistance;
            this.Camera.MovementDelta = this.agent.Velocity;
            this.Camera.SlowMovementDelta = this.agent.VelocitySlow;
            this.Camera.Mode = CameraModes.Free;
            this.Camera.Position = new Vector3(-8, 5.5f, -26);
            this.Camera.Interest = new Vector3(-6, 5.5f, -26);
        }

        public override void Initialized()
        {
            base.Initialized();

            //Graph
            {
                var nodes = this.GetNodes(this.agent);
                if (nodes != null && nodes.Length > 0)
                {
                    Random clrRnd = new Random(24);
                    Color[] regions = new Color[nodes.Length];
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        regions[i] = new Color(clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), 0.4f);
                    }

                    this.graphDrawer.Instance.Clear();

                    for (int i = 0; i < nodes.Length; i++)
                    {
                        var node = (NavigationMeshNode)nodes[i];
                        var color = regions[node.RegionId];
                        var poly = node.Poly;
                        var tris = poly.Triangulate();

                        this.graphDrawer.Instance.AddTriangles(color, tris);
                    }
                }
            }

            //Boxes
            {
                Random rndBoxes = new Random(1);

                var dict = this.scenery.Instance.GetObjectVolumes();

                foreach (var item in dict.Values)
                {
                    this.bboxesDrawer.Instance.SetLines(rndBoxes.NextColor(), Line3D.CreateWiredBox(item.ToArray()));
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.graphDrawer.Visible = !this.graphDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.bboxesDrawer.Visible = !this.bboxesDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning);
            }

            this.UpdateCamera(gameTime);

            this.fps.Instance.Text = this.Game.RuntimeText;
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

            Vector3 walkerPos;
            if (this.Walk(this.agent, prevPos, this.Camera.Position, out walkerPos))
            {
                this.Camera.Goto(walkerPos);
            }
            else
            {
                this.Camera.Goto(prevPos);
            }
        }
    }
}
