using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Threading.Tasks;

namespace Collada.DungeonWall
{
    public class SceneDungeonWall : Scene
    {
        private const int layerHUD = 99;

        private readonly string resourcesFolder = "dungeonwall/resources";

        private UITextArea fps = null;

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
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif
            _ = LoadResourcesAsync(
                new[]
                {
                    InitializeText(),
                    InitializeDungeon(),
                    InitializeEmitter()
                },
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    InitializeCamera();
                    InitializeEnvironment();
                    gameReady = true;
                });
        }
        private void InitializeCamera()
        {
            Camera.NearPlaneDistance = 0.5f;
            Camera.FarPlaneDistance = 500;
            Camera.Mode = CameraModes.Free;
            Camera.Position = new Vector3(-5, 3, -5);
            Camera.Interest = new Vector3(0, 0, 0);
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.Black;

            var desc = SceneLightPointDescription.Create(new Vector3(0, 1, -1), 10f, 10f);

            pointLight = new SceneLightPoint("light", false, Color.White, Color.White, true, desc);

            Lights.Add(pointLight);
        }
        private async Task InitializeText()
        {
            var title = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18, Color.White) }, layerHUD);
            title.Text = "Tiled Wall Test Scene";
            title.SetPosition(Vector2.Zero);

            fps = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) }, layerHUD);
            fps.Text = null;
            fps.SetPosition(new Vector2(0, 24));

            var picks = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) }, layerHUD);
            picks.Text = null;
            picks.SetPosition(new Vector2(0, 48));

            var spDesc = new SpriteDescription()
            {
                Width = Game.Form.RenderWidth,
                Height = picks.Top + picks.Height + 3,
                BaseColor = new Color4(0, 0, 0, 0.75f),
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
                    Content = ContentDescription.FromFile(resourcesFolder, "wall.xml"),
                });

            BoundingBox bbox = wall[0].GetBoundingBox();

            float x = bbox.Width * (10f / 11f);
            float z = bbox.Depth;

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
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            lightEmitter = await this.AddComponentModel(desc);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<Start.SceneStart>();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            UpdateCamera();

            UpdateLight(gameTime);

            fps.Text = Game.RuntimeText;
        }
        private void UpdateCamera()
        {
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
            if (Game.Input.RightMouseButtonPressed)
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
        }
        private void UpdateLight(GameTime gameTime)
        {
            var pos = pointLight.Position;

            if (Game.Input.KeyPressed(Keys.Left))
            {
                pos.X -= gameTime.ElapsedSeconds * 5f;
            }

            if (Game.Input.KeyPressed(Keys.Right))
            {
                pos.X += gameTime.ElapsedSeconds * 5f;
            }

            if (Game.Input.KeyPressed(Keys.Up))
            {
                pos.Z += gameTime.ElapsedSeconds * 5f;
            }

            if (Game.Input.KeyPressed(Keys.Down))
            {
                pos.Z -= gameTime.ElapsedSeconds * 5f;
            }

            lightEmitter.Manipulator.SetPosition(pos);
            pointLight.Position = pos;
        }
    }
}
