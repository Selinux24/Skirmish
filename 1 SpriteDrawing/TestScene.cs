using Engine;
using SharpDX.DirectInput;

namespace SpriteDrawing
{
    public class TestScene : Scene3D
    {
        private const float delta = 25f;

        private Sprite background = null;
        private Sprite spriteFixed = null;
        private Sprite spriteMov = null;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.spriteMov = this.AddSprite(
                "smiley.jpg",
                128,
                128,
                1);

            this.spriteFixed = this.AddSprite(
                "seafloor.dds",
                256,
                256,
                2);

            this.background = this.AddSprite(
                "background.jpg",
                this.Game.Form.ClientSize.Width,
                this.Game.Form.ClientSize.Height,
                99);

            this.background.FitScreen = true;

            this.spriteMov.Manipulator.SetPosition(256, 0);
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
                this.spriteMov.Manipulator.SetPosition(0, 0);
            }

            if (this.Game.Input.KeyPressed(Key.A))
            {
                this.spriteMov.Manipulator.MoveLeft(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Key.D))
            {
                this.spriteMov.Manipulator.MoveRight(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Key.W))
            {
                this.spriteMov.Manipulator.MoveUp(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Key.S))
            {
                this.spriteMov.Manipulator.MoveDown(gameTime, delta);
            }
        }
    }
}
