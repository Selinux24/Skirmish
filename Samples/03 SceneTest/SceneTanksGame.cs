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

        private UITextArea player1Name;
        private UITextArea player1Points;
        private UIProgressBar player1Life;
        private PlayerStatus player1Status;

        private UITextArea player2Name;
        private UITextArea player2Points;
        private UIProgressBar player2Life;
        private PlayerStatus player2Status;

        private UITextArea turnText;
        private int currentTurn;
        private Sprite gameIcon;
        private int currentPlayer;
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
            fadePanel = await this.AddComponentUIPanel(UIPanelDescription.Screen(this));
            fadePanel.Visible = false;

            loadingText = await this.AddComponentUITextArea(UITextAreaDescription.FromFile("SceneTanksGame/LeagueSpartan-Bold.otf", 20));
            loadingText.TextColor = Color.Yellow;
            loadingText.TextShadowColor = Color.Orange;
            loadingText.CenterHorizontally = CenterTargets.Screen;
            loadingText.Top = this.Game.Form.RenderCenter.Y - 75f;
            loadingText.Visible = false;

            loadingBar = await this.AddComponentUIProgressBar(UIProgressBarDescription.ScreenCentered("SceneTanksGame/LeagueSpartan-Bold.otf", 10));
            loadingBar.ProgressColor = Color.Yellow;
            loadingBar.BackgroundColor = Color.CornflowerBlue;
            loadingBar.Visible = false;
        }

        public async Task LoadUI()
        {
            await this.LoadResourcesAsync(
                new[] { InitializeUI(), InitializeModels() },
                () =>
                {
                    this.PrepareUI();
                    this.PrepareModels();

                    fadePanel.ClearTween();
                    fadePanel.Hide(5000);
                    loadingText.ClearTween();
                    loadingText.Hide(3000);
                    loadingBar.ClearTween();
                    loadingBar.Hide(3000);
                });
        }
        private async Task InitializeUI()
        {
            await Task.Delay(1000);
        }
        private async Task InitializeModels()
        {
            await Task.Delay(5000);
        }

        private void PrepareUI()
        {

        }
        private void PrepareModels()
        {

        }
    }

    public class PlayerStatus
    {

    }
}
