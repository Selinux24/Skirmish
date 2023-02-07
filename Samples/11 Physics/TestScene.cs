using Engine;
using Engine.Common;
using Engine.Content;
using Engine.Physics;
using Engine.UI;
using SharpDX;
using System.Threading.Tasks;

namespace Physics
{
    class TestScene : Scene
    {
        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea runtimeText = null;
        private UITextArea info = null;

        private Model sphere1 = null;
        private Model sphere2 = null;

        private PhysicsFloor floorBody = null;
        private PhysicsObject sphere1Body = null;
        private PhysicsObject sphere2Body = null;

        private readonly Simulator simulator = new Simulator();

        private bool gameReady = false;

        public TestScene(Game game) : base(game)
        {
            GameEnvironment.Background = Color.Black;
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
                    InitializeTexts(),
                    InitializeFloor(),
                    InitializeSphere(),
                },
                InitializeComponentsCompleted);
        }
        private async Task InitializeTexts()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Arial", 18);
            var defaultFont11 = TextDrawerDescription.FromFamily("Arial", 11);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            runtimeText = await AddComponentUI<UITextArea, UITextAreaDescription>("RuntimeText", "RuntimeText", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow });
            info = await AddComponentUI<UITextArea, UITextAreaDescription>("Information", "Information", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow });

            title.Text = "Physics test";
            runtimeText.Text = "";
            info.Text = "Press F1 for Help.";

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.66f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Panel", "Panel", spDesc, LayerUI - 1);
        }
        private async Task InitializeFloor()
        {
            float l = 25f;
            float h = 0f;

            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-l, -h, -l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 0.0f) },
                new VertexData{ Position = new Vector3(-l, -h, +l), Normal = Vector3.Up, Texture = new Vector2(0.0f, l) },
                new VertexData{ Position = new Vector3(+l, -h, -l), Normal = Vector3.Up, Texture = new Vector2(l, 0.0f) },
                new VertexData{ Position = new Vector3(+l, -h, +l), Normal = Vector3.Up, Texture = new Vector2(l, l) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            var material = MaterialBlinnPhongContent.Default;
            material.DiffuseTexture = "resources/floor.png";

            var desc = new ModelDescription()
            {
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(vertices, indices, material),
            };

            await AddComponentGround<Model, ModelDescription>("Floor", "Floor", desc);
        }
        private async Task InitializeSphere()
        {
            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            var sphere = GeometryUtil.CreateSphere(2f, 32, 32);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(sphere, mat),
            };

            sphere1 = await AddComponent<Model, ModelDescription>("sphere1", "sphere1", desc);
            sphere2 = await AddComponent<Model, ModelDescription>("sphere2", "sphere2", desc);
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            UpdateLayout();

            Camera.Goto(new Vector3(-48, 8, -30));
            Camera.LookTo(Vector3.Zero);
            Camera.FarPlaneDistance = 250;

            sphere1.Manipulator.SetPosition(Vector3.One * 5f, true);
            sphere2.Manipulator.SetPosition(Vector3.Up * 5f, true);

            floorBody = new PhysicsFloor(new RigidBody(float.PositiveInfinity, Matrix.Identity));
            sphere1Body = new PhysicsObject(new RigidBody(10, sphere1.Manipulator.FinalTransform), sphere1);
            sphere2Body = new PhysicsObject(new RigidBody(10, sphere2.Manipulator.FinalTransform), sphere2);

            simulator.AddPhysicsObject(floorBody);
            simulator.AddPhysicsObject(sphere1Body);
            simulator.AddPhysicsObject(sphere2Body);

            gameReady = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.Exit();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            if (!gameReady)
            {
                return;
            }

            UpdateCamera(gameTime);

            simulator.Update(gameTime);

            sphere1Body.Update();
            sphere2Body.Update();

            base.Update(gameTime);
        }
        private void UpdateCamera(GameTime gameTime)
        {
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

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Vector3 fwd = new Vector3(Camera.Forward.X, 0, Camera.Forward.Z);
                fwd.Normalize();
                Camera.Move(gameTime, fwd, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Vector3 bwd = new Vector3(Camera.Backward.X, 0, Camera.Backward.Z);
                bwd.Normalize();
                Camera.Move(gameTime, bwd, Game.Input.ShiftPressed);
            }
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            runtimeText.SetPosition(new Vector2(5, title.Top + title.Height + 3));
            info.SetPosition(new Vector2(5, runtimeText.Top + runtimeText.Height + 3));

            panel.Width = Game.Form.RenderWidth;
            panel.Height = info.Top + info.Height + 3;
        }
    }

    public class PhysicsFloor : IPhysicsObject
    {
        public IRigidBody Body { get; private set; }
        public ICollisionPrimitive Collider { get; private set; }

        public PhysicsFloor(IRigidBody body)
        {
            Body = body;
            Collider = new CollisionPlane(Body, new Plane(Vector3.Up, 0));
        }
    }

    public class PhysicsObject : IPhysicsObject
    {
        public IRigidBody Body { get; private set; }
        public ICollisionPrimitive Collider { get; private set; }
        public Model Model { get; private set; }

        public PhysicsObject(IRigidBody body, Model model)
        {
            Body = body;
            Model = model;
            Collider = new CollisionSphere(body, model.GetBoundingSphere(true).Radius);
        }

        public void Update()
        {
            if (Body == null)
            {
                return;
            }

            if (Model == null)
            {
                return;
            }

            Model.Manipulator.SetTransform(Body.TransformMatrix);
        }
    }
}
