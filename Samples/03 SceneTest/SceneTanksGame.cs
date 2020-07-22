using Engine;
using Engine.Content;
using Engine.Tween;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        private UIMinimap miniMap;
        private Sprite miniMapTank1;
        private Sprite miniMapTank2;
        private float maxWindVelocity = 10;
        private float currentWindVelocity = 1;
        private Vector2 windForce = Vector2.Normalize(Vector2.One);
        private UIProgressBar windVelocity;
        private Sprite windDirection;

        private Sprite landScape;
        private Scenery terrain;
        private ModelInstanced tanks;

        private Sprite trajectoryMarker;
        private Sprite tarjetMarker;

        private bool shooting = false;
        private bool gameEnding = false;

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
                loadingBar.Text = $"{(int)(progressValue * 100f)}%";
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
                    fadePanel.Color = Color.Black;
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
            loadingText.Visible = false;

            loadingBar = await this.AddComponentUIProgressBar(UIProgressBarDescription.ScreenCentered(fontFilename, 20, true), layerLoadingUI + 1);
            loadingBar.Width = this.Game.Form.RenderWidth * 0.8f;
            loadingBar.Height = 35;
            loadingBar.ProgressColor = Color.Yellow;
            loadingBar.BaseColor = Color.CornflowerBlue;
            loadingBar.TextColor = Color.Black;
            loadingBar.Text = "0%";
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
                    this.Camera.Position = new Vector3(0, 100, -200) * 0.75f;
                    this.Camera.Interest = Vector3.Zero;

                    this.PrepareUI();
                    this.PrepareModels();

                    Task.Run(async () =>
                    {
                        loadingText.ClearTween();
                        loadingText.Hide(500);
                        loadingBar.ClearTween();
                        loadingBar.Hide(500);

                        await Task.Delay(1000);

                        gameMessage.Text = "Ready!";
                        gameMessage.TweenScale(0, 1, 500, ScaleFuncs.CubicEaseIn);
                        gameMessage.Show(500);

                        await Task.Delay(2000);

                        gameMessage.ClearTween();
                        gameMessage.Hide(100);
                        fadePanel.ClearTween();
                        fadePanel.Hide(2000);

                        gameReady = true;
                    });
                });
        }

        private Task[] InitializeUI()
        {
            return new[]
            {
                InitializeUIPlayers(),
                InitializeUITurn(),
                InitializeUIKeyPanel(),
                InitializeUIFire(),
                InitializeUIMinimap(),
                InitializeUIGameMessages(),
            };
        }
        private async Task InitializeUIGameMessages()
        {
            gameMessage = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 120, false), layerLoadingUI);
            gameMessage.Width = this.Game.Form.RenderWidth;
            gameMessage.Height = this.Game.Form.RenderHeight;
            gameMessage.TextColor = Color.Yellow;
            gameMessage.TextShadowColor = Color.Yellow * 0.5f;
            gameMessage.Text = " ";
            gameMessage.Visible = false;
        }
        private async Task InitializeUIPlayers()
        {
            float playerWidth = 300;

            player1Name = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 20, true), layerUI);
            player1Name.TextColor = player1Status.Color;
            player1Name.TextShadowColor = player1Status.Color * 0.5f;
            player1Name.JustifyText(TextAlign.Left);
            player1Name.Width = playerWidth;
            player1Name.Top = 10;
            player1Name.Left = 10;
            player1Name.Visible = true;

            player1Points = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 25, true), layerUI);
            player1Points.TextColor = player1Status.Color;
            player1Points.TextShadowColor = player1Status.Color * 0.5f;
            player1Points.JustifyText(TextAlign.Center);
            player1Points.Width = playerWidth;
            player1Points.Top = 60;
            player1Points.Left = 10;
            player1Points.Visible = true;

            player1Life = await this.AddComponentUIProgressBar(UIProgressBarDescription.ScreenCentered(fontFilename, 10, true), layerUI);
            player1Life.Width = playerWidth;
            player1Life.Height = 30;
            player1Life.Top = 100;
            player1Life.Left = 10;
            player1Life.ProgressColor = player1Status.Color;
            player1Life.BaseColor = Color.Black;
            player1Life.TextColor = Color.White;
            player1Life.Text = "0%";
            player1Life.Visible = true;

            player2Name = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 20, true), layerUI);
            player2Name.TextColor = player2Status.Color;
            player2Name.TextShadowColor = player2Status.Color * 0.5f;
            player2Name.JustifyText(TextAlign.Right);
            player2Name.Width = playerWidth;
            player2Name.Top = 10;
            player2Name.Left = this.Game.Form.RenderWidth - 10 - player2Name.Width;
            player2Name.Visible = true;

            player2Points = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 25, true), layerUI);
            player2Points.TextColor = player2Status.Color;
            player2Points.TextShadowColor = player2Status.Color * 0.5f;
            player2Points.JustifyText(TextAlign.Center);
            player2Points.Width = playerWidth;
            player2Points.Top = 60;
            player2Points.Left = this.Game.Form.RenderWidth - 10 - player2Points.Width;
            player2Points.Visible = true;

            player2Life = await this.AddComponentUIProgressBar(UIProgressBarDescription.ScreenCentered(fontFilename, 10, true), layerUI);
            player2Life.Width = playerWidth;
            player2Life.Height = 30;
            player2Life.Top = 100;
            player2Life.Left = this.Game.Form.RenderWidth - 10 - player2Life.Width;
            player2Life.ProgressColor = player2Status.Color;
            player2Life.BaseColor = Color.Black;
            player2Life.TextColor = Color.White;
            player2Life.Text = "0%";
            player2Life.Visible = true;
        }
        private async Task InitializeUITurn()
        {
            turnText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 40, true), layerUI);
            turnText.TextColor = Color.Yellow;
            turnText.TextShadowColor = Color.Yellow * 0.5f;
            turnText.JustifyText(TextAlign.Center);
            turnText.CenterHorizontally = CenterTargets.Screen;
            turnText.Width = 300;
            turnText.Visible = true;

            gameIcon = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/GameIcon.png"), SceneObjectUsages.UI, layerUI);
            gameIcon.Color = Color.Yellow;
            gameIcon.Width = 92;
            gameIcon.Height = 82;
            gameIcon.Top = 55;
            gameIcon.CenterHorizontally = CenterTargets.Screen;
            gameIcon.Visible = true;
            gameIcon.TweenRotateBounce(-0.1f, 0.1f, 500, ScaleFuncs.CubicEaseInOut);

            playerTurnMarker = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Arrow.png"), SceneObjectUsages.UI, layerUI);
            playerTurnMarker.Color = Color.Turquoise;
            playerTurnMarker.Width = 112;
            playerTurnMarker.Height = 75;
            playerTurnMarker.Top = 35;
            playerTurnMarker.Left = this.Game.Form.RenderCenter.X - 112 - 120;
            playerTurnMarker.Visible = true;
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
            keyHelp.Visible = true;

            keyRotate = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Turn.png"), SceneObjectUsages.UI, layerUI + 1);
            keyRotate.Left = 0;
            keyRotate.Top = top + 25;
            keyRotate.Width = 372 * 0.25f;
            keyRotate.Height = 365 * 0.25f;
            keyRotate.Color = Color.Turquoise;
            keyRotate.Visible = true;

            keyMove = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Move.png"), SceneObjectUsages.UI, layerUI + 1);
            keyMove.Left = keyRotate.Width;
            keyMove.Top = top + 25;
            keyMove.Width = 232 * 0.25f;
            keyMove.Height = 365 * 0.25f;
            keyMove.Color = Color.Turquoise;
            keyMove.Visible = true;

            KeyPitch = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Pitch.png"), SceneObjectUsages.UI, layerUI + 1);
            KeyPitch.Left = keyRotate.Width + keyMove.Width;
            KeyPitch.Top = top + 25;
            KeyPitch.Width = 322 * 0.25f;
            KeyPitch.Height = 365 * 0.25f;
            KeyPitch.Color = Color.Turquoise;
            KeyPitch.Visible = true;

            keyRotateLeftText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyRotateLeftText.TextColor = Color.Yellow;
            keyRotateLeftText.Text = "A";
            keyRotateLeftText.Top = top + 20;
            keyRotateLeftText.Left = 10;
            keyRotateLeftText.Visible = true;

            keyRotateRightText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyRotateRightText.TextColor = Color.Yellow;
            keyRotateRightText.Text = "D";
            keyRotateRightText.Top = top + 20;
            keyRotateRightText.Left = keyRotate.Width - 30;
            keyRotateRightText.Visible = true;

            keyMoveForwardText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyMoveForwardText.TextColor = Color.Yellow;
            keyMoveForwardText.Text = "W";
            keyMoveForwardText.Top = top + 20;
            keyMoveForwardText.Left = keyMove.AbsoluteCenter.X - 5;
            keyMoveForwardText.Visible = true;

            keyMoveBackwardText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyMoveBackwardText.TextColor = Color.Yellow;
            keyMoveBackwardText.Text = "S";
            keyMoveBackwardText.Top = top + keyMove.Height + 10;
            keyMoveBackwardText.Left = keyMove.AbsoluteCenter.X - 5;
            keyMoveBackwardText.Visible = true;

            keyPitchUpText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyPitchUpText.TextColor = Color.Yellow;
            keyPitchUpText.Text = "Q";
            keyPitchUpText.Top = top + 20;
            keyPitchUpText.Left = KeyPitch.AbsoluteCenter.X - 15;
            keyPitchUpText.Visible = true;

            keyPitchDownText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 15, true), layerUI + 2);
            keyPitchDownText.TextColor = Color.Yellow;
            keyPitchDownText.Text = "Z";
            keyPitchDownText.Top = top + KeyPitch.Height + 10;
            keyPitchDownText.Left = KeyPitch.AbsoluteCenter.X + 10;
            keyPitchDownText.Visible = true;
        }
        private async Task InitializeUIFire()
        {
            pbFire = await this.AddComponentUIProgressBar(UIProgressBarDescription.Default, layerUI);
            pbFire.CenterHorizontally = CenterTargets.Screen;
            pbFire.Top = this.Game.Form.RenderHeight - 100;
            pbFire.Width = 500;
            pbFire.Height = 40;
            pbFire.ProgressColor = Color.Yellow;
            pbFire.Visible = true;

            fireKeyText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile(fontFilename, 25, true), layerUI + 2);
            fireKeyText.TextColor = Color.Yellow;
            fireKeyText.Text = "Press space to fire!";
            fireKeyText.CenterHorizontally = CenterTargets.Screen;
            fireKeyText.Top = this.Game.Form.RenderHeight - 40;
            fireKeyText.Width = 500;
            fireKeyText.Height = 40;
            fireKeyText.Visible = true;
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
            miniMapBackground.Visible = true;

            miniMapTank1 = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Tank.png"), SceneObjectUsages.UI, layerUI + 1);
            miniMapTank1.Width = 273 * 0.1f;
            miniMapTank1.Height = 365 * 0.1f;
            miniMapTank1.Left = this.Game.Form.RenderWidth - 150 - 10;
            miniMapTank1.Top = this.Game.Form.RenderHeight - 150 - 10;
            miniMapTank1.Color = Color.Blue;
            miniMapTank1.Visible = true;

            miniMapTank2 = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Tank.png"), SceneObjectUsages.UI, layerUI + 1);
            miniMapTank2.Width = 273 * 0.1f;
            miniMapTank2.Height = 365 * 0.1f;
            miniMapTank2.Left = this.Game.Form.RenderWidth - 85 - 10;
            miniMapTank2.Top = this.Game.Form.RenderHeight - 85 - 10;
            miniMapTank2.Color = Color.Red;
            miniMapTank2.Visible = true;

            windVelocity = await this.AddComponentUIProgressBar(UIProgressBarDescription.WithText(fontFilename, 8), layerUI + 2);
            windVelocity.Text = "Wind velocity";
            windVelocity.TextColor = Color.Yellow * 0.85f;
            windVelocity.Width = 180;
            windVelocity.Height = 15;
            windVelocity.Left = miniMapBackground.AbsoluteCenter.X - 90;
            windVelocity.Top = miniMapBackground.AbsoluteCenter.Y - 130;
            windVelocity.ProgressColor = Color.DeepSkyBlue;
            windVelocity.Visible = true;

            windDirection = await this.AddComponentSprite(SpriteDescription.FromFile("SceneTanksGame/Wind.png"), SceneObjectUsages.UI, layerUI + 1);
            windDirection.Width = 100;
            windDirection.Height = 100;
            windDirection.Left = miniMapBackground.AbsoluteCenter.X - 50;
            windDirection.Top = miniMapBackground.AbsoluteCenter.Y - 50;
            windDirection.Color = Color.Green;
            windDirection.Visible = true;
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
        }
        private async Task InitializeModelsTerrain()
        {
            terrain = await this.AddComponentScenery(GroundDescription.FromFile("SceneTanksGame/Terrain/terrain.xml"), SceneObjectUsages.Ground, layerModels);
            terrain.Visible = false;

            this.SetGround(terrain, true);
        }
        private void PrepareModels()
        {
            terrain.Visible = true;

            Vector3 p1 = new Vector3(-100, 100, 10);
            Vector3 n1 = Vector3.Up;
            Vector3 p2 = new Vector3(+100, 100, 10);
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
                return;
            }

            this.UpdateInputTanks(gameTime);
            this.UpdateInputShoot(gameTime);
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
            player1Life.Text = $"{player1Status.CurrentLife}";
            player1Life.ProgressValue = player1Status.Health;
            tanks[0].TextureIndex = player1Status.TextureIndex;

            player2Name.Text = player2Status.Name;
            player2Points.Text = $"{player2Status.Points} points";
            player2Life.Text = $"{player2Status.CurrentLife}";
            player2Life.ProgressValue = player2Status.Health;
            tanks[1].TextureIndex = player2Status.TextureIndex;
        }
        private void UpdateInputTanks(GameTime gameTime)
        {
            var tank = tanks[currentPlayer];
            var other = tanks[(currentPlayer + 1) % 2];

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                tank.Manipulator.Rotate(-gameTime.ElapsedSeconds, 0, 0);
            }
            if (this.Game.Input.KeyPressed(Keys.D))
            {
                tank.Manipulator.Rotate(+gameTime.ElapsedSeconds, 0, 0);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                tank.Manipulator.MoveForward(gameTime, 10);
            }
            if (this.Game.Input.KeyPressed(Keys.S))
            {
                tank.Manipulator.MoveBackward(gameTime, 10);
            }

            if (this.Game.Input.KeyPressed(Keys.Q))
            {
                tank["Barrel-mesh"].Manipulator.Rotate(0, gameTime.ElapsedSeconds, 0);
            }
            if (this.Game.Input.KeyPressed(Keys.Z))
            {
                tank["Barrel-mesh"].Manipulator.Rotate(0, -gameTime.ElapsedSeconds, 0);
            }

            if (this.FindTopGroundPosition(tank.Manipulator.Position.X, tank.Manipulator.Position.Z, out var r))
            {
                tank.Manipulator.SetPosition(r.Position - (Vector3.Up * 0.1f));
                tank.Manipulator.SetNormal(r.Item.Normal, 0.05f);
                tank["Turret-mesh"].Manipulator.RotateTo(other.Manipulator.Position, Vector3.Up, Axis.Y, 0.01f);
            }
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

                    await this.ResolveShoot(shooter, target);

                    pbFire.ProgressValue = 0;

                    if (target.CurrentLife == 0)
                    {
                        gameMessage.Text = $"The winner is {shooter.Name}!";
                        gameMessage.TextColor = target.Color;
                        gameMessage.TextShadowColor = target.Color * 0.5f;
                        gameMessage.Show(1000);
                        gameMessage.TweenScale(0, 1, 1000, ScaleFuncs.CubicEaseIn);

                        fadePanel.Show(3000);

                        gameEnding = true;
                    }

                    currentPlayer++;
                    currentPlayer %= 2;

                    if (currentPlayer == 0)
                    {
                        currentTurn++;

                        currentWindVelocity = Helper.RandomGenerator.NextFloat(0f, maxWindVelocity);
                        windForce = Helper.RandomGenerator.NextVector2(-Vector2.One, Vector2.One);
                    }

                    shooting = false;
                });
            }
        }

        private async Task ResolveShoot(PlayerStatus shooter, PlayerStatus target)
        {
            await Task.Delay(2000);

            if (Helper.RandomGenerator.NextFloat(0, 1) < 0.4f)
            {
                return;
            }

            int res = Helper.RandomGenerator.Next(10, 50);

            shooter.Points += res * 100;

            target.CurrentLife = MathUtil.Clamp(target.CurrentLife - res, 0, target.MaxLife);
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
