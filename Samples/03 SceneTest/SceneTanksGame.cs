using Engine;
using Engine.Common;
using Engine.Content;
using Engine.Tween;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SceneTest
{
    /// <summary>
    /// Tanks game scene
    /// </summary>
    class SceneTanksGame : Scene
    {
        const int layerLoadingUI = 100;
        const int layerUI = 50;
        const int layerModels = 10;

        const string fontFilename = "SceneTanksGame/LeagueSpartan-Bold.otf";

        private bool gameReady = false;

        private UITextArea loadingText;
        private UIProgressBar loadingBar;
        private float progressValue = 0;
        private UIPanel fadePanel;

        private UITextArea gameMessage;
        private UITextArea gameKeyHelp;

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
        private Vector2 windForce = Vector2.Normalize(Vector2.One);
        private UIProgressBar windVelocity;
        private Sprite windDirection;

        private Model landScape;
        private Scenery terrain;
        private ModelInstanced tanks;
        private float tankHeight = 0;

        private Sprite[] trajectoryMarkerPool;
        private Sprite targetMarker;

        private bool shooting = false;
        private bool gameEnding = false;

        private ModelInstance Shooter { get { return tanks[currentPlayer]; } }
        private ModelInstance Target { get { return tanks[(currentPlayer + 1) % 2]; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public SceneTanksGame(Game game) : base(game)
        {
            InitializePlayers();
        }

        public override void OnReportProgress(float value)
        {
            progressValue = Math.Max(progressValue, value);

            if (loadingBar != null)
            {
                loadingBar.ProgressValue = progressValue;
                loadingBar.Caption.Text = $"{(int)(progressValue * 100f)}%";
            }
        }

        public override Task Initialize()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 2000;

            return LoadLoadingUI();
        }

        private async Task LoadLoadingUI()
        {
            await this.LoadResourcesAsync(
                InitializeLoadingUI(),
                async () =>
                {
                    fadePanel.TintColor = Color.Black;
                    fadePanel.Visible = true;

                    loadingText.Text = "Please wait...";
                    loadingText.Visible = true;
                    loadingText.TweenAlphaBounce(1, 0, 1000, ScaleFuncs.CubicEaseInOut);

                    loadingBar.ProgressValue = 0;
                    loadingBar.Visible = true;

                    await this.LoadUI();
                });
        }
        private async Task InitializeLoadingUI()
        {
            fadePanel = await this.AddComponentUIPanel(UIPanelDescription.Screen(this), layerLoadingUI);
            fadePanel.Visible = false;

            loadingText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 40, true), layerLoadingUI + 1);
            loadingText.TextColor = Color.Yellow;
            loadingText.TextShadowColor = Color.Orange;
            loadingText.CenterHorizontally = CenterTargets.Screen;
            loadingText.Top = this.Game.Form.RenderCenter.Y - 75f;
            loadingText.Width = this.Game.Form.RenderWidth * 0.8f;
            loadingText.HorizontalAlign = HorizontalTextAlign.Center;
            loadingText.VerticalAlign = VerticalTextAlign.Middle;
            loadingText.AdjustAreaWithText = false;
            loadingText.Visible = false;

            loadingBar = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFile(fontFilename, 20, true), layerLoadingUI + 1);
            loadingBar.CenterHorizontally = CenterTargets.Screen;
            loadingBar.CenterVertically = CenterTargets.Screen;
            loadingBar.Width = this.Game.Form.RenderWidth * 0.8f;
            loadingBar.Height = 35;
            loadingBar.ProgressColor = Color.Yellow;
            loadingBar.BaseColor = Color.CornflowerBlue;
            loadingBar.Caption.TextColor = Color.Black;
            loadingBar.Caption.Text = "0%";
            loadingBar.Visible = false;
        }

        private async Task LoadUI()
        {
            List<Task> taskList = new List<Task>();
            taskList.AddRange(InitializeUI());
            taskList.AddRange(InitializeModels());

            await this.LoadResourcesAsync(
                taskList.ToArray(),
                () =>
                {
                    this.PrepareUI();
                    this.PrepareModels();

                    Task.Run(async () =>
                    {
                        loadingText.ClearTween();
                        loadingText.Hide(1000);
                        loadingBar.ClearTween();
                        loadingBar.Hide(500);

                        await Task.Delay(1500);

                        gameMessage.Text = "Ready!";
                        gameMessage.TweenScale(0, 1, 500, ScaleFuncs.CubicEaseIn);
                        gameMessage.Show(500);

                        await Task.Delay(2000);

                        gameMessage.ClearTween();
                        gameMessage.Hide(100);
                        fadePanel.ClearTween();
                        fadePanel.Hide(2000);

                        gameReady = true;

                        UpdateGameControls(true);

                        PaintShot();
                    });
                });
        }

        private Task[] InitializeUI()
        {
            return new[]
            {
                InitializeUIGameMessages(),
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
            gameMessage = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 120, false), layerLoadingUI + 1);
            gameMessage.CenterHorizontally = CenterTargets.Screen;
            gameMessage.CenterVertically = CenterTargets.Screen;
            gameMessage.TextColor = Color.Yellow;
            gameMessage.TextShadowColor = Color.Yellow * 0.5f;
            gameMessage.Visible = false;

            gameKeyHelp = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 25, true), layerLoadingUI + 1);
            gameKeyHelp.TextColor = Color.Yellow;
            gameKeyHelp.Text = "Press space to exit";
            gameKeyHelp.CenterHorizontally = CenterTargets.Screen;
            gameKeyHelp.Top = this.Game.Form.RenderHeight - 60;
            gameKeyHelp.Width = 500;
            gameKeyHelp.Height = 40;
            gameKeyHelp.HorizontalAlign = HorizontalTextAlign.Center;
            gameKeyHelp.VerticalAlign = VerticalTextAlign.Middle;
            gameKeyHelp.AdjustAreaWithText = false;
            gameKeyHelp.Visible = false;
        }
        private async Task InitializeUIPlayers()
        {
            float playerWidth = 300;

            player1Name = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 20, true), layerUI);
            player1Name.TextColor = player1Status.Color;
            player1Name.TextShadowColor = player1Status.Color * 0.5f;
            player1Name.AdjustAreaWithText = false;
            player1Name.HorizontalAlign = HorizontalTextAlign.Left;
            player1Name.Width = playerWidth;
            player1Name.Top = 10;
            player1Name.Left = 10;
            player1Name.Visible = false;

            player1Points = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 25, true), layerUI);
            player1Points.TextColor = player1Status.Color;
            player1Points.TextShadowColor = player1Status.Color * 0.5f;
            player1Points.AdjustAreaWithText = false;
            player1Points.HorizontalAlign = HorizontalTextAlign.Center;
            player1Points.Width = playerWidth;
            player1Points.Top = 60;
            player1Points.Left = 10;
            player1Points.Visible = false;

            player1Life = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFile(fontFilename, 10, true), layerUI);
            player1Life.Width = playerWidth;
            player1Life.Height = 30;
            player1Life.Top = 100;
            player1Life.Left = 10;
            player1Life.ProgressColor = player1Status.Color;
            player1Life.BaseColor = Color.Black;
            player1Life.Caption.TextColor = Color.White;
            player1Life.Caption.Text = "0%";
            player1Life.Visible = false;

            player2Name = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 20, true), layerUI);
            player2Name.TextColor = player2Status.Color;
            player2Name.TextShadowColor = player2Status.Color * 0.5f;
            player2Name.AdjustAreaWithText = false;
            player2Name.HorizontalAlign = HorizontalTextAlign.Right;
            player2Name.Width = playerWidth;
            player2Name.Top = 10;
            player2Name.Left = this.Game.Form.RenderWidth - 10 - player2Name.Width;
            player2Name.Visible = false;

            player2Points = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 25, true), layerUI);
            player2Points.TextColor = player2Status.Color;
            player2Points.TextShadowColor = player2Status.Color * 0.5f;
            player2Points.AdjustAreaWithText = false;
            player2Points.HorizontalAlign = HorizontalTextAlign.Center;
            player2Points.Width = playerWidth;
            player2Points.Top = 60;
            player2Points.Left = this.Game.Form.RenderWidth - 10 - player2Points.Width;
            player2Points.Visible = false;

            player2Life = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFile(fontFilename, 10, true), layerUI);
            player2Life.Width = playerWidth;
            player2Life.Height = 30;
            player2Life.Top = 100;
            player2Life.Left = this.Game.Form.RenderWidth - 10 - player2Life.Width;
            player2Life.ProgressColor = player2Status.Color;
            player2Life.BaseColor = Color.Black;
            player2Life.Caption.TextColor = Color.White;
            player2Life.Caption.Text = "0%";
            player2Life.Visible = false;
        }
        private async Task InitializeUITurn()
        {
            turnText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 40, true), layerUI);
            turnText.TextColor = Color.Yellow;
            turnText.TextShadowColor = Color.Yellow * 0.5f;
            turnText.HorizontalAlign = HorizontalTextAlign.Center;
            turnText.Width = 300;
            turnText.CenterHorizontally = CenterTargets.Screen;
            turnText.AdjustAreaWithText = false;
            turnText.Visible = false;

            gameIcon = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/GameIcon.png"), SceneObjectUsages.UI, layerUI);
            gameIcon.TintColor = Color.Yellow;
            gameIcon.Width = 92;
            gameIcon.Height = 82;
            gameIcon.Top = 55;
            gameIcon.CenterHorizontally = CenterTargets.Screen;
            gameIcon.Visible = false;
            gameIcon.TweenRotateBounce(-0.1f, 0.1f, 500, ScaleFuncs.CubicEaseInOut);

            playerTurnMarker = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Arrow.png"), SceneObjectUsages.UI, layerUI);
            playerTurnMarker.TintColor = Color.Turquoise;
            playerTurnMarker.Width = 112;
            playerTurnMarker.Height = 75;
            playerTurnMarker.Top = 35;
            playerTurnMarker.Left = this.Game.Form.RenderCenter.X - 112 - 120;
            playerTurnMarker.Visible = false;
            playerTurnMarker.TweenScaleBounce(1, 1.2f, 500, ScaleFuncs.CubicEaseInOut);
        }
        private async Task InitializeUIKeyPanel()
        {
            float top = this.Game.Form.RenderHeight - 150;

            keyHelp = await this.AddComponentUIPanel(UIPanelDescription.Default, layerUI);
            keyHelp.Left = 0;
            keyHelp.Top = top;
            keyHelp.Height = 150;
            keyHelp.Width = 250;
            keyHelp.Visible = false;

            keyRotate = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Turn.png"), SceneObjectUsages.UI, layerUI + 1);
            keyRotate.Left = 0;
            keyRotate.Top = top + 25;
            keyRotate.Width = 372 * 0.25f;
            keyRotate.Height = 365 * 0.25f;
            keyRotate.TintColor = Color.Turquoise;
            keyRotate.Visible = false;

            keyMove = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Move.png"), SceneObjectUsages.UI, layerUI + 1);
            keyMove.Left = keyRotate.Width;
            keyMove.Top = top + 25;
            keyMove.Width = 232 * 0.25f;
            keyMove.Height = 365 * 0.25f;
            keyMove.TintColor = Color.Turquoise;
            keyMove.Visible = false;

            KeyPitch = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Pitch.png"), SceneObjectUsages.UI, layerUI + 1);
            KeyPitch.Left = keyRotate.Width + keyMove.Width;
            KeyPitch.Top = top + 25;
            KeyPitch.Width = 322 * 0.25f;
            KeyPitch.Height = 365 * 0.25f;
            KeyPitch.TintColor = Color.Turquoise;
            KeyPitch.Visible = false;

            keyRotateLeftText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyRotateLeftText.TextColor = Color.Yellow;
            keyRotateLeftText.Text = "A";
            keyRotateLeftText.Top = top + 20;
            keyRotateLeftText.Left = 10;
            keyRotateLeftText.Visible = false;

            keyRotateRightText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyRotateRightText.TextColor = Color.Yellow;
            keyRotateRightText.Text = "D";
            keyRotateRightText.Top = top + 20;
            keyRotateRightText.Left = keyRotate.Width - 30;
            keyRotateRightText.Visible = false;

            keyMoveForwardText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyMoveForwardText.TextColor = Color.Yellow;
            keyMoveForwardText.Text = "W";
            keyMoveForwardText.Top = top + 20;
            keyMoveForwardText.Left = keyMove.AbsoluteCenter.X - 5;
            keyMoveForwardText.Visible = false;

            keyMoveBackwardText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyMoveBackwardText.TextColor = Color.Yellow;
            keyMoveBackwardText.Text = "S";
            keyMoveBackwardText.Top = top + keyMove.Height + 10;
            keyMoveBackwardText.Left = keyMove.AbsoluteCenter.X - 5;
            keyMoveBackwardText.Visible = false;

            keyPitchUpText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyPitchUpText.TextColor = Color.Yellow;
            keyPitchUpText.Text = "Q";
            keyPitchUpText.Top = top + 20;
            keyPitchUpText.Left = KeyPitch.AbsoluteCenter.X - 15;
            keyPitchUpText.Visible = false;

            keyPitchDownText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyPitchDownText.TextColor = Color.Yellow;
            keyPitchDownText.Text = "Z";
            keyPitchDownText.Top = top + KeyPitch.Height + 10;
            keyPitchDownText.Left = KeyPitch.AbsoluteCenter.X + 10;
            keyPitchDownText.Visible = false;
        }
        private async Task InitializeUIFire()
        {
            pbFire = await this.AddComponentUIProgressBar(UIProgressBarDescription.Default(), layerUI);
            pbFire.CenterHorizontally = CenterTargets.Screen;
            pbFire.Top = this.Game.Form.RenderHeight - 100;
            pbFire.Width = 500;
            pbFire.Height = 40;
            pbFire.ProgressColor = Color.Yellow;
            pbFire.Visible = false;

            fireKeyText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 25, true), layerUI + 2);
            fireKeyText.TextColor = Color.Yellow;
            fireKeyText.Text = "Press space to fire!";
            fireKeyText.CenterHorizontally = CenterTargets.Screen;
            fireKeyText.Top = this.Game.Form.RenderHeight - 60;
            fireKeyText.Width = 500;
            fireKeyText.Height = 40;
            fireKeyText.HorizontalAlign = HorizontalTextAlign.Center;
            fireKeyText.VerticalAlign = VerticalTextAlign.Middle;
            fireKeyText.AdjustAreaWithText = false;
            fireKeyText.Visible = false;
            fireKeyText.TweenScaleBounce(1, 1.01f, 500, ScaleFuncs.CubicEaseInOut);
        }
        private async Task InitializeUIMinimap()
        {
            miniMapBackground = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Compass.png"), SceneObjectUsages.UI, layerUI);
            miniMapBackground.Width = 200;
            miniMapBackground.Height = 200;
            miniMapBackground.Left = this.Game.Form.RenderWidth - 200 - 10;
            miniMapBackground.Top = this.Game.Form.RenderHeight - 200 - 10;
            miniMapBackground.Alpha = 0.5f;
            miniMapBackground.Visible = false;

            miniMapTank1 = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Tank.png"), SceneObjectUsages.UI, layerUI + 1);
            miniMapTank1.Width = 273 * 0.1f;
            miniMapTank1.Height = 365 * 0.1f;
            miniMapTank1.Left = this.Game.Form.RenderWidth - 150 - 10;
            miniMapTank1.Top = this.Game.Form.RenderHeight - 150 - 10;
            miniMapTank1.TintColor = Color.Blue;
            miniMapTank1.Visible = false;

            miniMapTank2 = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Tank.png"), SceneObjectUsages.UI, layerUI + 1);
            miniMapTank2.Width = 273 * 0.1f;
            miniMapTank2.Height = 365 * 0.1f;
            miniMapTank2.Left = this.Game.Form.RenderWidth - 85 - 10;
            miniMapTank2.Top = this.Game.Form.RenderHeight - 85 - 10;
            miniMapTank2.TintColor = Color.Red;
            miniMapTank2.Visible = false;

            windVelocity = await this.AddComponentUIProgressBar(UIProgressBarDescription.DefaultFromFile(fontFilename, 8), layerUI + 2);
            windVelocity.Caption.Text = "Wind velocity";
            windVelocity.Caption.TextColor = Color.Yellow * 0.85f;
            windVelocity.Width = 180;
            windVelocity.Height = 15;
            windVelocity.Left = miniMapBackground.AbsoluteCenter.X - 90;
            windVelocity.Top = miniMapBackground.AbsoluteCenter.Y - 130;
            windVelocity.ProgressColor = Color.DeepSkyBlue;
            windVelocity.Visible = false;

            windDirection = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Wind.png"), SceneObjectUsages.UI, layerUI + 1);
            windDirection.Width = 100;
            windDirection.Height = 100;
            windDirection.Left = miniMapBackground.AbsoluteCenter.X - 50;
            windDirection.Top = miniMapBackground.AbsoluteCenter.Y - 50;
            windDirection.TintColor = Color.Green;
            windDirection.Visible = false;
        }
        private async Task InitializeUIShotPath()
        {
            trajectoryMarkerPool = new Sprite[100];
            for (int i = 0; i < 100; i++)
            {
                var trajectoryMarker = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Dot.png"), SceneObjectUsages.UI, layerUI + 1);
                trajectoryMarker.Width = 50;
                trajectoryMarker.Height = 50;
                trajectoryMarker.TintColor = Color.Red;
                trajectoryMarker.Active = false;
                trajectoryMarker.Visible = false;
                trajectoryMarker.TweenRotateRepeat(0, MathUtil.TwoPi, 1000, ScaleFuncs.Linear);

                trajectoryMarkerPool[i] = trajectoryMarker;
            }

            targetMarker = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Dot.png"), SceneObjectUsages.UI, layerUI + 1);
            targetMarker.Width = 60;
            targetMarker.Height = 60;
            targetMarker.Active = false;
            targetMarker.Visible = false;
            targetMarker.TweenScaleBounce(1f, 1.5f, 1000, ScaleFuncs.QuadraticEaseInOut);
        }
        private void PrepareUI()
        {

        }

        private Task[] InitializeModels()
        {
            return new Task[]
            {
                InitializeModelsTanks(),
                InitializeModelsTerrain(),
                InitializeLandscape(),
            };
        }
        private async Task InitializeModelsTanks()
        {
            var tDesc = new ModelInstancedDescription()
            {
                Name = "Tanks",
                CastShadow = true,
                Optimize = false,
                Content = new ContentDescription()
                {
                    ContentFolder = "SceneTanksGame/Leopard",
                    ModelContentFilename = "Leopard.xml",
                },
                Instances = 2,
                TransformNames = new[] { "Barrel-mesh", "Turret-mesh", "Hull-mesh" },
                TransformDependences = new[] { 1, 2, -1 },
            };

            tanks = await this.AddComponentModelInstanced(tDesc, SceneObjectUsages.Agent, layerModels);
            tanks.Visible = false;

            tankHeight = tanks[0].GetBoundingBox().GetY() * 0.5f;
        }
        private async Task InitializeModelsTerrain()
        {
            terrain = await this.AddComponentScenery(GroundDescription.FromFile("SceneTanksGame/Terrain/terrain.xml"), SceneObjectUsages.Ground, layerModels);
            terrain.Visible = false;

            this.SetGround(terrain, true);
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

            var material = MaterialContent.Default;
            material.DiffuseTexture = "SceneTanksGame/Landscape.png";

            var content = ModelDescription.FromData(vertices, indices, material);

            landScape = await this.AddComponentModel(content, SceneObjectUsages.UI, layerModels);
            landScape.Visible = false;
        }
        private void PrepareModels()
        {
            this.Camera.Position = new Vector3(0, 100, -200) * (1000f / this.Game.Form.RenderWidth);
            this.Camera.Interest = new Vector3(0, 0, 100);

            landScape.Visible = true;
            terrain.Visible = true;

            Vector3 p1 = new Vector3(-100, 100, 0);
            Vector3 n1 = Vector3.Up;
            Vector3 p2 = new Vector3(+100, 100, 0);
            Vector3 n2 = Vector3.Up;

            if (this.FindTopGroundPosition(-100, 100, out var r1))
            {
                p1 = r1.Position - (Vector3.Up * 0.1f);
                n1 = r1.Item.Normal;
            }
            if (this.FindTopGroundPosition(+100, 100, out var r2))
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
                Color = Color.Blue,
            };

            player2Status = new PlayerStatus
            {
                Name = "Player 2",
                Points = 0,
                MaxLife = 100,
                CurrentLife = 100,
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

            miniMapBackground.Visible = visible;
            miniMapTank1.Visible = visible;
            miniMapTank2.Visible = visible;
            windVelocity.Visible = visible;
            windDirection.Visible = visible;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            UpdateTurnStatus();
            UpdateWindVelocity();
            UpdatePlayersStatus();

            if (shooting)
            {
                return;
            }

            if (gameEnding)
            {
                UpdateInputEndGame();

                return;
            }

            UpdateInputTanks(gameTime);
            UpdateInputShoot(gameTime);
        }
        private void UpdateTurnStatus()
        {
            turnText.Text = $"Turn {currentTurn}";

            if (currentPlayer == 0)
            {
                playerTurnMarker.Left = this.Game.Form.RenderCenter.X - 112 - 120;
                playerTurnMarker.Rotation = 0;
            }
            else
            {
                playerTurnMarker.Left = this.Game.Form.RenderCenter.X + 120;
                playerTurnMarker.Rotation = MathUtil.Pi;
            }
        }
        private void UpdateWindVelocity()
        {
            windVelocity.ProgressValue = currentWindVelocity / maxWindVelocity;

            windDirection.Rotation = Helper.AngleSigned(Vector2.UnitY, windForce);
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
        private void UpdateInputTanks(GameTime gameTime)
        {
            bool tankMoved = false;

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                Shooter.Manipulator.Rotate(-gameTime.ElapsedSeconds, 0, 0);
                tankMoved = true;
            }
            if (this.Game.Input.KeyPressed(Keys.D))
            {
                Shooter.Manipulator.Rotate(+gameTime.ElapsedSeconds, 0, 0);
                tankMoved = true;
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                Shooter.Manipulator.MoveForward(gameTime, 10);
                tankMoved = true;
            }
            if (this.Game.Input.KeyPressed(Keys.S))
            {
                Shooter.Manipulator.MoveBackward(gameTime, 10);
                tankMoved = true;
            }

            if (this.Game.Input.KeyPressed(Keys.Q))
            {
                Shooter["Barrel-mesh"].Manipulator.Rotate(0, gameTime.ElapsedSeconds, 0);
                tankMoved = true;
            }
            if (this.Game.Input.KeyPressed(Keys.Z))
            {
                Shooter["Barrel-mesh"].Manipulator.Rotate(0, -gameTime.ElapsedSeconds, 0);
                tankMoved = true;
            }

            Shooter["Turret-mesh"].Manipulator.RotateTo(Target.Manipulator.Position, Vector3.Up, Axis.Y, 0.01f);

            if (!tankMoved)
            {
                return;
            }

            if (this.FindTopGroundPosition(Shooter.Manipulator.Position.X, Shooter.Manipulator.Position.Z, out var r))
            {
                Shooter.Manipulator.SetPosition(r.Position - (Vector3.Up * 0.1f));
                Shooter.Manipulator.SetNormal(r.Item.Normal, 0.05f);
            }

            PaintShot();
        }
        private void UpdateInputShoot(GameTime gameTime)
        {
            if (this.Game.Input.KeyPressed(Keys.Space))
            {
                pbFire.ProgressValue += gameTime.ElapsedSeconds;
                pbFire.ProgressValue %= 1f;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                shooting = true;

                Task.Run(async () =>
                {
                    PlayerStatus shooter = currentPlayer == 0 ? player1Status : player2Status;
                    PlayerStatus target = currentPlayer == 0 ? player2Status : player1Status;

                    await ResolveShoot(shooter, target, pbFire.ProgressValue);

                    await EvaluateTurn(shooter, target);

                    shooting = false;
                });
            }
        }
        private void UpdateInputEndGame()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                this.Game.SetScene<SceneStart>();
            }
        }

        private void PaintShot()
        {
            Vector3 from = Shooter.Manipulator.Position;
            from.Y += tankHeight;

            Vector3 to = Target.Manipulator.Position;
            to.Y += tankHeight;

            trajectoryMarkerPool.ToList().ForEach(m =>
            {
                m.Active = false;
                m.Visible = false;
            });

            targetMarker.Active = false;
            targetMarker.Visible = false;

            float sampleDist = 20;
            float distance = Vector3.Distance(from, to);
            Vector3 shootDirection = Vector3.Normalize(to - from);
            int markers = Math.Min(trajectoryMarkerPool.Length, (int)(distance / sampleDist));
            if (markers == 0)
            {
                return;
            }

            // Distribute sample dist
            sampleDist = distance / markers;

            // Initialize sample dist
            float dist = sampleDist;
            for (int i = 0; i < markers - 1; i++)
            {
                Vector3 markerPos = from + (shootDirection * dist);
                Vector3 screenPos = Vector3.Project(markerPos,
                    this.Game.Graphics.Viewport.X,
                    this.Game.Graphics.Viewport.Y,
                    this.Game.Graphics.Viewport.Width,
                    this.Game.Graphics.Viewport.Height,
                    this.Game.Graphics.Viewport.MinDepth,
                    this.Game.Graphics.Viewport.MaxDepth,
                    this.Camera.View * this.Camera.Projection);
                float scale = (1f - screenPos.Z) * 1000f;

                trajectoryMarkerPool[i].Left = screenPos.X - (trajectoryMarkerPool[i].Width * 0.5f);
                trajectoryMarkerPool[i].Top = screenPos.Y - (trajectoryMarkerPool[i].Height * 0.5f);
                trajectoryMarkerPool[i].Scale = scale;
                trajectoryMarkerPool[i].Active = true;
                trajectoryMarkerPool[i].Visible = true;

                dist += sampleDist;
            }

            var targetScreenPos = Vector3.Project(to,
                this.Game.Graphics.Viewport.X,
                this.Game.Graphics.Viewport.Y,
                this.Game.Graphics.Viewport.Width,
                this.Game.Graphics.Viewport.Height,
                this.Game.Graphics.Viewport.MinDepth,
                this.Game.Graphics.Viewport.MaxDepth,
                this.Camera.View * this.Camera.Projection);
            float targetScale = (1f - targetScreenPos.Z) * 1000f;

            targetMarker.Left = targetScreenPos.X - (targetMarker.Width * 0.5f);
            targetMarker.Top = targetScreenPos.Y - (targetMarker.Height * 0.5f);
            targetMarker.Scale = targetScale;
            targetMarker.Active = true;
            targetMarker.Visible = true;
        }
        private async Task ResolveShoot(PlayerStatus shooter, PlayerStatus target, float shotForce)
        {
            await Task.Delay(2000);

            if (Helper.RandomGenerator.NextFloat(0, 1) < 0.4f)
            {
                return;
            }

            int res = Helper.RandomGenerator.Next((int)(shotForce * 10), (int)(shotForce * 50));

            shooter.Points += res * 100;

            target.CurrentLife = MathUtil.Clamp(target.CurrentLife - res, 0, target.MaxLife);
        }
        private async Task EvaluateTurn(PlayerStatus shooter, PlayerStatus target)
        {
            pbFire.ProgressValue = 0;

            if (target.CurrentLife == 0)
            {
                gameEnding = true;

                gameMessage.Text = $"The winner is {shooter.Name}!";
                gameMessage.TextColor = shooter.Color;
                gameMessage.TextShadowColor = shooter.Color * 0.5f;
                gameMessage.Show(1000);
                gameMessage.TweenScale(0, 1, 1000, ScaleFuncs.CubicEaseIn);

                fadePanel.Show(3000);

                await Task.Delay(3000);

                gameKeyHelp.Show(1000);
                gameKeyHelp.TweenScaleBounce(1, 1.01f, 500, ScaleFuncs.CubicEaseInOut);
            }

            currentPlayer++;
            currentPlayer %= 2;

            PaintShot();

            if (currentPlayer == 0)
            {
                currentTurn++;

                currentWindVelocity = Helper.RandomGenerator.NextFloat(0f, maxWindVelocity);
                windForce = Helper.RandomGenerator.NextVector2(-Vector2.One, Vector2.One);
            }
        }
    }

    public class PlayerStatus
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public int MaxLife { get; set; }
        public int CurrentLife { get; set; }
        public Color Color { get; set; }
        public float Health
        {
            get
            {
                return (float)CurrentLife / MaxLife;
            }
        }
        public uint TextureIndex
        {
            get
            {
                if (Health > 0.6666f)
                {
                    return 0;
                }
                else if (Health > 0)
                {
                    return 1;
                }

                return 2;
            }
        }
    }
}
