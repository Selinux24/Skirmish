using Engine;
using Engine.Audio;
using Engine.Common;
using Engine.Content;
using Engine.PathFinding.RecastNavigation;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skybox
{
    public class TestScene3D : Scene
    {
        private const float alpha = 0.25f;

        private readonly Color4 ruinsVolumeColor = new Color4(Color.Green.RGB(), alpha);
        private readonly Color4 torchVolumeColor = new Color4(Color.GreenYellow.RGB(), alpha);
        private readonly Color4 obeliskVolumeColor = new Color4(Color.DarkGreen.RGB(), alpha);
        private readonly Color4 fountainVolumeColor = new Color4(Color.DarkSeaGreen.RGB(), alpha);
        private readonly int bsphSlices = 20;
        private readonly int bsphStacks = 10;
        private readonly Vector2[] firePositions = new[]
        {
            new Vector2(+5, +5),
            new Vector2(-5, +5),
            new Vector2(+5, -5),
            new Vector2(-5, -5),
        };
        private readonly Vector3[] obeliskPositions = new[]
        {
            new Vector3(+100, -0.2f, +100),
            new Vector3(-100, -10, +100),
            new Vector3(+100, -1, -100),
            new Vector3(-100, -1, -100),
        };
        private readonly Quaternion[] obeliskRotations = new[]
        {
            Quaternion.RotationYawPitchRoll(-MathUtil.PiOverTwo * 0.75f, MathUtil.PiOverTwo*1.03f, 0.45f),
            Quaternion.RotationYawPitchRoll(0, -0.15f, 0),
            Quaternion.Identity,
            Quaternion.Identity,
        };

        private readonly Agent walker = new Agent()
        {
            Name = "Walker",
            Height = 1.7f,
            Radius = 0.4f,
            MaxClimb = 1.2f,
            MaxSlope = 45,
        };

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea help = null;
        private UITextArea fps = null;

        private Scenery ruins = null;
        private PrimitiveListDrawer<Line3D> volumesDrawer = null;
        private PrimitiveListDrawer<Triangle> graphDrawer = null;

        private readonly int mapSize = 256;
        private readonly float terrainSize = 512;
        private readonly float terrainHeight = 20;

        private ModelInstanced torchs = null;
        private ModelInstanced obelisks = null;
        private Model fountain = null;

        private Model movingFire = null;
        private ParticleEmitter movingFireEmitter = null;
        private SceneLightPoint movingFireLight = null;

        private ParticleManager pManager = null;
        private readonly ParticleSystemDescription pBigFire = ParticleSystemDescription.InitializeFire("resources", "fire.png", 0.5f);
        private readonly ParticleSystemDescription pFire = ParticleSystemDescription.InitializeFire("resources", "fire.png", 0.1f);
        private readonly ParticleSystemDescription pPlume = ParticleSystemDescription.InitializeSmokePlume("resources", "smoke.png", 0.1f);

        private int directionalLightCount = 0;

        private IAudioEffect fireAudioEffect;

        private bool gameReady = false;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            InitializeCamera();

            await LoadResourcesAsync(
                new[]
                {
                    InitializeUI(),
                    InitializeSkydom(),
                    InitializeLakeBottom(),
                    InitializeTorchs(),
                    InitializeObelisks(),
                    InitializeFountain(),
                    InitializeRuins(),
                    InitializeWater(),
                    InitializeParticles(),
                    InitializeEmitter(),
                    InitializeDebug(),
                },
                async (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    UpdateLayout();

                    InitializeNavigationMesh();

                    await UpdateNavigationGraph();

                    PrepareScene();

                    InitializeSound();

                    fireAudioEffect = AudioManager.CreateEffectInstance("Sphere", movingFire, Camera);
                    fireAudioEffect.Play();

                    AudioManager.MasterVolume = 1f;
                    AudioManager.Start();
                });
        }
        private void InitializeCamera()
        {
            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 5000.0f;
            Camera.Goto(new Vector3(-6, walker.Height, 5));
            Camera.LookTo(Vector3.UnitY + Vector3.UnitZ);
            Camera.MovementDelta = 4f;
            Camera.SlowMovementDelta = 2f;
        }
        private void InitializeNavigationMesh()
        {
            var nvInput = new InputGeometry(GetTrianglesForNavigationGraph);

            var nvSettings = BuildSettings.Default;
            nvSettings.TileSize = 32;
            nvSettings.CellSize = 0.05f;
            nvSettings.CellHeight = 0.2f;
            nvSettings.PartitionType = SamplePartitionTypes.Monotone;
            nvSettings.Agents[0] = walker;

            PathFinderDescription = new Engine.PathFinding.PathFinderDescription(nvSettings, nvInput);
        }
        private void InitializeSound()
        {
            AudioManager.LoadSound("target_balls_single_loop", "Resources/Audio/Effects", "target_balls_single_loop.wav");

            AudioManager.AddEffectParams(
                "Sphere",
                new GameAudioEffectParameters
                {
                    SoundName = "target_balls_single_loop",
                    IsLooped = true,
                    UseAudio3D = true,
                    ReverbPreset = ReverbPresets.StoneRoom,
                    Volume = 0.25f,
                    EmitterRadius = 6,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });
        }

        public override void NavigationGraphUpdated()
        {
            gameReady = true;
        }

        private async Task InitializeUI()
        {
            #region Cursor

            var cursorDesc = new UICursorDescription()
            {
                Textures = new[] { "target.png" },
                BaseColor = Color.Purple,
                Width = 16,
                Height = 16,
            };
            await this.AddComponentUICursor("Cursor", cursorDesc);

            #endregion

            #region Text

            title = await this.AddComponentUITextArea("Title", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18), TextForeColor = Color.White });
            help = await this.AddComponentUITextArea("Help", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12), TextForeColor = Color.Yellow });
            fps = await this.AddComponentUITextArea("FPS", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12), TextForeColor = Color.Yellow });

            title.Text = "Collada Scene with Skybox";
