using Engine;
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
        private const int layerUIBackground = LayerUI - 1;
        private const int layerUIObjects = LayerUI + 1;
        private const int layerUIDialogs = LayerUI + 2;
        private const float delta = 250f;

        private bool gameReady = false;

        private UIControlTweener uiTweener;

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


        private UITextArea scrollTextArea = null;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            await base.Initialize();

            LoadUserInterface();
        }

        private void LoadUserInterface()
        {
            LoadResourcesAsync(
                new[]
                {
                    InitializeTweener(),
                    InitializeConsole(),
                    InitializeBackground(),
                    InitializeProgressbar()
                },
                LoadUserInterfaceCompleted);
        }
        private async Task InitializeTweener()
        {
            await AddComponent(new Tweener(this, "Tweener", "Tweener"), SceneObjectUsages.None, 0);

            uiTweener = this.AddUIControlTweener();
        }
        private async Task InitializeConsole()
        {
            var desc = UITextAreaDescription.Default();
            desc.Width = Game.Form.RenderWidth * 0.5f;

            textDebug = await AddComponentUI<UITextArea, UITextAreaDescription>("textDebug", "textDebug", desc);
        }
        private async Task InitializeBackground()
        {
            var desc = SpriteDescription.Background("background.jpg");
            await AddComponentUI<Sprite, SpriteDescription>("Background", "Background", desc, layerUIBackground);
        }
        private async Task InitializeProgressbar()
        {
            var defaultFont = TextDrawerDescription.FromFile("LeagueSpartan-Bold.otf", 10, true);

            var desc = UIProgressBarDescription.Default(defaultFont, new Color(0, 0, 0, 0.5f), Color.Green);
            desc.Top = Game.Form.RenderHeight - 20;
            desc.Left = 100;
            desc.Width = Game.Form.RenderWidth - 200;
            desc.Height = 15;

            progressBar = await AddComponentUI<UIProgressBar, UIProgressBarDescription>("ProgressBar", "ProgressBar", desc);
        }
        private void LoadUserInterfaceCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            progressBar.Visible = true;
            progressBar.ProgressValue = 0;

            LoadControls();
        }

        private void LoadControls()
        {
            LoadResourcesAsync(
                new[]
                {
                    InitializeSmiley(),
                    InitializeStaticPan(),
                    InitializeDynamicPan(),
                    InitializeButtonTest(),
                    InitializeScroll(),
                },
                LoadControlsCompleted);
        }
        private async Task InitializeSmiley()
        {
            float size = Game.Form.RenderWidth * 0.3333f;

            var desc = SpriteDescription.Default("smiley.png", size, size);
            spriteSmiley = await AddComponentUI<Sprite, SpriteDescription>("SmileySprite", "SmileySprite", desc, layerUIObjects);
            spriteSmiley.Visible = false;
        }
        private async Task InitializeStaticPan()
        {
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
            staticPan = await AddComponentUI<UIPanel, UIPanelDescription>("StaticPanel", "StaticPanel", desc);

            var descText = new UITextAreaDescription()
            {
                Font = TextDrawerDescription.FromFile("LeagueSpartan-Bold.otf", 18, true),
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
            textArea = await CreateComponent<UITextArea, UITextAreaDescription>("StaticPanel.Text", "StaticPanel.Text", descText);

            staticPan.AddChild(textArea);
            staticPan.Visible = false;
        }
        private async Task InitializeDynamicPan()
        {
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
                },

                EventsEnabled = true,
            };
            dynamicPan = await AddComponentUI<UIPanel, UIPanelDescription>("DynamicPanel", "DynamicPanel", descPan, layerUIDialogs);

            float w0 = 0.0f;
            float w1 = 0.324634656f;
            float w2 = 0.655532359f;
            float w3 = 0.98434238f;
            var releasedRect = new Vector4(w0, 0, w1, 1f);
            var pressedRect = new Vector4(w2, 0, w3, 1f);

            var font = TextDrawerDescription.FromFile("LeagueSpartan-Bold.otf", 16, true);

            var descButClose = UIButtonDescription.DefaultTwoStateButton(font, "buttons.png", releasedRect, pressedRect);
            descButClose.Top = 10;
            descButClose.Left = dynamicPan.Width - 10 - 40;
            descButClose.Width = 40;
            descButClose.Height = 40;
            descButClose.TextHorizontalAlign = TextHorizontalAlign.Center;
            descButClose.TextVerticalAlign = TextVerticalAlign.Middle;
            descButClose.Text = "X";

            var butClose = await CreateComponent<UIButton, UIButtonDescription>("DynamicPanel.CloseButton", "DynamicPanel.CloseButton", descButClose);
            butClose.MouseDoubleClick += ButDoubleClose_Click;

            var descText = UITextAreaDescription.DefaultFromMap("MaraFont.png", "MaraFont.txt");
            descText.Text = @"Letters by Mara";
            descText.Padding = new Padding
            {
                Left = width * 0.1f,
                Right = width * 0.1f,
                Top = height * 0.1f,
                Bottom = height * 0.1f,
            };
            descText.TextHorizontalAlign = TextHorizontalAlign.Center;
            descText.TextVerticalAlign = TextVerticalAlign.Middle;

            var textMapped = await CreateComponent<UITextArea, UITextAreaDescription>("DynamicPanel.MaraText", "DynamicPanel.MaraText", descText);

            dynamicPan.AddChild(textMapped);
            dynamicPan.AddChild(butClose, false);
            dynamicPan.Visible = false;
        }
        private async Task InitializeButtonTest()
        {
            var font = TextDrawerDescription.FromFile("LeagueSpartan-Bold.otf", 16, true);

            var descButClose = UIButtonDescription.DefaultTwoStateButton(font, Color.Blue, Color.Green);
            descButClose.Top = 250;
            descButClose.Left = 150;
            descButClose.Width = 200;
            descButClose.Height = 55;
            descButClose.TextHorizontalAlign = TextHorizontalAlign.Center;
            descButClose.TextVerticalAlign = TextVerticalAlign.Middle;

            butTest2 = await AddComponentUI<UIButton, UIButtonDescription>("ButtonTest2", "ButtonTest2", descButClose);
            butTest2.MouseClick += ButTest2_Click;
            butTest2.MouseEnter += ButTest_MouseEnter;
            butTest2.MouseLeave += ButTest_MouseLeave;
            butTest2.Visible = false;

            butTest1 = await AddComponentUI<UIButton, UIButtonDescription>("ButtonTest1", "ButtonTest1", descButClose);
            butTest1.MouseClick += ButTest1_Click;
            butTest1.MouseEnter += ButTest_MouseEnter;
            butTest1.MouseLeave += ButTest_MouseLeave;
            butTest1.Visible = false;
        }
        private async Task InitializeScroll()
        {
            var panelDesc = UIPanelDescription.Default(Color.Gray);
            panelDesc.Top = 400;
            panelDesc.Left = 50;
            panelDesc.Width = 500;
            panelDesc.Height = 300;

            var panel = await AddComponentUI<UIPanel, UIPanelDescription>("scrollPanel", "Panel", panelDesc, LayerUI + 5);

            var areaFont = TextDrawerDescription.FromFamily("Tahoma", 20);
            var areaDesc = UITextAreaDescription.Default(areaFont);
            areaDesc.Scroll = ScrollModes.Vertical;
            areaDesc.ScrollbarSize = 20;
            areaDesc.ScrollbarMarkerSize = 100;
            areaDesc.ScrollbarBaseColor = new Color4(0, 0, 0, 0.7f);
            areaDesc.ScrollbarMarkerColor = Color.LightGray;
            areaDesc.Padding = new Padding(5, 1, 1, 25);

            scrollTextArea = await CreateComponent<UITextArea, UITextAreaDescription>("scrollText", "scrollText", areaDesc);
            scrollTextArea.Text = Properties.Resources.Lorem;

            panel.AddChild(scrollTextArea);
        }
        private async Task LoadControlsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            await Task.Delay(500);

            uiTweener.Show(staticPan, 1000);
            progressBar.Visible = false;

            gameReady = true;
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

            UpdateDebugInfo(gameTime);

            if (!gameReady)
            {
                return;
            }

            UpdateInput(gameTime);
            UpdateLorem(gameTime);
            UpdateSprite(gameTime);
        }
        private void UpdateDebugInfo(GameTime gameTime)
        {
            if (textDebug == null)
            {
                return;
            }

            var mousePos = Game.Input.MousePosition;
            var but = dynamicPan?.Children.OfType<UIButton>().FirstOrDefault();

            textDebug.Text = $@"GameTime paused {paused}|{gameTime.Paused}: Elapsed -> {gameTime.ElapsedTime}  Total -> {gameTime.TotalTime}
PanPressed: {dynamicPan?.PressedState ?? MouseButtons.None}; PanRect: {dynamicPan?.AbsoluteRectangle}; 
ButPressed: {but?.PressedState ?? MouseButtons.None}; ButRect: {but?.AbsoluteRectangle}; 
MousePos: {mousePos}; InputMousePos: {Game.Input.MousePosition}; 
FormCenter: {Game.Form.RenderCenter} ScreenCenter: {Game.Form.ScreenCenter}
TopMostControl: {TopMostControl} {TopMostControl?.Width},{TopMostControl?.Height} - {TopMostControl?.GetRenderArea(true)}
FocusedControl: {FocusedControl} {FocusedControl?.Width},{FocusedControl?.Height} - {FocusedControl?.GetRenderArea(true)}
Progress: {(int)(progressValue * 100f)}%";
        }

        bool paused = false;
        private void UpdateInput(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.Exit();
            }

            if (Game.Input.KeyJustReleased(Keys.Home))
            {
                spriteSmiley.Anchor = Anchors.Center;
            }

            if (Game.Input.KeyJustReleased(Keys.Space))
            {
                paused = !paused;

                if (!paused)
                {
                    gameTime.Resume();
                }
                else
                {
                    gameTime.Pause();
                }
            }

            if (Game.Input.MouseWheelDelta != 0 && scrollTextArea.IsMouseOver)
            {
                scrollTextArea.ScrollVerticalPosition -= Game.Input.MouseWheelDelta * gameTime.ElapsedSeconds * 0.01f;
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
                uiTweener.ClearTween(spriteSmiley);
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
                    uiTweener.Hide(staticPan, 1000);
                    dynamicPan.Visible = true;
                    uiTweener.ShowRoll(dynamicPan, 2000);
                }

                textArea.Text = currentText;
            }
        }

        private void ButDoubleClose_Click(IUIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            uiTweener.HideRoll(dynamicPan, 1000);

            spriteSmiley.Visible = true;
            spriteSmiley.Anchor = Anchors.Center;
            uiTweener.Show(spriteSmiley, 1000);
            uiTweener.ScaleInScaleOut(spriteSmiley, 0.85f, 1f, 250);

            butTest2.Caption.Text = $"Press Me with the{Environment.NewLine}{Color.Black}Right Button";
            butTest2.Visible = true;
            uiTweener.Show(butTest2, 250);
            uiTweener.TweenBaseColorBounce(butTest2, Color.Yellow, Color.Red, 2000, ScaleFuncs.Linear);

            butTest1.Caption.Text = $"Press Me with the{Environment.NewLine}{Color.Black}Middle Button";
            butTest1.Visible = true;
            uiTweener.Show(butTest1, 250);
            uiTweener.TweenBaseColorBounce(butTest1, Color.Yellow, Color.Red, 2000, ScaleFuncs.Linear);
        }

        private void ButTest1_Click(IUIControl sender, MouseEventArgs e)
        {
            if (sender is UIButton button && e.Buttons.HasFlag(MouseButtons.Middle))
            {
                uiTweener.ClearTween(button);
                button.MouseClick -= ButTest1_Click;
                button.MouseLeave -= ButTest_MouseLeave;
                button.MouseEnter -= ButTest_MouseEnter;
                uiTweener.Hide(button, 500);
            }
        }
        private void ButTest2_Click(IUIControl sender, MouseEventArgs e)
        {
            if (sender is UIButton button && e.Buttons.HasFlag(MouseButtons.Right))
            {
                uiTweener.ClearTween(spriteSmiley);
                uiTweener.Hide(spriteSmiley, 500);

                uiTweener.ClearTween(button);
                button.MouseClick -= ButTest2_Click;
                button.MouseLeave -= ButTest_MouseLeave;
                button.MouseEnter -= ButTest_MouseEnter;
                uiTweener.Hide(button, 500);

                Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    Game.Exit();
                });
            }
        }
        private void ButTest_MouseLeave(IUIControl sender, MouseEventArgs e)
        {
            if (sender is UIButton button)
            {
                uiTweener.ClearTween(button);
                uiTweener.TweenScale(button, button.Scale, 1, 150, ScaleFuncs.QuadraticEaseOut);
                uiTweener.TweenBaseColorBounce(button, Color.Yellow, Color.Red, 2000, ScaleFuncs.Linear);
            }
        }
        private void ButTest_MouseEnter(IUIControl sender, MouseEventArgs e)
        {
            if (sender is UIButton button)
            {
                uiTweener.ClearTween(button);
                uiTweener.TweenScale(button, button.Scale, 2, 150, ScaleFuncs.QuadraticEaseIn);
                uiTweener.TweenBaseColorBounce(button, button.BaseColor, Color.Yellow, 500, ScaleFuncs.Linear);
            }
        }
    }
}
