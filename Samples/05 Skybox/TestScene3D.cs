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
        private const int layerHUD = 99;
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
            Height = 1.2f,
            Radius = 0.4f,
            MaxClimb = 1.2f,
            MaxSlope = 45,
        };

        private UITextArea fps = null;

        private Scenery ruins = null;
        private PrimitiveListDrawer<Line3D> volumesDrawer = null;
        private PrimitiveListDrawer<Triangle> graphDrawer = null;

        private ModelInstanced torchs = null;
        private ModelInstanced obelisks = null;
        private Model fountain = null;

        private Model movingFire = null;
        private ParticleEmitter movingFireEmitter = null;
        private SceneLightPoint movingFireLight = null;

        private readonly ParticleSystemDescription pBigFire = ParticleSystemDescription.InitializeFire("resources", "fire.png", 0.5f);
        private readonly ParticleSystemDescription pFire = ParticleSystemDescription.InitializeFire("resources", "fire.png", 0.1f);
        private readonly ParticleSystemDescription pPlume = ParticleSystemDescription.InitializeSmokePlume("resources", "smoke.png", 0.1f);

        private int directionalLightCount = 0;

        private GameAudioEffect fireAudioEffect;

        private bool gameReady = false;

        public TestScene3D(Game game)
            : base(game, SceneModes.ForwardLigthning)
        {

        }

        public override async Task Initialize()
        {
            this.InitializeCamera();

            await this.LoadResourcesAsync(
                InitializeAssets(),
                () =>
                {
                    InitializeNavigationMesh();

                    Task.WhenAll(this.UpdateNavigationGraph());

                    InitializeSound();

                    fireAudioEffect = this.AudioManager.CreateEffectInstance("Sphere", this.movingFire, this.Camera);
                    fireAudioEffect.Play();

                    this.AudioManager.MasterVolume = 1f;
                    this.AudioManager.Start();
                });
        }
        private async Task InitializeAssets()
        {
            #region Cursor

            var cursorDesc = new UICursorDescription()
            {
                Name = "Cursor",
                Textures = new[] { "target.png" },
                Color = Color.Purple,
                Width = 16,
                Height = 16,
            };
            await this.AddComponentUICursor(cursorDesc, layerHUD + 1);

            #endregion

            #region Text

            var title = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18, Color.White) }, layerHUD);
            var help = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) }, layerHUD);
            this.fps = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) }, layerHUD);

            title.Text = "Collada Scene with Skybox";
#if DEBUG
            help.Text = "Escape: Exit | Home: Reset camera | AWSD: Move camera | Right Mouse: Drag view | Left Mouse: Pick";
#else
            help.Text = "Escape: Exit | Home: Reset camera | AWSD: Move camera | Move Mouse: View | Left Mouse: Pick";
