using Engine;
using Engine.Content;
using SharpDX;
using SharpDX.DirectInput;

namespace SceneTest
{
    public class TestScene3D : Scene3D
    {
        private Model model = null;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.model = this.AddModel("poly.dae");
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
                this.model.Manipulator.SetPosition(Vector3.Zero);
            }

            if (this.Game.Input.KeyPressed(Key.A))
            {
                this.model.Manipulator.MoveLeft(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.D))
            {
                this.model.Manipulator.MoveRight(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.W))
            {
                this.model.Manipulator.MoveUp(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.S))
            {
                this.model.Manipulator.MoveDown(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.Z))
            {
                this.model.Manipulator.MoveForward(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.X))
            {
                this.model.Manipulator.MoveBackward(0.1f);
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
