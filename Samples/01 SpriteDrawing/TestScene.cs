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

        private UIButton butTest = null;

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
            var desc = new UITextAreaDescription()
            {
                Width = this.Game.Form.RenderWidth * 0.5f,
                Font = new TextDrawerDescription
                {
                    TextColor = Color.Yellow,
                },
            };
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
            var desc = new UIProgressBarDescription
            {
                Name = "Progress Bar",
                Top = this.Game.Form.RenderHeight - 20,
                Left = 100,
                Width = this.Game.Form.RenderWidth - 200,
                Height = 15,
                BaseColor = new Color(0, 0, 0, 0.5f),
                ProgressColor = Color.Green,
                Font = new TextDrawerDescription
                {
                    FontFileName = "LeagueSpartan-Bold.otf",
                    FontSize = 10,
                    LineAdjust = true,
                },
            };
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
                Top = 0,
                Left = 0,
                Width = size,
                Height = size,
                FitParent = false,
            };
            this.spriteSmiley = await this.AddComponentSprite(desc, SceneObjectUsages.None, layerObjects);
            this.spriteSmiley.Visible = false;
        }
        private async Task InitializeStaticPan()
        {
            await Task.Delay(1000);

            var desc = new UIPanelDescription()
            {
                Name = "WoodPanel",
                Background = new SpriteDescription
                {
                    Textures = new[] { "pan_bw.png" },
                    Color = new Color(176, 77, 45),
                },
                Top = 100,
                Left = 700,
                Width = 800,
                Height = 650,
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
                MarginLeft = 90,
                MarginRight = 90,
                MarginTop = 40,
                MarginBottom = 40,
            };
            this.textArea = new UITextArea(this, descText);

            this.staticPan.AddChild(this.textArea);
            this.staticPan.Visible = false;
        }
        private async Task InitializeDynamicPan()
        {
            await Task.Delay(1500);

            var descPan = new UIPanelDescription
            {
                Name = "Test Panel",

                Width = 800,
                Height = 600,
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

            var descButClose = new UIButtonDescription
            {
                Name = "CloseButton",

                Top = 10,
                Left = this.dynamicPan.Width - 10 - 40,
                Width = 40,
                Height = 40,

                TwoStateButton = true,

                TextureReleased = "buttons.png",
                TextureReleasedUVMap = new Vector4(0, 0, w, 1f),

                TexturePressed = "buttons.png",
                TexturePressedUVMap = new Vector4(w * 2f, 0, w * 3f, 1f),

                Font = new TextDrawerDescription()
                {
                    FontFileName = "LeagueSpartan-Bold.otf",
                    FontSize = 16,
                    LineAdjust = true,
                },
                Text = "X",
            };
            var butClose = new UIButton(this, descButClose);
            butClose.JustReleased += ButClose_Click;

            var descText = new UITextAreaDescription()
            {
                Font = new TextDrawerDescription()
                {
                    Name = "MaraText",
                    FontMapping = new FontMapping
                    {
                        ImageFile = "MaraFont.png",
                        MapFile = "MaraFont.txt",
                    },
                    UseTextureColor = true,
                },
                CenterHorizontally = CenterTargets.Parent,
                CenterVertically = CenterTargets.Parent,
                Text = @"Letters by Mara",
            };
            var textArea = new UITextArea(this, descText)
            {
                Scale = 0.25f,
            };

            this.dynamicPan.AddChild(butClose, false);
            this.dynamicPan.AddChild(textArea);
            this.dynamicPan.Visible = false;
        }
        private async Task InitializeButtonTest()
        {
            var descButClose = new UIButtonDescription
            {
                Name = "Test Button",

                Top = 250,
                Left = 150,
                Width = 200,
                Height = 55,

                TwoStateButton = true,
                ColorReleased = Color.Blue,
                ColorPressed = Color.Green,

                Font = new TextDrawerDescription()
                {
                    FontFileName = "LeagueSpartan-Bold.otf",
                    FontSize = 16,
                    LineAdjust = true,
                },
                Text = "Press Me",
            };
            butTest = await this.AddComponentUIButton(descButClose, layerHUD);
            butTest.JustReleased += ButTest_Click;
            butTest.MouseEnter += ButTest_MouseEnter;
            butTest.MouseLeave += ButTest_MouseLeave;
            butTest.Visible = false;
        }

        public override void OnReportProgress(float value)
        {
            progressValue = Math.Max(progressValue, value);

            if (progressBar != null)
            {
                progressBar.ProgressValue = progressValue;
                progressBar.Text = $"{(int)(progressValue * 100f)}%";
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

                    staticPan.Visible = true;
                    staticPan.Hide(1);
                    dynamicPan.Visible = true;
                    dynamicPan.ShowRoll(1);
                }

                textArea.Text = currentText;
            }
        }

        private void ButClose_Click(object sender, EventArgs e)
        {
            dynamicPan.HideRoll(1);

            spriteSmiley.Visible = true;
            spriteSmiley.CenterHorizontally = CenterTargets.Screen;
            spriteSmiley.CenterVertically = CenterTargets.Screen;
            spriteSmiley.Show(1);
            spriteSmiley.ScaleInScaleOut(0.85f, 1f, 0.25f);

            butTest.Visible = true;
            butTest.Show(0.25f);
        }

        private void ButTest_Click(object sender, EventArgs e)
        {
            spriteSmiley.ClearTween();
            spriteSmiley.Hide(0.5f);

            butTest.ClearTween();
            butTest.MouseLeave -= ButTest_MouseLeave;
            butTest.MouseEnter -= ButTest_MouseEnter;
            butTest.Hide(0.5f);
        }
        private void ButTest_MouseLeave(object sender, EventArgs e)
        {
            butTest.TweenScale(butTest.Scale, 1, 0.15f, ScaleFuncs.QuadraticEaseOut);
        }
        private void ButTest_MouseEnter(object sender, EventArgs e)
        {
            butTest.TweenScale(butTest.Scale, 2, 0.15f, ScaleFuncs.QuadraticEaseIn);
        }
    }
}
