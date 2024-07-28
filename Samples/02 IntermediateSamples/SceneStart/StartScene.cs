using Engine;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.UI;
using Engine.Content;
using Engine.Tween;
using Engine.UI;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace IntermediateSamples.SceneStart
{
    class StartScene : Scene
    {
        private const string EmtpyNameString = "Empty";

        private SoundEffectsManager soundEffectsManager;

        private Model backGround = null;
        private UITextArea title = null;
        private UIPanel mainPanel = null;

        private readonly string titleFonts = "Showcard Gothic, Verdana, Consolas";
        private readonly string buttonFonts = "Verdana, Consolas";

        public StartScene(Game game) : base(game)
        {
            Game.VisibleMouse = false;
            Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;
        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeUI();
        }

        private void InitializeUI()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeTweener,
                    InitializeCursor,
                    InitializeBackground,
                    InitializeTitle,
                    InitializeMainPanel,
                    InitializeMusic,
                ],
                PrepareAssets);

            LoadResources(group);
        }
        private async Task InitializeTweener()
        {
            await AddComponent(new Tweener(this, "Tweener", "Tweener"), SceneObjectUsages.None, 0);
        }
        private async Task InitializeCursor()
        {
            var cursorDesc = UICursorDescription.Default("scenestart/resources/pointer.png", 48, 48, false, new Vector2(-14f, -7f));
            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", cursorDesc);
        }
        private async Task InitializeBackground()
        {
            var backGroundDesc = new ModelDescription()
            {
                Content = ContentDescription.FromFile("scenestart/resources", "SkyPlane.json"),
            };
            backGround = await AddComponentUI<Model, ModelDescription>("Background", "Background", backGroundDesc);
        }
        private async Task InitializeTitle()
        {
            var titleFont = FontDescription.FromFamily(titleFonts, 72, true);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", UITextAreaDescription.Default(titleFont));
            title.GrowControlWithText = false;
            title.Text = "Intermediate Samples";
            title.TextForeColor = Color.Gold;
            title.TextShadowColor = new Color4(Color.LightYellow.RGB(), 0.25f);
            title.TextShadowDelta = new Vector2(4, 4);
            title.TextHorizontalAlign = TextHorizontalAlign.Center;
            title.TextVerticalAlign = TextVerticalAlign.Middle;
        }
        private async Task InitializeMainPanel()
        {
            mainPanel = await AddComponentUI<UIPanel, UIPanelDescription>("MainPanel", "MainPanel", UIPanelDescription.Default(Color.Transparent));
            mainPanel.Spacing = 10;
            mainPanel.Padding = 15;
            mainPanel.SetGridLayout(GridLayout.FixedRows(3));

            var buttonFont = FontDescription.FromFamily(buttonFonts, 24, true);

            var highlightColor = new Color4(0.3333f, 0.3333f, 0.3333f, 0f);
            var buttonDesc = UIButtonDescription.DefaultTwoStateButton(buttonFont, Color.Red, Color.Red.ToColor4() + highlightColor);
            buttonDesc.TextForeColor = Color.Gold;
            buttonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            buttonDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            var exitDesc = UIButtonDescription.DefaultTwoStateButton(buttonFont, Color.Orange, Color.Orange.ToColor4() + highlightColor);
            exitDesc.TextForeColor = Color.Gold;
            exitDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            exitDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            var emptyDesc = SpriteDescription.Default("scenestart/resources/empty.png");

            var panSimpleAnimation = await AddButtonPanel<SceneSimpleAnimation.SimpleAnimationScene>(buttonDesc, "Simple Animation");
            var panAnimationParts = await AddButtonPanel<SceneAnimationParts.AnimationPartsScene>(buttonDesc, "Animation Parts");
            var panSmoothTransitions = await AddButtonPanel<SceneSmoothTransitions.SmoothTransitionsScene>(buttonDesc, "Smooth Transitions");
            var panMixamo = await AddButtonPanel<SceneMixamo.MixamoScene>(buttonDesc, "Mixamo Models");
            var panDeferredLights = await AddButtonPanel<SceneDeferredLights.DeferredLightsScene>(buttonDesc, "Deferred Lighting", SceneModes.DeferredLightning);
            var panInstancing = await AddButtonPanel<SceneInstancing.InstancingScene>(buttonDesc, "Instancing");
            var panTransforms = await AddButtonPanel<SceneTransforms.TransformsScene>(buttonDesc, "Transforms");
            var panGardener = await AddButtonPanel<SceneGardener.GardenerScene>(buttonDesc, "Gardener");
            var panExit = await AddButtonExit(exitDesc, "Exit");

            mainPanel.AddChild(panSimpleAnimation);
            mainPanel.AddChild(await CreateComponent<Sprite, SpriteDescription>("Empty1", EmtpyNameString, emptyDesc));
            mainPanel.AddChild(panTransforms);
            mainPanel.AddChild(await CreateComponent<Sprite, SpriteDescription>("Empty2", EmtpyNameString, emptyDesc));
            mainPanel.AddChild(panGardener);
            mainPanel.AddChild(await CreateComponent<Sprite, SpriteDescription>("Empty3", EmtpyNameString, emptyDesc));

            mainPanel.AddChild(await CreateComponent<Sprite, SpriteDescription>("Empty4", EmtpyNameString, emptyDesc));
            mainPanel.AddChild(panMixamo);
            mainPanel.AddChild(await CreateComponent<Sprite, SpriteDescription>("Empty5", EmtpyNameString, emptyDesc));
            mainPanel.AddChild(panDeferredLights);
            mainPanel.AddChild(await CreateComponent<Sprite, SpriteDescription>("Empty6", EmtpyNameString, emptyDesc));
            mainPanel.AddChild(panSmoothTransitions);

            mainPanel.AddChild(panAnimationParts);
            mainPanel.AddChild(await CreateComponent<Sprite, SpriteDescription>("Empty7", EmtpyNameString, emptyDesc));
            mainPanel.AddChild(panInstancing);
            mainPanel.AddChild(await CreateComponent<Sprite, SpriteDescription>("Empty8", EmtpyNameString, emptyDesc));
            mainPanel.AddChild(await CreateComponent<Sprite, SpriteDescription>("Empty9", EmtpyNameString, emptyDesc));
            mainPanel.AddChild(panExit);
        }
        private async Task<UIPanel> AddButtonPanel<T>(UIButtonDescription desc, string text, SceneModes mode = SceneModes.ForwardLigthning) where T : Scene
        {
            var button = await CreateButton(desc, text, (sender, args) =>
            {
                if (!args.Buttons.HasFlag(MouseButtons.Left))
                {
                    return;
                }

                Game.SetScene<T>(mode);
            });

            var panel = await CreatePanel(text);
            panel.AddChild(button, true);

            return panel;
        }
        private async Task<UIPanel> AddButtonExit(UIButtonDescription desc, string text)
        {
            var button = await CreateButton(desc, text, (sender, args) =>
            {
                Game.Exit();
            });

            var panel = await CreatePanel(text);
            panel.AddChild(button, true);

            return panel;
        }
        private async Task<UIPanel> CreatePanel(string text)
        {
            return await CreateComponent<UIPanel, UIPanelDescription>($"MainPanel.Panel.{text}", $"MainPanel.Panel.{text}", UIPanelDescription.Default(new Color4(1, 1, 1, 0.25f)));
        }
        private async Task<UIButton> CreateButton(UIButtonDescription desc, string text, MouseEventHandler mouseClickHandler)
        {
            var button = await CreateComponent<UIButton, UIButtonDescription>($"MainPanel.Button.{text}", $"MainPanel.Button.{text}", desc);
            button.Caption.Text = text;
            button.MouseClick += mouseClickHandler;

            return button;
        }
        private async Task InitializeMusic()
        {
            soundEffectsManager = await AddComponent<SoundEffectsManager>("audioManager", "audioManager");
            soundEffectsManager.InitializeAudio("scenestart/resources");
        }
        private void PrepareAssets(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            soundEffectsManager.Start(1f);
            soundEffectsManager.Play();

            backGround.Manipulator.SetScaling(1.5f, 1.25f, 1.5f);

            UpdateLayout();
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            UpdateCamera();
        }
        private void UpdateCamera()
        {
            float xmouse = ((Game.Input.MouseX / (float)Game.Form.RenderWidth) - 0.5f) * 2f;
            float ymouse = ((Game.Input.MouseY / (float)Game.Form.RenderHeight) - 0.5f) * 2f;

            float d = 0.25f;
            float vx = 0.5f;
            float vy = 0.25f;

            Vector3 position = Vector3.Zero;
            position.X = +((xmouse * d) + (0.2f * MathF.Cos(vx * Game.GameTime.TotalSeconds)));
            position.Y = -((ymouse * d) + (0.1f * MathF.Sin(vy * Game.GameTime.TotalSeconds)));

            Camera.SetPosition(0, 0, -5f);
            Camera.LookTo(position);
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            mainPanel.Width = Game.Form.RenderWidth * 0.8f;
            mainPanel.Height = Game.Form.RenderHeight * 0.7f;
            mainPanel.Anchor = Anchors.HorizontalCenter;
            mainPanel.Top = Game.Form.RenderHeight * 0.25f;

            var rect = Game.Form.RenderRectangle;
            rect.Height = Game.Form.RenderHeight * 0.3f;
            title.SetRectangle(rect);
        }
    }
}
