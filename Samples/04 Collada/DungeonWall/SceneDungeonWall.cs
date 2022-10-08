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
        private readonly string resourcesFolder = "dungeonwall/resources";

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea fps = null;
        private UITextArea picks = null;

        private Model lightEmitter = null;
        private SceneLightPoint pointLight = null;

        private bool gameReady = false;

        public SceneDungeonWall(Game game)
            : base(game)
        {
#if DEBUG
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            LoadResourcesAsync(
                new[]
                {
                    InitializeText(),
                    InitializeDungeon(),
                    InitializeEmitter()
                },
                InitializeComponentsCompleted);
        }
        private async Task InitializeText()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Tahoma", 18);
            var defaultFont12 = TextDrawerDescription.FromFamily("Tahoma", 12);
            defaultFont18.LineAdjust = true;
            defaultFont12.LineAdjust = true;

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            title.Text = "Tiled Wall Test Scene";

            fps = await AddComponentUI<UITextArea, UITextAreaDescription>("FPS", "FPS", new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow });
            fps.Text = null;

            picks = await AddComponentUI<UITextArea, UITextAreaDescription>("Picks", "Picks", new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow });
            picks.Text = null;

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("BackPanel", "BackPanel", spDesc, LayerUI - 1);
        }
        private async Task InitializeDungeon()
        {
            var desc = new ModelInstancedDescription()
            {
                Instances = 7,
                CastShadow = true,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromFile(resourcesFolder, "wall.json"),
            };

            var wall = await AddComponent<ModelInstanced, ModelInstancedDescription>("Wall", "Wall", desc);

            var bbox = wall[0].GetBoundingBox();

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
            var mat = MaterialPhongContent.Default;
            mat.EmissiveColor = Color.White.RGB();

            var sphere = GeometryUtil.CreateSphere(0.05f, 16, 5);
            var vertices = VertexData.FromDescriptor(sphere);
            var indices = sphere.Indices;

            var desc = new ModelDescription()
            {
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                Content = ContentDescription.FromContentData(vertices, indices, mat),
            };

            lightEmitter = await AddComponent<Model, ModelDescription>("Emitter", "Emitter", desc);
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }
            UpdateLayout();
            InitializeCamera();
            InitializeEnvironment();
            gameReady = true;
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

            pointLight = new SceneLightPoint("light", false, Color3.White, Color3.White, true, desc);

            Lights.Add(pointLight);
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

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            fps.SetPosition(new Vector2(0, 24));
            picks.SetPosition(new Vector2(0, 48));
            panel.Width = Game.Form.RenderWidth;
            panel.Height = picks.Top + picks.Height + 3;
        }
    }
}
