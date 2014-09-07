using Common;
using Common.Utils;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DirectInput;

namespace SceneTest
{
    public class TestScene3D : Scene3D
    {
        private BasicModel model = null;

        public TestScene3D(Game game)
            : base(game)
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[]
            {
                new VertexPositionTexture{ Position = new Vector3(-1.0f, 0.0f, 0.0f), Texture = new Vector2(0.0f, 1.0f) },
                new VertexPositionTexture{ Position = new Vector3(0.0f, 2.0f, 0.0f), Texture = new Vector2(0.5f, 0.0f) },
                new VertexPositionTexture{ Position = new Vector3(1.0f, 0.0f, 0.0f), Texture = new Vector2(1.0f, 1.0f) },
                new VertexPositionTexture{ Position = new Vector3(0.0f, -2.0f, 0.0f), Texture = new Vector2(0.5f, 0.0f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                0, 2, 3,
            };

            Material mat = Material.CreateTextured("resources/seafloor.dds");

            Geometry geo = game.Graphics.Device.CreateGeometry(
                mat,
                vertices,
                PrimitiveTopology.TriangleList,
                indices);

            this.model = this.AddModel(geo);
        }
        public override void Update()
        {
            base.Update();

            if (this.Game.Input.KeyJustReleased(Key.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Key.Home))
            {
                this.model.Transform.SetPosition(Vector3.Zero);
            }

            if (this.Game.Input.KeyPressed(Key.A))
            {
                this.model.Transform.MoveLeft(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.D))
            {
                this.model.Transform.MoveRight(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.W))
            {
                this.model.Transform.MoveUp(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.S))
            {
                this.model.Transform.MoveDown(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.Z))
            {
                this.model.Transform.MoveForward(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.X))
            {
                this.model.Transform.MoveBackward(0.1f);
            }

            this.UpdateCamera();
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
