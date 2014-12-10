using System;
using Engine;
using SharpDX;
using SharpDX.DirectInput;

namespace Collada
{
    public class TestScene3D : Scene3D
    {
        private const float delta = 0.1f;
        private readonly Vector3 minSize = new Vector3(0.5f);
        private readonly Vector3 maxSize = new Vector3(2f);

        private TextControl title = null;
        private Terrain theGround = null;
        private ModelInstanced lamps = null;
        private ModelInstanced helicopters = null;
        private Model aniModel = null;

        private bool chaseCamera = false;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.title.Text = "Collada Scene with billboards and skinned instanced models";
            this.title.Position = Vector2.Zero;

            TerrainDescription terrainDescription = new TerrainDescription()
            {
                AddVegetation = true,
                VegetarionTextures = new[] { "tree0.dds", "tree1.dds", "tree2.dds", "tree3.dds", },
                Saturation = 0.5f,
                MinSize = Vector2.One * 3f,
                MaxSize = Vector2.One * 5f,
                Seed = 1024,
            };

            this.theGround = this.AddTerrain("Ground.dae", Matrix.Scaling(20, 40, 20), terrainDescription);
            this.aniModel = this.AddModel("anicube.dae");
            this.helicopters = this.AddInstancingModel("Helicopter.dae", 15);
            this.lamps = this.AddInstancingModel("Poly.dae", 2);

            this.InitializeTransforms();
            this.InitializeCamera();
            this.InitializeEnvironment();
        }
        private void InitializeTransforms()
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
                if (x >= rows) x = 0;
                z = i / rows;

                float posX = (x++ * left);
                float posZ = (z * -back);

                Vector3? v = this.theGround.SetToGround(posX, posZ);
                if (v.HasValue)
                {
                    this.helicopters[i].SetPosition(v.Value + (Vector3.UnitY * 15f));
                }
            }

            Vector3? aniModelPosition = this.theGround.SetToGround(0, 0);
            if (aniModelPosition.HasValue)
            {
                this.aniModel.Manipulator.SetPosition(aniModelPosition.Value);
            }

            this.aniModel.Manipulator.SetScale(0.5f);
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 250;

            this.Camera.Mode = CameraModes.Free;
            //this.Camera.Position = new Vector3(-5, 15, -10);
            this.Camera.Position = this.helicopters.Manipulator.Position + (Vector3.One * 10f);
            this.Camera.Interest = this.helicopters.Manipulator.Position + (Vector3.UnitY * 2.5f);
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            this.Lights.FogStart = this.Camera.FarPlaneDistance * 0.25f;
            this.Lights.FogRange = this.Camera.FarPlaneDistance * 0.75f;
            this.Lights.FogColor = Color.CornflowerBlue;

            this.Lights.PointLightEnabled = true;
            this.Lights.PointLight.Ambient = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
            this.Lights.PointLight.Diffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Specular = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Attributes = new Vector3(1.0f, 0.0f, 0.0f);
            this.Lights.PointLight.Range = 4.0f;

            this.Lights.SpotLightEnabled = true;
            this.Lights.SpotLight.Direction = Vector3.Down;
            this.Lights.SpotLight.Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.SpotLight.Diffuse = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
            this.Lights.SpotLight.Specular = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            this.Lights.SpotLight.Attributes = new Vector3(1.0f, 0.0f, 0.0f);
            this.Lights.SpotLight.Spot = 16f;
            this.Lights.SpotLight.Range = 10000.0f;
        }

        public override void Update(GameTime gameTime)
        {
            #region First lamp

            float lampPosX = 4.0f * (float)Math.Cos(0.3f * this.Game.GameTime.TotalSeconds);
            float lampPosZ = 4.0f * (float)Math.Sin(0.3f * this.Game.GameTime.TotalSeconds);

            Vector3? lampPos = this.theGround.SetToGround(lampPosX, lampPosZ);
            if (lampPos.HasValue)
            {
                this.lamps[0].SetPosition(lampPos.Value + (Vector3.UnitY * 3f));
            }

            #endregion

            #region Second lamp

            this.lamps[1].Following = this.helicopters.Manipulator;
            this.lamps[1].FollowingRelative = Matrix.Translation(0, 5, 0);

            #endregion

            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Key.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Key.Home))
            {
                if (this.Game.Input.KeyPressed(Key.LeftShift) || this.Game.Input.KeyPressed(Key.RightShift))
                {
                    this.InitializeCamera();
                }
                else
                {
                    this.InitializeTransforms();

                    this.Camera.Interest = this.helicopters.Manipulator.Position;
                }
            }

            if (this.Game.Input.KeyPressed(Key.LeftControl))
            {
                this.UpdateTerrain();
            }
            else
            {
                this.UpdateCamera();
            }

            this.UpdateHelicopter();

            this.UpdateLights();
        }
        private void UpdateCamera()
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
        private void UpdateTerrain()
        {
            if (this.Game.Input.KeyPressed(Key.W))
            {
                this.theGround.Manipulator.MoveForward(delta);
            }

            if (this.Game.Input.KeyPressed(Key.S))
            {
                this.theGround.Manipulator.MoveBackward(delta);
            }

            if (this.Game.Input.KeyPressed(Key.A))
            {
                this.theGround.Manipulator.MoveLeft(delta);
            }

            if (this.Game.Input.KeyPressed(Key.D))
            {
                this.theGround.Manipulator.MoveRight(delta);
            }
        }
        private void UpdateLights()
        {
            this.Lights.PointLight.Position = this.lamps[0].Position;
            this.Lights.SpotLight.Position = this.lamps[1].Position;

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
        }
        private void UpdateHelicopter()
        {
            if (this.Game.Input.KeyJustReleased(Key.Tab))
            {
                this.helicopters.Next();
            }

            if (this.Game.Input.KeyPressed(Key.O))
            {
                this.helicopters.Manipulator.MoveLeft(delta);
            }

            if (this.Game.Input.KeyPressed(Key.P))
            {
                this.helicopters.Manipulator.MoveRight(delta);
            }

            if (this.Game.Input.KeyPressed(Key.Z))
            {
                this.helicopters.Manipulator.MoveUp(delta);
            }

            if (this.Game.Input.KeyPressed(Key.X))
            {
                this.helicopters.Manipulator.MoveDown(delta);
            }

            if (this.Game.Input.KeyPressed(Key.Up))
            {
                this.helicopters.Manipulator.MoveForward(delta);
            }

            if (this.Game.Input.KeyPressed(Key.Down))
            {
                this.helicopters.Manipulator.MoveBackward(delta);
            }

            if (this.Game.Input.KeyPressed(Key.Left))
            {
                this.helicopters.Manipulator.YawLeft(delta * 0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.Right))
            {
                this.helicopters.Manipulator.YawRight(delta * 0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.U))
            {
                this.helicopters.Manipulator.RollLeft(delta * 0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.I))
            {
                this.helicopters.Manipulator.RollRight(delta * 0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.Add))
            {
                this.helicopters.Manipulator.Scale(delta, minSize, maxSize);
            }

            if (this.Game.Input.KeyPressed(Key.Subtract))
            {
                this.helicopters.Manipulator.Scale(-delta, minSize, maxSize);
            }
        }
    }
}
