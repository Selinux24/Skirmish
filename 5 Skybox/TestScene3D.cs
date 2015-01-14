using System;
using System.Windows.Forms;
using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;

namespace Skybox
{
    public class TestScene3D : Scene3D
    {
        private const float globalScale = 1f;

        private Vector3[] firePositions = new[]
        {
            new Vector3(5, 1, 5),
            new Vector3(-5, 1, -5),
        };
        private Vector3 walkerHeight = Vector3.UnitY;
        private float walkerClimb = MathUtil.DegreesToRadians(45);

        private Terrain ruins = null;
        private ModelInstanced lamp = null;
        private TextDrawer title = null;
        private TextDrawer help = null;
        private TextDrawer fps = null;
        private ParticleSystem rain = null;
        private ParticleSystem fire = null;
        private LineListDrawer pickedTri = null;
        private LineListDrawer bboxGlobalDrawer = null;
        private LineListDrawer bboxMeshesDrawer = null;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            TerrainDescription desc = new TerrainDescription()
            {
                AddSkydom = true,
                SkydomTexture = "sunset.dds",
            };

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.help = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.fps = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.ruins = this.AddTerrain("ruinas.dae", desc);
            this.lamp = this.AddInstancingModel("Poly.dae", 3);
            this.rain = this.AddParticleSystem(ParticleSystemDescription.Rain("raindrop.dds"));
            this.fire = this.AddParticleSystem(ParticleSystemDescription.Fire(firePositions, "flare2.png"));
            this.pickedTri = this.AddLineListDrawer(new Line[3], Color.Red);
            this.bboxGlobalDrawer = this.AddLineListDrawer(ModelContent.CreateBoxWired(this.ruins.BoundingBox), Color.Yellow);
            this.bboxMeshesDrawer = this.AddLineListDrawer(ModelContent.CreateBoxWired(this.ruins.GetBoundingBoxes()), Color.Gray);

            this.Lights.PointLightEnabled = true;
            this.Lights.PointLight.Ambient = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
            this.Lights.PointLight.Diffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Specular = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Attributes = new Vector3(1.0f, 0.0f, 0.0f);
            this.Lights.PointLight.Range = 20.0f;

            this.title.Text = "Collada Scene with Skybox";
#if DEBUG
            this.help.Text = "Escape: Exit | Home: Reset camera | AWSD: Move camera | Left Mouse: Drag view | Middle Mouse: Pick";
#else
            this.help.Text = "Escape: Exit | Home: Reset camera | AWSD: Move camera | Left Mouse: View | Middle Mouse: Pick";
#endif
            this.fps.Text = null;

            this.title.Position = Vector2.Zero;
            this.help.Position = new Vector2(0, 24);
            this.fps.Position = new Vector2(0, 40);

            this.InitializeCamera();
            this.InitializeTerrain();
        }
        private void InitializeTerrain()
        {
            this.ruins.Manipulator.SetScale(globalScale);
            this.ruins.ComputeVolumes(Matrix.Scaling(globalScale));

            this.bboxGlobalDrawer.SetLines(ModelContent.CreateBoxWired(this.ruins.BoundingBox), Color.Yellow);
            this.bboxMeshesDrawer.SetLines(ModelContent.CreateBoxWired(this.ruins.GetBoundingBoxes()), Color.Gray);

            this.lamp[0].SetScale(0.01f * globalScale);
            this.lamp[1].SetScale(0.01f * globalScale);
            this.lamp[2].SetScale(0.01f * globalScale);

            this.lamp[1].SetPosition(firePositions[0]);
            this.lamp[2].SetPosition(firePositions[1]);
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 50.0f * globalScale;
            this.Camera.Goto(this.walkerHeight);
            this.Camera.LookTo(Vector3.UnitY + Vector3.UnitZ + this.ruins.Manipulator.Position * globalScale);
            this.Camera.MovementDelta = 8f;
            this.Camera.SlowMovementDelta = 4f;
        }

        public override void Update(GameTime gameTime)
        {
            Vector3 previousPosition = this.Camera.Position;

            this.UpdateInput();

#if DEBUG
            this.fps.Text = string.Format(
                "Mouse (X:{0}; Y:{1}, Wheel: {2}) Absolute (X:{3}; Y:{4})",
                this.Game.Input.MouseXDelta,
                this.Game.Input.MouseYDelta,
                this.Game.Input.MouseWheelDelta,
                this.Game.Input.MouseX,
                this.Game.Input.MouseY);
#else
            this.fps.Text = this.Game.RuntimeText;
#endif

            base.Update(gameTime);

            Vector3 currentPosition = this.Camera.Position;

            #region Light

            float d = globalScale * 0.5f;

            Vector3 position = Vector3.Zero;
            position.X = 3.0f * d * (float)Math.Cos(0.4f * this.Game.GameTime.TotalSeconds);
            position.Y = globalScale;
            position.Z = 3.0f * d * (float)Math.Sin(0.4f * this.Game.GameTime.TotalSeconds);

            this.Lights.PointLight.Position = position;
            this.lamp[0].SetPosition(position);

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
                if (this.ruins.FindGroundPosition(v.X, v.Z, out p, out tri))
                {
                    if (tri.Inclination > walkerClimb)
                    {
                        this.Camera.Goto(previousPosition);
                    }
                    else
                    {
                        //Position test
                        if (this.ruins.FindGroundPosition(currentPosition.X, currentPosition.Z, out p, out tri))
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
            if (this.Game.Input.LeftMouseButtonPressed)
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

            if (this.Game.Input.MiddleMouseButtonPressed)
            {
                Ray pRay = this.GetPickingRay();

                Vector3 p;
                Triangle t;
                if (this.ruins.Pick(pRay, out p, out t))
                {
                    this.pickedTri.SetLines(ModelContent.CreateTriangleWired(t), Color.Red);
                }
            }
        }
    }
}