#endif
            this.fps.Text = "";

            title.SetPosition(Vector2.Zero);
            help.SetPosition(new Vector2(0, 24));
            this.fps.SetPosition(new Vector2(0, 40));

            var spDesc = new SpriteDescription()
            {
                Name = "UI Back pannel",
                Width = this.Game.Form.RenderWidth,
                Height = 120,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);

            #endregion

            #region Skydom

            await this.AddComponentSkydom(new SkydomDescription()
            {
                Name = "Skydom",
                ContentPath = "Resources",
                Radius = this.Camera.FarPlaneDistance,
                Texture = "sunset.dds",
            });

            #endregion

            #region Torchs

            this.torchs = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Torchs",
                    Instances = this.firePositions.Length,
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources",
                        ModelContentFilename = "torch.xml",
                    }
                });

            this.AttachToGround(this.torchs, true);

            #endregion

            #region Obelisks

            this.obelisks = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Obelisks",
                    Instances = this.firePositions.Length,
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/obelisk",
                        ModelContentFilename = "obelisk.xml",
                    },
                });

            #endregion

            #region Fountain

            this.fountain = await this.AddComponentModel(
                new ModelDescription()
                {
                    Name = "Fountain",
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Fountain",
                        ModelContentFilename = "Fountain.xml",
                    },
                });

            this.AttachToGround(this.fountain, true);

            #endregion

            #region Terrain

            GroundDescription desc = new GroundDescription()
            {
                Name = "Terrain",
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "Resources",
                    ModelContentFilename = "ruins.xml",
                }
            };
            this.ruins = await this.AddComponentScenery(desc);
            this.SetGround(this.ruins, true);

            #endregion

            #region Water

            var waterDesc = WaterDescription.CreateCalm("Ocean", 5000f, -1f);
            await this.AddComponentWater(waterDesc, SceneObjectUsages.None);

            #endregion

            #region Particle Systems

            var pManager = await this.AddComponentParticleManager(new ParticleManagerDescription() { Name = "Particle Systems" });

            #endregion

            #region Moving fire

            MaterialContent mat = MaterialContent.Default;
            mat.EmissionColor = Color.Yellow;

            var sphere = GeometryUtil.CreateSphere(0.05f, 32, 32);
            var vertices = VertexData.FromDescriptor(sphere);
            var indices = sphere.Indices;
            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

            var mFireDesc = new ModelDescription()
            {
                Name = "Emitter",
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            this.movingFire = await this.AddComponentModel(mFireDesc);

            this.movingFireEmitter = new ParticleEmitter() { EmissionRate = 0.1f, InfiniteDuration = true };

            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pBigFire, this.movingFireEmitter);

            #endregion

            #region Positioning and lights

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[0].CastShadow = true;

            this.Lights.DirectionalLights[1].Enabled = true;

            this.Lights.DirectionalLights[2].Enabled = false;

            this.directionalLightCount = this.Lights.DirectionalLights.Length;

            this.movingFireLight = new SceneLightPoint(
                "Moving fire light",
                false,
                Color.Orange,
                Color.Orange,
                true,
                SceneLightPointDescription.Create(Vector3.Zero, 15f, 20f));

            this.Lights.Add(this.movingFireLight);

            Vector3[] firePositions3D = new Vector3[this.firePositions.Length];
            SceneLightPoint[] torchLights = new SceneLightPoint[this.firePositions.Length];
            for (int i = 0; i < this.firePositions.Length; i++)
            {
                Color color = Color.Yellow;
                if (i == 1) color = Color.Red;
                if (i == 2) color = Color.Green;
                if (i == 3) color = Color.LightBlue;

                this.FindTopGroundPosition(
                    this.firePositions[i].X, this.firePositions[i].Y,
                    out PickingResult<Triangle> result);
                firePositions3D[i] = result.Position;

                this.torchs[i].Manipulator.SetScale(0.20f, true);
                this.torchs[i].Manipulator.SetPosition(firePositions3D[i], true);

                BoundingBox bbox = this.torchs[i].GetBoundingBox();

                firePositions3D[i].Y += (bbox.Maximum.Y - bbox.Minimum.Y) * 0.95f;

                torchLights[i] = new SceneLightPoint(
                    string.Format("Torch {0}", i),
                    false,
                    color,
                    color,
                    true,
                    SceneLightPointDescription.Create(firePositions3D[i], 4f, 20f));

                this.Lights.Add(torchLights[i]);

                pManager.AddParticleSystem(ParticleSystemTypes.CPU, pFire, new ParticleEmitter() { Position = firePositions3D[i], InfiniteDuration = true, EmissionRate = 0.1f });
                pManager.AddParticleSystem(ParticleSystemTypes.CPU, pPlume, new ParticleEmitter() { Position = firePositions3D[i], InfiniteDuration = true, EmissionRate = 0.5f });
            }

            for (int i = 0; i < this.obeliskPositions.Length; i++)
            {
                this.obelisks[i].Manipulator.SetPosition(obeliskPositions[i]);
                this.obelisks[i].Manipulator.SetRotation(obeliskRotations[i]);
                this.obelisks[i].Manipulator.SetScale(10f);
            }

            this.fountain.Manipulator.SetScale(2.3f);

            #endregion

            #region DEBUG drawers

            this.volumesDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(new PrimitiveListDrawerDescription<Line3D>() { Count = 10000 });
            this.volumesDrawer.Visible = false;

            this.graphDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(new PrimitiveListDrawerDescription<Triangle>() { Count = 10000 });
            this.graphDrawer.Visible = false;

            #endregion
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 5000.0f;
            this.Camera.Goto(new Vector3(-6, this.walker.Height, 5));
            this.Camera.LookTo(Vector3.UnitY + Vector3.UnitZ);
            this.Camera.MovementDelta = 4f;
            this.Camera.SlowMovementDelta = 2f;
        }
        private void InitializeNavigationMesh()
        {
            var nvInput = new InputGeometry(GetTrianglesForNavigationGraph);

            var nvSettings = BuildSettings.Default;
            nvSettings.TileSize = 32;
            nvSettings.CellSize = 0.05f;
            nvSettings.CellHeight = 0.2f;
            nvSettings.PartitionType = SamplePartitionTypes.Monotone;
            nvSettings.Agents[0] = this.walker;

            this.PathFinderDescription = new Engine.PathFinding.PathFinderDescription(nvSettings, nvInput);
        }
        private void InitializeSound()
        {
            this.AudioManager.LoadSound("target_balls_single_loop", "Resources/Audio/Effects", "target_balls_single_loop.wav");

            this.AudioManager.AddEffectParams(
                "Sphere",
                new GameAudioEffectParameters
                {
                    SoundName = "target_balls_single_loop",
                    IsLooped = true,
                    UseAudio3D = true,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Default,
                    Volume = 0.25f,
                    EmitterRadius = 6,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });
        }

        public override void NavigationGraphUpdated()
        {
            gameReady = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!gameReady)
            {
                return;
            }

            Vector3 previousPosition = this.Camera.Position;
            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            this.UpdateInput(shift);

            #region Walk

            if (this.Walk(this.walker, previousPosition, this.Camera.Position, true, out Vector3 walkerPosition))
            {
                this.Camera.Goto(walkerPosition);
            }
            else
            {
                this.Camera.Goto(previousPosition);
            }

            #endregion

            #region Light

            float r = 5.5f;
            float h = 1.25f;
            float d = 0.5f;
            float v = 0.8f;

            Vector3 position = Vector3.Zero;
            position.X = r * d * (float)Math.Cos(v * this.Game.GameTime.TotalSeconds);
            position.Y = h + (0.25f * (1f + (float)Math.Sin(this.Game.GameTime.TotalSeconds)));
            position.Z = r * d * (float)Math.Sin(v * this.Game.GameTime.TotalSeconds);

            this.movingFire.Manipulator.SetPosition(position);
            this.movingFireEmitter.Position = position;
            this.movingFireLight.Position = position;

            this.DEBUGUpdateMovingVolumesDrawer();

            #endregion

            base.Update(gameTime);
        }
        private void UpdateInput(bool shift)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            this.UpdateInputDebug();

            this.UpdateInputCamera(shift);

            this.UpdateInputLights();

            var m = fireAudioEffect.GetOutputMatrix();
            var ep = movingFire.Manipulator.Position.GetDescription();
            var ev = movingFire.Manipulator.Velocity.GetDescription();
            var lp = Camera.Position.GetDescription();
            var lv = Camera.Velocity.GetDescription();
            var d = Vector3.Distance(movingFire.Manipulator.Position, Camera.Position);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Mouse (X:{this.Game.Input.MouseXDelta}; Y:{this.Game.Input.MouseYDelta}, Wheel: {this.Game.Input.MouseWheelDelta}) Absolute (X:{this.Game.Input.MouseX}; Y:{this.Game.Input.MouseY})");
            sb.AppendLine($"L {m[0]:0.000} R {m[1]:0.000} Distance {d}");
            sb.AppendLine($"Emitter  pos: {ep} Emitter  vel: {ev}");
            sb.AppendLine($"Listener pos: {lp} Listener vel: {lv}");
            this.fps.Text = sb.ToString();
        }
        private void UpdateInputCamera(bool shift)
        {
            if (this.Game.Input.LeftMouseButtonPressed)
            {
                var pRay = this.GetPickingRay();

                if (this.ruins.PickNearest(pRay, out PickingResult<Triangle> r))
                {
                    var tri = Line3D.CreateWiredTriangle(r.Item);

                    this.volumesDrawer.SetPrimitives(Color.White, tri);
                }
            }

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
#else
            this.Camera.RotateMouse(
                this.Game.GameTime,
                this.Game.Input.MouseXDelta,
                this.Game.Input.MouseYDelta);
#endif

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(this.Game.GameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(this.Game.GameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(this.Game.GameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(this.Game.GameTime, shift);
            }
        }
        private void UpdateInputLights()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Add))
            {
                this.directionalLightCount++;
                if (this.directionalLightCount > 3)
                {
                    this.directionalLightCount = 0;
                }

                this.UpdateInputEnabledLights();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Subtract))
            {
                this.directionalLightCount--;
                if (this.directionalLightCount < 0)
                {
                    this.directionalLightCount = 3;
                }

                this.UpdateInputEnabledLights();
            }
        }
        private void UpdateInputEnabledLights()
        {
            this.Lights.DirectionalLights[0].Enabled = this.directionalLightCount > 0;
            this.Lights.DirectionalLights[1].Enabled = this.directionalLightCount > 1;
            this.Lights.DirectionalLights[2].Enabled = this.directionalLightCount > 2;
        }
        private void UpdateInputDebug()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.InitializeCamera();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.volumesDrawer.Visible = !this.volumesDrawer.Visible;

                if (this.volumesDrawer.Visible)
                {
                    this.DEBUGUpdateVolumesDrawer();
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.graphDrawer.Visible = !this.graphDrawer.Visible;

                if (this.graphDrawer.Visible)
                {
                    this.DEBUGUpdateGraphDrawer();
                }
            }
        }

        private void DEBUGUpdateVolumesDrawer()
        {
            this.volumesDrawer.SetPrimitives(this.ruinsVolumeColor, Line3D.CreateWiredBox(this.ruins.GetBoundingBox()));

            List<Line3D> volumesTorchs = new List<Line3D>();
            for (int i = 0; i < this.torchs.InstanceCount; i++)
            {
                volumesTorchs.AddRange(Line3D.CreateWiredBox(this.torchs[i].GetBoundingBox()));
            }
            this.volumesDrawer.SetPrimitives(this.torchVolumeColor, volumesTorchs);

            List<Line3D> volumesObelisks = new List<Line3D>();
            for (int i = 0; i < this.obelisks.InstanceCount; i++)
            {
                volumesObelisks.AddRange(Line3D.CreateWiredBox(this.obelisks[i].GetBoundingBox()));
            }
            this.volumesDrawer.SetPrimitives(this.obeliskVolumeColor, volumesObelisks);

            var volumeFountain = Line3D.CreateWiredBox(this.fountain.GetBoundingBox());
            this.volumesDrawer.SetPrimitives(this.fountainVolumeColor, volumeFountain);

            for (int i = 1; i < this.Lights.PointLights.Length; i++)
            {
                var light = this.Lights.PointLights[i];

                this.volumesDrawer.SetPrimitives(
                    new Color4(light.DiffuseColor.RGB(), alpha),
                    Line3D.CreateWiredSphere(light.BoundingSphere, this.bsphSlices, this.bsphStacks));
            }
        }
        private void DEBUGUpdateMovingVolumesDrawer()
        {
            var light = this.Lights.PointLights[0];

            this.volumesDrawer.SetPrimitives(
                new Color4(light.DiffuseColor.RGB(), alpha),
                Line3D.CreateWiredSphere(light.BoundingSphere, this.bsphSlices, this.bsphStacks));
        }
        private void DEBUGUpdateGraphDrawer()
        {
            var nodes = this.GetNodes(this.walker).OfType<GraphNode>();
            if (nodes.Any())
            {
                this.graphDrawer.Clear();

                foreach (var node in nodes)
                {
                    this.graphDrawer.AddPrimitives(node.Color, node.Triangles);
                }
            }
        }
    }
}
