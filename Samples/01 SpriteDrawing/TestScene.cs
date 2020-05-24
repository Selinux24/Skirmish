using Engine;
using Engine.Tween;
using Engine.UI;
using SharpDX;
using System.Linq;
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
        private UIPanel pan = null;
        private TextDrawer textDebug = null;

        private PrimitiveListDrawer<Line3D> lineDrawer = null;

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
                            InitializeTextDrawer(),
                            InitializeDrawer(),
                        },
                        () =>
                        {
                            progressBar.Visible = false;

                            textDrawer.Text = null;

                            gameReady = true;
                        });
                });
        }
        private async Task InitializeDrawer()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 20000,
                DepthEnabled = true,
            };
            this.lineDrawer = await this.AddComponentPrimitiveListDrawer(desc, SceneObjectUsages.None, layerHUD);
            this.lineDrawer.Visible = true;
        }
        private async Task InitializeBackground()
        {
            var desc = new BackgroundDescription()
            {
                Textures = new[] { "background.jpg" },
            };
            var p = await this.AddComponentSprite(desc, SceneObjectUsages.UI, layerBackground);
            p.Active = p.Visible = false;

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
            this.progressBar.Active = this.progressBar.Visible = false;
        }
        private async Task InitializeSmiley()
        {
            var desc = new SpriteDescription()
            {
                Textures = new[] { "smiley.png" },
                Top = 0,
                Left = 0,
                Width = 256,
                Height = 256,
                FitParent = false,
            };
            this.spriteMov = await this.AddComponentSprite(desc, SceneObjectUsages.None, layerObjects);
            //this.spriteMov.Active = this.spriteMov.Visible = false;
        }
        private async Task InitializePan()
        {
            var desc = new SpriteDescription()
            {
                Name = "WoodPanel",
                Textures = new[] { "pan.jpg" },
                Top = 100,
                Left = 700,
                Width = 800,
                Height = 650,
            };
            var p = await this.AddComponentSprite(desc, SceneObjectUsages.UI, layerHUD);
            p.Active = p.Visible = false;

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
            this.pan = await this.AddComponentUIPanel(descPan, layerHUD);
            //this.pan.Top = 0;
            //this.pan.Left = 0;
            //this.pan.Width = 800;
            //this.pan.Height = 600;
            //this.pan.CenterVertically();
            //this.pan.CenterHorizontally();
            this.pan.Visible = true;

            var descButClose = new UIButtonDescription
            {
                Name = "CloseButton",
                Top = 25,
                Left = 25,
                Width = 50,
                Height = 50,

                TwoStateButton = true,

                //TextureReleased = "pan.jpg",
                ColorReleased = Color.Blue,

                //TexturePressed = "pan.jpg",
                ColorPressed = Color.Green,
            };
            var butClose = await this.AddComponentUIButton(descButClose, layerHUD);
            //butClose.Top = 25;
            //butClose.Left = 25;
            //butClose.Width = 50;
            //butClose.Height = 50;
            this.pan.AddChild(butClose);

            butClose.JustReleased += ButClose_Click;
        }

        private void ButClose_Click(object sender, System.EventArgs e)
        {
            pan.HideRoll(1);
        }

        private async Task InitializeTextDrawer()
        {
            var desc = new TextDrawerDescription()
            {
                Name = "Text",
                FontFileName = "SEASRN__.ttf",
                FontSize = 20,
                Style = FontMapStyles.Regular,
                TextColor = Color.LightGoldenrodYellow,
            };
            this.textDrawer = await this.AddComponentTextDrawer(desc, SceneObjectUsages.UI, layerHUD);
            this.textDrawer.TextArea = new Rectangle(780, 140, 650, 550);

            var desc2 = new TextDrawerDescription()
            {
                Name = "Text Debug",
                Font = "Consolas",
                FontSize = 13,
                Style = FontMapStyles.Regular,
                TextColor = Color.LightGoldenrodYellow,
            };
            this.textDebug = await this.AddComponentTextDrawer(desc2, SceneObjectUsages.UI, layerHUD);
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
                pan.ShowRoll(1);
            }

            if (this.textDebug != null)
            {
                var mousePos = Cursor.ScreenPosition;
                var but = pan.Children.OfType<UIButton>().FirstOrDefault();

                this.textDebug.Text = $@"PanPressed: {pan?.IsPressed ?? false}; PanRect: {pan.Rectangle}; 
ButPressed: {but?.IsPressed ?? false}; ButRect: {but.Rectangle}; 
MousePos: {mousePos}; InputMousePos: {new Vector2(this.Game.Input.MouseX, this.Game.Input.MouseY)}; 
FormCenter: {this.Game.Form.RenderCenter} ScreenCenter: {this.Game.Form.ScreenCenter}";
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
