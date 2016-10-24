using Engine;
using Engine.PathFinding.NavMesh;
using SharpDX;
using System;
using System.Collections.Generic;

namespace Skybox
{
    public class TestScene3D : Scene
    {
        private Vector2[] firePositions = new[]
        {
            new Vector2(+5, +5),
            new Vector2(-5, +5),
            new Vector2(+5, -5),
            new Vector2(-5, -5),
        };
        private Color ruinsVolumeColor = Color.Green;
        private Color torchVolumeColor = Color.GreenYellow;
        private int bsphSlices = 20;
        private int bsphStacks = 10;

        private NavigationMeshAgent walker = new NavigationMeshAgent()
        {
            Name = "agent",
            Height = 1f,
            Radius = 0.3f,
            MaxClimb = 0.9f,
        };

        private Cursor cursor;

        private TextDrawer title = null;
        private TextDrawer help = null;
        private TextDrawer fps = null;

        private Skydom skydom = null;
        private Scenery ruins = null;
        private LineListDrawer volumesDrawer = null;
        private TriangleListDrawer graphDrawer = null;

        private ParticleSystem rain = null;

        private ModelInstanced torchs = null;
        private ParticleSystem torchFire = null;
        private SceneLightPoint[] torchLights = null;

        private ParticleSystem movingfire = null;
        private SceneLightPoint movingFireLight = null;

        private int directionalLightCount = 0;

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            #region Cursor

            SpriteDescription cursorDesc = new SpriteDescription()
            {
                Textures = new[] { "target.png" },
                Width = 16,
                Height = 16,
            };

            this.cursor = this.AddCursor(cursorDesc);

            #endregion

            #region Text

            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White));
            this.help = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow));
            this.fps = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow));

            this.title.Text = "Collada Scene with Skybox";
#if DEBUG
            this.help.Text = "Escape: Exit | Home: Reset camera | AWSD: Move camera | Right Mouse: Drag view | Left Mouse: Pick";
#else
            this.help.Text = "Escape: Exit | Home: Reset camera | AWSD: Move camera | Move Mouse: View | Left Mouse: Pick";
