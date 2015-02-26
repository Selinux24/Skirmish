﻿using Engine;

namespace SpriteDrawing
{
    public class TestScene : Scene
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

            SpriteDescription spriteMovDesc = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "smiley.jpg" },
                Width = 128,
                Height = 128,
                FitScreen = true,
            };
            this.spriteMov = this.AddSprite(spriteMovDesc, 1);

            SpriteDescription spriteFixedDesc = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "seafloor.dds" },
                Width = 256,
                Height = 256,
                FitScreen = true,
            };
            this.spriteFixed = this.AddSprite(spriteFixedDesc, 2);

            BackgroundDescription bkDescription = new BackgroundDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "background.jpg" },
            };
            this.background = this.AddBackgroud(bkDescription, 99);

            this.spriteMov.Manipulator.SetPosition(256, 0);
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
                this.spriteMov.Manipulator.SetPosition(0, 0);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.spriteMov.Manipulator.MoveLeft(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.spriteMov.Manipulator.MoveRight(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.spriteMov.Manipulator.MoveUp(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.spriteMov.Manipulator.MoveDown(gameTime, delta);
            }
        }
    }
}
