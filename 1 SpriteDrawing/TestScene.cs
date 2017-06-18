using Engine;

namespace SpriteDrawing
{
    public class TestScene : Scene
    {
        private const float delta = 25f;

        private SceneObject<Sprite> background = null;
        private SceneObject<Sprite> spriteFixed = null;
        private SceneObject<Sprite> spriteMov = null;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            SpriteDescription spriteMovDesc = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "smiley.jpg" },
                Width = 128,
                Height = 128,
                FitScreen = true,
            };
            this.spriteMov = this.AddSprite(spriteMovDesc, 3);

            SpriteDescription spriteFixedDesc = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "seafloor.dds" },
                Width = 256,
                Height = 256,
                FitScreen = true,
            };
            this.spriteFixed = this.AddSprite(spriteFixedDesc, 2);

            SpriteBackgroundDescription bkDescription = new SpriteBackgroundDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "background.jpg" },
            };
            this.background = this.AddBackgroud(bkDescription, 1);

            this.spriteMov.Transform2D.SetPosition(256, 0);
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
                this.spriteMov.Transform2D.SetPosition(0, 0);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.spriteMov.Transform2D.MoveLeft(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.spriteMov.Transform2D.MoveRight(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.spriteMov.Transform2D.MoveUp(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.spriteMov.Transform2D.MoveDown(gameTime, delta);
            }
        }
    }
}