#endif
            this.fps.Text = null;

            this.title.Position = Vector2.Zero;
            this.help.Position = new Vector2(0, 24);
            this.fps.Position = new Vector2(0, 40);

            #endregion

            #region Skydom

            this.skydom = this.AddSkydom(new SkydomDescription()
            {
                ContentPath = "Resources",
                Radius = this.Camera.FarPlaneDistance,
                Texture = "sunset.dds",
            });

            #endregion

            #region Torchs

            this.torchs = this.AddInstancingModel(
                "Resources",
                "torch.xml",
                new ModelInstancedDescription()
                {
                    Instances = this.firePositions.Length,
                    CastShadow = true,
                });

            #endregion

            #region Rain

            var rainEmitter = new ParticleEmitter()
            {
                Color = Color.LightBlue,
                Size = 0.5f,
                Position = Vector3.Zero,
            };

            this.rain = this.AddParticleSystem(ParticleSystemDescription.Rain(rainEmitter, "raindrop.dds"));

            #endregion

            #region Terrain

            var nvSettings = NavigationMeshGenerationSettings.Default;
            nvSettings.Agents[0] = this.walker;

            GroundDescription desc = new GroundDescription()
            {
                PathFinder = new GroundDescription.PathFinderDescription()
                {
                    Settings = nvSettings,
                },
                CastShadow = true,
                DelayGeneration = true,
            };
            this.ruins = this.AddScenery(
                "Resources",
                "ruins.xml",
                desc, 
                false);

            #endregion

            #region Moving fire

            var movingFireEmitter = new ParticleEmitter()
            {
                Color = Color.Orange,
                Size = 0.5f,
                Position = Vector3.Zero,
            };

            this.movingfire = this.AddParticleSystem(ParticleSystemDescription.Fire(movingFireEmitter, "flare2.png"));

            #endregion

            #region DEBUG drawers

            this.volumesDrawer = this.AddLineListDrawer(10000);
            this.volumesDrawer.Visible = false;
            this.volumesDrawer.DeferredEnabled = false;
            this.volumesDrawer.EnableDepthStencil = false;
            this.volumesDrawer.EnableAlphaBlending = true;
            this.volumesDrawer.CastShadow = false;

            this.graphDrawer = this.AddTriangleListDrawer(10000);
            this.graphDrawer.EnableDepthStencil = false;
            this.graphDrawer.EnableAlphaBlending = true;
            this.graphDrawer.Visible = false;
            this.graphDrawer.DeferredEnabled = false;

            #endregion

            #region Positioning and lights

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[1].Enabled = true;
            this.Lights.DirectionalLights[2].Enabled = false;
            this.directionalLightCount = this.Lights.DirectionalLights.Length;

            this.movingFireLight = new SceneLightPoint()
            {
                Name = "Moving fire light",
                LightColor = Color.Orange,
                AmbientIntensity = 0.1f,
                DiffuseIntensity = 1f,
                Position = Vector3.Zero,
                Radius = 5f,
                Enabled = true,
                CastShadow = false,
            };
            this.Lights.Add(this.movingFireLight);

            ParticleEmitter[] firePositions3D = new ParticleEmitter[this.firePositions.Length];
            this.torchLights = new SceneLightPoint[this.firePositions.Length];
            for (int i = 0; i < this.firePositions.Length; i++)
            {
                Color color = Color.Yellow;
                if (i == 1) color = Color.Red;
                if (i == 2) color = Color.Green;
                if (i == 3) color = Color.LightBlue;

                firePositions3D[i] = new ParticleEmitter()
                {
                    Color = color,
                    Size = 0.2f,
                    Position = Vector3.Zero,
                };

                Triangle t;
                float d;
                this.ruins.FindTopGroundPosition(this.firePositions[i].X, this.firePositions[i].Y, out firePositions3D[i].Position, out t, out d);

                this.torchs.Instances[i].Manipulator.SetScale(0.20f, true);
                this.torchs.Instances[i].Manipulator.SetPosition(firePositions3D[i].Position, true);

                BoundingBox bbox = this.torchs.Instances[i].GetBoundingBox();

                firePositions3D[i].Position.Y += (bbox.Maximum.Y - bbox.Minimum.Y) * 0.95f;

                this.torchLights[i] = new SceneLightPoint()
                {
                    Name = string.Format("Torch {0}", i),
                    LightColor = color,
                    AmbientIntensity = 0.1f,
                    DiffuseIntensity = 5f,
                    Position = firePositions3D[i].Position,
                    Radius = 4f,
                    Enabled = true,
                    CastShadow = false,
                };

                this.Lights.Add(this.torchLights[i]);
            }
            this.torchFire = this.AddParticleSystem(ParticleSystemDescription.Fire(firePositions3D, "flare1.png"));

            this.ruins.AttachFullPickingFullPathFinding(this.torchs);

            #endregion

            this.SceneVolume = this.ruins.GetBoundingSphere();

            this.InitializeCamera();
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 50.0f;
            this.Camera.Goto(new Vector3(-6, this.walker.Height, 5));
            this.Camera.LookTo(Vector3.UnitY + Vector3.UnitZ);
            this.Camera.MovementDelta = 4f;
            this.Camera.SlowMovementDelta = 2f;
        }

        public override void Update(GameTime gameTime)
        {
            Vector3 previousPosition = this.Camera.Position;
            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            this.UpdateInput(shift, rightBtn);

            #region Walk

            {
                Vector3 walkerPosition;
                if (this.ruins.Walk(this.walker, previousPosition, this.Camera.Position, out walkerPosition))
                {
                    this.Camera.Goto(walkerPosition);
                }
                else
                {
                    this.Camera.Goto(previousPosition);
                }
            }

            #endregion

            #region Light

            if (this.movingfire.Visible)
            {
                float d = 0.5f;

                Vector3 position = Vector3.Zero;
                position.X = 3.0f * d * (float)Math.Cos(0.4f * this.Game.GameTime.TotalSeconds);
                position.Y = 1f;
                position.Z = 3.0f * d * (float)Math.Sin(0.4f * this.Game.GameTime.TotalSeconds);

                this.movingfire.Manipulator.SetPosition(position);
                this.movingFireLight.Position = this.movingfire.Manipulator.Position;
            }

            this.DEBUGUpdateMovingVolumesDrawer();

            #endregion

            base.Update(gameTime);
        }

        private void UpdateInput(bool shift, bool rightBtn)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.InitializeCamera();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.RenderMode = this.RenderMode == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning;
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

