using Engine;
using SharpDX;
using System;

namespace SpriteDrawing
{
    public class TestScene : Scene
    {
        private const int layerHUD = 99;
        private const float delta = 250f;

        private Sprite spriteMov = null;
        private Sprite textBackPanel = null;
        private TextDrawer textDrawer = null;

        private readonly Random rnd = new Random();
        private readonly string allText = Properties.Resources.Lorem;
        private string currentText = "";
        private float textTime = 0;
        private float textInterval = 100f;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeBackground();
            InitializePan();
            InitializeSmiley();
            InitializeTextDrawer();
        }
        private void InitializeBackground()
        {
            var desc = new SpriteBackgroundDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "background.jpg" },
            };
            this.AddComponent<Sprite>(desc, SceneObjectUsages.UI, 1);
        }
        private void InitializeSmiley()
        {
            var desc = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "smiley.png" },
                Width = 256,
                Height = 256,
                FitScreen = true,
            };
            var sprite = this.AddComponent<Sprite>(desc, SceneObjectUsages.None, 3);
            sprite.ScreenTransform.SetPosition(256, 0);

            this.spriteMov = sprite.Instance;
        }
        private void InitializePan()
        {
            var desc = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "pan.jpg" },
                Width = 800,
                Height = 650,
                FitScreen = true,
            };
            textBackPanel = this.AddComponent<Sprite>(desc, SceneObjectUsages.UI, layerHUD).Instance;
        }
        private void InitializeTextDrawer()
        {
            var desc = new TextDrawerDescription()
            {
                Name = "Text",
                Font = "Viner Hand ITC",
                FontSize = 18,
                Style = FontMapStyles.Bold,
                TextColor = Color.LightGoldenrodYellow,
            };
            textDrawer = this.AddComponent<TextDrawer>(desc, SceneObjectUsages.UI, layerHUD).Instance;
        }

        public override void Initialized()
        {
            base.Initialized();

            textBackPanel.Manipulator.SetPosition(700, 100);
            textDrawer.Rectangle = new RectangleF(780, 140, 650, 550);
            textDrawer.Text = "";
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateLorem(gameTime);

            UpdateInput(gameTime);
        }
        private void UpdateInput(GameTime gameTime)
        {
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
        private void UpdateLorem(GameTime gameTime)
        {
            if (textInterval == 0)
            {
                return;
            }

            textTime += gameTime.ElapsedMilliseconds;
            if (textTime >= textInterval)
            {
                textTime = 0;

                textInterval = rnd.NextFloat(50, 100);
                int chars = rnd.Next(1, 5);

                //Add text
                if (allText.Length >= currentText.Length + chars)
                {
                    currentText += allText.Substring(currentText.Length, chars);
                }
                else
                {
                    currentText = allText;
                    textInterval = 0;
                }

                textDrawer.Text = currentText;
            }
        }
    }
}
