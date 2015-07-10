using System;
using Engine;
using Engine.Common;
using SharpDX;

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
        private Vector3 walkerHeight = Vector3.UnitY;
        private float walkerClimb = MathUtil.DegreesToRadians(45);
        private Color globalColor = Color.Green;
        private Color bboxColor = Color.GreenYellow;
        private Color bsphColor = Color.LightYellow;
        //private int bsphSlices = 20;
        //private int bsphStacks = 10;

        private Cursor cursor;

        private TextDrawer title = null;
        private TextDrawer help = null;
        private TextDrawer fps = null;

        private Terrain ruins = null;
        private TriangleListDrawer pickedTri = null;
        private LineListDrawer bboxGlobalDrawer = null;
        //private LineListDrawer bboxMeshesDrawer = null;
        //private LineListDrawer bsphMeshesDrawer = null;

        private ModelInstanced torchs = null;
        private ParticleSystem rain = null;
        private ParticleSystem fire = null;
        private ParticleSystem movingfire = null;

        public TestScene3D(Game game)
            : base(game)
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

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.help = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.fps = this.AddText("Lucida Casual", 12, Color.Yellow);

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

            #region Terrain

            TerrainDescription desc = new TerrainDescription()
            {
                ContentPath = "Resources",
                ModelFileName = "ruins.dae",
                AddSkydom = true,
                SkydomTexture = "sunset.dds",
                DropShadow = true,
            };
            this.ruins = this.AddTerrain(desc, false);

            this.bboxGlobalDrawer = this.AddLineListDrawer(GeometryUtil.CreateWiredBox(this.ruins.GetBoundingBox()), this.globalColor);
            //this.bboxMeshesDrawer = this.AddLineListDrawer(GeometryUtil.CreateWiredBox(this.ruins.StaticBoundingBoxes), this.bboxColor);
            //this.bsphMeshesDrawer = this.AddLineListDrawer(GeometryUtil.CreateWiredSphere(this.ruins.StaticBoundingSpheres, this.bsphSlices, this.bsphStacks), this.bsphColor);
            this.bboxGlobalDrawer.UseZBuffer = false;
            //this.bboxMeshesDrawer.UseZBuffer = true;
            //this.bsphMeshesDrawer.UseZBuffer = true;

            this.pickedTri = this.AddTriangleListDrawer(1);
            this.pickedTri.UseZBuffer = false;

            #endregion

            #region Lights

            Vector3[] firePositions3D = new Vector3[this.firePositions.Length];

            this.torchs = this.AddInstancingModel(new ModelInstancedDescription()
            {
                ContentPath = "Resources",
                ModelFileName = "torch.dae",
                Instances = this.firePositions.Length,
                DropShadow = true,
            });
            for (int i = 0; i < this.firePositions.Length; i++)
            {
                this.ruins.FindTopGroundPosition(this.firePositions[i].X, this.firePositions[i].Y, out firePositions3D[i]);

                this.torchs.Instances[i].Manipulator.SetScale(0.20f, true);
                this.torchs.Instances[i].Manipulator.SetPosition(firePositions3D[i], true);

                BoundingBox bbox = this.torchs.Instances[i].GetBoundingBox();

                firePositions3D[i].Y += (bbox.Maximum.Y - bbox.Minimum.Y) * 0.9f;
            }

            this.fire = this.AddParticleSystem(ParticleSystemDescription.Fire(firePositions3D, 0.5f, "flare1.png"));

            #endregion

            #region Particles

            this.rain = this.AddParticleSystem(ParticleSystemDescription.Rain(0.5f, "raindrop.dds"));
            this.movingfire = this.AddParticleSystem(ParticleSystemDescription.Fire(new[] { Vector3.Zero }, 0.5f, "flare2.png"));

            #endregion

            this.Lights.PointLightEnabled = true;
            this.Lights.PointLight.Ambient = new Color4(1f, 0.5f, 0.5f, 1.0f);
            this.Lights.PointLight.Diffuse = new Color4(1f, 0.8f, 0.8f, 1.0f);
            this.Lights.PointLight.Specular = new Color4(1f, 0.8f, 0.8f, 1.0f);
            this.Lights.PointLight.Attenuation = new Vector3(1.0f, 0.0f, 0.0f);
            this.Lights.PointLight.Range = 20.0f;
            this.Lights.EnableShadows = true;

            this.SceneVolume = this.ruins.GetBoundingSphere();

            this.InitializeCamera();
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 50.0f;
            this.Camera.Goto(this.walkerHeight);
            this.Camera.LookTo(Vector3.UnitY + Vector3.UnitZ);
            this.Camera.MovementDelta = 8f;
            this.Camera.SlowMovementDelta = 4f;
        }

        public override void Update(GameTime gameTime)
        {
            Vector3 previousPosition = this.Camera.Position;

            this.UpdateInput();

            base.Update(gameTime);

            Vector3 currentPosition = this.Camera.Position;

            #region Light

            float d = 0.5f;

            Vector3 position = Vector3.Zero;
            position.X = 3.0f * d * (float)Math.Cos(0.4f * this.Game.GameTime.TotalSeconds);
            position.Y = 1f;
            position.Z = 3.0f * d * (float)Math.Sin(0.4f * this.Game.GameTime.TotalSeconds);

            this.Lights.PointLight.Position = position;
            this.movingfire.Manipulator.SetPosition(position);

            #endregion

            #region Walk

            //Direction test
            {
                Vector3 v = Vector3.Zero;
                if (previousPosition == currentPosition)
                {
                    v = this.Camera.Direction * 2f;
                }
                else
                {
                    Vector3 v1 = new Vector3(previousPosition.X, 0f, previousPosition.Z);
                    Vector3 v2 = new Vector3(currentPosition.X, 0f, currentPosition.Z);

                    v = Vector3.Normalize(v1 - v2) * this.Camera.NearPlaneDistance;
                }

                Vector3 p;
                Triangle tri;
                if (this.ruins.FindTopGroundPosition(v.X, v.Z, out p, out tri))
                {
                    if (tri.Inclination > walkerClimb)
                    {
                        this.Camera.Goto(previousPosition);
                    }
                    else
                    {
                        //Position test
                        if (this.ruins.FindTopGroundPosition(currentPosition.X, currentPosition.Z, out p, out tri))
                        {
                            if (tri.Inclination > walkerClimb)
                            {
                                this.Camera.Goto(previousPosition);
                            }
                            else
                            {
                                this.Camera.Goto(p + this.walkerHeight);
                            }
                        }
                        else
                        {
                            this.Camera.Goto(previousPosition);
                        }
                    }
                }
                else
                {
                    this.Camera.Goto(previousPosition);
                }
            }

            #endregion
        }
        private void UpdateInput()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.InitializeCamera();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.bboxGlobalDrawer.Visible = !this.bboxGlobalDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                //this.bboxMeshesDrawer.Visible = !this.bboxMeshesDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                //this.bsphMeshesDrawer.Visible = !this.bsphMeshesDrawer.Visible;
            }

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

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                Ray pRay = this.GetPickingRay();

                Vector3 p;
                Triangle t;
                if (this.ruins.PickNearest(ref pRay, out p, out t))
                {
                    this.pickedTri.SetTriangles(Color.Red, new[] { t });
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
    }
}
