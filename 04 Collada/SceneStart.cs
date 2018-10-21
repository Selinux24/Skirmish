using Engine;
using SharpDX;
using System;

namespace Collada
{
    class SceneStart : Scene
    {
        private const int layerHUD = 99;
        private const int layerCursor = 100;

        SceneObject<Cursor> cursor = null;
        SceneObject<Model> backGround = null;
        SceneObject<TextDrawer> title = null;
        SceneObject<SpriteButton> sceneDungeonWallButton = null;
        SceneObject<SpriteButton> sceneNavMeshTestButton = null;
        SceneObject<SpriteButton> sceneDungeonButton = null;
        SceneObject<SpriteButton> sceneModularDungeonButton = null;
        SceneObject<SpriteButton> exitButton = null;

        private readonly Color sceneButtonColor = Color.AdjustSaturation(Color.RosyBrown, 1.5f);
        private readonly Color exitButtonColor = Color.AdjustSaturation(Color.OrangeRed, 1.5f);

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
                ContentPath = "Resources/Common",
                Textures = new[] { "pointer.png" },
                Height = 36,
                Width = 36,
                Centered = false,
                Color = Color.White,
            };
            this.cursor = this.AddComponent<Cursor>(cursorDesc, SceneObjectUsages.UI, layerCursor);

            #endregion

            #region Background

            var backGroundDesc = ModelDescription.FromXml("Background", "Resources/SceneStart", "SkyPlane.xml");
            this.backGround = this.AddComponent<Model>(backGroundDesc, SceneObjectUsages.UI);

            #endregion

            #region Title text

            var titleDesc = new TextDrawerDescription()
            {
                Name = "Title",
                Font = "Viner Hand ITC",
                FontSize = 90,
                Style = FontMapStyles.Bold,
                TextColor = Color.IndianRed,
                ShadowColor = new Color4(Color.Brown.RGB(), 0.55f),
                ShadowDelta = new Vector2(2, -3),
            };
            this.title = this.AddComponent<TextDrawer>(titleDesc, SceneObjectUsages.UI, layerHUD);

            #endregion

            #region Scene buttons

            var buttonDesc = new SpriteButtonDescription()
            {
                Name = "Scene buttons",

                Width = 200,
                Height = 36,

                TwoStateButton = true,

                TextureReleased = "common/buttons.png",
                TextureReleasedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f),

                TexturePressed = "common/buttons.png",
                TexturePressedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f),

                TextDescription = new TextDrawerDescription()
                {
                    Font = "Buxton Sketch",
                    Style = FontMapStyles.Regular,
                    FontSize = 22,
                    TextColor = Color.Gold,
                }
            };
            this.sceneDungeonWallButton = this.AddComponent<SpriteButton>(buttonDesc, SceneObjectUsages.UI, layerHUD);
            this.sceneNavMeshTestButton = this.AddComponent<SpriteButton>(buttonDesc, SceneObjectUsages.UI, layerHUD);
            this.sceneDungeonButton = this.AddComponent<SpriteButton>(buttonDesc, SceneObjectUsages.UI, layerHUD);
            this.sceneModularDungeonButton = this.AddComponent<SpriteButton>(buttonDesc, SceneObjectUsages.UI, layerHUD);

            #endregion

            #region Exit button

            var exitButtonDesc = new SpriteButtonDescription()
            {
                Name = "Exit button",

                Width = 200,
                Height = 36,

                TwoStateButton = true,

                TextureReleased = "common/buttons.png",
                TextureReleasedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f),

                TexturePressed = "common/buttons.png",
                TexturePressedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f),

                TextDescription = new TextDrawerDescription()
                {
                    Font = "Buxton Sketch",
                    Style = FontMapStyles.Bold,
                    FontSize = 22,
                    TextColor = Color.Gold,
                }
            };
            this.exitButton = this.AddComponent<SpriteButton>(exitButtonDesc, SceneObjectUsages.UI, layerHUD);

            #endregion
        }
        public override void Initialized()
        {
            base.Initialized();

            this.Camera.Position = Vector3.BackwardLH * 8f;
            this.Camera.Interest = Vector3.Zero;

            this.backGround.Transform.SetScale(1.5f, 1.25f, 1.5f);

            this.title.Instance.Text = "Collada Loader Test";
            this.title.Instance.CenterHorizontally();
            this.title.Instance.Top = this.Game.Form.RenderHeight / 4;

            this.sceneDungeonWallButton.Instance.Text = "Dungeon Wall";
            this.sceneDungeonWallButton.Instance.Left = ((this.Game.Form.RenderWidth / 6) * 1) - (this.sceneDungeonWallButton.Instance.Width / 2);
            this.sceneDungeonWallButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneDungeonWallButton.Instance.Height / 2);
            this.sceneDungeonWallButton.Instance.Click += SceneButtonClick;

            this.sceneNavMeshTestButton.Instance.Text = "Navmesh Test";
            this.sceneNavMeshTestButton.Instance.Left = ((this.Game.Form.RenderWidth / 6) * 2) - (this.sceneNavMeshTestButton.Instance.Width / 2);
            this.sceneNavMeshTestButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneNavMeshTestButton.Instance.Height / 2);
            this.sceneNavMeshTestButton.Instance.Click += SceneButtonClick;

            this.sceneDungeonButton.Instance.Text = "Dungeon";
            this.sceneDungeonButton.Instance.Left = ((this.Game.Form.RenderWidth / 6) * 3) - (this.sceneDungeonButton.Instance.Width / 2);
            this.sceneDungeonButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneDungeonButton.Instance.Height / 2);
            this.sceneDungeonButton.Instance.Click += SceneButtonClick;

            this.sceneModularDungeonButton.Instance.Text = "Modular Dungeon";
            this.sceneModularDungeonButton.Instance.Left = ((this.Game.Form.RenderWidth / 6) * 4) - (this.sceneModularDungeonButton.Instance.Width / 2);
            this.sceneModularDungeonButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.sceneModularDungeonButton.Instance.Height / 2);
            this.sceneModularDungeonButton.Instance.Click += SceneButtonClick;

            this.exitButton.Instance.Text = "Exit";
            this.exitButton.Instance.Left = (this.Game.Form.RenderWidth / 6) * 5 - (this.exitButton.Instance.Width / 2);
            this.exitButton.Instance.Top = (this.Game.Form.RenderHeight / 4) * 3 - (this.exitButton.Instance.Height / 2);
            this.exitButton.Instance.Click += ExitButtonClick;
        }

        private void SceneButtonClick(object sender, EventArgs e)
        {
            if (sender == this.sceneDungeonWallButton.Instance)
            {
                this.Game.SetScene<SceneDungeonWall>();
            }
            else if (sender == this.sceneNavMeshTestButton.Instance)
            {
                this.Game.SetScene<SceneNavmeshTest>();
            }
            else if (sender == this.sceneDungeonButton.Instance)
            {
                this.Game.SetScene<SceneDungeon>();
            }
            else if (sender == this.sceneModularDungeonButton.Instance)
            {
                this.Game.SetScene<SceneModularDungeon>();
            }
        }
        private void ExitButtonClick(object sender, EventArgs e)
        {
            this.Game.Exit();
        }
    }
}
