using System;
using Common;
using SharpDX;
using SharpDX.DirectInput;

namespace Collada
{
    public class TestScene3D : Scene3D
    {
        private const float delta = 0.1f;
        private readonly Vector3 minSize = new Vector3(0.5f);
        private readonly Vector3 maxSize = new Vector3(2f);

        private InstancingModel lamps = null;
        private InstancingModel helicopters = null;
        private BasicModel ground = null;
        private Billboard bb = null;

        public TestScene3D(Game game)
            : base(game)
        {
            GameEnvironment.Background = Color.CornflowerBlue.ToColor4();

            this.Lights.FogStart = this.Camera.FarPlaneDistance * 0.25f;
            this.Lights.FogRange = this.Camera.FarPlaneDistance * 0.75f;
            this.Lights.FogColor = Color.CornflowerBlue.ToColor4();

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

            this.Game.Input.HideMouse();

            this.Camera.Position = Vector3.UnitZ * 12f + Vector3.UnitX * 24f + Vector3.UnitY * 12f;
            this.Camera.Interest = Vector3.Zero;
            this.Camera.Mode = CameraModes.Free;

            this.lamps = this.AddInstancingModel("Poly.dae", 2);
            this.helicopters = this.AddInstancingModel("Helicopter.dae", 6);

            this.lamps[1].Following = this.helicopters[0];
            this.lamps[1].FollowingRelative = Matrix.Translation(0, -1, 0);

            Matrix groundScale = Matrix.Scaling(5f, 5f, 5f);

            this.ground = this.AddModel(
                "Ground.dae",
                Matrix.Identity, Matrix.Identity, groundScale);
            this.bb = this.AddBillboard(
                "Ground.dae",
                Matrix.Identity, Matrix.Identity, groundScale,
                new string[] 
                {
                    "tree0.dds",
                    "tree1.dds",
                    "tree2.dds",
                    "tree3.dds",
                },
                0.75f);

            this.InitializeTransforms();
        }
        public override void Update()
        {
            base.Update();

            if (this.Game.Input.KeyJustReleased(Key.Escape))
            {
                this.Game.Exit();
            }

            this.UpdateCamera();

            this.UpdateHelicopter();

            this.UpdateLights();
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

                this.helicopters[i].SetPosition((Vector3.UnitX * (x++ * left)) + (Vector3.UnitY * 15f) + (Vector3.UnitZ * (z * -back)));
                this.helicopters[i].SetRotation(Quaternion.Identity);
                this.helicopters[i].SetScale(1);
            }
        }
        private void UpdateCamera()
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
        private void UpdateLights()
        {
            this.Lights.PointLight.Position.X = 4.0f * (float)Math.Cos(0.3f * this.Game.GameTime.TotalTime.TotalSeconds);
            this.Lights.PointLight.Position.Y = 3f;
            this.Lights.PointLight.Position.Z = 4.0f * (float)Math.Sin(0.3f * this.Game.GameTime.TotalTime.TotalSeconds);

            this.lamps[0].SetPosition(this.Lights.PointLight.Position);

            this.Lights.SpotLight.Position = this.helicopters.Transform.Position;

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
            if (this.Game.Input.KeyJustReleased(Key.Home))
            {
                this.InitializeTransforms();
            }

            if (this.Game.Input.KeyJustReleased(Key.Tab))
            {
                this.helicopters.Next();
            }

            if (this.Game.Input.KeyPressed(Key.O))
            {
                this.helicopters.Transform.MoveLeft(delta);
            }

            if (this.Game.Input.KeyPressed(Key.P))
            {
                this.helicopters.Transform.MoveRight(delta);
            }

            if (this.Game.Input.KeyPressed(Key.Z))
            {
                this.helicopters.Transform.MoveUp(delta);
            }

            if (this.Game.Input.KeyPressed(Key.X))
            {
                this.helicopters.Transform.MoveDown(delta);
            }

            if (this.Game.Input.KeyPressed(Key.Up))
            {
                this.helicopters.Transform.MoveForward(delta);
            }

            if (this.Game.Input.KeyPressed(Key.Down))
            {
                this.helicopters.Transform.MoveBackward(delta);
            }

            if (this.Game.Input.KeyPressed(Key.Left))
            {
                this.helicopters.Transform.YawLeft(delta * 0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.Right))
            {
                this.helicopters.Transform.YawRight(delta * 0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.U))
            {
                this.helicopters.Transform.RollLeft(delta * 0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.I))
            {
                this.helicopters.Transform.RollRight(delta * 0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.Add))
            {
                this.helicopters.Transform.Scale(delta, minSize, maxSize);
            }

            if (this.Game.Input.KeyPressed(Key.Subtract))
            {
                this.helicopters.Transform.Scale(-delta, minSize, maxSize);
            }
        }
    }
}
