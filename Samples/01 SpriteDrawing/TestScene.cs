using Engine;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace SpriteDrawing
{
    public class TestScene : Scene
    {
        private const int layerBackground = 1;
        private const int layerObjects = 50;
        private const int layerHUD = 99;
        private const float delta = 250f;

        private Sprite spriteMov = null;
        private Sprite textBackPanel = null;
        private TextDrawer textDrawer = null;
        private SpriteProgressBar progressBar = null;

        private readonly string allText = Properties.Resources.Lorem;
        private string currentText = "";
        private float textTime = 0;
        private float textInterval = 200f;

        private bool userInterfaceInitialized = false;
        private Guid userInterfaceId = Guid.NewGuid();
        private bool gameAssetsInitialized = false;
        private bool gameAssetsInitializing = false;
        private Guid gameAssetsId = Guid.NewGuid();
        private bool gameReady = false;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override Task Initialize()
        {
            return LoadUserInteface();
        }
        protected override void GameResourcesLoaded(object sender, GameLoadResourcesEventArgs e)
        {
            if (e.Id == userInterfaceId && !userInterfaceInitialized)
            {
                userInterfaceInitialized = true;

                return;
            }

            if (e.Id == gameAssetsId && !gameAssetsInitialized)
            {
                gameAssetsInitialized = true;

                progressBar.Visible = false;
                textBackPanel.Manipulator.SetPosition(700, 100);
                textDrawer.Rectangle = new RectangleF(780, 140, 650, 550);
                textDrawer.Text = null;

                gameReady = true;
            }
        }
        public override void OnReportProgress(float value)
        {
            if (this.progressBar != null)
            {
                this.progressBar.ProgressValue = value;
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateInput();

            if (!gameAssetsInitialized && !gameAssetsInitializing)
            {
                gameAssetsInitializing = true;

                Task.WhenAll(this.LoadGameAssets());

                return;
            }

            if (!gameReady)
            {
                return;
            }

            UpdateLorem(gameTime);
            UpdateSprite(gameTime);
        }

        private async Task LoadUserInteface()
        {
            this.userInterfaceInitialized = false;

            await this.Game.LoadResourcesAsync(userInterfaceId, InitializeBackground());
        }
        private async Task LoadGameAssets()
        {
            gameAssetsInitialized = false;

            this.progressBar.Visible = true;
            this.progressBar.ProgressValue = 0;

            await this.Game.LoadResourcesAsync(gameAssetsId,
                InitializePan(),
                InitializeSmiley(),
                InitializeTextDrawer());
        }
        private async Task InitializeBackground()
        {
            var desc = new SpriteBackgroundDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "background.jpg" },
            };
            await this.AddComponentSprite(desc, SceneObjectUsages.UI, layerBackground);

            var pbDesc = new SpriteProgressBarDescription
            {
                Name = "Progress Bar",
                Top = this.Game.Form.RenderHeight - 20,
                Left = 100,
                Width = this.Game.Form.RenderWidth - 200,
                Height = 10,
                BaseColor = Color.Transparent,
                ProgressColor = Color.Green,
            };
            this.progressBar = await this.AddComponentSpriteProgressBar(pbDesc, SceneObjectUsages.UI, layerHUD);
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
            this.spriteMov = await this.AddComponentSprite(desc, SceneObjectUsages.None, layerObjects);
            this.spriteMov.Manipulator.SetPosition(256, 0);
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
            this.textBackPanel = await this.AddComponentSprite(desc, SceneObjectUsages.UI, layerHUD);
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
            this.textDrawer = await this.AddComponentTextDrawer(desc, SceneObjectUsages.UI, layerHUD);
        }

        private void UpdateInput()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.spriteMov.Manipulator.SetPosition(0, 0);
            }
        }
        private void UpdateSprite(GameTime gameTime)
        {
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
