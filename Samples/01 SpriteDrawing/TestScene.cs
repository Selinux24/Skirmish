using Engine;
using Engine.Tween;
using Engine.UI;
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
        private TextDrawer textDrawer = null;
        private UIProgressBar progressBar = null;
        private Sprite pan = null;

        private readonly string allText = Properties.Resources.Lorem;
        private string currentText = "";
        private float textTime = 0;
        private float textInterval = 200f;

        private bool gameReady = false;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override Task Initialize()
        {
            return LoadUserInteface();
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

            FloatTweenManager.Update(gameTime.ElapsedSeconds);

            UpdateInput();

            if (!gameReady)
            {
                return;
            }

            UpdateLorem(gameTime);
            UpdateSprite(gameTime);
        }

        private async Task LoadUserInteface()
        {
            await this.LoadResourcesAsync(
                InitializeBackground(),
                () =>
                {
                    progressBar.Visible = true;
                    progressBar.ProgressValue = 0;

                    _ = this.LoadResourcesAsync(
                        new[]
                        {
                            InitializePan(),
                            InitializeSmiley(),
                            InitializeTextDrawer()
                        },
                        () =>
                        {
                            progressBar.Visible = false;

                            textDrawer.Text = null;

                            gameReady = true;
                        });
                });
        }
        private async Task InitializeBackground()
        {
            var desc = new BackgroundDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "background.jpg" },
            };
            await this.AddComponentSprite(desc, SceneObjectUsages.UI, layerBackground);

            var pbDesc = new UIProgressBarDescription
            {
                Name = "Progress Bar",
                Top = this.Game.Form.RenderHeight - 20,
                Left = 100,
                Width = this.Game.Form.RenderWidth - 200,
                Height = 10,
                BaseColor = Color.Transparent,
                ProgressColor = Color.Green,
            };
            this.progressBar = await this.AddComponentUIProgressBar(pbDesc, layerHUD);
        }
        private async Task InitializeSmiley()
        {
            var desc = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "smiley.png" },
                Top = 0,
                Left = 0,
                Width = 256,
                Height = 256,
                FitScreen = false,
            };
            this.spriteMov = await this.AddComponentSprite(desc, SceneObjectUsages.None, layerObjects);
        }
        private async Task InitializePan()
        {
            var desc = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "pan.jpg" },
                Top = 100,
                Left = 700,
                Width = 800,
                Height = 650,
                FitScreen = false,
            };
            _ = await this.AddComponentSprite(desc, SceneObjectUsages.UI, layerHUD);

            var descPan = new SpriteDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "pan.jpg" },
                Width = 800,
                Height = 600,
                CenterHorizontally = true,
                CenterVertically = true,
                FitScreen = false,
                Color = Color.Red,
            };
            this.pan = await this.AddComponentSprite(descPan, SceneObjectUsages.UI, layerHUD);
            this.pan.Visible = false;
        }
        private async Task InitializeTextDrawer()
        {
            var desc = new TextDrawerDescription()
            {
                Name = "Text",
                Font = "Viner Hand ITC",
                FontSize = 17,
                Style = FontMapStyles.Bold,
                TextColor = Color.LightGoldenrodYellow,
            };
            this.textDrawer = await this.AddComponentTextDrawer(desc, SceneObjectUsages.UI, layerHUD);
            this.textDrawer.TextArea = new Rectangle(780, 140, 650, 550);

            var desc2 = new TextDrawerDescription()
            {
                Name = "Text",
                Font = "Consolas",
                FontSize = 14,
                Style = FontMapStyles.Regular,
                TextColor = Color.White,
            };
            _ = await this.AddComponentTextDrawer(desc2, SceneObjectUsages.UI, layerHUD);
        }

        private void UpdateInput()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.spriteMov.Left = 0;
                this.spriteMov.Top = 0;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                if (!pan.Visible || pan.Scale == 0)
                {
                    pan.ShowRoll(2);
                }
                else if (pan.Scale == 1)
                {
                    pan.HideRoll(1);
                }
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
