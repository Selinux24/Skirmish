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

        private bool gameReady = false;

        private UITextArea textDebug = null;
        private UIProgressBar progressBar = null;
        private float progressValue = 0;

        private Sprite spriteSmiley = null;

        private UIPanel staticPan = null;
        private UITextArea textArea = null;
        private readonly string allText = Properties.Resources.TinyLorem;
        private string currentText = "";
        private float textTime = 0;
        private float textInterval = 200f;

        private UIPanel dynamicPan = null;

        private UIButton butTest1 = null;
        private UIButton butTest2 = null;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override Task Initialize()
        {
            return LoadUserInterface();
        }

        private async Task LoadUserInterface()
        {
            await this.LoadResourcesAsync(
                new Task[]
                {
                    InitializeConsole(),
                    InitializeBackground(),
                    InitializeProgressbar(),
                },
                async () =>
                {
                    progressBar.Visible = true;
                    progressBar.ProgressValue = 0;

                    await LoadControls();
                });
        }
        private async Task InitializeConsole()
        {
            var desc = UITextAreaDescription.Default();
            desc.Width = this.Game.Form.RenderWidth * 0.5f;

            this.textDebug = await this.AddComponentUITextArea(desc, layerHUD);
        }
        private async Task InitializeBackground()
        {
            var desc = new BackgroundDescription()
            {
                Textures = new[] { "background.jpg" },
            };
            await this.AddComponentSprite(desc, SceneObjectUsages.UI, layerBackground);
        }
        private async Task InitializeProgressbar()
        {
            var desc = UIProgressBarDescription.DefaultFromFile("LeagueSpartan-Bold.otf", 10, true);
            desc.Name = "Progress Bar";
            desc.Top = this.Game.Form.RenderHeight - 20;
            desc.Left = 100;
            desc.Width = this.Game.Form.RenderWidth - 200;
            desc.Height = 15;
            desc.BaseColor = new Color(0, 0, 0, 0.5f);
            desc.ProgressColor = Color.Green;

            this.progressBar = await this.AddComponentUIProgressBar(desc, layerHUD);
        }

        private async Task LoadControls()
        {
            await this.LoadResourcesAsync(
                new[]
                {
                    InitializeSmiley(),
                    InitializeStaticPan(),
                    InitializeDynamicPan(),
                    InitializeButtonTest(),
                },
                async () =>
                {
                    await Task.Delay(500);

                    staticPan.Visible = true;
                    progressBar.Visible = false;

                    gameReady = true;
                });
        }
        private async Task InitializeSmiley()
        {
            await Task.Delay(500);

            float size = this.Game.Form.RenderWidth * 0.3333f;

            var desc = new SpriteDescription()
            {
                Textures = new[] { "smiley.png" },
                Width = size,
                Height = size,
            };
            this.spriteSmiley = await this.AddComponentSprite(desc, SceneObjectUsages.None, layerObjects);
            this.spriteSmiley.Visible = false;
        }
        private async Task InitializeStaticPan()
        {
            await Task.Delay(1000);

            float width = this.Game.Form.RenderWidth / 2.25f;
            float height = width * 0.6666f;

            var desc = new UIPanelDescription()
            {
                Name = "WoodPanel",
                Background = new SpriteDescription
                {
                    Textures = new[] { "pan_bw.png" },
                    Color = new Color(176, 77, 45),
                },
                Top = this.Game.Form.RenderHeight / 8f,
                Left = this.Game.Form.RenderCenter.X,
                Width = width,
                Height = height,
            };
            this.staticPan = await this.AddComponentUIPanel(desc, layerHUD);

            var descText = new UITextAreaDescription()
            {
                Font = new TextDrawerDescription()
                {
                    Name = "Text",
                    FontFileName = "LeagueSpartan-Bold.otf",
                    FontSize = 18,
                    TextColor = Color.LightGoldenrodYellow,
                    ShadowColor = new Color4(0, 0, 0, 0.2f),
                    ShadowDelta = new Vector2(8, 5),
                    LineAdjust = true,
                },
                MarginLeft = width * 0.1f,
                MarginRight = width * 0.1f,
                MarginTop = height * 0.1f,
                MarginBottom = height * 0.1f,
            };
            this.textArea = new UITextArea(this, descText);

            this.staticPan.AddChild(this.textArea);
            this.staticPan.Visible = false;
        }
        private async Task InitializeDynamicPan()
        {
            await Task.Delay(1500);

            float width = this.Game.Form.RenderWidth / 1.5f;
            float height = width * 0.6666f;

            var descPan = new UIPanelDescription
            {
                Name = "Test Panel",

                Width = width,
                Height = height,
                CenterVertically = CenterTargets.Screen,
                CenterHorizontally = CenterTargets.Screen,

                Background = new SpriteDescription()
                {
                    Textures = new[] { "pan_bw.png" },
                    Color = Color.Pink,
                }
            };
            this.dynamicPan = await this.AddComponentUIPanel(descPan, layerHUDDialogs);

            float w = 0.3333f;

            var font = TextDrawerDescription.FromFile("LeagueSpartan-Bold.otf", 16);
            font.LineAdjust = true;
            font.HorizontalAlign = TextAlign.Center;
            font.VerticalAlign = VerticalAlign.Middle;

            var descButClose = UIButtonDescription.DefaultTwoStateButton("buttons.png", new Vector4(0, 0, w, 1f), new Vector4(w * 2f, 0, w * 3f, 1f), font);
            descButClose.Name = "CloseButton";
            descButClose.Top = 10;
            descButClose.Left = this.dynamicPan.Width - 10 - 40;
            descButClose.Width = 40;
            descButClose.Height = 40;
            descButClose.Text = "X";

            var butClose = new UIButton(this, descButClose);
            butClose.JustReleased += ButClose_Click;

            var descText = UITextAreaDescription.FromMap("MaraFont.png", "MaraFont.txt");
            descText.Name = "MaraText";
            descText.Text = @"Letters by Mara";
            descText.MarginLeft = width * 0.1f;
            descText.MarginRight = width * 0.1f;
            descText.MarginTop = height * 0.1f;
            descText.MarginBottom = height * 0.1f;

            var textMapped = new UITextArea(this, descText);

            this.dynamicPan.AddChild(textMapped);
            this.dynamicPan.AddChild(butClose, false);
            this.dynamicPan.Visible = false;
        }
        private async Task InitializeButtonTest()
        {
            var font = TextDrawerDescription.FromFile("LeagueSpartan-Bold.otf", 16);
            font.LineAdjust = true;
            font.HorizontalAlign = TextAlign.Center;
            font.VerticalAlign = VerticalAlign.Middle;

            var descButClose = UIButtonDescription.DefaultTwoStateButton(Color.Blue, Color.Green, font);
            descButClose.Name = "Test Button";
            descButClose.Top = 250;
            descButClose.Left = 150;
            descButClose.Width = 200;
            descButClose.Height = 55;
            descButClose.Text = "Press Me";

            butTest2 = await this.AddComponentUIButton(descButClose, layerHUD);
            butTest2.JustReleased += ButTest2_Click;
            butTest2.MouseEnter += ButTest_MouseEnter;
            butTest2.MouseLeave += ButTest_MouseLeave;
            butTest2.Visible = false;

            butTest1 = await this.AddComponentUIButton(descButClose, layerHUD);
            butTest1.JustReleased += ButTest1_Click;
            butTest1.MouseEnter += ButTest_MouseEnter;
            butTest1.MouseLeave += ButTest_MouseLeave;
            butTest1.Visible = false;
        }

        public override void OnReportProgress(float value)
        {
            progressValue = Math.Max(progressValue, value);

            if (progressBar != null)
            {
                progressBar.ProgressValue = progressValue;
                progressBar.Caption.Text = $"{(int)(progressValue * 100f)}%";
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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
                var but = dynamicPan?.Children.OfType<UIButton>().FirstOrDefault();

                textDebug.Text = $@"PanPressed: {dynamicPan?.IsPressed ?? false}; PanRect: {dynamicPan?.AbsoluteRectangle}; 
ButPressed: {but?.IsPressed ?? false}; ButRect: {but?.AbsoluteRectangle}; 
MousePos: {mousePos}; InputMousePos: {this.Game.Input.MousePosition}; 
FormCenter: {this.Game.Form.RenderCenter} ScreenCenter: {this.Game.Form.ScreenCenter}
Progress: {(int)(progressValue * 100f)}%";
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
                this.spriteSmiley.CenterHorizontally = CenterTargets.Screen;
                this.spriteSmiley.CenterVertically = CenterTargets.Screen;
            }
        }
        private void UpdateSprite(GameTime gameTime)
        {
            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.spriteSmiley.MoveLeft(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.spriteSmiley.MoveRight(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.spriteSmiley.MoveUp(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.spriteSmiley.MoveDown(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.X))
            {
                this.spriteSmiley.ClearTween();
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
                    progressBar.Caption.Text = $"Loading Lorem ipsum random text - {(int)(progress * 100f)}%";
                    progressBar.Visible = true;
                }
                else
                {
                    currentText = allText;
                    textInterval = 0;

                    progressBar.ProgressValue = 1;
                    progressBar.Caption.Text = null;
                    progressBar.Visible = false;

                    staticPan.Visible = true;
                    staticPan.Hide(1000);
                    dynamicPan.Visible = true;
                    dynamicPan.ShowRoll(2000);
                }

                textArea.Text = currentText;
            }
        }

        private void ButClose_Click(object sender, EventArgs e)
        {
            dynamicPan.HideRoll(1000);

            spriteSmiley.Visible = true;
            spriteSmiley.CenterHorizontally = CenterTargets.Screen;
            spriteSmiley.CenterVertically = CenterTargets.Screen;
            spriteSmiley.Show(1000);
            spriteSmiley.ScaleInScaleOut(0.85f, 1f, 250);

            butTest2.Visible = true;
            butTest2.Show(250);
            butTest2.TweenColorBounce(Color.Yellow, Color.Red, 2000, ScaleFuncs.Linear);

            butTest1.Caption.Text = "The other";
            butTest1.Visible = true;
            butTest1.Show(250);
            butTest1.TweenColorBounce(Color.Yellow, Color.Red, 2000, ScaleFuncs.Linear);
        }

        private void ButTest1_Click(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                button.ClearTween();
                button.JustReleased -= ButTest1_Click;
                button.MouseLeave -= ButTest_MouseLeave;
                button.MouseEnter -= ButTest_MouseEnter;
                button.Hide(500);
            }
        }
        private void ButTest2_Click(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                spriteSmiley.ClearTween();
                spriteSmiley.Hide(500);

                button.ClearTween();
                button.JustReleased -= ButTest2_Click;
                button.MouseLeave -= ButTest_MouseLeave;
                button.MouseEnter -= ButTest_MouseEnter;
                button.Hide(500);

                Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    this.Game.Exit();
                });
            }
        }
        private void ButTest_MouseLeave(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                button.ClearTween();
                button.TweenScale(button.Scale, 1, 150, ScaleFuncs.QuadraticEaseOut);
                button.TweenColorBounce(Color.Yellow, Color.Red, 2000, ScaleFuncs.Linear);
            }
        }
        private void ButTest_MouseEnter(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                button.ClearTween();
                button.TweenScale(button.Scale, 2, 150, ScaleFuncs.QuadraticEaseIn);
                button.TweenColor(button.Color, Color.Yellow, 500, ScaleFuncs.Linear);
            }
        }
    }
}
