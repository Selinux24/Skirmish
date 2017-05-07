using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;
using SharpDX.Direct3D;

namespace Collada
{
    public class DungeonWall : Scene
    {
        private const int layerHUD = 99;

        private TextDrawer title = null;
        private TextDrawer fps = null;
        private TextDrawer picks = null;
        private Sprite backPannel = null;

        private ModelInstanced wall = null;
        private Model lightEmitter = null;

        private SceneLightPoint pointLight = null;

        public DungeonWall(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.InitializeText();
            this.InitializeDungeon();
            this.InitializeEmitter();
            this.InitializeCamera();
            this.InitializeEnvironment();
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Mode = CameraModes.Free;
            this.Camera.Position = new Vector3(-5, 3, -5);
            this.Camera.Interest = new Vector3(0, 0, 0);
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.Black;

            this.pointLight = new SceneLightPoint("light", false, Color.White, Color.White, true, new Vector3(0, 1, -1), 10f, 10f);

            this.Lights.Add(this.pointLight);
        }
        private void InitializeText()
        {
            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White), layerHUD);
            this.title.Text = "Tiled Wall Test Scene";
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
        }
        private void InitializeDungeon()
        {
            this.wall = this.AddInstancingModel("Resources",
                "wall.xml",
                new ModelInstancedDescription()
                {
                    Name = "wall",
                    Instances = 7,
                    CastShadow = true,
                });

            BoundingBox bbox = this.wall[0].GetBoundingBox();

            float x = bbox.GetX() * (10f / 11f);
            float z = bbox.GetZ();

            this.wall[0].Manipulator.SetPosition(new Vector3(+3 * x, 0, +0 * z));
            this.wall[1].Manipulator.SetPosition(new Vector3(+2 * x, 0, +0 * z));
            this.wall[2].Manipulator.SetPosition(new Vector3(+1 * x, 0, +0 * z));
            this.wall[3].Manipulator.SetPosition(new Vector3(+0 * x, 0, +0 * z));
            this.wall[4].Manipulator.SetPosition(new Vector3(-1 * x, 0, +0 * z));
            this.wall[5].Manipulator.SetPosition(new Vector3(-2 * x, 0, +0 * z));
            this.wall[6].Manipulator.SetPosition(new Vector3(-3 * x, 0, +0 * z));
        }
        private void InitializeEmitter()
        {
            MaterialContent mat = MaterialContent.Default;
            mat.EmissionColor = Color.White;

            Vector3[] v = null;
            Vector3[] n = null;
            Vector2[] uv = null;
            uint[] ix = null;
            GeometryUtil.CreateSphere(0.05f, (uint)16, (uint)5, out v, out n, out uv, out ix);

            VertexData[] vertices = new VertexData[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vertices[i] = new VertexData()
                {
                    Position = v[i],
                    Normal = n[i],
                    Texture = uv[i],
                };
            }

            var content = ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionNormalColor, vertices, ix, mat);

            var desc = new ModelDescription()
            {
                Name = "Emitter",
                Static = false,
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                AlphaEnabled = false,
            };

            this.lightEmitter = this.AddModel(content, desc);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            this.UpdateCamera(gameTime);

            this.UpdateLight(gameTime);

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
        private void UpdateLight(GameTime gameTime)
        {
            var pos = this.pointLight.Position;

            if (this.Game.Input.KeyPressed(Keys.Left))
            {
                pos.X -= gameTime.ElapsedSeconds * 5f;
            }

            if (this.Game.Input.KeyPressed(Keys.Right))
            {
                pos.X += gameTime.ElapsedSeconds * 5f;
            }

            if (this.Game.Input.KeyPressed(Keys.Up))
            {
                pos.Z += gameTime.ElapsedSeconds * 5f;
            }

            if (this.Game.Input.KeyPressed(Keys.Down))
            {
                pos.Z -= gameTime.ElapsedSeconds * 5f;
            }

            this.lightEmitter.Manipulator.SetPosition(pos);
            this.pointLight.Position = pos;
        }
    }
}
