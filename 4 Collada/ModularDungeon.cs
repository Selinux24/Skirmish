using Engine;
using SharpDX;

namespace Collada
{
    public class ModularDungeon : Scene
    {
        private const int layerHUD = 99;

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> fps = null;
        private SceneObject<TextDrawer> picks = null;
        private SceneObject<Sprite> backPannel = null;

        private SceneObject<ModelInstanced> room1 = null;
        private SceneObject<ModelInstanced> room2 = null;
        private SceneObject<ModelInstanced> corridor1 = null;

        public ModularDungeon(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsageEnum.UI, layerHUD);
            this.title.Instance.Text = "Collada Dungeon Scene";
            this.title.Instance.Position = Vector2.Zero;

            this.fps = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.fps.Instance.Text = null;
            this.fps.Instance.Position = new Vector2(0, 24);

            this.picks = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.picks.Instance.Text = null;
            this.picks.Instance.Position = new Vector2(0, 48);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.picks.Instance.Top + this.picks.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHUD - 1);

            this.room1 = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "room1",
                    Instances = 2,
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources",
                        ModelContentFilename = "Room1.xml",
                    }
                });

            this.room2 = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "room2",
                    Instances = 6,
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources",
                        ModelContentFilename = "Room2.xml",
                    }
                });

            this.corridor1 = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "corridor1",
                    Instances = 8,
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources",
                        ModelContentFilename = "Corridor1.xml",
                    }
                });

            this.InitializeDungeon();
            this.InitializeCamera();
            this.InitializeEnvironment();
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Mode = CameraModes.Free;
            this.Camera.Position = new Vector3(20, 20, 20);
            this.Camera.Interest = new Vector3(0, 0, 0);
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.DarkGray;
        }
        private void InitializeDungeon()
        {
            BoundingBox bbox = this.room1.Instance[0].GetBoundingBox();

            float x = bbox.GetX();
            float z = bbox.GetZ();

            this.room1.Instance[0].Manipulator.SetPosition(new Vector3(+0 * x, 0, +0 * z));
            this.room1.Instance[1].Manipulator.SetPosition(new Vector3(-2 * x, 0, +0 * z));

            this.room2.Instance[0].Manipulator.SetPosition(new Vector3(-4 * x, 0, +0 * z));
            this.room2.Instance[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);
            this.room2.Instance[1].Manipulator.SetPosition(new Vector3(+3 * x, 0, +0 * z));
            this.room2.Instance[1].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            this.room2.Instance[2].Manipulator.SetPosition(new Vector3(+0 * x, 0, -2 * z));
            this.room2.Instance[2].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.room2.Instance[3].Manipulator.SetPosition(new Vector3(+0 * x, 0, +2 * z));
            this.room2.Instance[4].Manipulator.SetPosition(new Vector3(-2 * x, 0, -2 * z));
            this.room2.Instance[4].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.room2.Instance[5].Manipulator.SetPosition(new Vector3(-2 * x, 0, +2 * z));

            this.corridor1.Instance[0].Manipulator.SetPosition(new Vector3(-1 * x, 0, 0));
            this.corridor1.Instance[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            this.corridor1.Instance[1].Manipulator.SetPosition(new Vector3(-3 * x, 0, 0));
            this.corridor1.Instance[1].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            this.corridor1.Instance[2].Manipulator.SetPosition(new Vector3(+1 * x, 0, 0));
            this.corridor1.Instance[2].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            this.corridor1.Instance[3].Manipulator.SetPosition(new Vector3(+2 * x, 0, 0));
            this.corridor1.Instance[3].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            this.corridor1.Instance[4].Manipulator.SetPosition(new Vector3(+0 * x, 0, -1 * z));
            this.corridor1.Instance[5].Manipulator.SetPosition(new Vector3(+0 * x, 0, +1 * z));
            this.corridor1.Instance[6].Manipulator.SetPosition(new Vector3(-2 * x, 0, -1 * z));
            this.corridor1.Instance[7].Manipulator.SetPosition(new Vector3(-2 * x, 0, +1 * z));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            this.UpdateCamera(gameTime);

            this.fps.Instance.Text = this.Game.RuntimeText;
        }
        private void UpdateCamera(GameTime gameTime)
        {
            bool slow = this.Game.Input.KeyPressed(Keys.LShiftKey);

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
        }
    }
}
