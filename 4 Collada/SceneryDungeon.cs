using Engine;
using Engine.PathFinding.NavMesh;
using SharpDX;

namespace Collada
{
    public class SceneryDungeon : Scene
    {
        private const int layerHUD = 99;

        private TextDrawer title = null;
        private TextDrawer fps = null;
        private TextDrawer picks = null;
        private Sprite backPannel = null;

        private Scenery dungeon = null;
        private Player agent = null;

        public SceneryDungeon(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White), layerHUD);
            this.title.Text = "Collada Dungeon Scene";
            this.title.Position = Vector2.Zero;

            this.fps = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), layerHUD);
            this.fps.Text = null;
            this.fps.Position = new Vector2(0, 24);

            this.picks = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), layerHUD);
            this.picks.Text = null;
            this.picks.Position = new Vector2(0, 48);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.picks.Top + this.picks.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddSprite(spDesc, layerHUD - 1);

            this.agent = new Player()
            {
                Name = "Player",
                Height = 1,
                MaxClimb = 1,
                Radius = 0.35f,
                Velocity = 4f,
                VelocitySlow = 1f,
            };

            this.dungeon = this.AddScenery(
                "Resources",
                "Dungeon.xml",
                new GroundDescription()
                {
                    Name = "room1",
                    PathFinder = new GroundDescription.PathFinderDescription()
                    {
                        Settings = new NavigationMeshGenerationSettings()
                        {
                            Agents = new[] { agent },
                        }
                    },
                    Quadtree = new GroundDescription.QuadtreeDescription()
                    {
                        MaximumDepth = 2,
                    },
                });

            this.Lights.AddRange(this.dungeon.Lights);

            this.InitializeCamera();
            this.InitializeEnvironment();
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.MovementDelta = this.agent.Velocity;
            this.Camera.SlowMovementDelta = this.agent.VelocitySlow;
            this.Camera.Mode = CameraModes.Free;
            this.Camera.Position = new Vector3(0, this.agent.Height, 0);
            this.Camera.Interest = new Vector3(0, 1, 1);
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.DarkGray;

            this.Lights.KeyLight.Enabled = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            this.UpdateCamera(gameTime);

            this.fps.Text = this.Game.RuntimeText;
        }
        private void UpdateCamera(GameTime gameTime)
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
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            Vector3 walkerPos;
            if (this.dungeon.Walk(this.agent, prevPos, this.Camera.Position, out walkerPos))
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
