using Engine;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.UI;
using SharpDX;
using System.Threading.Tasks;

namespace Collada
{
    public class SceneDungeon : Scene
    {
        private const int layerHUD = 99;

        private UITextArea fps = null;
        private Scenery dungeon = null;

        private Player agent = null;

        private bool userInterfaceInitialized = false;
        private bool gameReady = false;

        public SceneDungeon(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            await base.Initialize();

#if DEBUG
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;
#else
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = true;
#endif
            InitializeUI();

            await Task.CompletedTask;
        }
        public override void NavigationGraphUpdated()
        {
            gameReady = true;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            if (!userInterfaceInitialized)
            {
                return;
            }

            this.fps.Text = this.Game.RuntimeText;

            if (!gameReady)
            {
                return;
            }

            this.UpdateCamera();
        }

        private void InitializeUI()
        {
            _ = this.LoadResourcesAsync(
                new[] { InitializeUIComponents() },
                () =>
                {
                    userInterfaceInitialized = true;

                    this.InitializeEnvironment();

                    this.LoadGameAssets();
                });
        }
        private void LoadGameAssets()
        {
            _ = this.LoadResourcesAsync(
                new[] { InitializeDungeon() },
                () =>
                {
                    this.Lights.AddRange(this.dungeon.Lights);

                    this.agent = new Player()
                    {
                        Name = "Player",
                        Height = 0.5f,
                        Radius = 0.15f,
                        MaxClimb = 0.225f,
                    };

                    this.InitializeCamera();

                    this.SetGround(this.dungeon, true);

                    var settings = new BuildSettings()
                    {
                        Agents = new[] { agent },
                    };

                    var input = new InputGeometry(GetTrianglesForNavigationGraph);

                    this.PathFinderDescription = new PathFinderDescription(settings, input);

                    Task.WhenAll(this.UpdateNavigationGraph());
                });
        }

        private async Task InitializeUIComponents()
        {
            var title = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18, Color.White) }, layerHUD);
            title.Text = "Collada Dungeon Scene";
            title.SetPosition(Vector2.Zero);

            this.fps = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) }, layerHUD);
            this.fps.Text = null;
            this.fps.SetPosition(new Vector2(0, 24));

            var picks = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) }, layerHUD);
            picks.Text = null;
            picks.SetPosition(new Vector2(0, 48));

            var spDesc = new SpriteDescription()
            {
                Width = this.Game.Form.RenderWidth,
                Height = picks.Top + picks.Height + 3,
                TintColor = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private async Task InitializeDungeon()
        {
            this.dungeon = await this.AddComponentScenery(
                new GroundDescription()
                {
                    Name = "room1",
                    Quadtree = new GroundDescription.QuadtreeDescription()
                    {
                        MaximumDepth = 2,
                    },
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/SceneDungeon",
                        ModelContentFilename = "Dungeon.xml",
                    },
                });
        }

        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.MovementDelta = this.agent.Velocity;
            this.Camera.SlowMovementDelta = this.agent.VelocitySlow;
            this.Camera.Mode = CameraModes.Free;
            this.Camera.Position = new Vector3(0, this.agent.Height, 0);
            this.Camera.Interest = new Vector3(0, this.agent.Height, 1);
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.Black;

            this.Lights.KeyLight.Enabled = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = true;
        }

        private void UpdateCamera()
        {
            bool slow = this.Game.Input.KeyPressed(Keys.LShiftKey);

            var prevPos = this.Camera.Position;

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(this.Game.GameTime, slow);
            }

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
#else
            this.Camera.RotateMouse(
                this.Game.GameTime,
                this.Game.Input.MouseXDelta,
                this.Game.Input.MouseYDelta);
#endif

            if (this.Walk(this.agent, prevPos, this.Camera.Position, true, out Vector3 walkerPos))
            {
                this.Camera.Goto(walkerPos);
            }
            else
            {
                this.Camera.Goto(prevPos);
            }
        }
    }
}
