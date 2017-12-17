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

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> fps = null;
        private SceneObject<TextDrawer> picks = null;
        private SceneObject<Sprite> backPannel = null;

        private Player agent = null;

        private SceneObject<ModularScenery> scenery = null;

        private SceneObject<ModelInstanced> torchs = null;

        private SceneObject<ParticleManager> particles = null;
        private ParticleSystemDescription pFire = null;

        private SceneObject<TriangleListDrawer> graphDrawer = null;

        public ModularDungeon(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.InitializeUI();
            this.InitializeEffects();
            this.InitializeModularScenery();
            this.InitializeEnvironment();
            this.InitializeSceneryObjects();
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
        }
        private void InitializeUI()
        {
            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsageEnum.UI, layerHUD);
            this.title.Instance.Text = "Collada Dungeon Scene";
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
        private void InitializeEffects()
        {
            this.particles = this.AddComponent<ParticleManager>(new ParticleManagerDescription() { Name = "Particle Systems" });

            this.pFire = ParticleSystemDescription.InitializeFire("resources", "fire.png", 0.25f);
        }
        private void InitializeModularScenery()
        {
            var desc = new ModularSceneryDescription()
            {
                Name = "Dungeon",
                UseAnisotropic = true,
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "Resources",
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
                    CellHeight = 0.08f,
                }
            };
        }
        private void InitializeSceneryObjects()
        {
            var rot0 = Quaternion.Identity;
            var rot90 = Quaternion.RotationAxis(Vector3.Up, MathUtil.PiOverTwo);
            var rot180 = Quaternion.RotationAxis(Vector3.Up, MathUtil.Pi);
            var rot270 = Quaternion.RotationAxis(Vector3.Up, MathUtil.PiOverTwo * 3);

            var trn = new Matrix[]
            {
                Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One*0.75f, Vector3.Zero, rot90, new Vector3(0,0,9)),
                Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One*0.75f, Vector3.Zero, rot0, new Vector3(-3,0,6)),
                Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One*0.75f, Vector3.Zero, rot180, new Vector3(3,0,6)),

                Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One*0.75f, Vector3.Zero, rot180, new Vector3(1,0,-4)),
                Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One*0.75f, Vector3.Zero, rot180, new Vector3(1,0,-8)),

                Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One*0.75f, Vector3.Zero, rot270, new Vector3(-5,0,-1)),
                Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One*0.75f, Vector3.Zero, rot270, new Vector3(-12,0,-1)),
                Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One*0.75f, Vector3.Zero, rot270, new Vector3(5,0,-1)),

                Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One*0.75f, Vector3.Zero, rot90, new Vector3(12,0,5)),
                Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One*0.75f, Vector3.Zero, rot270, new Vector3(12,0,-5)),
            };

            var desc = new ModelInstancedDescription()
            {
                Name = "Torchs",
                CastShadow = true,
                Instances = trn.Length,
                Static = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "Resources",
                    ModelContentFilename = "torch.xml",
                },
            };

            this.torchs = this.AddComponent<ModelInstanced>(desc, SceneObjectUsageEnum.Ground);

            this.torchs.Instance.SetTransforms(trn);

            for (int i = 0; i < this.torchs.Instance.Count; i++)
            {
                var torchTrn = this.torchs.Instance[i].Manipulator.LocalTransform;

                var lights = this.torchs.Instance[i].Lights;
                foreach (var light in lights)
                {
                    var pointL = light as SceneLightPoint;
                    if (pointL != null)
                    {
                        var pos = Vector3.TransformCoordinate(pointL.Position, torchTrn);

                        var emitter = new ParticleEmitter() { Position = pos, InfiniteDuration = true, EmissionRate = 0.1f };
                        this.particles.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pFire, emitter);
                    }
                }

                this.Lights.AddRange(this.torchs.Instance[i].Lights);
            }
        }
        private void InitializeDebug()
        {
            this.graphDrawer = this.AddComponent<TriangleListDrawer>(new TriangleListDrawerDescription() { AlphaEnabled = true, Count = 10000 });
            this.graphDrawer.Visible = false;
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.MovementDelta = this.agent.Velocity;
            this.Camera.SlowMovementDelta = this.agent.VelocitySlow;
            this.Camera.Mode = CameraModes.Free;
            this.Camera.Position = new Vector3(0, 1.5f, -12);
            this.Camera.Interest = new Vector3(0, 0, 0);
        }

        public override void Initialized()
        {
            base.Initialized();

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
