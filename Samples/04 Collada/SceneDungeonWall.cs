using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;

namespace Collada
{
    public class SceneDungeonWall : Scene
    {
        private const int layerHUD = 99;

        private SceneObject<TextDrawer> fps = null;

        private SceneObject<Model> lightEmitter = null;

        private SceneLightPoint pointLight = null;

        public SceneDungeonWall(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

#if DEBUG
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;
#else
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = true;
#endif

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

            var desc = SceneLightPointDescription.Create(new Vector3(0, 1, -1), 10f, 10f);

            this.pointLight = new SceneLightPoint("light", false, Color.White, Color.White, true, desc);

            this.Lights.Add(this.pointLight);
        }
        private void InitializeText()
        {
            var title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsages.UI, layerHUD);
            title.Instance.Text = "Tiled Wall Test Scene";
            title.Instance.Position = Vector2.Zero;

            this.fps = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.fps.Instance.Text = null;
            this.fps.Instance.Position = new Vector2(0, 24);

            var picks = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            picks.Instance.Text = null;
            picks.Instance.Position = new Vector2(0, 48);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = picks.Instance.Top + picks.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.AddComponent<Sprite>(spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private void InitializeDungeon()
        {
            var wall = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "wall",
                    Instances = 7,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/SceneDungeonWall",
                        ModelContentFilename = "wall.xml",
                    }
                });

            BoundingBox bbox = wall.Instance[0].GetBoundingBox();

            float x = bbox.GetX() * (10f / 11f);
            float z = bbox.GetZ();

            wall.Instance[0].Manipulator.SetPosition(new Vector3(+3 * x, 0, +0 * z));
            wall.Instance[1].Manipulator.SetPosition(new Vector3(+2 * x, 0, +0 * z));
            wall.Instance[2].Manipulator.SetPosition(new Vector3(+1 * x, 0, +0 * z));
            wall.Instance[3].Manipulator.SetPosition(new Vector3(+0 * x, 0, +0 * z));
            wall.Instance[4].Manipulator.SetPosition(new Vector3(-1 * x, 0, +0 * z));
            wall.Instance[5].Manipulator.SetPosition(new Vector3(-2 * x, 0, +0 * z));
            wall.Instance[6].Manipulator.SetPosition(new Vector3(-3 * x, 0, +0 * z));
        }
        private void InitializeEmitter()
        {
            MaterialContent mat = MaterialContent.Default;
            mat.EmissionColor = Color.White;

            var sphere = GeometryUtil.CreateSphere(0.05f, 16, 5);
            var vertices = VertexData.FromDescriptor(sphere);
            var indices = sphere.Indices;
            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

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
                this.Game.SetScene<SceneStart>();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            this.UpdateCamera();

            this.UpdateLight(gameTime);

            this.fps.Instance.Text = this.Game.RuntimeText;
        }
        private void UpdateCamera()
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
