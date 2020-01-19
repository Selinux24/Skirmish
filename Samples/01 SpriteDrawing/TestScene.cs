using Engine;
using SharpDX;
using System.Threading.Tasks;

namespace SpriteDrawing
{
    public class TestScene : Scene
    {
        private const int layerHUD = 99;
        private const float delta = 250f;

        private Sprite spriteMov = null;
        private Sprite textBackPanel = null;
        private TextDrawer textDrawer = null;

        private readonly string allText = Properties.Resources.Lorem;
        private string currentText = "";
        private float textTime = 0;
        private float textInterval = 200f;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            await InitializeBackground();
            await InitializePan();
            await InitializeSmiley();
            await InitializeTextDrawer();
        }
        private async Task InitializeBackground()
        {
            var desc = new SpriteBackgroundDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "background.jpg" },
            };
            await this.AddComponent<Sprite>(desc, SceneObjectUsages.UI, 1);
        }
        private async Task InitializeSmiley()
        {
            var desc = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "smiley.png" },
                Width = 256,
                Height = 256,
                FitScreen = true,
            };
            var sprite = await this.AddComponent<Sprite>(desc, SceneObjectUsages.None, 3);
            sprite.ScreenTransform.SetPosition(256, 0);

            this.spriteMov = sprite.Instance;
        }
        private async Task InitializePan()
        {
            var desc = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "pan.jpg" },
                Width = 800,
                Height = 650,
                FitScreen = true,
            };
            textBackPanel = (await this.AddComponent<Sprite>(desc, SceneObjectUsages.UI, layerHUD)).Instance;
        }
        private async Task InitializeTextDrawer()
        {
            var desc = new TextDrawerDescription()
            {
                Name = "Text",
                Font = "Viner Hand ITC",
                FontSize = 18,
                Style = FontMapStyles.Bold,
                TextColor = Color.LightGoldenrodYellow,
            };
            textDrawer = (await this.AddComponent<TextDrawer>(desc, SceneObjectUsages.UI, layerHUD)).Instance;
        }

        public override async Task Initialized()
        {
            await base.Initialized();

            textBackPanel.Manipulator.SetPosition(700, 100);
            textDrawer.Rectangle = new RectangleF(780, 140, 650, 550);
            textDrawer.Text = null;
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

                textInterval = Helper.RandomGenerator.NextFloat(50, 200);
                int chars = Helper.RandomGenerator.Next(1, 5);

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
