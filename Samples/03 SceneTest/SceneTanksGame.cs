using Engine;
using Engine.Tween;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private bool gameReady = false;

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
        private int currentPlayer = 1;
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

        private Sprite landScape;
        private Terrain terrain;
        private ModelInstanced tanks;
        private ModelInstanced trees;
        private ModelInstanced rocks;

        private Sprite trajectoryMarker;
        private Sprite tarjetMarker;

        private UIMinimap miniMap;
        private Vector2 windForce;
        private UIProgressBar windVelocity;
        private Sprite windDirection;
        private Sprite miniMapTank1;
        private Sprite miniMapTank2;

        private UITextArea loadingText;
        private UIProgressBar loadingBar;
        private float progressValue = 0;
        private UIPanel fadePanel;

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

        public async Task LoadLoadingUI()
        {
            await this.LoadResourcesAsync(
                InitializeLoadingUI(),
                async () =>
                {
                    fadePanel.Color = Color.Black;
                    fadePanel.Visible = true;

                    loadingText.Text = "Please wait...";
                    loadingText.Visible = true;
                    loadingText.TweenAlphaBounce(0, 1, 1000, ScaleFuncs.CubicEaseInOut);

                    loadingBar.ProgressValue = 0;
                    loadingBar.Visible = true;

                    await this.LoadUI();
                });
        }
        private async Task InitializeLoadingUI()
        {
            fadePanel = await this.AddComponentUIPanel(UIPanelDescription.Screen(this), layerLoadingUI);
            fadePanel.Visible = false;

            loadingText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile("SceneTanksGame/LeagueSpartan-Bold.otf", 40, true), layerLoadingUI + 1);
            loadingText.TextColor = Color.Yellow;
            loadingText.TextShadowColor = Color.Orange;
            loadingText.CenterHorizontally = CenterTargets.Screen;
            loadingText.Top = this.Game.Form.RenderCenter.Y - 75f;
            loadingText.Visible = false;

            loadingBar = await this.AddComponentUIProgressBar(UIProgressBarDescription.ScreenCentered("SceneTanksGame/LeagueSpartan-Bold.otf", 20, true), layerLoadingUI + 1);
            loadingBar.Width = this.Game.Form.RenderWidth * 0.8f;
            loadingBar.Height = 35;
            loadingBar.ProgressColor = Color.Yellow;
            loadingBar.BaseColor = Color.CornflowerBlue;
            loadingBar.TextColor = Color.Black;
            loadingBar.Text = "0%";
            loadingBar.Visible = false;
        }

        public async Task LoadUI()
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

                    fadePanel.ClearTween();
                    fadePanel.Hide(5000);
                    loadingText.ClearTween();
                    loadingText.Hide(1000);
                    loadingBar.ClearTween();
                    loadingBar.Hide(1000);

                    gameReady = true;
                });
        }
        private Task[] InitializeUI()
        {
            return new[]
            {
                InitializeUIPlayers(),
                InitializeUITurn(),
            };
        }
        private async Task InitializeUIPlayers()
        {
            player1Name = await this.AddComponentUITextArea(UITextAreaDescription.FromFile("SceneTanksGame/LeagueSpartan-Bold.otf", 20, true), layerUI);
            player1Name.TextColor = player1Status.Color;
            player1Name.TextShadowColor = player1Status.Color * 0.5f;
            player1Name.JustifyText(TextAlign.Left);
            player1Name.Width = 200;
            player1Name.Top = 10;
            player1Name.Left = 10;
            player1Name.Visible = true;

            player1Points = await this.AddComponentUITextArea(UITextAreaDescription.FromFile("SceneTanksGame/LeagueSpartan-Bold.otf", 25, true), layerUI);
            player1Points.TextColor = player1Status.Color;
            player1Points.TextShadowColor = player1Status.Color * 0.5f;
            player1Points.JustifyText(TextAlign.Center);
            player1Points.Width = 200;
            player1Points.Top = 60;
            player1Points.Left = 10;
            player1Points.Visible = true;

            player1Life = await this.AddComponentUIProgressBar(UIProgressBarDescription.ScreenCentered("SceneTanksGame/LeagueSpartan-Bold.otf", 10, true), layerUI);
            player1Life.Width = 200;
            player1Life.Height = 30;
            player1Life.Top = 100;
            player1Life.Left = 10;
            player1Life.ProgressColor = player1Status.Color;
            player1Life.BaseColor = Color.Black;
            player1Life.TextColor = Color.White;
            player1Life.Text = "0%";
            player1Life.Visible = true;

            player2Name = await this.AddComponentUITextArea(UITextAreaDescription.FromFile("SceneTanksGame/LeagueSpartan-Bold.otf", 20, true), layerUI);
            player2Name.TextColor = player2Status.Color;
            player2Name.TextShadowColor = player2Status.Color * 0.5f;
            player2Name.JustifyText(TextAlign.Right);
            player2Name.Width = 200;
            player2Name.Top = 10;
            player2Name.Left = this.Game.Form.RenderWidth - 10 - player2Name.Width;
            player2Name.Visible = true;

            player2Points = await this.AddComponentUITextArea(UITextAreaDescription.FromFile("SceneTanksGame/LeagueSpartan-Bold.otf", 25, true), layerUI);
            player2Points.TextColor = player2Status.Color;
            player2Points.TextShadowColor = player2Status.Color * 0.5f;
            player2Points.JustifyText(TextAlign.Center);
            player2Points.Width = 200;
            player2Points.Top = 60;
            player2Points.Left = this.Game.Form.RenderWidth - 10 - player2Points.Width;
            player2Points.Visible = true;

            player2Life = await this.AddComponentUIProgressBar(UIProgressBarDescription.ScreenCentered("SceneTanksGame/LeagueSpartan-Bold.otf", 10, true), layerUI);
            player2Life.Width = 200;
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
            turnText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile("SceneTanksGame/LeagueSpartan-Bold.otf", 40, true), layerUI);
            turnText.TextColor = Color.Yellow;
            turnText.TextShadowColor = Color.Yellow * 0.5f;
            turnText.JustifyText(TextAlign.Center);
            turnText.CenterHorizontally = CenterTargets.Screen;
            turnText.Width = 200;
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

        private Task[] InitializeModels()
        {
            return new Task[] { };
        }

        private void PrepareUI()
        {

        }
        private void PrepareModels()
        {

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

            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                currentPlayer = 0;
            }
            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                currentPlayer = 1;
            }

            UpdateTurnStatus();
            UpdatePlayersStatus();
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
        private void UpdatePlayersStatus()
        {
            player1Name.Text = player1Status.Name;
            player1Points.Text = $"{player1Status.Points} points";
            player1Life.Text = $"{player1Status.CurrentLife}";
            player1Life.ProgressValue = player1Status.Health;

            player2Name.Text = player2Status.Name;
            player2Points.Text = $"{player2Status.Points} points";
            player2Life.Text = $"{player2Status.CurrentLife}";
            player2Life.ProgressValue = player2Status.Health;
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
                return (int)((float)CurrentLife / MaxLife);
            }
        }
    }
}
