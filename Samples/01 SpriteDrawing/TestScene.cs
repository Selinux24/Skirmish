﻿using Engine;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
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
            await LoadResourcesAsync(
                new[] { InitializeConsole(), InitializeBackground(), InitializeProgressbar() },
                async (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    progressBar.Visible = true;
                    progressBar.ProgressValue = 0;

                    await LoadControls();
                });
        }
        private async Task InitializeConsole()
        {
            var desc = UITextAreaDescription.Default();
            desc.Width = Game.Form.RenderWidth * 0.5f;

            textDebug = await this.AddComponentUITextArea("Console", desc, layerHUD);
        }
        private async Task InitializeBackground()
        {
            var desc = SpriteDescription.Background("background.jpg");
            await this.AddComponentSprite("Background", desc, SceneObjectUsages.UI, layerBackground);
        }
        private async Task InitializeProgressbar()
        {
            var desc = UIProgressBarDescription.DefaultFromFile("LeagueSpartan-Bold.otf", 10, true);
            desc.Top = Game.Form.RenderHeight - 20;
            desc.Left = 100;
            desc.Width = Game.Form.RenderWidth - 200;
            desc.Height = 15;
            desc.BaseColor = new Color(0, 0, 0, 0.5f);
            desc.ProgressColor = Color.Green;

            progressBar = await this.AddComponentUIProgressBar("ProgressBar", desc, layerHUD);
        }

        private async Task LoadControls()
        {
            await LoadResourcesAsync(
                new[]
                {
                    InitializeSmiley(),
                    InitializeStaticPan(),
                    InitializeDynamicPan(),
                    InitializeButtonTest(),
                },
                async (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    await Task.Delay(500);

                    staticPan.Show(1000);
                    progressBar.Visible = false;

                    gameReady = true;
                });
        }
        private async Task InitializeSmiley()
        {
            await Task.Delay(500);

            float size = Game.Form.RenderWidth * 0.3333f;

            var desc = SpriteDescription.Default("smiley.png", size, size);
            spriteSmiley = await this.AddComponentSprite("SmileySprite", desc, SceneObjectUsages.None, layerObjects);
            spriteSmiley.Visible = false;
        }
        private async Task InitializeStaticPan()
        {
            await Task.Delay(1000);

            float width = Game.Form.RenderWidth / 2.25f;
            float height = width * 0.6666f;

            var desc = new UIPanelDescription()
            {
                Top = Game.Form.RenderHeight / 8f,
                Left = Game.Form.RenderCenter.X,
                Width = width,
                Height = height,

                Background = new SpriteDescription
                {
                    Textures = new[] { "pan_bw.png" },
                    BaseColor = new Color(176, 77, 45),
                },
            };
            staticPan = await this.AddComponentUIPanel("StaticPanel", desc, layerHUD);

            var descText = new UITextAreaDescription()
            {
                Font = new TextDrawerDescription()
                {
                    FontFileName = "LeagueSpartan-Bold.otf",
                    FontSize = 18,
                    LineAdjust = true,
                },
                Padding = new Padding
                {
                    Left = width * 0.1f,
                    Right = width * 0.1f,
                    Top = height * 0.1f,
                    Bottom = height * 0.1f,
                },
                TextForeColor = Color.LightGoldenrodYellow,
                TextShadowColor = new Color4(0, 0, 0, 0.2f),
                TextShadowDelta = new Vector2(8, 5),
            };
            textArea = new UITextArea("StaticPanel.Text", this, descText);

            staticPan.AddChild(textArea);
            staticPan.Visible = false;
        }
        private async Task InitializeDynamicPan()
        {
            await Task.Delay(1500);

            float width = Game.Form.RenderWidth / 1.5f;
            float height = width * 0.6666f;

            var descPan = new UIPanelDescription
            {
                Width = width,
                Height = height,
                Anchor = Anchors.Center,

                Background = new SpriteDescription()
                {
                    Textures = new[] { "pan_bw.png" },
                    BaseColor = Color.Pink,
                }
            };
            dynamicPan = await this.AddComponentUIPanel("DynamicPanel", descPan, layerHUDDialogs);

            float w = 0.3333f;

            var font = TextDrawerDescription.FromFile("LeagueSpartan-Bold.otf", 16, true);

            var descButClose = UIButtonDescription.DefaultTwoStateButton("buttons.png", new Vector4(0, 0, w, 1f), new Vector4(w * 2f, 0, w * 3f, 1f));
            descButClose.Top = 10;
            descButClose.Left = dynamicPan.Width - 10 - 40;
            descButClose.Width = 40;
            descButClose.Height = 40;
            descButClose.Font = font;
            descButClose.TextHorizontalAlign = HorizontalTextAlign.Center;
            descButClose.TextVerticalAlign = VerticalTextAlign.Middle;
            descButClose.Text = "X";

            var butClose = new UIButton("DynamicPanel.CloseButton", this, descButClose);
            butClose.JustReleased += ButClose_Click;

            var descText = UITextAreaDescription.DefaultFromMap("MaraFont.png", "MaraFont.txt");
            descText.Text = @"Letters by Mara";
            descText.Padding = new Padding
            {
                Left = width * 0.1f,
                Right = width * 0.1f,
                Top = height * 0.1f,
                Bottom = height * 0.1f,
            };
            descText.TextHorizontalAlign = HorizontalTextAlign.Center;
            descText.TextVerticalAlign = VerticalTextAlign.Middle;

            var textMapped = new UITextArea("DynamicPanel.MaraText", this, descText);

            dynamicPan.AddChild(textMapped);
            dynamicPan.AddChild(butClose, false);
            dynamicPan.Visible = false;
        }
        private async Task InitializeButtonTest()
        {
            var font = TextDrawerDescription.FromFile("LeagueSpartan-Bold.otf", 16, true);

            var descButClose = UIButtonDescription.DefaultTwoStateButton(Color.Blue, Color.Green);
            descButClose.Top = 250;
            descButClose.Left = 150;
            descButClose.Width = 200;
            descButClose.Height = 55;
            descButClose.Font = font;
            descButClose.TextHorizontalAlign = HorizontalTextAlign.Center;
            descButClose.TextVerticalAlign = VerticalTextAlign.Middle;
            descButClose.Text = "Press Me";

            butTest2 = await this.AddComponentUIButton("ButtonTest2", descButClose, layerHUD);
            butTest2.JustReleased += ButTest2_Click;
            butTest2.MouseEnter += ButTest_MouseEnter;
            butTest2.MouseLeave += ButTest_MouseLeave;
            butTest2.Visible = false;

            butTest1 = await this.AddComponentUIButton("ButtonTest1", descButClose, layerHUD);
            butTest1.JustReleased += ButTest1_Click;
            butTest1.MouseEnter += ButTest_MouseEnter;
            butTest1.MouseLeave += ButTest_MouseLeave;
            butTest1.Visible = false;
        }

        public override void OnReportProgress(LoadResourceProgress value)
        {
            progressValue = Math.Max(progressValue, value.Progress);

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
            if (textDebug != null)
            {
                var mousePos = Cursor.ScreenPosition;
                var but = dynamicPan?.Children.OfType<UIButton>().FirstOrDefault();

                textDebug.Text = $@"PanPressed: {dynamicPan?.IsPressed ?? false}; PanRect: {dynamicPan?.AbsoluteRectangle}; 
ButPressed: {but?.IsPressed ?? false}; ButRect: {but?.AbsoluteRectangle}; 
MousePos: {mousePos}; InputMousePos: {Game.Input.MousePosition}; 
FormCenter: {Game.Form.RenderCenter} ScreenCenter: {Game.Form.ScreenCenter}
Progress: {(int)(progressValue * 100f)}%";
            }
        }
        private void UpdateInput()
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.Exit();
            }

            if (Game.Input.KeyJustReleased(Keys.Home))
            {
                spriteSmiley.Anchor = Anchors.Center;
            }
        }
        private void UpdateSprite(GameTime gameTime)
        {
            if (Game.Input.KeyPressed(Keys.A))
            {
                spriteSmiley.MoveLeft(gameTime, delta);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                spriteSmiley.MoveRight(gameTime, delta);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                spriteSmiley.MoveUp(gameTime, delta);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                spriteSmiley.MoveDown(gameTime, delta);
            }

            if (Game.Input.KeyPressed(Keys.X))
            {
                spriteSmiley.ClearTween();
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
            spriteSmiley.Anchor = Anchors.Center;
            spriteSmiley.Show(1000);
            spriteSmiley.ScaleInScaleOut(0.85f, 1f, 250);

            butTest2.Visible = true;
            butTest2.Show(250);
            butTest2.TweenBaseColorBounce(Color.Yellow, Color.Red, 2000, ScaleFuncs.Linear);

            butTest1.Caption.Text = "The other";
            butTest1.Visible = true;
            butTest1.Show(250);
            butTest1.TweenBaseColorBounce(Color.Yellow, Color.Red, 2000, ScaleFuncs.Linear);
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
                    Game.Exit();
                });
            }
        }
        private void ButTest_MouseLeave(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                button.ClearTween();
                button.TweenScale(button.Scale, 1, 150, ScaleFuncs.QuadraticEaseOut);
                button.TweenBaseColorBounce(Color.Yellow, Color.Red, 2000, ScaleFuncs.Linear);
            }
        }
        private void ButTest_MouseEnter(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                button.ClearTween();
                button.TweenScale(button.Scale, 2, 150, ScaleFuncs.QuadraticEaseIn);
                button.TweenBaseColor(button.BaseColor, Color.Yellow, 500, ScaleFuncs.Linear);
            }
        }
    }
}