#if DEBUG
            if (rightBtn)
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                Ray pRay = this.GetPickingRay();

                Vector3 p;
                Triangle t;
                float d;
                if (this.ruins.PickNearest(ref pRay, true, out p, out t, out d))
                {
                    this.volumesDrawer.SetTriangles(Color.White, new[] { t });
                }
            }

#if DEBUG
            this.fps.Text = string.Format(
                "Mouse (X:{0}; Y:{1}, Wheel: {2}) Absolute (X:{3}; Y:{4}) Cursor ({5})",
                this.Game.Input.MouseXDelta,
                this.Game.Input.MouseYDelta,
                this.Game.Input.MouseWheelDelta,
                this.Game.Input.MouseX,
                this.Game.Input.MouseY,
                this.cursor.CursorPosition);
#else
            this.fps.Text = this.Game.RuntimeText;
#endif
        }
        private void UpdateInputEnabledLights()
        {
            this.Lights.DirectionalLights[0].Enabled = this.directionalLightCount > 0;
            this.Lights.DirectionalLights[1].Enabled = this.directionalLightCount > 1;
            this.Lights.DirectionalLights[2].Enabled = this.directionalLightCount > 2;
        }

        private void DEBUGUpdateVolumesDrawer()
        {
            this.volumesDrawer.SetLines(this.ruinsVolumeColor, Line3D.CreateWiredBox(this.ruins.GetBoundingBox()));

            List<Line3D> volumesTorchs = new List<Line3D>();
            for (int i = 0; i < this.torchs.Count; i++)
            {
                volumesTorchs.AddRange(Line3D.CreateWiredBox(this.torchs.Instances[i].GetBoundingBox()));
            }
            this.volumesDrawer.SetLines(this.torchVolumeColor, volumesTorchs.ToArray());

            for (int i = 1; i < this.Lights.PointLights.Length; i++)
            {
                var light = this.Lights.PointLights[i];

                this.volumesDrawer.SetLines(
                    light.LightColor,
                    Line3D.CreateWiredSphere(light.BoundingSphere, this.bsphSlices, this.bsphStacks));
            }
        }
        private void DEBUGUpdateMovingVolumesDrawer()
        {
            var light = this.Lights.PointLights[0];

            this.volumesDrawer.SetLines(
                light.LightColor,
                Line3D.CreateWiredSphere(light.BoundingSphere, this.bsphSlices, this.bsphStacks));
        }
        private void DEBUGUpdateGraphDrawer()
        {
            var nodes = this.ruins.GetNodes(this.walker);
            if (nodes != null && nodes.Length > 0)
            {
                Random clrRnd = new Random(1);
                Color[] regions = new Color[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    regions[i] = new Color(clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), 0.55f);
                }

                this.graphDrawer.Clear();

                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = (NavigationMeshNode)nodes[i];
                    var color = regions[node.RegionId];
                    var poly = node.Poly;
                    var tris = poly.Triangulate();

                    this.graphDrawer.AddTriangles(color, tris);
                }
            }
        }
    }
}