#if DEBUG
            help.Text = "Escape: Exit | Home: Reset camera | AWSD: Move camera | Right Mouse: Drag view | Left Mouse: Pick";
#else
            help.Text = "Escape: Exit | Home: Reset camera | AWSD: Move camera | Move Mouse: View | Left Mouse: Pick";
#endif
            fps.Text = "";

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            panel = await this.AddComponentSprite("Backpanel", spDesc, SceneObjectUsages.UI, LayerUI - 1);

            #endregion
        }
        private async Task InitializeSkydom()
        {
            var skydomDesc = new SkydomDescription()
            {
                ContentPath = "Resources",
                Radius = Camera.FarPlaneDistance,
                Texture = "sunset.dds",
            };

            await this.AddComponentSkydom("Skydom", skydomDesc);
        }
        private async Task InitializeLakeBottom()
        {
            // Generates a random terrain using perlin noise
            NoiseMapDescriptor nmDesc = new NoiseMapDescriptor
            {
                MapWidth = mapSize,
                MapHeight = mapSize,
                Scale = 0.5f,
                Lacunarity = 2f,
                Persistance = 0.5f,
                Octaves = 4,
                Offset = Vector2.One,
                Seed = 5,
            };
            var noiseMap = NoiseMap.CreateNoiseMap(nmDesc);

            Curve heightCurve = new Curve();
            heightCurve.Keys.Add(0, 0);
            heightCurve.Keys.Add(0.4f, 0f);
            heightCurve.Keys.Add(1f, 1f);

            float cellSize = terrainSize / mapSize;

            var textures = new HeightmapTexturesDescription
            {
                ContentPath = "resources/lakebottom",
                TexturesLR = new[] { "Diffuse.jpg" },
                NormalMaps = new[] { "Normal.jpg" },
                Scale = 0.0333f,
            };
            GroundDescription groundDesc = GroundDescription.FromHeightmap(noiseMap, cellSize, terrainHeight, heightCurve, textures, 2);
            groundDesc.Heightmap.UseFalloff = true;
            groundDesc.Heightmap.Transform = Matrix.Translation(0, -terrainHeight * 0.33f, 0);

            await this.AddComponentScenery("Lage Bottom", groundDesc);
        }
        private async Task InitializeTorchs()
        {
            var torchDesc = new ModelInstancedDescription()
            {
                Instances = firePositions.Length,
                CastShadow = true,
                Content = ContentDescription.FromFile("Resources", "torch.xml"),
            };

            torchs = await this.AddComponentModelInstanced("Torchs", torchDesc);

            AttachToGround(torchs, true);
        }
        private async Task InitializeObelisks()
        {
            var obeliskDesc = new ModelInstancedDescription()
            {
                Instances = firePositions.Length,
                CastShadow = true,
                Content = ContentDescription.FromFile("Resources/obelisk", "obelisk.xml"),
            };

            obelisks = await this.AddComponentModelInstanced("Obelisks", obeliskDesc, SceneObjectUsages.Ground);
        }
        private async Task InitializeFountain()
        {
            var fountainDesc = new ModelDescription()
            {
                CastShadow = true,
                Content = ContentDescription.FromFile("Resources/Fountain", "Fountain.xml"),
            };

            fountain = await this.AddComponentModel("Fountain", fountainDesc, SceneObjectUsages.Ground);

            AttachToGround(fountain, true);
        }
        private async Task InitializeRuins()
        {
            var ruinsDesc = GroundDescription.FromFile("Resources", "ruins.xml");

            ruins = await this.AddComponentScenery("Ruins", ruinsDesc, SceneObjectUsages.Ground);

            SetGround(ruins, true);
        }
        private async Task InitializeWater()
        {
            var waterDesc = WaterDescription.CreateCalm(5000f, -2f);
            waterDesc.BaseColor = new Color3(0.067f, 0.065f, 0.003f);
            waterDesc.WaterColor = new Color4(0.003f, 0.267f, 0.096f, 0.98f);

            await this.AddComponentWater("Water", waterDesc);
        }
        private async Task InitializeEmitter()
        {
            var mat = MaterialCookTorranceContent.Default;
            mat.EmissiveColor = Color.Yellow.RGB();

            var sphere = GeometryUtil.CreateSphere(0.05f, 32, 32);

            var mFireDesc = new ModelDescription()
            {
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                Content = ContentDescription.FromContentData(sphere, mat),
            };

            movingFire = await this.AddComponentModel("Emitter", mFireDesc);
        }
        private async Task InitializeParticles()
        {
            var pManagerDesc = ParticleManagerDescription.Default();

            pManager = await this.AddComponentParticleManager("ParticleManager", pManagerDesc);

            movingFireEmitter = new ParticleEmitter()
            {
                EmissionRate = 0.1f,
                InfiniteDuration = true,
                MaximumDistance = GameEnvironment.LODDistanceLow,
            };

            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pBigFire, movingFireEmitter);
        }
        private async Task InitializeDebug()
        {
            volumesDrawer = await this.AddComponentPrimitiveListDrawer("DebugVolumesDrawer", new PrimitiveListDrawerDescription<Line3D>() { Count = 10000 });
            volumesDrawer.Visible = false;

            graphDrawer = await this.AddComponentPrimitiveListDrawer("DebugGraphDrawer", new PrimitiveListDrawerDescription<Triangle>() { Count = 10000 });
            graphDrawer.Visible = false;
        }

        private void PrepareScene()
        {
            Lights.DirectionalLights[0].Enabled = true;
            Lights.DirectionalLights[0].CastShadow = true;

            Lights.DirectionalLights[1].Enabled = true;

            Lights.DirectionalLights[2].Enabled = false;

            directionalLightCount = Lights.DirectionalLights.Length;

            movingFireLight = new SceneLightPoint(
                "Moving fire light",
                false,
                Color.Orange.RGB(),
                Color.Orange.RGB(),
                true,
                SceneLightPointDescription.Create(Vector3.Zero, 15f, 20f));

            Lights.Add(movingFireLight);

            Vector3[] firePositions3D = new Vector3[firePositions.Length];
            SceneLightPoint[] torchLights = new SceneLightPoint[firePositions.Length];
            Parallel.For(0, firePositions.Length, (i, loopState) =>
            {
                if (loopState.IsExceptional)
                {
                    return;
                }

                Color3 color = Color.Yellow.RGB();
                if (i == 1) color = Color.Red.RGB();
                if (i == 2) color = Color.Green.RGB();
                if (i == 3) color = Color.LightBlue.RGB();

                FindTopGroundPosition(firePositions[i].X, firePositions[i].Y, out PickingResult<Triangle> result);
                firePositions3D[i] = result.Position;

                torchs[i].Manipulator.SetScale(0.20f, true);
                torchs[i].Manipulator.SetPosition(firePositions3D[i], true);

                BoundingBox bbox = torchs[i].GetBoundingBox();

                firePositions3D[i].Y += (bbox.Maximum.Y - bbox.Minimum.Y) * 0.95f;

                torchLights[i] = new SceneLightPoint(
                    string.Format("Torch {0}", i),
                    false,
                    color,
                    color,
                    true,
                    SceneLightPointDescription.Create(firePositions3D[i], 4f, 20f));

                Lights.Add(torchLights[i]);

                pManager.AddParticleSystem(ParticleSystemTypes.CPU, pFire, new ParticleEmitter() { Position = firePositions3D[i], InfiniteDuration = true, EmissionRate = 0.1f, MaximumDistance = GameEnvironment.LODDistanceLow });
                pManager.AddParticleSystem(ParticleSystemTypes.CPU, pPlume, new ParticleEmitter() { Position = firePositions3D[i], InfiniteDuration = true, EmissionRate = 0.5f, MaximumDistance = GameEnvironment.LODDistanceLow });
            });

            Parallel.For(0, obeliskPositions.Length, (i, loopState) =>
            {
                if (loopState.IsExceptional)
                {
                    return;
                }

                obelisks[i].Manipulator.SetPosition(obeliskPositions[i]);
                obelisks[i].Manipulator.SetRotation(obeliskRotations[i]);
                obelisks[i].Manipulator.SetScale(10f);
            });

            fountain.Manipulator.SetScale(2.3f);
        }

        public override void Update(GameTime gameTime)
        {
            if (!gameReady)
            {
                return;
            }

            Vector3 previousPosition = Camera.Position;

            UpdateInput();

            #region Walk

            if (Walk(walker, previousPosition, Camera.Position, true, out Vector3 walkerPosition))
            {
                Camera.Goto(walkerPosition);
            }
            else
            {
                Camera.Goto(previousPosition);
            }

            #endregion

            #region Light

            float r = 5.5f;
            float h = 1.25f;
            float d = 0.5f;
            float v = 0.8f;

            Vector3 position = Vector3.Zero;
            position.X = r * d * (float)Math.Cos(v * Game.GameTime.TotalSeconds);
            position.Y = h + (0.25f * (1f + (float)Math.Sin(Game.GameTime.TotalSeconds)));
            position.Z = r * d * (float)Math.Sin(v * Game.GameTime.TotalSeconds);

            movingFire.Manipulator.SetPosition(position);
            movingFireEmitter.Position = position;
            movingFireLight.Position = position;

            DEBUGUpdateMovingVolumesDrawer();

            #endregion

            base.Update(gameTime);
        }
        private void UpdateInput()
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.Exit();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            UpdateInputDebug();

            UpdateInputCamera();

            UpdateInputLights();

            var m = fireAudioEffect.GetOutputMatrix();
            var ep = movingFire.Manipulator.Position.GetDescription();
            var ev = movingFire.Manipulator.Velocity.GetDescription();
            var lp = Camera.Position.GetDescription();
            var lv = Camera.Velocity.GetDescription();
            var d = Vector3.Distance(movingFire.Manipulator.Position, Camera.Position);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Mouse (X:{Game.Input.MouseXDelta}; Y:{Game.Input.MouseYDelta}, Wheel: {Game.Input.MouseWheelDelta}) Absolute (X:{Game.Input.MouseX}; Y:{Game.Input.MouseY})");
            sb.AppendLine($"L {m[0]:0.000} R {m[1]:0.000} Distance {d}");
            sb.AppendLine($"Emitter  pos: {ep} Emitter  vel: {ev}");
            sb.AppendLine($"Listener pos: {lp} Listener vel: {lv}");
            fps.Text = sb.ToString();
        }
        private void UpdateInputCamera()
        {
            if (Game.Input.MouseButtonPressed(MouseButtons.Left))
            {
                var pRay = GetPickingRay();

                if (ruins.PickNearest(pRay, out PickingResult<Triangle> r))
                {
                    var tri = Line3D.CreateWiredTriangle(r.Item);

                    volumesDrawer.SetPrimitives(Color.White, tri);
                }
            }

#if DEBUG
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                Game.GameTime,
                Game.Input.MouseXDelta,
                Game.Input.MouseYDelta);
