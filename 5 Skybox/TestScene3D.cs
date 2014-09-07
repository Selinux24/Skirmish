using System;
using Common;
using SharpDX;
using SharpDX.DirectInput;

namespace Skybox
{
    public class TestScene3D : Scene3D
    {
        Cubemap skybox = null;
        BasicModel ruins = null;
        TextControl text = null;

        public TestScene3D(Game game)
            : base(game)
        {
            this.ruins = this.AddModel(
                "ruinas.dae",
                Matrix.Identity,
                Matrix.Identity,
                Matrix.Scaling(4f),
                1);

            this.skybox = this.AddCubemap(
                "sunset.dds",
                99);

            this.Lights.PointLightEnabled = true;
            this.Lights.PointLight.Ambient = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
            this.Lights.PointLight.Diffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Specular = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Attributes = new Vector3(1.0f, 0.0f, 0.1f);
            this.Lights.PointLight.Range = 20.0f;

            this.text = this.AddText("Arial", 12);
            this.text.SetText(0, 0, "Hello World!");
            this.text.SetText(0, 0, "Hola!");
            this.text.SetText(0, 0, "Hola qué tal!");

            this.Camera.Position = Vector3.One * 20f;
            this.Camera.Interest = Vector3.Zero;
        }
        public override void Update()
        {
            base.Update();

            if (this.Game.Input.KeyJustReleased(Key.Escape))
            {
                this.Game.Exit();
            }

            this.UpdateCamera();

            this.Lights.PointLight.Position.X = 25.0f * (float)Math.Cos(0.4f * this.Game.GameTime.TotalTime.TotalSeconds);
            this.Lights.PointLight.Position.Y = 3f;
            this.Lights.PointLight.Position.Z = 25.0f * (float)Math.Sin(0.4f * this.Game.GameTime.TotalTime.TotalSeconds);

            this.text.SetText(0, 0, this.Game.Form.Text);
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
    }
}
