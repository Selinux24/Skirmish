using Engine;

namespace SpriteDrawing
{
    public class TestScene : Scene
    {
        private const float delta = 250f;

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
                Textures = new[] { "smiley.png" },
                Width = 256,
                Height = 256,
                FitScreen = true,
            };
            this.spriteMov = this.AddComponent<Sprite>(spriteMovDesc, SceneObjectUsageEnum.None, 3);

            SpriteDescription spriteFixedDesc = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "seafloor.dds" },
                Width = 512,
                Height = 512,
                FitScreen = true,
            };
            this.spriteFixed = this.AddComponent<Sprite>(spriteFixedDesc, SceneObjectUsageEnum.None, 2);

            SpriteBackgroundDescription bkDescription = new SpriteBackgroundDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "background.jpg" },
            };
            this.background = this.AddComponent<Sprite>(bkDescription, SceneObjectUsageEnum.None, 1);

            this.spriteMov.ScreenTransform.SetPosition(256, 0);
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
                this.spriteMov.ScreenTransform.SetPosition(0, 0);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.spriteMov.ScreenTransform.MoveLeft(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.spriteMov.ScreenTransform.MoveRight(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.spriteMov.ScreenTransform.MoveUp(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.spriteMov.ScreenTransform.MoveDown(gameTime, delta);
            }
        }
    }
}
