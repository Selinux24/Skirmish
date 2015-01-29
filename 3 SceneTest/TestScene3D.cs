using Engine;
using Engine.Common;
using SharpDX;

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

            this.ContentPath = "Resources3D";

            this.model = this.AddModel("poly.dae");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.model.Manipulator.SetPosition(Vector3.Zero);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.model.Manipulator.MoveLeft(gameTime);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.model.Manipulator.MoveRight(gameTime);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.model.Manipulator.MoveUp(gameTime);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.model.Manipulator.MoveDown(gameTime);
            }

            if (this.Game.Input.KeyPressed(Keys.Z))
            {
                this.model.Manipulator.MoveForward(gameTime);
            }

            if (this.Game.Input.KeyPressed(Keys.X))
            {
                this.model.Manipulator.MoveBackward(gameTime);
            }
        }
    }
}
