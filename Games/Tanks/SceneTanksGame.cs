using Engine;
using Engine.Audio;
using Engine.Common;
using Engine.Content;
using Engine.PostProcessing;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tanks
{
    /// <summary>
    /// Tanks game scene
    /// </summary>
    class SceneTanksGame : Scene
    {
        const int layerUIModal = LayerUIEffects + 3;
        const string fontFilename = "Resources/LeagueSpartan-Bold.otf";

        private bool gameReady = false;

        private UITextArea loadingText;
        private UIProgressBar loadingBar;
        private float progressValue = 0;
        private UIPanel fadePanel;

        private UITextArea gameMessage;
        private UITextArea gameKeyHelp;

        private UIPanel dialog;
        private UIButton dialogCancel;
        private UIButton dialogAccept;
        private UITextArea dialogText;
        private bool dialogActive = false;
        private MouseEventHandler lastOnCloseHandler;
        private MouseEventHandler lastOnAcceptHandler;

        private UITextArea player1Name;
        private UITextArea player1Points;
        private UIProgressBar player1Life;
        private PlayerStatus player1Status;

        private UITextArea player2Name;
        private UITextArea player2Points;
        private UIProgressBar player2Life;
        private PlayerStatus player2Status;

        private UITextArea turnText;
        private int currentTurn = 1;
        private Sprite gameIcon;
        private int currentPlayer = 0;
        private Sprite playerTurnMarker;

        private UIPanel keyHelp;
        private Sprite keyRotate;
        private Sprite keyMove;
        private Sprite KeyPitch;
        private UITextArea keyRotateLeftText;
        private UITextArea keyRotateRightText;
        private UITextArea keyMoveForwardText;
        private UITextArea keyMoveBackwardText;
        private UITextArea keyPitchUpText;
        private UITextArea keyPitchDownText;

        private UIProgressBar pbFire;
        private UITextArea fireKeyText;

        private Sprite miniMapBackground;
        private Sprite miniMapTank1;
        private Sprite miniMapTank2;
        private readonly float maxWindVelocity = 10;
        private float currentWindVelocity = 1;
        private Vector2 windDirection = Vector2.Normalize(Vector2.One);
        private UIProgressBar windVelocity;
        private Sprite windDirectionArrow;

        private Model landScape;
        private Scenery terrain;
        private float terrainTop;
        private readonly float terrainHeight = 100;
        private readonly float terrainSize = 1024;
        private readonly int mapSize = 256;
        private ModelInstanced tanks;
        private float tankHeight = 0;
        private Model projectile;

        private Sprite[] trajectoryMarkerPool;

        private readonly Dictionary<string, ParticleSystemDescription> particleDescriptions = new Dictionary<string, ParticleSystemDescription>();
        private ParticleManager particleManager = null;

        private bool shooting = false;
        private bool gameEnding = false;
        private bool freeCamera = false;

        private ModelInstance Shooter { get { return tanks[currentPlayer]; } }
        private ModelInstance Target { get { return tanks[(currentPlayer + 1) % 2]; } }
        private PlayerStatus ShooterStatus { get { return currentPlayer == 0 ? player1Status : player2Status; } }
        private PlayerStatus TargetStatus { get { return currentPlayer == 0 ? player2Status : player1Status; } }
        private ParabolicShot shot;

        private string tankMoveEffect;
        private IAudioEffect tankMoveEffectInstance;
        private string tankDestroyedEffect;
        private string tankShootingEffect;
        private string[] impactEffects;
        private string[] damageEffects;

        private DecalDrawer decalDrawer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public SceneTanksGame(Game game) : base(game)
        {
            InitializePlayers();
        }

        public override void OnReportProgress(LoadResourceProgress value)
        {
            progressValue = Math.Max(progressValue, value.Progress);

            if (loadingBar != null)
            {
                loadingBar.ProgressValue = progressValue;
                loadingBar.Caption.Text = $"{(int)(progressValue * 100f)}%";
            }
        }

        public override Task Initialize()
        {
            GameEnvironment.Background = Color.Black;

            Game.VisibleMouse = false;
            Game.LockMouse = false;

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 2000;

            GameEnvironment.ShadowDistanceLow *= 2f;

            return LoadLoadingUI();
        }

        private async Task LoadLoadingUI()
        {
            await LoadResourcesAsync(
                InitializeLoadingUI(),
                async (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    UpdateLayoutLoadingUI();

                    fadePanel.BaseColor = Color.Black;
                    fadePanel.Visible = true;

                    await Task.Delay(1000);

                    loadingText.Text = "Please wait...";
                    loadingText.Visible = true;
                    loadingText.TweenAlphaBounce(1, 0, 1000, ScaleFuncs.CubicEaseInOut);

                    await Task.Delay(2000);

                    loadingBar.ProgressValue = 0;
                    loadingBar.Visible = true;

                    _ = Task.Run(LoadUI);
                });
        }
        private async Task InitializeLoadingUI()
        {
            fadePanel = await this.AddComponentUIPanel("FadePanel", "FadePanel", UIPanelDescription.Screen(this, Color4.Black * 0.3333f), LayerUIEffects);
            fadePanel.Visible = false;

            loadingText = await this.AddComponentUITextArea("LoadingText", "LoadingText", UITextAreaDescription.DefaultFromFile(fontFilename, 40, true), LayerUIEffects + 1);
            loadingText.TextForeColor = Color.Yellow;
            loadingText.TextShadowColor = Color.Orange;
            loadingText.TextHorizontalAlign = TextHorizontalAlign.Center;
            loadingText.TextVerticalAlign = TextVerticalAlign.Middle;
            loadingText.GrowControlWithText = false;
            loadingText.Visible = false;

            loadingBar = await this.AddComponentUIProgressBar("LoadingBar", "LoadingBar", UIProgressBarDescription.DefaultFromFile(fontFilename, 20, true), LayerUIEffects + 1);
            loadingBar.ProgressColor = Color.CornflowerBlue;
            loadingBar.BaseColor = Color.Yellow;
            loadingBar.Caption.TextForeColor = Color.Black;
            loadingBar.Caption.Text = "0%";
            loadingBar.Visible = false;
        }

        private async Task LoadUI()
        {
            List<Task> taskList = new List<Task>();
            taskList.AddRange(InitializeUI());
            taskList.AddRange(InitializeModels());

            await LoadResourcesAsync(
                taskList,
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    UpdateLayout();

                    PrepareModels();
                    UpdateCamera(true);

                    AudioManager.MasterVolume = 1f;
                    AudioManager.Start();

                    Task.Run(async () =>
                    {
                        loadingText.ClearTween();
                        loadingText.Hide(1000);
                        loadingBar.ClearTween();
                        loadingBar.Hide(500);

                        await Task.Delay(1500);

                        await ShowMessage("Ready!", 2000);

                        SetOnGameEffects();

                        fadePanel.ClearTween();
                        fadePanel.Hide(2000);

                        gameReady = true;

                        UpdateGameControls(true);

                        PaintShot(true);
                    });
                });
        }

        private Task[] InitializeUI()
        {
            return new[]
            {
                InitializeUIGameMessages(),
                InitializeUIModalDialog(),
                InitializeUIPlayers(),
                InitializeUITurn(),
                InitializeUIKeyPanel(),
                InitializeUIFire(),
                InitializeUIMinimap(),
                InitializeUIShotPath(),
            };
        }
        private async Task InitializeUIGameMessages()
        {
            gameMessage = await this.AddComponentUITextArea("GameMessage", "GameMessage", UITextAreaDescription.DefaultFromFile(fontFilename, 120, FontMapStyles.Regular, false), LayerUIEffects + 1);
            gameMessage.TextForeColor = Color.Yellow;
            gameMessage.TextShadowColor = Color.Yellow * 0.5f;
            gameMessage.TextHorizontalAlign = TextHorizontalAlign.Center;
            gameMessage.TextVerticalAlign = TextVerticalAlign.Middle;
            gameMessage.GrowControlWithText = false;
            gameMessage.Visible = false;

            gameKeyHelp = await this.AddComponentUITextArea("GameKeyHelp", "GameKeyHelp", UITextAreaDescription.DefaultFromFile(fontFilename, 25, FontMapStyles.Regular, true), LayerUIEffects + 1);
            gameKeyHelp.TextForeColor = Color.Yellow;
            gameKeyHelp.Text = "Press space to exit";
            gameKeyHelp.TextHorizontalAlign = TextHorizontalAlign.Center;
            gameKeyHelp.TextVerticalAlign = TextVerticalAlign.Middle;
            gameKeyHelp.GrowControlWithText = false;
            gameKeyHelp.Visible = false;
        }
        private async Task InitializeUIModalDialog()
        {
            var descPan = UIPanelDescription.Default(Color.DarkGreen);
            dialog = await this.AddComponentUIPanel("Modal Dialog", "Modal Dialog", descPan, layerUIModal);

            var font = TextDrawerDescription.FromFile(fontFilename, 20, true);

            Color4 releasedColor = new Color4((Color.DarkGray * 0.6666f).ToColor3(), 1f);
            Color4 pressedColor = new Color4((Color.DarkGray * 0.7777f).ToColor3(), 1f);
            var descButton = UIButtonDescription.DefaultTwoStateButton(font, releasedColor, pressedColor);
            descButton.TextHorizontalAlign = TextHorizontalAlign.Center;
            descButton.TextVerticalAlign = TextVerticalAlign.Middle;

            dialogAccept = new UIButton("DialogAccept", "DialogAccept", this, descButton);
            dialogAccept.Caption.Text = "Ok";

            dialogCancel = new UIButton("DialogCancel", "DialogCancel", this, descButton);
            dialogCancel.Caption.Text = "Cancel";

            var descText = UITextAreaDescription.DefaultFromFile(fontFilename, 28);
            descText.TextHorizontalAlign = TextHorizontalAlign.Center;
            descText.TextVerticalAlign = TextVerticalAlign.Middle;

            dialogText = new UITextArea("DialogText", "DialogText", this, descText);

            dialog.AddChild(dialogText);
            dialog.AddChild(dialogCancel, false);
            dialog.AddChild(dialogAccept, false);
            dialog.Visible = false;
            dialog.EventsEnabled = true;
        }
        private async Task InitializeUIPlayers()
        {
            player1Name = await this.AddComponentUITextArea("Player1Name", "Player1Name", UITextAreaDescription.DefaultFromFile(fontFilename, 20, true));
            player1Name.TextForeColor = player1Status.Color;
            player1Name.TextShadowColor = player1Status.Color * 0.5f;
            player1Name.GrowControlWithText = false;
            player1Name.TextHorizontalAlign = TextHorizontalAlign.Left;
            player1Name.Visible = false;

            player1Points = await this.AddComponentUITextArea("Player1Points", "Player1Points", UITextAreaDescription.DefaultFromFile(fontFilename, 25, true));
            player1Points.TextForeColor = player1Status.Color;
            player1Points.TextShadowColor = player1Status.Color * 0.5f;
            player1Points.GrowControlWithText = false;
            player1Points.TextHorizontalAlign = TextHorizontalAlign.Center;
            player1Points.Visible = false;

            player1Life = await this.AddComponentUIProgressBar("Player1Life", "Player1Life", UIProgressBarDescription.DefaultFromFile(fontFilename, 10, true));
            player1Life.ProgressColor = Color.DarkRed;
            player1Life.BaseColor = player1Status.Color;
            player1Life.Caption.TextForeColor = Color.White;
            player1Life.Caption.Text = "0%";
            player1Life.Visible = false;

            player2Name = await this.AddComponentUITextArea("Player2Name", "Player2Name", UITextAreaDescription.DefaultFromFile(fontFilename, 20, true));
            player2Name.TextForeColor = player2Status.Color;
            player2Name.TextShadowColor = player2Status.Color * 0.5f;
            player2Name.GrowControlWithText = false;
            player2Name.TextHorizontalAlign = TextHorizontalAlign.Right;
            player2Name.Visible = false;

            player2Points = await this.AddComponentUITextArea("Player2Points", "Player2Points", UITextAreaDescription.DefaultFromFile(fontFilename, 25, true));
            player2Points.TextForeColor = player2Status.Color;
            player2Points.TextShadowColor = player2Status.Color * 0.5f;
            player2Points.GrowControlWithText = false;
            player2Points.TextHorizontalAlign = TextHorizontalAlign.Center;
            player2Points.Visible = false;

            player2Life = await this.AddComponentUIProgressBar("Player2Life", "Player2Life", UIProgressBarDescription.DefaultFromFile(fontFilename, 10, true));
            player2Life.ProgressColor = Color.DarkRed;
            player2Life.BaseColor = player2Status.Color;
            player2Life.Caption.TextForeColor = Color.White;
            player2Life.Caption.Text = "0%";
            player2Life.Visible = false;
        }
        private async Task InitializeUITurn()
        {
            turnText = await this.AddComponentUITextArea("TurnText", "TurnText", UITextAreaDescription.DefaultFromFile(fontFilename, 40, true));
            turnText.TextForeColor = Color.Yellow;
            turnText.TextShadowColor = Color.Yellow * 0.5f;
            turnText.TextHorizontalAlign = TextHorizontalAlign.Center;
            turnText.GrowControlWithText = false;
            turnText.Visible = false;

            gameIcon = await this.AddComponentSprite("GameIcon", "GameIcon", SpriteDescription.Default("Resources/GameIcon.png"), SceneObjectUsages.UI);
            gameIcon.BaseColor = Color.Yellow;
            gameIcon.Visible = false;
            gameIcon.TweenRotateBounce(-0.1f, 0.1f, 500, ScaleFuncs.CubicEaseInOut);

            playerTurnMarker = await this.AddComponentSprite("PlayerTurnMarker", "PlayerTurnMarker", SpriteDescription.Default("Resources/Arrow.png"), SceneObjectUsages.UI);
            playerTurnMarker.BaseColor = Color.Turquoise;
            playerTurnMarker.Visible = false;
            playerTurnMarker.TweenScaleBounce(1, 1.2f, 500, ScaleFuncs.CubicEaseInOut);
        }
        private async Task InitializeUIKeyPanel()
        {
            int layerPanel = LayerUI;
            int layerSprites = layerPanel + 1;
            int layerKeys = layerSprites + 1;

            keyHelp = await this.AddComponentUIPanel("KeyHelp", "KeyHelp", UIPanelDescription.Default(Color4.Black * 0.3333f), layerPanel);
            keyHelp.Visible = false;

            keyRotate = await this.AddComponentSprite("KeyRotate", "KeyRotate", SpriteDescription.Default("Resources/Turn.png"), SceneObjectUsages.UI, layerSprites);
            keyRotate.BaseColor = Color.Turquoise;
            keyRotate.Visible = false;

            keyMove = await this.AddComponentSprite("KeyMove", "KeyMove", SpriteDescription.Default("Resources/Move.png"), SceneObjectUsages.UI, layerSprites);
            keyMove.BaseColor = Color.Turquoise;
            keyMove.Visible = false;

            KeyPitch = await this.AddComponentSprite("KeyPitch", "KeyPitch", SpriteDescription.Default("Resources/Pitch.png"), SceneObjectUsages.UI, layerSprites);
            KeyPitch.BaseColor = Color.Turquoise;
            KeyPitch.Visible = false;

            keyRotateLeftText = await this.AddComponentUITextArea("KeyRotateLeftText", "KeyRotateLeftText", UITextAreaDescription.DefaultFromFile(fontFilename, 15, FontMapStyles.Regular, true), layerKeys);
            keyRotateLeftText.TextForeColor = Color.Yellow;
            keyRotateLeftText.Text = "A";
            keyRotateLeftText.Visible = false;

            keyRotateRightText = await this.AddComponentUITextArea("KeyRotateRightText", "KeyRotateRightText", UITextAreaDescription.DefaultFromFile(fontFilename, 15, FontMapStyles.Regular, true), layerKeys);
            keyRotateRightText.TextForeColor = Color.Yellow;
            keyRotateRightText.Text = "D";
            keyRotateRightText.Visible = false;

            keyMoveForwardText = await this.AddComponentUITextArea("KeyMoveForwardText", "KeyMoveForwardText", UITextAreaDescription.DefaultFromFile(fontFilename, 15, FontMapStyles.Regular, true), layerKeys);
            keyMoveForwardText.TextForeColor = Color.Yellow;
            keyMoveForwardText.Text = "W";
            keyMoveForwardText.Visible = false;

            keyMoveBackwardText = await this.AddComponentUITextArea("KeyMoveBackwardText", "KeyMoveBackwardText", UITextAreaDescription.DefaultFromFile(fontFilename, 15, FontMapStyles.Regular, true), layerKeys);
            keyMoveBackwardText.TextForeColor = Color.Yellow;
            keyMoveBackwardText.Text = "S";
            keyMoveBackwardText.Visible = false;

            keyPitchUpText = await this.AddComponentUITextArea("KeyPitchUpText", "KeyPitchUpText", UITextAreaDescription.DefaultFromFile(fontFilename, 15, FontMapStyles.Regular, true), layerKeys);
            keyPitchUpText.TextForeColor = Color.Yellow;
            keyPitchUpText.Text = "Q";
            keyPitchUpText.Visible = false;

            keyPitchDownText = await this.AddComponentUITextArea("KeyPitchDownText", "KeyPitchDownText", UITextAreaDescription.DefaultFromFile(fontFilename, 15, FontMapStyles.Regular, true), layerKeys);
            keyPitchDownText.TextForeColor = Color.Yellow;
            keyPitchDownText.Text = "Z";
            keyPitchDownText.Visible = false;
        }
        private async Task InitializeUIFire()
        {
            pbFire = await this.AddComponentUIProgressBar("PbFire", "PbFire", UIProgressBarDescription.Default());
            pbFire.Anchor = Anchors.HorizontalCenter;
            pbFire.ProgressColor = Color.Yellow;
            pbFire.BaseColor = new Color4(0, 0, 0, 0.5f);
            pbFire.Visible = false;

            fireKeyText = await this.AddComponentUITextArea("FireKeyText", "FireKeyText", UITextAreaDescription.DefaultFromFile(fontFilename, 25, FontMapStyles.Regular, true));
            fireKeyText.TextForeColor = Color.Yellow;
            fireKeyText.Text = "Press space to fire!";
            fireKeyText.TextHorizontalAlign = TextHorizontalAlign.Center;
            fireKeyText.TextVerticalAlign = TextVerticalAlign.Middle;
            fireKeyText.GrowControlWithText = false;
            fireKeyText.Visible = false;
        }
        private async Task InitializeUIMinimap()
        {
            int layerPanel = LayerUI;
            int layerIcons = layerPanel + 1;
            int layerMarkers = layerIcons + 1;

            miniMapBackground = await this.AddComponentSprite("MiniMapBackground", "MiniMapBackground", SpriteDescription.Default("Resources/Compass.png"), SceneObjectUsages.UI, layerPanel);
            miniMapBackground.Alpha = 0.85f;
            miniMapBackground.Visible = false;

            miniMapTank1 = await this.AddComponentSprite("MiniMapTank1", "MiniMapTank1", SpriteDescription.Default("Resources/Tank.png"), SceneObjectUsages.UI, layerIcons);
            miniMapTank1.BaseColor = Color.Blue;
            miniMapTank1.Visible = false;

            miniMapTank2 = await this.AddComponentSprite("MiniMapTank2", "MiniMapTank2", SpriteDescription.Default("Resources/Tank.png"), SceneObjectUsages.UI, layerIcons);
            miniMapTank2.BaseColor = Color.Red;
            miniMapTank2.Visible = false;

            windVelocity = await this.AddComponentUIProgressBar("WindVelocity", "WindVelocity", UIProgressBarDescription.DefaultFromFile(fontFilename, 8), layerMarkers);
            windVelocity.Caption.Text = "Wind velocity";
            windVelocity.Caption.TextForeColor = Color.Yellow * 0.85f;
            windVelocity.ProgressColor = Color.DeepSkyBlue;
            windVelocity.BaseColor = new Color4(0, 0, 0, 0.5f);
            windVelocity.Visible = false;

            windDirectionArrow = await this.AddComponentSprite("WindDirectionArrow", "WindDirectionArrow", SpriteDescription.Default("Resources/Wind.png"), SceneObjectUsages.UI, layerMarkers);
            windDirectionArrow.BaseColor = Color.Green;
            windDirectionArrow.Visible = false;
        }
        private async Task InitializeUIShotPath()
        {
            trajectoryMarkerPool = new Sprite[5];
            for (int i = 0; i < trajectoryMarkerPool.Length; i++)
            {
                var trajectoryMarker = await this.AddComponentSprite($"TrajectoryMarker_{i}", $"TrajectoryMarker_{i}", SpriteDescription.Default("Resources/Dot_w.png"), SceneObjectUsages.UI);
                trajectoryMarker.Width = 50;
                trajectoryMarker.Height = 50;
                trajectoryMarker.BaseColor = Color.Transparent;
                trajectoryMarker.Active = false;
                trajectoryMarker.Visible = false;
                trajectoryMarker.TweenRotateRepeat(0, MathUtil.TwoPi, 1000, ScaleFuncs.Linear);

                trajectoryMarkerPool[i] = trajectoryMarker;
            }
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            UpdateLayoutLoadingUI();

            gameMessage.Anchor = Anchors.Center;
            gameMessage.Width = Game.Form.RenderWidth;
            gameMessage.Height = Game.Form.RenderHeight;

            gameKeyHelp.Anchor = Anchors.HorizontalCenter;
            gameKeyHelp.Top = Game.Form.RenderHeight - 60;
            gameKeyHelp.Width = 500;
            gameKeyHelp.Height = 40;

            float width = Game.Form.RenderWidth / 2f;
            float height = width * 0.6666f;
            dialog.Width = width;
            dialog.Height = height;
            dialog.Anchor = Anchors.Center;

            float butWidth = 150;
            float butHeight = 55;
            float butMargin = 15;

            dialogAccept.Width = butWidth;
            dialogAccept.Height = butHeight;
            dialogAccept.Top = dialog.Height - butMargin - butHeight;
            dialogAccept.Left = (dialog.Width * 0.5f) - (butWidth * 0.5f) - (butWidth * 0.6666f);

            dialogCancel.Width = butWidth;
            dialogCancel.Height = butHeight;
            dialogCancel.Top = dialog.Height - butMargin - butHeight;
            dialogCancel.Left = (dialog.Width * 0.5f) - (butWidth * 0.5f) + (butWidth * 0.6666f);

            dialogText.Padding = new Padding
            {
                Left = width * 0.1f,
                Right = width * 0.1f,
                Top = height * 0.1f,
                Bottom = butHeight + (butMargin * 2f),
            };

            float playerWidth = 300;
            player1Name.Width = playerWidth;
            player1Name.Top = 10;
            player1Name.Left = 10;
            player1Points.Width = playerWidth;
            player1Points.Top = 60;
            player1Points.Left = 10;
            player1Life.Width = playerWidth;
            player1Life.Height = 30;
            player1Life.Top = 100;
            player1Life.Left = 10;
            player2Name.Width = playerWidth;
            player2Name.Top = 10;
            player2Name.Left = Game.Form.RenderWidth - 10 - player2Name.Width;
            player2Points.Width = playerWidth;
            player2Points.Top = 60;
            player2Points.Left = Game.Form.RenderWidth - 10 - player2Points.Width;
            player2Life.Width = playerWidth;
            player2Life.Height = 30;
            player2Life.Top = 100;
            player2Life.Left = Game.Form.RenderWidth - 10 - player2Life.Width;

            turnText.Width = 300;
            turnText.Anchor = Anchors.HorizontalCenter;
            gameIcon.Width = 92;
            gameIcon.Height = 82;
            gameIcon.Top = 55;
            gameIcon.Anchor = Anchors.HorizontalCenter;
            playerTurnMarker.Width = 112;
            playerTurnMarker.Height = 75;
            playerTurnMarker.Top = 35;
            playerTurnMarker.Left = Game.Form.RenderCenter.X - 112 - 120;

            float top = Game.Form.RenderHeight - 150;
            keyHelp.Left = 0;
            keyHelp.Top = top;
            keyHelp.Height = 150;
            keyHelp.Width = 250;
            keyRotate.Left = 0;
            keyRotate.Top = top + 25;
            keyRotate.Width = 372 * 0.25f;
            keyRotate.Height = 365 * 0.25f;
            keyMove.Left = keyRotate.Width;
            keyMove.Top = top + 25;
            keyMove.Width = 232 * 0.25f;
            keyMove.Height = 365 * 0.25f;
            KeyPitch.Left = keyRotate.Width + keyMove.Width;
            KeyPitch.Top = top + 25;
            KeyPitch.Width = 322 * 0.25f;
            KeyPitch.Height = 365 * 0.25f;
            keyRotateLeftText.Top = top + 20;
            keyRotateLeftText.Left = 10;
            keyRotateRightText.Top = top + 20;
            keyRotateRightText.Left = keyRotate.Width - 30;
            keyMoveForwardText.Top = top + 20;
            keyMoveForwardText.Left = keyMove.AbsoluteCenter.X - 5;
            keyMoveBackwardText.Top = top + keyMove.Height + 10;
            keyMoveBackwardText.Left = keyMove.AbsoluteCenter.X - 5;
            keyPitchUpText.Top = top + 20;
            keyPitchUpText.Left = KeyPitch.AbsoluteCenter.X - 15;
            keyPitchDownText.Top = top + KeyPitch.Height + 10;
            keyPitchDownText.Left = KeyPitch.AbsoluteCenter.X + 10;

            pbFire.Top = Game.Form.RenderHeight - 100;
            pbFire.Width = 500;
            pbFire.Height = 40;
            fireKeyText.Anchor = Anchors.HorizontalCenter;
            fireKeyText.Top = Game.Form.RenderHeight - 60;
            fireKeyText.Width = 500;
            fireKeyText.Height = 40;

            miniMapBackground.Width = 200;
            miniMapBackground.Height = 200;
            miniMapBackground.Left = Game.Form.RenderWidth - 200 - 10;
            miniMapBackground.Top = Game.Form.RenderHeight - 200 - 10;
            miniMapTank1.Width = 273 * 0.1f;
            miniMapTank1.Height = 365 * 0.1f;
            miniMapTank1.Left = Game.Form.RenderWidth - 150 - 10;
            miniMapTank1.Top = Game.Form.RenderHeight - 150 - 10;
            miniMapTank2.Width = 273 * 0.1f;
            miniMapTank2.Height = 365 * 0.1f;
            miniMapTank2.Left = Game.Form.RenderWidth - 85 - 10;
            miniMapTank2.Top = Game.Form.RenderHeight - 85 - 10;
            windVelocity.Width = 180;
            windVelocity.Height = 15;
            windVelocity.Left = miniMapBackground.AbsoluteCenter.X - 90;
            windVelocity.Top = miniMapBackground.AbsoluteCenter.Y - 130;
            windDirectionArrow.Width = 100;
            windDirectionArrow.Height = 100;
            windDirectionArrow.Left = miniMapBackground.AbsoluteCenter.X - 50;
            windDirectionArrow.Top = miniMapBackground.AbsoluteCenter.Y - 50;
        }
        private void UpdateLayoutLoadingUI()
        {
            fadePanel.Width = Game.Form.RenderWidth;
            fadePanel.Height = Game.Form.RenderHeight;

            loadingText.Anchor = Anchors.HorizontalCenter;
            loadingText.Top = Game.Form.RenderCenter.Y - 75f;
            loadingText.Width = Game.Form.RenderWidth * 0.8f;

            loadingBar.Anchor = Anchors.Center;
            loadingBar.Width = Game.Form.RenderWidth * 0.8f;
            loadingBar.Height = 35;
        }

        private Task[] InitializeModels()
        {
            return new Task[]
            {
                InitializeModelsTanks(),
                InitializeModelsTerrain(),
                InitializeLandscape(),
                InitializeModelProjectile(),
                InitializeParticleManager(),
                InitializeDecalDrawer(),
                InitializeAudio(),
            };
        }
        private async Task InitializeModelsTanks()
        {
            var tDesc = new ModelInstancedDescription()
            {
                CastShadow = true,
                Optimize = false,
                Content = ContentDescription.FromFile("Resources/Leopard", "Leopard.json"),
                Instances = 2,
                TransformNames = new[] { "Barrel-mesh", "Turret-mesh", "Hull-mesh" },
                TransformDependences = new[] { 1, 2, -1 },
            };

            tanks = await this.AddComponentModelInstanced("Tanks", "Tanks", tDesc, SceneObjectUsages.Agent);
            tanks.Visible = false;

            tankHeight = tanks[0].GetBoundingBox().Height * 0.5f;
        }
        private async Task InitializeModelsTerrain()
        {
            // Generates a random terrain using perlin noise
            NoiseMapDescriptor nmDesc = new NoiseMapDescriptor
            {
                MapWidth = mapSize,
                MapHeight = mapSize,
                Scale = 0.5f,
                Lacunarity = 2f,
                Persistance = 0.5f,
                Octaves = 4,
                Offset = Vector2.One,
                Seed = Helper.RandomGenerator.Next(),
            };
            var noiseMap = NoiseMap.CreateNoiseMap(nmDesc);

            Curve heightCurve = new Curve();
            heightCurve.Keys.Add(0, 0);
            heightCurve.Keys.Add(0.4f, 0f);
            heightCurve.Keys.Add(1f, 1f);

            float cellSize = terrainSize / mapSize;

            var textures = new HeightmapTexturesDescription
            {
                ContentPath = "Resources/terrain",
                TexturesLR = new[] { "Diffuse.jpg" },
                NormalMaps = new[] { "Normal.jpg" },
                Scale = 0.2f,
            };
            GroundDescription groundDesc = GroundDescription.FromHeightmap(noiseMap, cellSize, terrainHeight, heightCurve, textures, 2);
            groundDesc.Heightmap.UseFalloff = true;

            terrain = await this.AddComponentScenery("Terrain", "Terrain", groundDesc);
            terrain.Visible = false;

            terrainTop = terrain.GetBoundingBox().Maximum.Y;

            SetGround(terrain, true);
        }
        private async Task InitializeLandscape()
        {
            float w = 1920f * 0.5f;
            float h = 1080f * 0.5f;
            float d = 2000f * 0.5f;
            float elevation = 500 * 0.5f;

            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-w*3, +h+elevation, d), Normal = Vector3.Up, Texture = new Vector2(0f, 0f) },
                new VertexData{ Position = new Vector3(+w*3, +h+elevation, d), Normal = Vector3.Up, Texture = new Vector2(3f, 0f) },
                new VertexData{ Position = new Vector3(-w*3, -h+elevation, d), Normal = Vector3.Up, Texture = new Vector2(0f, 1f) },
                new VertexData{ Position = new Vector3(+w*3, -h+elevation, d), Normal = Vector3.Up, Texture = new Vector2(3f, 1f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            var material = MaterialBlinnPhongContent.Default;
            material.DiffuseTexture = "Resources/Landscape.png";

            var content = new ModelDescription
            {
                Content = ContentDescription.FromContentData(vertices, indices, material),
            };

            landScape = await this.AddComponentModel("Landscape", "Landscape", content, SceneObjectUsages.None, LayerDefault);
            landScape.Visible = false;
        }
        private async Task InitializeModelProjectile()
        {
            var sphereDesc = GeometryUtil.CreateSphere(1, 5, 5);
            var material = MaterialBlinnPhongContent.Default;
            material.DiffuseColor = Color.Black;

            var content = new ModelDescription
            {
                Content = ContentDescription.FromContentData(sphereDesc, material),
            };
            content.DepthEnabled = false;

            projectile = await this.AddComponentModel("Projectile", "Projectile", content, SceneObjectUsages.None, LayerDefault + 1);
            projectile.Visible = false;
        }
        private async Task InitializeParticleManager()
        {
            particleManager = await this.AddComponentParticleManager("ParticleManager", "ParticleManager", ParticleManagerDescription.Default());

            var pPlume = ParticleSystemDescription.InitializeSmokePlume("Resources/particles", "smoke.png", 5);
            var pFire = ParticleSystemDescription.InitializeFire("Resources/particles", "fire.png", 5);
            var pDust = ParticleSystemDescription.InitializeDust("Resources/particles", "smoke.png", 5);
            var pProjectile = ParticleSystemDescription.InitializeProjectileTrail("Resources/particles", "smoke.png", 5);
            var pExplosion = ParticleSystemDescription.InitializeExplosion("Resources/particles", "fire.png", 5);
            var pSmokeExplosion = ParticleSystemDescription.InitializeExplosion("Resources/particles", "smoke.png", 5);

            particleDescriptions.Add("Plume", pPlume);
            particleDescriptions.Add("Fire", pFire);
            particleDescriptions.Add("Dust", pDust);
            particleDescriptions.Add("Projectile", pProjectile);
            particleDescriptions.Add("Explosion", pExplosion);
            particleDescriptions.Add("SmokeExplosion", pSmokeExplosion);
        }
        private async Task InitializeDecalDrawer()
        {
            var desc = DecalDrawerDescription.DefaultRotate(@"Resources/Crater.png", 100);

            decalDrawer = await this.AddComponentDecalDrawer("Craters", "Craters", desc);
            decalDrawer.TintColor = new Color(223, 194, 179);
        }
        private async Task InitializeAudio()
        {
            float nearRadius = 1000;
            ReverbPresets preset = ReverbPresets.Default;

            tankMoveEffect = "TankMove";
            tankDestroyedEffect = "TankDestroyed";
            tankShootingEffect = "TankShooting";
            impactEffects = new[] { "Impact1", "Impact2", "Impact3", "Impact4" };
            damageEffects = new[] { "Damage1", "Damage2", "Damage3", "Damage4" };

            AudioManager.LoadSound("Tank", "Resources/Audio", "tank_engine.wav");
            AudioManager.LoadSound("TankDestroyed", "Resources/Audio", "explosion_vehicle_small_close_01.wav");
            AudioManager.LoadSound("TankShooting", "Resources/Audio", "cannon-shooting.wav");
            AudioManager.LoadSound(impactEffects[0], "Resources/Audio", "metal_grate_large_01.wav");
            AudioManager.LoadSound(impactEffects[1], "Resources/Audio", "metal_grate_large_02.wav");
            AudioManager.LoadSound(impactEffects[2], "Resources/Audio", "metal_grate_large_03.wav");
            AudioManager.LoadSound(impactEffects[3], "Resources/Audio", "metal_grate_large_04.wav");
            AudioManager.LoadSound(damageEffects[0], "Resources/Audio", "metal_pipe_large_01.wav");
            AudioManager.LoadSound(damageEffects[1], "Resources/Audio", "metal_pipe_large_02.wav");
            AudioManager.LoadSound(damageEffects[2], "Resources/Audio", "metal_pipe_large_03.wav");
            AudioManager.LoadSound(damageEffects[3], "Resources/Audio", "metal_pipe_large_04.wav");

            AudioManager.AddEffectParams(
                tankMoveEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "Tank",
                    DestroyWhenFinished = false,
                    IsLooped = true,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 0.5f,
                });

            AudioManager.AddEffectParams(
                tankDestroyedEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankDestroyed",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                tankShootingEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankShooting",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                impactEffects[0],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[0],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                impactEffects[1],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[1],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                impactEffects[2],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[2],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                impactEffects[3],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[3],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                damageEffects[0],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[0],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                damageEffects[1],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[1],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                damageEffects[2],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[2],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                damageEffects[3],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[3],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = nearRadius,
                    ReverbPreset = preset,
                    Volume = 1f,
                });

            await Task.CompletedTask;
        }
        private void PrepareModels()
        {
            landScape.Visible = true;
            terrain.Visible = true;

            Vector3 p1 = new Vector3(-100, 100, 0);
            Vector3 n1 = Vector3.Up;
            Vector3 p2 = new Vector3(+100, 100, 0);
            Vector3 n2 = Vector3.Up;

            if (FindTopGroundPosition<Triangle>(p1.X, p1.Z, out var r1))
            {
                p1 = r1.Position - (Vector3.Up * 0.1f);
                n1 = r1.Item.Normal;
            }
            if (FindTopGroundPosition<Triangle>(p2.X, p2.Z, out var r2))
            {
                p2 = r2.Position - (Vector3.Up * 0.1f);
                n2 = r2.Item.Normal;
            }

            tanks[0].Manipulator.SetPosition(p1);
            tanks[0].Manipulator.RotateTo(p2);
            tanks[0].Manipulator.SetNormal(n1);

            tanks[1].Manipulator.SetPosition(p2);
            tanks[1].Manipulator.RotateTo(p1);
            tanks[1].Manipulator.SetNormal(n2);

            tanks.Visible = true;
        }

        private void InitializePlayers()
        {
            player1Status = new PlayerStatus
            {
                Name = "Player 1",
                Points = 0,
                MaxLife = 100,
                CurrentLife = 100,
                MaxMove = 25,
                CurrentMove = 25,
                Color = Color.Blue,
            };

            player2Status = new PlayerStatus
            {
                Name = "Player 2",
                Points = 0,
                MaxLife = 100,
                CurrentLife = 100,
                MaxMove = 25,
                CurrentMove = 25,
                Color = Color.Red,
            };
        }
        private void UpdateGameControls(bool visible)
        {
            player1Name.Visible = visible;
            player1Points.Visible = visible;
            player1Life.Visible = visible;
            player2Name.Visible = visible;
            player2Points.Visible = visible;
            player2Life.Visible = visible;

            turnText.Visible = visible;
            gameIcon.Visible = visible;
            playerTurnMarker.Visible = visible;

            keyHelp.Visible = visible;
            keyRotate.Visible = visible;
            keyMove.Visible = visible;
            KeyPitch.Visible = visible;
            keyRotateLeftText.Visible = visible;
            keyRotateRightText.Visible = visible;
            keyMoveForwardText.Visible = visible;
            keyMoveBackwardText.Visible = visible;
            keyPitchUpText.Visible = visible;
            keyPitchDownText.Visible = visible;

            pbFire.Visible = visible;
            fireKeyText.Visible = visible;
            fireKeyText.TweenScaleBounce(1, 1.01f, 500, ScaleFuncs.CubicEaseInOut);

            miniMapBackground.Visible = visible;
            miniMapTank1.Visible = visible;
            miniMapTank2.Visible = visible;
            windVelocity.Visible = visible;
            windDirectionArrow.Visible = visible;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            UpdateTurnStatus();
            UpdatePlayersStatus();

            if (gameEnding)
            {
                UpdateInputEndGame();

                return;
            }

            if (dialogActive)
            {
                return;
            }

            UpdateInputGame();

            if (shooting && shot != null)
            {
                IntegrateShot(gameTime);

                return;
            }

            if (freeCamera)
            {
                UpdateInputFree(gameTime);
                PaintShot(true);

                return;
            }

            UpdateInputPlayer(gameTime);
            UpdateInputShooting(gameTime);

            UpdateTanks();
            UpdateCamera(false);
        }

        private void UpdateInputGame()
        {
            if (freeCamera)
            {
                if (Game.Input.KeyJustReleased(Keys.F) ||
                    Game.Input.KeyJustReleased(Keys.Escape))
                {
                    ToggleFreeCamera();
                }
            }
            else
            {
                if (Game.Input.KeyJustReleased(Keys.F))
                {
                    ToggleFreeCamera();

                    return;
                }

                if (Game.Input.KeyJustReleased(Keys.Escape))
                {
                    ShowDialog(
                        @"Press Ok if you want to exit.

You will lost all the game progress.",
                        CloseDialog,
                        () =>
                        {
                            Game.Exit();
                        });
                }
            }
        }
        private void ToggleFreeCamera()
        {
            freeCamera = !freeCamera;

            if (freeCamera)
            {
                Camera.MovementDelta *= 10f;
                Game.LockMouse = true;
            }
            else
            {
                Camera.MovementDelta /= 10f;
                Game.LockMouse = false;
            }
        }
        private void UpdateInputPlayer(GameTime gameTime)
        {
            if (Game.Input.KeyPressed(Keys.A))
            {
                Shooter.Manipulator.Rotate(-gameTime.ElapsedSeconds, 0, 0);

                PlayEffectMove(Shooter);
            }
            if (Game.Input.KeyPressed(Keys.D))
            {
                Shooter.Manipulator.Rotate(+gameTime.ElapsedSeconds, 0, 0);

                PlayEffectMove(Shooter);
            }

            if (Game.Input.KeyPressed(Keys.Q))
            {
                Shooter.GetModelPartByName("Barrel-mesh").Manipulator.Rotate(0, gameTime.ElapsedSeconds, 0);
            }
            if (Game.Input.KeyPressed(Keys.Z))
            {
                Shooter.GetModelPartByName("Barrel-mesh").Manipulator.Rotate(0, -gameTime.ElapsedSeconds, 0);
            }

            if (ShooterStatus.CurrentMove <= 0)
            {
                return;
            }

            Vector3 prevPosition = Shooter.Manipulator.Position;

            if (Game.Input.KeyPressed(Keys.W))
            {
                Shooter.Manipulator.MoveForward(gameTime, 10);

                PlayEffectMove(Shooter);
            }
            if (Game.Input.KeyPressed(Keys.S))
            {
                Shooter.Manipulator.MoveBackward(gameTime, 10);

                PlayEffectMove(Shooter);
            }

            Vector3 position = Shooter.Manipulator.Position;

            ShooterStatus.CurrentMove -= Vector3.Distance(prevPosition, position);
            ShooterStatus.CurrentMove = Math.Max(0, ShooterStatus.CurrentMove);
        }
        private void UpdateInputShooting(GameTime gameTime)
        {
            if (Game.Input.KeyPressed(Keys.Space))
            {
                pbFire.ProgressValue += gameTime.ElapsedSeconds * 0.5f;
                pbFire.ProgressValue %= 1f;
                pbFire.ProgressColor = pbFire.ProgressValue < 0.75f ? Color.Yellow : Color4.Lerp(Color.Yellow, Color.Red, (pbFire.ProgressValue - 0.75f) / 0.25f);
            }

            if (Game.Input.KeyJustReleased(Keys.Space))
            {
                Shoot(pbFire.ProgressValue);
            }

            if (Game.Input.MouseButtonJustReleased(MouseButtons.Left))
            {
                PlayEffectImpact(Target);
            }
        }
        private void UpdateInputEndGame()
        {
            if (Game.Input.KeyJustReleased(Keys.Space))
            {
                Game.Exit();
            }
        }
        private void UpdateInputFree(GameTime gameTime)
        {
#if DEBUG
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    gameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                gameTime,
                Game.Input.MouseXDelta,
                Game.Input.MouseYDelta);
