using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Threading.Tasks;

namespace Collada
{
    public class SceneDungeonWall : Scene
    {
        private const int layerHUD = 99;

        private TextDrawer fps = null;

        private Model lightEmitter = null;
        private SceneLightPoint pointLight = null;

        private bool gameReady = false;

        public SceneDungeonWall(Game game)
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
            _ = this.LoadResourcesAsync(
                new[] {
                this.InitializeText(),
                this.InitializeDungeon(),
                this.InitializeEmitter() },
                () =>
                {
                    this.InitializeCamera();
                    this.InitializeEnvironment();
                    this.gameReady = true;
                });
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
        private async Task InitializeText()
        {
            var title = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsages.UI, layerHUD);
            title.Text = "Tiled Wall Test Scene";
            title.Position = Vector2.Zero;

            this.fps = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.fps.Text = null;
            this.fps.Position = new Vector2(0, 24);

            var picks = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            picks.Text = null;
            picks.Position = new Vector2(0, 48);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = picks.Top + picks.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private async Task InitializeDungeon()
        {
            var wall = await this.AddComponentModelInstanced(
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

            BoundingBox bbox = wall[0].GetBoundingBox();

            float x = bbox.GetX() * (10f / 11f);
            float z = bbox.GetZ();

            wall[0].Manipulator.SetPosition(new Vector3(+3 * x, 0, +0 * z));
            wall[1].Manipulator.SetPosition(new Vector3(+2 * x, 0, +0 * z));
            wall[2].Manipulator.SetPosition(new Vector3(+1 * x, 0, +0 * z));
            wall[3].Manipulator.SetPosition(new Vector3(+0 * x, 0, +0 * z));
            wall[4].Manipulator.SetPosition(new Vector3(-1 * x, 0, +0 * z));
            wall[5].Manipulator.SetPosition(new Vector3(-2 * x, 0, +0 * z));
            wall[6].Manipulator.SetPosition(new Vector3(-3 * x, 0, +0 * z));
        }
        private async Task InitializeEmitter()
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
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                AlphaEnabled = false,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            this.lightEmitter = await this.AddComponentModel(desc);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

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

            this.fps.Text = this.Game.RuntimeText;
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

            this.lightEmitter.Manipulator.SetPosition(pos);
            this.pointLight.Position = pos;
        }
    }
}
