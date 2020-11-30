using Engine;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.UI;
using SharpDX;
using System.Threading.Tasks;

namespace Collada.Dungeon
{
    public class SceneDungeon : Scene
    {
        private readonly string resourcesFolder = "dungeon/resources";

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea fps = null;
        private UITextArea picks = null;
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
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
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

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<Start.SceneStart>();
            }

            if (!userInterfaceInitialized)
            {
                return;
            }

            fps.Text = Game.RuntimeText;

            if (!gameReady)
            {
                return;
            }

            UpdateCamera();
        }
        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }

        private void InitializeUI()
        {
            _ = LoadResourcesAsync(
                InitializeUIComponents(),
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    UpdateLayout();

                    userInterfaceInitialized = true;

                    InitializeEnvironment();

                    LoadGameAssets();
                });
        }
        private void LoadGameAssets()
        {
            _ = LoadResourcesAsync(
                InitializeDungeon(),
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    Lights.AddRange(dungeon.Lights);

                    agent = new Player()
                    {
                        Name = "Player",
                        Height = 0.5f,
                        Radius = 0.15f,
                        MaxClimb = 0.225f,
                    };

                    InitializeCamera();

                    SetGround(dungeon, true);

                    var settings = new BuildSettings()
                    {
                        Agents = new[] { agent },
                    };

                    var input = new InputGeometry(GetTrianglesForNavigationGraph);

                    PathFinderDescription = new PathFinderDescription(settings, input);

                    Task.WhenAll(UpdateNavigationGraph());
                });
        }

        private async Task InitializeUIComponents()
        {
            title = await this.AddComponentUITextArea("Title", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18), TextForeColor = Color.White });
            title.Text = "Collada Dungeon Scene";

            fps = await this.AddComponentUITextArea("FPS", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12), TextForeColor = Color.Yellow });
            fps.Text = null;

            picks = await this.AddComponentUITextArea("Picks", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12), TextForeColor = Color.Yellow });
            picks.Text = null;

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            panel = await this.AddComponentSprite("Backpanel", spDesc, SceneObjectUsages.UI, LayerUI - 1);
        }
        private async Task InitializeDungeon()
        {
            dungeon = await this.AddComponentScenery("Dungeon", GroundDescription.FromFile(resourcesFolder, "Dungeon.xml", 2));
        }

        private void InitializeCamera()
        {
            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 500;
            Camera.MovementDelta = agent.Velocity;
            Camera.SlowMovementDelta = agent.VelocitySlow;
            Camera.Mode = CameraModes.Free;
            Camera.Position = new Vector3(0, agent.Height, 0);
            Camera.Interest = new Vector3(0, agent.Height, 1);
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.Black;

            Lights.KeyLight.Enabled = false;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = true;
        }

        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            fps.SetPosition(new Vector2(0, 24));
            picks.SetPosition(new Vector2(0, 48));
            panel.Width = Game.Form.RenderWidth;
            panel.Height = picks.Top + picks.Height + 3;
        }

        private void UpdateCamera()
        {
            var prevPos = Camera.Position;

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(Game.GameTime, Game.Input.ShiftPressed);
            }

#if DEBUG
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                Game.GameTime,
                Game.Input.MouseXDelta,
                Game.Input.MouseYDelta);
#endif

            if (Walk(agent, prevPos, Camera.Position, true, out Vector3 walkerPos))
            {
                Camera.Goto(walkerPos);
            }
            else
            {
                Camera.Goto(prevPos);
            }
        }
    }
}
