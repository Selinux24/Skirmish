using Engine;
using Engine.Tween;
using Engine.UI;
using SharpDX;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpriteDrawing
{
    public class TestScene : Scene
    {
        private const int layerBackground = 1;
        private const int layerObjects = 50;
        private const int layerHUD = 99;
        private const int layerHUDDialogs = 200;
        private const float delta = 250f;

        private Sprite spriteMov = null;
        private TextDrawer textDrawer = null;
        private UIProgressBar progressBar = null;
        private UIPanel staticPan = null;
        private UIPanel pan = null;
        private TextDrawer textDebug = null;

        private readonly string allText = Properties.Resources.TinyLorem;
        private string currentText = "";
        private float textTime = 0;
        private float textInterval = 200f;

        private bool gameReady = false;

        private float progressValue = 0;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override Task Initialize()
        {
            return LoadUserInteface();
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
                            InitializeSmiley(),
                            InitializeStaticPan(),
                            InitializePan(),
                        },
                        async () =>
                        {
                            this.spriteMov.Visible = true;
                            this.staticPan.Visible = true;
                            this.textDrawer.Visible = true;

                            await Task.Delay(500);

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
                Textures = new[] { "background.jpg" },
            };
            await this.AddComponentSprite(desc, SceneObjectUsages.UI, layerBackground);

            var pbDesc = new UIProgressBarDescription
            {
                Name = "Progress Bar",
                Top = this.Game.Form.RenderHeight - 20,
                Left = 100,
                Width = this.Game.Form.RenderWidth - 200,
                Height = 15,
                BaseColor = new Color(0, 0, 0, 0.5f),
                ProgressColor = Color.Green,
                TextDescription = new TextDrawerDescription
                {
                    FontFileName = "LeagueSpartan-Bold.otf",
                    FontSize = 10,
                    Style = FontMapStyles.Regular,
                    TextColor = Color.White,
                },
            };
            this.progressBar = await this.AddComponentUIProgressBar(pbDesc, layerHUD);

            var txtDesc = new TextDrawerDescription()
            {
                Name = "Text Debug",
                Font = "Consolas",
                FontSize = 13,
                Style = FontMapStyles.Regular,
                TextColor = Color.Yellow,
            };
            this.textDebug = await this.AddComponentTextDrawer(txtDesc, SceneObjectUsages.UI, layerHUD);
        }
        public override void OnReportProgress(float value)
        {
            progressValue = Math.Max(progressValue, value);

            if (this.progressBar != null)
            {
                this.progressBar.ProgressValue = progressValue;
                this.progressBar.Text = $"{progressValue * 100f}%";
            }
        }
        private async Task InitializeSmiley()
        {
            await Task.Delay(500);

            var desc = new SpriteDescription()
            {
                Textures = new[] { "smiley.png" },
                Top = 0,
                Left = 10,
                Width = 256,
                Height = 256,
                FitParent = false,
                CenterVertically = true,
            };
            this.spriteMov = await this.AddComponentSprite(desc, SceneObjectUsages.None, layerObjects);
            this.spriteMov.Visible = false;
        }
        private async Task InitializeStaticPan()
        {
            await Task.Delay(1000);

            var desc = new UIPanelDescription()
            {
                Name = "WoodPanel",
                Background = new SpriteDescription
                {
                    Textures = new[] { "pan.jpg" },
                },
                Top = 100,
                Left = 700,
                Width = 800,
                Height = 650,
            };
            this.staticPan = await this.AddComponentUIPanel(desc, layerHUD);
            this.staticPan.Visible = false;

            var descText = new TextDrawerDescription()
            {
                Name = "Text",
                FontFileName = "LeagueSpartan-Bold.otf",
                FontSize = 18,
                Style = FontMapStyles.Regular,
                TextColor = Color.LightGoldenrodYellow,
            };
            this.textDrawer = await this.AddComponentTextDrawer(descText, SceneObjectUsages.UI, layerHUD + 1);
            this.textDrawer.Parent = this.staticPan;
            this.textDrawer.Visible = false;
        }
        private async Task InitializePan()
        {
            await Task.Delay(1500);

            var descPan = new UIPanelDescription
            {
                Name = "Test Panel",

                Width = 800,
                Height = 600,
                CenterVertically = true,
                CenterHorizontally = true,

                Background = new SpriteDescription()
                {
                    Textures = new[] { "pan.jpg" },
                    Color = Color.Red,
                }
            };
            this.pan = await this.AddComponentUIPanel(descPan, layerHUDDialogs);
            this.pan.Visible = false;

            var descButClose = new UIButtonDescription
            {
                Name = "CloseButton",

                Top = 10,
                Left = this.pan.Width - 10 - 20,
                Width = 20,
                Height = 20,

                TwoStateButton = true,
                ColorReleased = Color.Blue,
                ColorPressed = Color.Green,

                TextDescription = new TextDrawerDescription
                {
                    FontFileName = "LeagueSpartan-Bold.otf",
                    FontSize = 12,
                    Style = FontMapStyles.Regular,
                    TextColor = Color.White,
                },
                Text = "X",
            };
            var butClose = new UIButton(this, descButClose);
            butClose.JustReleased += ButClose_Click;
            this.pan.AddChild(butClose);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            FloatTweenManager.Update(gameTime.ElapsedSeconds);

            UpdateDebugInfo();

            if (!gameReady)
            {
                return;
            }

            UpdateInput();
            UpdateLorem(gameTime);
            UpdateSprite(gameTime);
        }
        private void UpdateDebugInfo()
        {
            if (this.textDebug != null)
            {
                var mousePos = Cursor.ScreenPosition;
                var but = pan?.Children.OfType<UIButton>().FirstOrDefault();

                this.textDebug.Text = $@"PanPressed: {pan?.IsPressed ?? false}; PanRect: {pan?.AbsoluteRectangle}; 
ButPressed: {but?.IsPressed ?? false}; ButRect: {but?.AbsoluteRectangle}; 
MousePos: {mousePos}; InputMousePos: {this.Game.Input.MousePosition}; 
FormCenter: {this.Game.Form.RenderCenter} ScreenCenter: {this.Game.Form.ScreenCenter}
Progress: {progressValue * 100f}%";
            }
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
        }
        private void UpdateSprite(GameTime gameTime)
        {
            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.spriteMov.MoveLeft(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.spriteMov.MoveRight(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.spriteMov.MoveUp(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.spriteMov.MoveDown(gameTime, delta);
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

                    float progress = currentText.Length / (float)allText.Length;
                    progressBar.ProgressValue = progress;
                    progressBar.Text = $"Loading Lorem ipsum random text - {(int)(progress * 100f)}%";
                    progressBar.Visible = true;
                }
                else
                {
                    currentText = allText;
                    textInterval = 0;

                    progressBar.ProgressValue = 1;
                    progressBar.Text = null;
                    progressBar.Visible = false;

                    pan.ShowRoll(1);
                }

                textDrawer.Text = currentText;
            }
        }

        private void ButClose_Click(object sender, System.EventArgs e)
        {
            pan.HideRoll(60);
        }
    }
}
