﻿using Engine;
using Engine.BuiltIn.Drawers.PostProcess;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace AISamples.SceneStart
{
    class StartScene : Scene
    {
        private const int layerHUD = 99;
        private const int layerCursor = 100;

        private UIControlTweener uiTweener;

        private Sprite background = null;
        private UITextArea title = null;

        private UIButton[] sceneButtons;
        private UIButton sceneSelfDrivingCarButton = null;
        private UIButton sceneVirtualWorldButton = null;
        private UIButton exitButton = null;

        private readonly string resourcesFolder = "SceneStart";
        private readonly string titleFonts = "Gill Sans MT, Verdana, Consolas";
        private readonly string buttonFonts = "Gill Sans MT, Consolas";
        private readonly Color sceneButtonColor = Color.WhiteSmoke;
        private readonly Color sceneButtonTextColor = Color.Black;
        private readonly Color exitButtonColor = Color.White;
        private readonly Color exitButtonTextColor = Color.Black;

        private bool sceneReady = false;

        public StartScene(Game game) : base(game)
        {
            Game.VisibleMouse = false;
            Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;
        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeTweener,
                    InitializeCursor,
                    InitializeBackground,
                    InitializeAssets,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeTweener()
        {
            await AddComponent(new Tweener(this, "Tweener", "Tweener"), SceneObjectUsages.None, 0);

            uiTweener = this.AddUIControlTweener();
        }
        private async Task InitializeCursor()
        {
            var cursorDesc = UICursorDescription.Default("pointer.png", 48, 48, false, new Vector2(-14f, -7f));
            cursorDesc.ContentPath = resourcesFolder;
            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", cursorDesc, layerCursor);
        }
        private async Task InitializeBackground()
        {
            //Credits to the background image author: https://www.goodfon.com/user/sisko1701/
            //Taken from: https://www.goodfon.com/minimalism/wallpaper-ahoy-2001-a-space-odyssey-hal-9000-computer-science-fiction.html
            var backGroundDesc = new SpriteDescription()
            {
                ContentPath = resourcesFolder,
                Textures = ["background.jpg"],
                Width = Game.Form.RenderWidth,
                Height = Game.Form.RenderHeight,
                BaseColor = Color.White,
                TintColor = Color.White,
            };
            background = await AddComponentUI<Sprite, SpriteDescription>("Background", "Background", backGroundDesc, LayerUI - 1);
        }
        private async Task InitializeAssets()
        {
            #region Title text

            var titleFont = TextDrawerDescription.FromFamily(titleFonts, 72, FontMapStyles.Regular, true);
            titleFont.ContentPath = resourcesFolder;

            var titleDesc = UITextAreaDescription.Default(titleFont);
            titleDesc.ContentPath = resourcesFolder;
            titleDesc.TextForeColor = Color.White;
            titleDesc.TextShadowColor = new Color4(Color.White.RGB(), 0.25f);
            titleDesc.TextShadowDelta = new Vector2(4, 4);
            titleDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            titleDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", titleDesc, layerHUD);
            title.GrowControlWithText = false;
            title.Text = "AI SAMPLES";

            #endregion

            #region Scene buttons

            var buttonsFont = TextDrawerDescription.FromFamily(buttonFonts, 20, FontMapStyles.Regular, true);
            buttonsFont.ContentPath = resourcesFolder;

            var startButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
            startButtonDesc.ContentPath = resourcesFolder;
            startButtonDesc.Width = 275;
            startButtonDesc.Height = 35;
            startButtonDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            startButtonDesc.ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);
            startButtonDesc.TextForeColor = sceneButtonTextColor;
            startButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            startButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            sceneSelfDrivingCarButton = await InitializeButton(nameof(sceneSelfDrivingCarButton), "SELF-DRIVING CAR", startButtonDesc);
            sceneVirtualWorldButton = await InitializeButton(nameof(sceneVirtualWorldButton), "A VIRTUAL WORLD", startButtonDesc);

            #endregion

            #region Exit button

            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
            exitButtonDesc.ContentPath = resourcesFolder;
            exitButtonDesc.Width = 275;
            exitButtonDesc.Height = 35;
            exitButtonDesc.ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f);
            exitButtonDesc.ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f);
            exitButtonDesc.TextForeColor = exitButtonTextColor;
            exitButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            exitButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            exitButton = await AddComponentUI<UIButton, UIButtonDescription>("ButtonExit", "ButtonExit", exitButtonDesc, layerHUD);
            exitButton.MouseClick += ExitButtonClick;
            exitButton.MouseEnter += SceneButtonMouseEnter;
            exitButton.MouseLeave += SceneButtonMouseLeave;
            exitButton.Caption.Text = "EXIT";

            #endregion

            sceneButtons =
            [
                sceneSelfDrivingCarButton,
                sceneVirtualWorldButton,
                null,
                exitButton,
            ];
        }
        private async Task<UIButton> InitializeButton(string name, string caption, UIButtonDescription desc)
        {
            var button = await AddComponentUI<UIButton, UIButtonDescription>(name, name, desc, layerHUD);
            button.MouseClick += SceneButtonClick;
            button.MouseEnter += SceneButtonMouseEnter;
            button.MouseLeave += SceneButtonMouseLeave;
            button.Caption.Text = caption;

            return button;
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            Renderer.PostProcessingObjectsEffects.AddToneMapping(BuiltInToneMappingTones.Uncharted2);

            UpdateLayout();

            uiTweener.ScaleColor(background, Color.White, Color.LightPink, 1000);

            sceneReady = true;
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();
            UpdateLayout();
        }
        private void UpdateLayout()
        {
            var rect = Game.Form.RenderRectangle;
            rect.Height /= 6;
            rect.Top = 0;
            title.SetRectangle(rect);
            title.Anchor = Anchors.HorizontalCenter;

            UIControlExtensions.LocateButtons(Game.Form, sceneButtons, exitButton.Width, exitButton.Height, 4);
        }

        private void SceneButtonClick(IUIControl sender, MouseEventArgs e)
        {
            if (!sceneReady)
            {
                return;
            }

            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            if (sender == sceneSelfDrivingCarButton) Game.SetScene<SceneCWRSelfDrivingCar.SelfDrivingCarScene>();
            if (sender == sceneVirtualWorldButton) Game.SetScene<SceneCWRVirtualWorld.VirtualWorldScene>();
        }
        private void SceneButtonMouseEnter(IUIControl sender, MouseEventArgs e)
        {
            uiTweener.ScaleInScaleOut(sender, 1.0f, 1.10f, 250);
        }
        private void SceneButtonMouseLeave(IUIControl sender, MouseEventArgs e)
        {
            uiTweener.ClearTween(sender);
            uiTweener.TweenScale(sender, sender.Scale, 1.0f, 500, ScaleFuncs.Linear);
        }

        private void ExitButtonClick(IUIControl sender, MouseEventArgs e)
        {
            if (!sceneReady)
            {
                return;
            }

            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            Game.Exit();
        }
    }
}