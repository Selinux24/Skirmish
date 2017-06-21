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

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> fps = null;
        private SceneObject<TextDrawer> picks = null;
        private SceneObject<Sprite> backPannel = null;

        private SceneObject<ModelInstanced> wall = null;
        private SceneObject<Model> lightEmitter = null;

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
            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsageEnum.UI, layerHUD);
            this.title.Instance.Text = "Tiled Wall Test Scene";
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
        }
        private void InitializeDungeon()
        {
            this.wall = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "wall",
                    Instances = 7,
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources",
                        ModelContentFilename = "wall.xml",
                    }
                });

            BoundingBox bbox = this.wall.Instance[0].GetBoundingBox();

            float x = bbox.GetX() * (10f / 11f);
            float z = bbox.GetZ();

            this.wall.Instance[0].Manipulator.SetPosition(new Vector3(+3 * x, 0, +0 * z));
            this.wall.Instance[1].Manipulator.SetPosition(new Vector3(+2 * x, 0, +0 * z));
            this.wall.Instance[2].Manipulator.SetPosition(new Vector3(+1 * x, 0, +0 * z));
            this.wall.Instance[3].Manipulator.SetPosition(new Vector3(+0 * x, 0, +0 * z));
            this.wall.Instance[4].Manipulator.SetPosition(new Vector3(-1 * x, 0, +0 * z));
            this.wall.Instance[5].Manipulator.SetPosition(new Vector3(-2 * x, 0, +0 * z));
            this.wall.Instance[6].Manipulator.SetPosition(new Vector3(-3 * x, 0, +0 * z));
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
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            this.lightEmitter = this.AddComponent<Model>(desc);
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

            this.lightEmitter.Transform.SetPosition(pos);
            this.pointLight.Position = pos;
        }
    }
}
