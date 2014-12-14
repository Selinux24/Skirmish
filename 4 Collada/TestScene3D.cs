using System;
using Engine;
using SharpDX;
using SharpDX.DirectInput;

namespace Collada
{
    public class TestScene3D : Scene3D
    {
        private const float fogStartRel = 0.25f;
        private const float fogRangeRel = 0.75f;

        private readonly Vector3 minScaleSize = new Vector3(0.5f);
        private readonly Vector3 maxScaleSize = new Vector3(2f);

        private TextControl title = null;
        private TextControl fps = null;
        private Terrain ground = null;
        private ModelInstanced lamps = null;
        private ModelInstanced helicopters = null;

        private bool chaseCamera = false;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.title.Text = "Collada Scene with billboards and animation";
            this.title.Position = Vector2.Zero;

            this.fps = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.fps.Text = null;
            this.fps.Position = new Vector2(0, 24);

            TerrainDescription terrainDescription = new TerrainDescription()
            {
                AddVegetation = true,
                VegetarionTextures = new[] { "tree0.dds", "tree1.dds", "tree2.dds", "tree3.dds", },
                Saturation = 0.5f,
                MinSize = Vector2.One * 3f,
                MaxSize = Vector2.One * 5f,
                Seed = 1024,
            };

            this.ground = this.AddTerrain("Ground.dae", Matrix.Scaling(20, 40, 20), terrainDescription);
            this.helicopters = this.AddInstancingModel("Helicopter.dae", 15);
            this.lamps = this.AddInstancingModel("Poly.dae", 2);

            this.InitializeCamera();
            this.InitializeEnvironment();
            this.InitializeHelicopters();
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 250;
            this.Camera.Mode = CameraModes.Free;
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            this.Lights.FogStart = this.Camera.FarPlaneDistance * fogStartRel;
            this.Lights.FogRange = this.Camera.FarPlaneDistance * fogRangeRel;
            this.Lights.FogColor = Color.CornflowerBlue;

            this.Lights.PointLightEnabled = true;
            this.Lights.PointLight.Ambient = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
            this.Lights.PointLight.Diffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Specular = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Attributes = new Vector3(0.1f, 0.0f, 0.0f);
            this.Lights.PointLight.Range = 80.0f;

            this.Lights.SpotLightEnabled = true;
            this.Lights.SpotLight.Direction = Vector3.Down;
            this.Lights.SpotLight.Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.SpotLight.Diffuse = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
            this.Lights.SpotLight.Specular = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            this.Lights.SpotLight.Attributes = new Vector3(0.15f, 0.0f, 0.0f);
            this.Lights.SpotLight.Spot = 16f;
            this.Lights.SpotLight.Range = 100.0f;
        }
        private void InitializeHelicopters()
        {
            int rows = 3;
            float left = 15f;
            float back = 25f;
            int x = 0;
            int z = 0;

            this.lamps[0].SetScale(0.1f);
            this.lamps[1].SetScale(0.1f);

            for (int i = 0; i < this.helicopters.Count; i++)
            {
                this.helicopters[i].LinearVelocity = 10f;
                this.helicopters[i].AngularVelocity = 45f;

                if (x >= rows) x = 0;
                z = i / rows;

                float posX = (x++ * left);
                float posZ = (z * -back);

                Vector3? v = this.ground.SetToGround(posX, posZ);
                if (v.HasValue)
                {
                    this.helicopters[i].SetScale(1);
                    this.helicopters[i].SetRotation(Quaternion.Identity);
                    this.helicopters[i].SetPosition(v.Value + (Vector3.UnitY * 15f));
                }
            }

            this.Camera.Position = this.helicopters.Manipulator.Position + (Vector3.One * 10f);
            this.Camera.Interest = this.helicopters.Manipulator.Position + (Vector3.UnitY * 2.5f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Key.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Key.Home))
            {
                this.InitializeHelicopters();
            }

            this.UpdateCamera(gameTime);
            this.UpdateEnvironment(gameTime);
            this.UpdateHelicopters(gameTime);

