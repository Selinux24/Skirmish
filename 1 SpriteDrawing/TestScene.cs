using Common;
using SharpDX.DirectInput;

namespace SpriteDrawing
{
    public class TestScene : Scene3D
    {
        private BasicSprite background = null;
        private BasicSprite spriteFixed = null;
        private BasicSprite spriteMov = null;

        public TestScene(Game game)
            : base(game)
        {
            this.background = this.AddSprite(
                "background.jpg",
                game.Form.ClientSize.Width,
                game.Form.ClientSize.Height,
                99);

            this.spriteMov = this.AddSprite(
                "smiley.jpg",
                128,
                128,
                0);

            this.spriteFixed = this.AddSprite(
                "seafloor.dds",
                256,
                256,
                1);

            this.spriteMov.SetPosition(256, 0);
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
                this.spriteMov.SetPosition(0, 0);
            }

            if (this.Game.Input.KeyJustReleased(Key.A))
            {
                this.spriteMov.MoveLeft(1f);
            }

            if (this.Game.Input.KeyJustReleased(Key.D))
            {
                this.spriteMov.MoveRight(1f);
            }

            if (this.Game.Input.KeyJustReleased(Key.W))
            {
                this.spriteMov.MoveUp(1f);
            }

            if (this.Game.Input.KeyJustReleased(Key.S))
            {
                this.spriteMov.MoveDown(1f);
            }
        }
    }
}
