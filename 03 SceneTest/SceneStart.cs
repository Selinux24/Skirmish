using Engine;
using SharpDX;
using System;

namespace SceneTest
{
    class SceneStart : Scene
    {
        private const int layerHUD = 99;
        private const int layerCursor = 100;

        SceneObject<Cursor> cursor = null;
        SceneObject<Model> backGround = null;
        SceneObject<TextDrawer> title = null;
        SceneObject<SpriteButton> startButton = null;
        SceneObject<SpriteButton> exitButton = null;

        public SceneStart(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;

            #region Cursor

            var cursorDesc = new CursorDescription()
            {
                Name = "Cursor",
                ContentPath = "Common",
                Textures = new[] { "pointer.png" },
                Height = 48,
                Width = 48,
                Centered = false,
                Delta = new Vector2(-14, -6),
                Color = Color.White,
            };
            this.cursor = this.AddComponent<Cursor>(cursorDesc, SceneObjectUsageEnum.UI, layerCursor);

            #endregion

            #region Background

            var backGroundDesc = ModelDescription.FromXml("Background", "SceneStart", "SkyPlane.xml");
            this.backGround = this.AddComponent<Model>(backGroundDesc, SceneObjectUsageEnum.UI);

            #endregion

            #region Title text

            var titleDesc = new TextDrawerDescription()
            {
                Name = "Title",
                Font = "Showcard Gothic",
                FontSize = 72,
                Style = FontMapStyleEnum.Bold,
                TextColor = Color.Gold,
                ShadowColor = new Color4(Color.LightYellow.RGB(), 0.55f),
                ShadowDelta = new Vector2(4, -4),
            };
            this.title = this.AddComponent<TextDrawer>(titleDesc, SceneObjectUsageEnum.UI, layerHUD);

            #endregion

            #region Start button

            var startButtonDesc = new SpriteButtonDescription()
            {
                Name = "Start button",

                Width = 200,
                Height = 40,

                TwoStateButton = true,

                TextureReleased = "common/buttons.png",
                TextureReleasedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorReleased = new Color4(Color.CornflowerBlue.RGB(), 0.8f),

                TexturePressed = "common/buttons.png",
                TexturePressedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorPressed = new Color4(Color.CornflowerBlue.RGB() * 1.2f, 0.9f),

                TextDescription = new TextDrawerDescription()
                {
                    Font = "Verdana",
                    Style = FontMapStyleEnum.Bold,
                    FontSize = 24,
                    TextColor = Color.Gold,
                }
            };
            this.startButton = this.AddComponent<SpriteButton>(startButtonDesc, SceneObjectUsageEnum.UI, layerHUD);

            #endregion

            #region Exit button

            var exitButtonDesc = new SpriteButtonDescription()
            {
                Name = "Exit button",

                Width = 200,
                Height = 40,

                TwoStateButton = true,

                TextureReleased = "common/buttons.png",
                TextureReleasedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorReleased = new Color4(Color.CornflowerBlue.RGB(), 0.8f),

                TexturePressed = "common/buttons.png",
                TexturePressedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorPressed = new Color4(Color.CornflowerBlue.RGB() * 1.2f, 0.9f),

                TextDescription = new TextDrawerDescription()
                {
                    Font = "Verdana",
                    Style = FontMapStyleEnum.Bold,
                    FontSize = 24,
                    TextColor = Color.Gold,
                }
            };
            this.exitButton = this.AddComponent<SpriteButton>(exitButtonDesc, SceneObjectUsageEnum.UI, layerHUD);

            #endregion
        }
        public override void Initialized()
        {
            base.Initialized();

            this.backGround.Transform.SetScale(1.5f, 1.25f, 1.5f);

            this.title.Instance.Text = "Scene Manager Test";
            this.title.Instance.CenterHorizontally();
            this.title.Instance.Top = this.Game.Form.RenderHeight / 4;

            this.startButton.Instance.Text = "Start";
            this.startButton.Instance.Left = ((this.Game.Form.RenderWidth / 6) * 2) - (this.startButton.Instance.Width / 2);
            this.startButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.startButton.Instance.Height / 2);
            this.startButton.Instance.Click += startButtonClick;

            this.exitButton.Instance.Text = "Exit";
            this.exitButton.Instance.Left = (this.Game.Form.RenderWidth / 6) * 4 - (this.exitButton.Instance.Width / 2);
            this.exitButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.exitButton.Instance.Height / 2);
            this.exitButton.Instance.Click += exitButtonClick;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float xmouse = (((float)this.Game.Input.MouseX / (float)this.Game.Form.RenderWidth) - 0.5f) * 2f;
            float ymouse = (((float)this.Game.Input.MouseY / (float)this.Game.Form.RenderHeight) - 0.5f) * 2f;

            float d = 0.25f;
            float vx = 0.5f;
            float vy = 0.25f;

            Vector3 position = Vector3.Zero;
            position.X = +((xmouse * d) + (0.2f * (float)Math.Cos(vx * this.Game.GameTime.TotalSeconds)));
            position.Y = -((ymouse * d) + (0.1f * (float)Math.Sin(vy * this.Game.GameTime.TotalSeconds)));

            this.Camera.Position = new Vector3(0, 0, -5f);
            this.Camera.LookTo(position);
        }

        private void startButtonClick(object sender, EventArgs e)
        {
            this.Game.SetScene<SceneMaterials>();
        }
        private void exitButtonClick(object sender, EventArgs e)
        {
            this.Game.Exit();
        }
    }
}
