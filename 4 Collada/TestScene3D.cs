using Engine;
using SharpDX;

namespace Collada
{
    public class TestScene3D : Scene
    {
        private TextDrawer title = null;
        private TextDrawer fps = null;
        private TextDrawer picks = null;
        private ModelInstanced dungeon = null;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White));
            this.title.Text = "Collada Dungeon Scene";
            this.title.Position = Vector2.Zero;

            this.fps = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow));
            this.fps.Text = null;
            this.fps.Position = new Vector2(0, 24);

            this.picks = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow));
            this.picks.Text = null;
            this.picks.Position = new Vector2(0, 48);

            this.dungeon = this.AddInstancingModel("Resources",
                "dungeon.xml",
                new ModelInstancedDescription()
                {
                    Name = "Torchs",
                    Instances = 9,
                    CastShadow = true,
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

            BoundingSphere sph = this.dungeon.Instances[0].GetBoundingSphere();
            sph.Radius *= 5;
            this.SceneVolume = sph;
        }
        private void InitializeDungeon()
        {
            BoundingBox bbox = this.dungeon.Instances[0].GetBoundingBox();

            float x = bbox.GetX();
            float z = bbox.GetZ();

            int index = 0;

            this.dungeon.Instances[index++].Manipulator.SetPosition(new Vector3(0, 0, 0));
            this.dungeon.Instances[index++].Manipulator.SetPosition(new Vector3(-x, 0, 0));
            this.dungeon.Instances[index++].Manipulator.SetPosition(new Vector3(-2 * x, 0, 0));
            this.dungeon.Instances[index++].Manipulator.SetPosition(new Vector3(x, 0, 0));
            this.dungeon.Instances[index++].Manipulator.SetPosition(new Vector3(2 * x, 0, 0));
            this.dungeon.Instances[index++].Manipulator.SetPosition(new Vector3(0, 0, -z));
            this.dungeon.Instances[index++].Manipulator.SetPosition(new Vector3(0, 0, -2 * z));
            this.dungeon.Instances[index++].Manipulator.SetPosition(new Vector3(0, 0, z));
            this.dungeon.Instances[index++].Manipulator.SetPosition(new Vector3(0, 0, 2 * z));
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