#endif

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
        }
        private void UpdateInputLights()
        {
            if (Game.Input.KeyJustReleased(Keys.Add))
            {
                directionalLightCount++;
                if (directionalLightCount > 3)
                {
                    directionalLightCount = 0;
                }

                UpdateInputEnabledLights();
            }

            if (Game.Input.KeyJustReleased(Keys.Subtract))
            {
                directionalLightCount--;
                if (directionalLightCount < 0)
                {
                    directionalLightCount = 3;
                }

                UpdateInputEnabledLights();
            }
        }
        private void UpdateInputEnabledLights()
        {
            Lights.DirectionalLights[0].Enabled = directionalLightCount > 0;
            Lights.DirectionalLights[1].Enabled = directionalLightCount > 1;
            Lights.DirectionalLights[2].Enabled = directionalLightCount > 2;
        }
        private void UpdateInputDebug()
        {
            if (Game.Input.KeyJustReleased(Keys.Home))
            {
                InitializeCamera();
            }

            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                volumesDrawer.Visible = !volumesDrawer.Visible;

                if (volumesDrawer.Visible)
                {
                    DEBUGUpdateVolumesDrawer();
                }
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                graphDrawer.Visible = !graphDrawer.Visible;

                if (graphDrawer.Visible)
                {
                    DEBUGUpdateGraphDrawer();
                }
            }
        }

        private void DEBUGUpdateVolumesDrawer()
        {
            volumesDrawer.SetPrimitives(ruinsVolumeColor, Line3D.CreateWiredBox(ruins.GetBoundingBox()));

            List<Line3D> volumesTorchs = new List<Line3D>();
            for (int i = 0; i < torchs.InstanceCount; i++)
            {
                volumesTorchs.AddRange(Line3D.CreateWiredBox(torchs[i].GetBoundingBox()));
            }
            volumesDrawer.SetPrimitives(torchVolumeColor, volumesTorchs);

            List<Line3D> volumesObelisks = new List<Line3D>();
            for (int i = 0; i < obelisks.InstanceCount; i++)
            {
                volumesObelisks.AddRange(Line3D.CreateWiredBox(obelisks[i].GetBoundingBox()));
            }
            volumesDrawer.SetPrimitives(obeliskVolumeColor, volumesObelisks);

            var volumeFountain = Line3D.CreateWiredBox(fountain.GetBoundingBox());
            volumesDrawer.SetPrimitives(fountainVolumeColor, volumeFountain);

            for (int i = 1; i < Lights.PointLights.Length; i++)
            {
                var light = Lights.PointLights[i];

                volumesDrawer.SetPrimitives(
                    new Color4(light.DiffuseColor, alpha),
                    Line3D.CreateWiredSphere(light.BoundingSphere, bsphSlices, bsphStacks));
            }
        }
        private void DEBUGUpdateMovingVolumesDrawer()
        {
            var light = Lights.PointLights[0];

            volumesDrawer.SetPrimitives(
                new Color4(light.DiffuseColor, alpha),
                Line3D.CreateWiredSphere(light.BoundingSphere, bsphSlices, bsphStacks));
        }
        private void DEBUGUpdateGraphDrawer()
        {
            var nodes = GetNodes(walker).OfType<GraphNode>();
            if (nodes.Any())
            {
                graphDrawer.Clear();

                foreach (var node in nodes)
                {
                    graphDrawer.AddPrimitives(node.Color, node.Triangles);
                }
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
            help.SetPosition(new Vector2(0, 24));
            fps.SetPosition(new Vector2(0, 40));
            panel.Width = Game.Form.RenderWidth;
            panel.Height = 120;
        }
    }
}
