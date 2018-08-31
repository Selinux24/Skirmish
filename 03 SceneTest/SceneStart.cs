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
        SceneObject<SpriteButton> sceneMaterialsButton = null;
        SceneObject<SpriteButton> sceneWaterButton = null;
        SceneObject<SpriteButton> sceneStencilPassButton = null;
        SceneObject<SpriteButton> sceneLightsButton = null;
        SceneObject<SpriteButton> sceneTexturesButton = null;
        SceneObject<SpriteButton> exitButton = null;

        private Color sceneButtonColor = Color.AdjustSaturation(Color.CornflowerBlue, 1.5f);
        private Color exitButtonColor = Color.AdjustSaturation(Color.Orange, 1.5f);

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

            #region Scene buttons

            var startButtonDesc = new SpriteButtonDescription()
            {
                Name = "Scene buttons",

                Width = 200,
                Height = 40,

                TwoStateButton = true,

                TextureReleased = "common/buttons.png",
                TextureReleasedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f),

                TexturePressed = "common/buttons.png",
                TexturePressedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f),

                TextDescription = new TextDrawerDescription()
                {
                    Font = "Verdana",
                    Style = FontMapStyleEnum.Bold,
                    FontSize = 24,
                    TextColor = Color.Gold,
                }
            };
            this.sceneMaterialsButton = this.AddComponent<SpriteButton>(startButtonDesc, SceneObjectUsageEnum.UI, layerHUD);
            this.sceneWaterButton = this.AddComponent<SpriteButton>(startButtonDesc, SceneObjectUsageEnum.UI, layerHUD);
            this.sceneStencilPassButton = this.AddComponent<SpriteButton>(startButtonDesc, SceneObjectUsageEnum.UI, layerHUD);
            this.sceneLightsButton = this.AddComponent<SpriteButton>(startButtonDesc, SceneObjectUsageEnum.UI, layerHUD);
            this.sceneTexturesButton = this.AddComponent<SpriteButton>(startButtonDesc, SceneObjectUsageEnum.UI, layerHUD);

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
                ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f),

                TexturePressed = "common/buttons.png",
                TexturePressedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f),

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

            this.sceneMaterialsButton.Instance.Text = "Materials";
            this.sceneMaterialsButton.Instance.Left = ((this.Game.Form.RenderWidth / 7) * 1) - (this.sceneMaterialsButton.Instance.Width / 2);
            this.sceneMaterialsButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneMaterialsButton.Instance.Height / 2);
            this.sceneMaterialsButton.Instance.Click += SceneButtonClick;

            this.sceneWaterButton.Instance.Text = "Water";
            this.sceneWaterButton.Instance.Left = ((this.Game.Form.RenderWidth / 7) * 2) - (this.sceneWaterButton.Instance.Width / 2);
            this.sceneWaterButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneWaterButton.Instance.Height / 2);
            this.sceneWaterButton.Instance.Click += SceneButtonClick;

            this.sceneStencilPassButton.Instance.Text = "Stencil Pass";
            this.sceneStencilPassButton.Instance.Left = ((this.Game.Form.RenderWidth / 7) * 3) - (this.sceneStencilPassButton.Instance.Width / 2);
            this.sceneStencilPassButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneStencilPassButton.Instance.Height / 2);
            this.sceneStencilPassButton.Instance.Click += SceneButtonClick;

            this.sceneLightsButton.Instance.Text = "Lights";
            this.sceneLightsButton.Instance.Left = ((this.Game.Form.RenderWidth / 7) * 4) - (this.sceneLightsButton.Instance.Width / 2);
            this.sceneLightsButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneLightsButton.Instance.Height / 2);
            this.sceneLightsButton.Instance.Click += SceneButtonClick;

            this.sceneTexturesButton.Instance.Text = "Textures";
            this.sceneTexturesButton.Instance.Left = ((this.Game.Form.RenderWidth / 7) * 5) - (this.sceneTexturesButton.Instance.Width / 2);
            this.sceneTexturesButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneTexturesButton.Instance.Height / 2);
            this.sceneTexturesButton.Instance.Click += SceneButtonClick;

            this.exitButton.Instance.Text = "Exit";
            this.exitButton.Instance.Left = (this.Game.Form.RenderWidth / 7) * 6 - (this.exitButton.Instance.Width / 2);
            this.exitButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.exitButton.Instance.Height / 2);
            this.exitButton.Instance.Click += ExitButtonClick;
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

        private void SceneButtonClick(object sender, EventArgs e)
        {
            if (sender == this.sceneMaterialsButton.Instance)
            {
                this.Game.SetScene<SceneMaterials>();
            }
            else if (sender == this.sceneWaterButton.Instance)
            {
                this.Game.SetScene<SceneWater>();
            }
            else if (sender == this.sceneStencilPassButton.Instance)
            {
                this.Game.SetScene<SceneStencilPass>();
            }
            else if (sender == this.sceneLightsButton.Instance)
            {
                this.Game.SetScene<SceneLights>();
            }
            else if (sender == this.sceneTexturesButton.Instance)
            {
                this.Game.SetScene<SceneTextures>();
            }
        }
        private void ExitButtonClick(object sender, EventArgs e)
        {
            this.Game.Exit();
        }
    }
}