#endif

            Vector3 prevPosition = Camera.Position;

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(gameTime, Game.Input.ShiftPressed);
            }

            if (terrain.Intersects(new IntersectionVolumeSphere(Camera.Position, Camera.CameraRadius), out var res))
            {
                Camera.Position = prevPosition;
            }
        }

        private void UpdateTurnStatus()
        {
            turnText.Text = $"Turn {currentTurn}";

            if (currentPlayer == 0)
            {
                playerTurnMarker.Left = Game.Form.RenderCenter.X - 112 - 120;
                playerTurnMarker.Rotation = 0;
            }
            else
            {
                playerTurnMarker.Left = Game.Form.RenderCenter.X + 120;
                playerTurnMarker.Rotation = MathUtil.Pi;
            }
        }
        private void UpdatePlayersStatus()
        {
            player1Name.Text = player1Status.Name;
            player1Points.Text = $"{player1Status.Points} points";
            player1Life.Caption.Text = $"{player1Status.CurrentLife}";
            player1Life.ProgressValue = player1Status.Health;
            tanks[0].TextureIndex = player1Status.TextureIndex;

            player2Name.Text = player2Status.Name;
            player2Points.Text = $"{player2Status.Points} points";
            player2Life.Caption.Text = $"{player2Status.CurrentLife}";
            player2Life.ProgressValue = player2Status.Health;
            tanks[1].TextureIndex = player2Status.TextureIndex;
        }
        private void UpdateTanks()
        {
            if (FindTopGroundPosition<Triangle>(Shooter.Manipulator.Position.X, Shooter.Manipulator.Position.Z, out var r))
            {
                Shooter.Manipulator.SetPosition(r.Position - (Vector3.Up * 0.1f));
                Shooter.Manipulator.SetNormal(r.Item.Normal, 0.05f);
            }

            Shooter.GetModelPartByName("Turret-mesh").Manipulator.RotateTo(Target.Manipulator.Position, Vector3.Up, Axis.Y, 0.01f);

            PaintMinimap();

            PaintShot(true);
        }
        private void UpdateCamera(bool firstUpdate)
        {
            // Find tanks distance vector
            Vector3 diffV = tanks[1].Manipulator.Position - tanks[0].Manipulator.Position;
            Vector3 distV = Vector3.Normalize(diffV);
            float dist = diffV.Length();

            // Interest to medium point
            Vector3 interest = tanks[0].Manipulator.Position + (distV * dist * 0.5f);

            // Perpendicular to diff
            Vector3 perp = Vector3.Normalize(Vector3.Cross(Vector3.Up, diffV));
            float y = Math.Max(100f, dist * 0.5f);
            float z = Math.Max(200f, dist);
            Vector3 position = interest + (perp * z) + (Vector3.Up * y);

            if (firstUpdate)
            {
                Camera.Position = position;
            }
            else
            {
                Camera.Goto(position, CameraTranslations.Quick);
            }

            Camera.Interest = interest;
        }

        private void PaintShot(bool visible)
        {
            trajectoryMarkerPool.ToList().ForEach(m =>
            {
                m.Active = false;
                m.Visible = false;
            });

            if (!visible)
            {
                return;
            }

            Vector3 from = Shooter.Manipulator.Position;
            from.Y += tankHeight;

            var shotDirection = Shooter.GetModelPartByName("Barrel-mesh").Manipulator.FinalTransform.Forward;

            Vector3 to = from + (shotDirection * 1000f);

            float sampleDist = 20;
            float distance = Vector3.Distance(from, to);
            Vector3 shootDirection = Vector3.Normalize(to - from);
            int markers = Math.Min(trajectoryMarkerPool.Length, (int)(distance / sampleDist));
            if (markers == 0)
            {
                return;
            }

            // Initialize sample dist
            float dist = sampleDist;
            for (int i = 0; i < markers; i++)
            {
                Vector3 markerPos = from + (shootDirection * dist);
                Vector3 screenPos = Vector3.Project(markerPos,
                    Game.Graphics.Viewport.X,
                    Game.Graphics.Viewport.Y,
                    Game.Graphics.Viewport.Width,
                    Game.Graphics.Viewport.Height,
                    Game.Graphics.Viewport.MinDepth,
                    Game.Graphics.Viewport.MaxDepth,
                    Camera.View * Camera.Projection);
                float scale = (1f - screenPos.Z) * 1000f;

                trajectoryMarkerPool[i].Left = screenPos.X - (trajectoryMarkerPool[i].Width * 0.5f);
                trajectoryMarkerPool[i].Top = screenPos.Y - (trajectoryMarkerPool[i].Height * 0.5f);
                trajectoryMarkerPool[i].Scale = scale;
                trajectoryMarkerPool[i].BaseColor = ShooterStatus.Color;
                trajectoryMarkerPool[i].Alpha = 1f - (i / (float)markers);
                trajectoryMarkerPool[i].Active = true;
                trajectoryMarkerPool[i].Visible = true;

                dist += sampleDist;
            }
        }
        private void PaintMinimap()
        {
            // Set wind velocity and direction
            windVelocity.ProgressValue = currentWindVelocity / maxWindVelocity;
            windDirectionArrow.Rotation = Helper.AngleSigned(Vector2.UnitY, windDirection);

            // Get terrain minimap rectangle
            BoundingBox bbox = terrain.GetBoundingBox();
            RectangleF terrainRect = new RectangleF(bbox.Minimum.X, bbox.Minimum.Z, bbox.Width, bbox.Depth);

            // Get object space positions and transform to screen space
            Vector2 tank1 = tanks[0].Manipulator.Position.XZ() - terrainRect.TopLeft;
            Vector2 tank2 = tanks[1].Manipulator.Position.XZ() - terrainRect.TopLeft;

            // Get the mini map rectangle
            RectangleF miniMapRect = miniMapBackground.GetRenderArea(false);

            // Get the marker sprite bounds
            Vector2 markerBounds1 = new Vector2(miniMapTank1.Width, miniMapTank1.Height);
            Vector2 markerBounds2 = new Vector2(miniMapTank2.Width, miniMapTank2.Height);

            // Calculate proportional 2D locations (tank to terrain)
            float tank1ToTerrainX = tank1.X / terrainRect.Width;
            float tank1ToTerrainY = tank1.Y / terrainRect.Height;
            float tank2ToTerrainX = tank2.X / terrainRect.Width;
            float tank2ToTerrainY = tank2.Y / terrainRect.Height;

            // Marker to minimap inverting Y coordinates
            Vector2 markerToMinimap1 = new Vector2(miniMapRect.Width * tank1ToTerrainX, miniMapRect.Height * (1f - tank1ToTerrainY));
            Vector2 markerToMinimap2 = new Vector2(miniMapRect.Width * tank2ToTerrainX, miniMapRect.Height * (1f - tank2ToTerrainY));

            // Translate and center into the minimap
            Vector2 mt1Position = markerToMinimap1 + miniMapRect.TopLeft - (markerBounds1 * 0.5f);
            Vector2 mt2Position = markerToMinimap2 + miniMapRect.TopLeft - (markerBounds2 * 0.5f);

            // Set marker position
            miniMapTank1.SetPosition(mt1Position);
            miniMapTank2.SetPosition(mt2Position);

            // Set marker rotation
            miniMapTank1.Rotation = Helper.AngleSigned(Vector2.UnitY, Vector2.Normalize(tanks[0].Manipulator.Forward.XZ()));
            miniMapTank2.Rotation = Helper.AngleSigned(Vector2.UnitY, Vector2.Normalize(tanks[1].Manipulator.Forward.XZ()));
        }

        private void Shoot(float shotForce)
        {
            var shotDirection = Shooter.GetModelPartByName("Barrel-mesh").Manipulator.FinalTransform.Forward;

            shot = new ParabolicShot();
            shot.Configure(Game.GameTime, shotDirection, shotForce * 200, windDirection, currentWindVelocity);

            shooting = true;

            PlayEffectShooting(Shooter);
        }
        private void IntegrateShot(GameTime gameTime)
        {
            // Set projectile position
            Vector3 shotPos = shot.Integrate(gameTime, Vector3.Zero, Vector3.Zero);
            Vector3 projectilePosition = Shooter.Manipulator.Position + shotPos;
            projectilePosition.Y += tankHeight;
            projectile.Manipulator.SetPosition(projectilePosition, true);
            var projVolume = projectile.GetBoundingSphere(true);
            projectile.Visible = true;

            // Test collision with target
            if (Target.Intersects(projVolume, out var targetImpact))
            {
                ResolveShot(true, targetImpact.Position, targetImpact.Item.Normal);

                return;
            }

            // Test if projectile is under the terrain box
            var terrainBox = terrain.GetBoundingBox();
            if (projVolume.Center.Y + projVolume.Radius < terrainBox.Minimum.Y)
            {
                ResolveShot(false, null, null);

                return;
            }

            // Test full collision with terrain mesh
            if (terrain.Intersects(projVolume, out var terrainImpact))
            {
                ResolveShot(false, terrainImpact.Position, terrainImpact.Item.Normal);
            }
        }
        private void ResolveShot(bool impact, Vector3? impactPosition, Vector3? impactNormal)
        {
            shot = null;
            shooting = false;

            Vector3 outPosition = Vector3.Up * (terrainTop + 1);
            projectile.Manipulator.SetPosition(outPosition, true);
            projectile.Visible = false;

            if (impact)
            {
                //Target damaged
                int res = Helper.RandomGenerator.Next(10, 50);

                ShooterStatus.Points += res * 100;
                TargetStatus.CurrentLife = MathUtil.Clamp(TargetStatus.CurrentLife - res, 0, TargetStatus.MaxLife);

                if (impactPosition.HasValue)
                {
                    //Add damage effects to tank
                    AddExplosionSystem(impactPosition.Value);
                    PlayEffectDamage(Target);
                    PlayEffectImpact(Target);
                }

                if (TargetStatus.CurrentLife == 0)
                {
                    //Tank destroyed

                    Task.Run(async () =>
                    {
                        Vector3 min = Vector3.One * -5f;
                        Vector3 max = Vector3.One * +5f;

                        AddExplosionSystem(Target.Manipulator.Position + Helper.RandomGenerator.NextVector3(min, max));
                        PlayEffectDestroyed(Target);

                        await Task.Delay(500);

                        AddExplosionSystem(Target.Manipulator.Position + Helper.RandomGenerator.NextVector3(min, max));

                        await Task.Delay(500);

                        AddExplosionSystem(Target.Manipulator.Position + Helper.RandomGenerator.NextVector3(min, max));
                        AddExplosionSystem(Target.Manipulator.Position + Helper.RandomGenerator.NextVector3(min, max));

                        await Task.Delay(3000);

                        AddExplosionSystem(Target.Manipulator.Position + Helper.RandomGenerator.NextVector3(min, max));
                        AddExplosionSystem(Target.Manipulator.Position + Helper.RandomGenerator.NextVector3(min, max));
                        AddExplosionSystem(Target.Manipulator.Position + Helper.RandomGenerator.NextVector3(min, max));
                        PlayEffectDestroyed(Target);
                    });
                }
            }
            else
            {
                //Ground impact
                if (impactPosition.HasValue)
                {
                    AddSmokePlumeSystem(impactPosition.Value);
                    AddCrater(impactPosition.Value, impactNormal.Value);
                    PlayEffectDestroyed(impactPosition.Value);
                }
            }

            Task.Run(async () =>
            {
                dialogActive = true;

                await ShowMessage(impact ? "Impact!" : "You miss!", 2000);

                await EvaluateTurn(ShooterStatus, TargetStatus);

                if (!gameEnding)
                {
                    await ShowMessage($"Your turn {ShooterStatus.Name}", 2000);
                }

                dialogActive = false;
            });
        }

        private async Task EvaluateTurn(PlayerStatus shooter, PlayerStatus target)
        {
            pbFire.ProgressValue = 0;

            if (target.CurrentLife == 0)
            {
                gameEnding = true;

                gameMessage.Text = $"The winner is {shooter.Name}!";
                gameMessage.TextForeColor = shooter.Color;
                gameMessage.TextShadowColor = shooter.Color * 0.5f;
                gameMessage.Show(1000);
                gameMessage.TweenScale(0, 1, 1000, ScaleFuncs.CubicEaseIn);

                fadePanel.Show(3000);

                await Task.Delay(3000);

                gameKeyHelp.Show(1000);
                gameKeyHelp.TweenScaleBounce(1, 1.01f, 500, ScaleFuncs.CubicEaseInOut);

                return;
            }

            currentPlayer++;
            currentPlayer %= 2;

            PaintShot(true);

            if (currentPlayer == 0)
            {
                currentTurn++;

                ShooterStatus.NewTurn();
                TargetStatus.NewTurn();

                currentWindVelocity = Helper.RandomGenerator.NextFloat(0f, maxWindVelocity);
                windDirection = Helper.RandomGenerator.NextVector2(-Vector2.One, Vector2.One);

                Parallel.ForEach(particleManager.ParticleSystems, p =>
                {
                    var particleParams = p.GetParameters();
                    particleParams.Gravity = new Vector3(windDirection.X, 0, windDirection.Y) * currentWindVelocity;
                    p.SetParameters(particleParams);
                });
            }
        }

        private void AddExplosionSystem(Vector3 position)
        {
            Vector3 velocity = Vector3.Up;
            float duration = 0.5f;
            float rate = 0.01f;

            var emitter1 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate,
                InfiniteDuration = false,
                MaximumDistance = 1000f,
            };
            var emitter2 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration * 5f,
                EmissionRate = rate * 10f,
                InfiniteDuration = false,
                MaximumDistance = 1000f,
            };

            particleManager.AddParticleSystem(ParticleSystemTypes.CPU, particleDescriptions["Explosion"], emitter1);
            particleManager.AddParticleSystem(ParticleSystemTypes.CPU, particleDescriptions["SmokeExplosion"], emitter2);
        }
        private void AddSmokePlumeSystem(Vector3 position)
        {
            Vector3 velocity = Vector3.Up;
            float duration = Helper.RandomGenerator.NextFloat(10, 30);
            float rate = Helper.RandomGenerator.NextFloat(0.1f, 1f);

            var emitter1 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate * 0.5f,
                InfiniteDuration = false,
                MaximumDistance = 1000f,
            };

            var emitter2 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration + (duration * 0.1f),
                EmissionRate = rate,
                InfiniteDuration = false,
                MaximumDistance = 5000f,
            };

            particleManager.AddParticleSystem(ParticleSystemTypes.CPU, particleDescriptions["Fire"], emitter1);
            particleManager.AddParticleSystem(ParticleSystemTypes.CPU, particleDescriptions["Plume"], emitter2);
        }
        private void AddCrater(Vector3 position, Vector3 normal)
        {
            decalDrawer.AddDecal(position + (normal * 0.2f), normal, Vector2.One * 20f, float.PositiveInfinity);
        }

        private async Task ShowMessage(string text, int delay)
        {
            gameMessage.Text = text;
            gameMessage.TweenScale(0, 1, 500, ScaleFuncs.CubicEaseIn);
            gameMessage.Show(500);

            await Task.Delay(delay);

            gameMessage.ClearTween();
            gameMessage.Hide(100);

            await Task.Delay(100);
        }

        private void PlayEffectMove(ITransformable3D emitter)
        {
            if (tankMoveEffectInstance == null)
            {
                tankMoveEffectInstance = AudioManager.CreateEffectInstance(tankMoveEffect, emitter, Camera);
                tankMoveEffectInstance.Play();

                Task.Run(async () =>
                {
                    await Task.Delay(10000);
                    tankMoveEffectInstance.Stop();
                    tankMoveEffectInstance.Dispose();
                    tankMoveEffectInstance = null;
                });
            }
        }
        private void PlayEffectShooting(ITransformable3D emitter)
        {
            AudioManager.CreateEffectInstance(tankShootingEffect, emitter, Camera)?.Play();
        }
        private void PlayEffectImpact(ITransformable3D emitter)
        {
            int index = Helper.RandomGenerator.Next(0, impactEffects.Length);
            index %= impactEffects.Length - 1;
            AudioManager.CreateEffectInstance(impactEffects[index], emitter, Camera)?.Play();
        }
        private void PlayEffectDamage(ITransformable3D emitter)
        {
            int index = Helper.RandomGenerator.Next(0, damageEffects.Length);
            index %= damageEffects.Length - 1;
            AudioManager.CreateEffectInstance(damageEffects[index], emitter, Camera)?.Play();
        }
        private void PlayEffectDestroyed(ITransformable3D emitter)
        {
            AudioManager.CreateEffectInstance(tankDestroyedEffect, emitter, Camera)?.Play();
        }
        private void PlayEffectDestroyed(Vector3 emitter)
        {
            AudioManager.CreateEffectInstance(tankDestroyedEffect, emitter, Camera)?.Play();
        }

        private void ShowDialog(string message, Action onCloseCallback, Action onAcceptCallback)
        {
            dialogActive = true;

            if (lastOnCloseHandler != null)
            {
                dialogCancel.MouseClick -= lastOnCloseHandler;
            }
            if (onCloseCallback != null)
            {
                lastOnCloseHandler = (sender, args) =>
                {
                    onCloseCallback.Invoke();
                };

                dialogCancel.MouseClick += lastOnCloseHandler;
            }

            if (lastOnAcceptHandler != null)
            {
                dialogAccept.MouseClick -= lastOnAcceptHandler;
            }
            if (onAcceptCallback != null)
            {
                lastOnAcceptHandler = (sender, args) =>
                {
                    onAcceptCallback.Invoke();
                };

                dialogAccept.MouseClick += lastOnAcceptHandler;
            }

            dialogText.Text = message;

            dialog.Show(500);
            fadePanel.TweenAlpha(0, 0.5f, 500, ScaleFuncs.Linear);

            SetOnModalEffects();

            Game.VisibleMouse = true;
        }
        private void CloseDialog()
        {
            dialog.Hide(500);
            fadePanel.TweenAlpha(0.5f, 0f, 500, ScaleFuncs.Linear);

            SetOnGameEffects();

            Game.VisibleMouse = false;

            Task.Run(async () =>
            {
                await Task.Delay(500);

                dialogActive = false;
            });
        }
        private void SetOnGameEffects()
        {
            Renderer.ClearPostProcessingEffects();
            Renderer?.SetPostProcessingEffect(RenderPass.Objects, PostProcessingEffects.ToneMapping, PostProcessToneMappingParams.RomBinDaHouse);
        }
        private void SetOnModalEffects()
        {
            Renderer.ClearPostProcessingEffects();
            Renderer.SetPostProcessingEffect(RenderPass.Objects, PostProcessingEffects.Grayscale, null);
            Renderer.SetPostProcessingEffect(RenderPass.Objects, PostProcessingEffects.Blur, PostProcessBlurParams.Strong);
        }
    }
}
