using Engine;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace SceneTest
{
    class SceneStart : Scene
    {
        private const int layerHUD = 99;
        private const int layerCursor = 100;

        private Model backGround = null;
        private TextDrawer title = null;
        private SpriteButton sceneMaterialsButton = null;
        private SpriteButton sceneWaterButton = null;
        private SpriteButton sceneStencilPassButton = null;
        private SpriteButton sceneLightsButton = null;
        private SpriteButton sceneCascadedShadowsButton = null;
        private SpriteButton sceneTestButton = null;
        private SpriteButton exitButton = null;

        private readonly Color sceneButtonColor = Color.AdjustSaturation(Color.CornflowerBlue, 1.5f);
        private readonly Color exitButtonColor = Color.AdjustSaturation(Color.Orange, 1.5f);

        public SceneStart(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;

            await this.LoadResourcesAsync(Guid.NewGuid(), InitializeAssets());
        }

        private async Task InitializeAssets()
        {
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
            await this.AddComponentCursor(cursorDesc, SceneObjectUsages.UI, layerCursor);

            #endregion

            #region Background

            var backGroundDesc = ModelDescription.FromXml("Background", "SceneStart", "SkyPlane.xml");
            this.backGround = await this.AddComponentModel(backGroundDesc, SceneObjectUsages.UI);

            #endregion

            #region Title text

            var titleDesc = new TextDrawerDescription()
            {
                Name = "Title",
                Font = "Showcard Gothic",
                FontSize = 72,
                Style = FontMapStyles.Bold,
                TextColor = Color.Gold,
                ShadowColor = new Color4(Color.LightYellow.RGB(), 0.55f),
                ShadowDelta = new Vector2(4, -4),
            };
            this.title = await this.AddComponentTextDrawer(titleDesc, SceneObjectUsages.UI, layerHUD);

            #endregion

            #region Scene buttons

            var startButtonDesc = new SpriteButtonDescription()
            {
                Name = "Scene buttons",

                Width = 185,
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
                    Style = FontMapStyles.Bold,
                    FontSize = 20,
                    TextColor = Color.Gold,
                }
            };
            this.sceneMaterialsButton = await this.AddComponentSpriteButton(startButtonDesc, SceneObjectUsages.UI, layerHUD);
            this.sceneWaterButton = await this.AddComponentSpriteButton(startButtonDesc, SceneObjectUsages.UI, layerHUD);
            this.sceneStencilPassButton = await this.AddComponentSpriteButton(startButtonDesc, SceneObjectUsages.UI, layerHUD);
            this.sceneLightsButton = await this.AddComponentSpriteButton(startButtonDesc, SceneObjectUsages.UI, layerHUD);
            this.sceneCascadedShadowsButton = await this.AddComponentSpriteButton(startButtonDesc, SceneObjectUsages.UI, layerHUD);
            this.sceneTestButton = await this.AddComponentSpriteButton(startButtonDesc, SceneObjectUsages.UI, layerHUD);

            #endregion

            #region Exit button

            var exitButtonDesc = new SpriteButtonDescription()
            {
                Name = "Exit button",

                Width = 185,
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
                    Style = FontMapStyles.Bold,
                    FontSize = 20,
                    TextColor = Color.Gold,
                }
            };
            this.exitButton = await this.AddComponentSpriteButton(exitButtonDesc, SceneObjectUsages.UI, layerHUD);

            #endregion
        }
        public override void GameResourcesLoaded(Guid id)
        {
            this.backGround.Manipulator.SetScale(1.5f, 1.25f, 1.5f);

            this.title.Text = "Scene Manager Test";
            this.sceneMaterialsButton.Text = "Materials";
            this.sceneWaterButton.Text = "Water";
            this.sceneStencilPassButton.Text = "Stencil Pass";
            this.sceneLightsButton.Text = "Lights";
            this.sceneCascadedShadowsButton.Text = "Cascaded";
            this.sceneTestButton.Text = "Test";
            this.exitButton.Text = "Exit";

            var sceneButtons = new[]
            {
                this.sceneMaterialsButton,
                this.sceneWaterButton,
                this.sceneStencilPassButton,
                this.sceneLightsButton,
                this.sceneCascadedShadowsButton,
                this.sceneTestButton,
            };

            int numButtons = sceneButtons.Length + 1;
            int div = numButtons + 1;
            int h = 4;
            int hv = h - 1;

            this.title.CenterHorizontally();
            this.title.Top = this.Game.Form.RenderHeight / h;

            for (int i = 0; i < sceneButtons.Length; i++)
            {
                sceneButtons[i].Left = ((this.Game.Form.RenderWidth / div) * (i + 1)) - (this.sceneMaterialsButton.Width / 2);
                sceneButtons[i].Top = (this.Game.Form.RenderHeight / h) * hv - (this.sceneMaterialsButton.Height / 2);
                sceneButtons[i].Click += SceneButtonClick;
            }

            this.exitButton.Left = (this.Game.Form.RenderWidth / div) * numButtons - (this.exitButton.Width / 2);
            this.exitButton.Top = (this.Game.Form.RenderHeight / h) * hv - (this.exitButton.Height / 2);
            this.exitButton.Click += ExitButtonClick;
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
            if (sender == this.sceneMaterialsButton)
            {
                this.Game.SetScene<SceneMaterials>();
            }
            else if (sender == this.sceneWaterButton)
            {
                this.Game.SetScene<SceneWater>();
            }
            else if (sender == this.sceneStencilPassButton)
            {
                this.Game.SetScene<SceneStencilPass>();
            }
            else if (sender == this.sceneLightsButton)
            {
                this.Game.SetScene<SceneLights>();
            }
            else if (sender == this.sceneCascadedShadowsButton)
            {
                this.Game.SetScene<SceneCascadedShadows>();
            }
            else if (sender == this.sceneTestButton)
            {
                this.Game.SetScene<SceneTest>();
            }
        }
        private void ExitButtonClick(object sender, EventArgs e)
        {
            this.Game.Exit();
        }
    }
}