            this.fps.Text = this.Game.RuntimeText;
        }
        private void UpdateCamera(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Key.C))
            {
                this.chaseCamera = !this.chaseCamera;
            }

            if (!this.chaseCamera)
            {
                bool slow = this.Game.Input.KeyPressed(Key.LeftShift);

                if (this.Game.Input.KeyPressed(Key.A))
                {
                    this.Camera.MoveLeft(this.Game.GameTime, slow);
                }

                if (this.Game.Input.KeyPressed(Key.D))
                {
                    this.Camera.MoveRight(this.Game.GameTime, slow);
                }

                if (this.Game.Input.KeyPressed(Key.W))
                {
                    this.Camera.MoveForward(this.Game.GameTime, slow);
                }

                if (this.Game.Input.KeyPressed(Key.S))
                {
                    this.Camera.MoveBackward(this.Game.GameTime, slow);
                }

                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseX,
                    this.Game.Input.MouseY);
            }
            else
            {
                Vector3 position = this.helicopters.Manipulator.Position;
                Vector3 interest = (position - (this.helicopters.Manipulator.Forward * 10f));
                position += this.helicopters.Manipulator.Up * 2f;
                position += this.helicopters.Manipulator.Forward * -2f;

                this.Camera.Position = position;
                this.Camera.Interest = interest;
            }
        }
        private void UpdateEnvironment(GameTime gameTime)
        {
            #region First lamp

            float r = 500.0f;

            float lampPosX = r * (float)Math.Cos(1f / r * this.Game.GameTime.TotalSeconds);
            float lampPosZ = r * (float)Math.Sin(1f / r * this.Game.GameTime.TotalSeconds);

            Vector3? lampPos = this.ground.SetToGround(lampPosX, lampPosZ);
            if (lampPos.HasValue)
            {
                this.lamps[0].SetPosition(lampPos.Value + (Vector3.UnitY * 30f));
            }

            this.Lights.PointLight.Position = this.lamps[0].Position;

            #endregion

            if (this.Game.Input.KeyJustReleased(Key.NumberPad0))
            {
                this.Lights.DirectionalLight1Enabled = true;
                this.Lights.DirectionalLight2Enabled = true;
                this.Lights.DirectionalLight3Enabled = true;
                this.Lights.PointLightEnabled = true;
                this.Lights.SpotLightEnabled = true;
            }

            if (this.Game.Input.KeyJustReleased(Key.NumberPad1))
            {
                this.Lights.DirectionalLight1Enabled = !this.Lights.DirectionalLight1Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Key.NumberPad2))
            {
                this.Lights.DirectionalLight2Enabled = !this.Lights.DirectionalLight2Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Key.NumberPad3))
            {
                this.Lights.DirectionalLight3Enabled = !this.Lights.DirectionalLight3Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Key.NumberPad4))
            {
                this.Lights.PointLightEnabled = !this.Lights.PointLightEnabled;
            }

            if (this.Game.Input.KeyJustReleased(Key.NumberPad5))
            {
                this.Lights.SpotLightEnabled = !this.Lights.SpotLightEnabled;
            }

            if (this.Game.Input.KeyJustReleased(Key.NumberPad6))
            {
                if (this.Lights.FogRange == 0)
                {
                    this.Lights.FogStart = this.Camera.FarPlaneDistance * fogStartRel;
                    this.Lights.FogRange = this.Camera.FarPlaneDistance * fogRangeRel;
                }
                else
                {
                    this.Lights.FogStart = 0;
                    this.Lights.FogRange = 0;
                }
            }
        }
        private void UpdateHelicopters(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Key.Tab))
            {
                this.helicopters.Next();
            }

            if (this.Game.Input.KeyPressed(Key.O))
            {
                this.helicopters.Manipulator.MoveLeft(gameTime);
            }

            if (this.Game.Input.KeyPressed(Key.P))
            {
                this.helicopters.Manipulator.MoveRight(gameTime);
            }

            if (this.Game.Input.KeyPressed(Key.Z))
            {
                this.helicopters.Manipulator.MoveUp(gameTime);
            }

            if (this.Game.Input.KeyPressed(Key.X))
            {
                this.helicopters.Manipulator.MoveDown(gameTime);
            }

            if (this.Game.Input.KeyPressed(Key.Up))
            {
                this.helicopters.Manipulator.MoveForward(gameTime);
            }

            if (this.Game.Input.KeyPressed(Key.Down))
            {
                this.helicopters.Manipulator.MoveBackward(gameTime);
            }

            if (this.Game.Input.KeyPressed(Key.Left))
            {
                this.helicopters.Manipulator.YawLeft(gameTime);
            }

            if (this.Game.Input.KeyPressed(Key.Right))
            {
                this.helicopters.Manipulator.YawRight(gameTime);
            }

            if (this.Game.Input.KeyPressed(Key.U))
            {
                this.helicopters.Manipulator.RollLeft(gameTime);
            }

            if (this.Game.Input.KeyPressed(Key.I))
            {
                this.helicopters.Manipulator.RollRight(gameTime);
            }

            if (this.Game.Input.KeyPressed(Key.Add))
            {
                this.helicopters.Manipulator.Scale(gameTime, 0.1f, minScaleSize, maxScaleSize);
            }

            if (this.Game.Input.KeyPressed(Key.Subtract))
            {
                this.helicopters.Manipulator.Scale(gameTime, -0.1f, minScaleSize, maxScaleSize);
            }

            #region Second lamp

            Vector3 pos = (this.helicopters.Manipulator.Backward * 3f);
            Quaternion rot = Quaternion.RotationAxis(this.helicopters.Manipulator.Left, MathUtil.DegreesToRadians(45f));

            this.lamps[1].SetPosition(pos + this.helicopters.Manipulator.Position);
            this.lamps[1].SetRotation(rot * this.helicopters.Manipulator.Rotation);

            this.Lights.SpotLight.Position = this.lamps[1].Position;
            this.Lights.SpotLight.Direction = this.lamps[1].Down;

            #endregion
        }
    }
}
