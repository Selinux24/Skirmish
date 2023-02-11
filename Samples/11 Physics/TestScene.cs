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

        private readonly Simulator simulator = new Simulator();

        private Model floor = null;
        private PhysicsFloor floorBody = null;

        private readonly Vector3 sphere1Position = Vector3.Up * 15f;
        private Model sphere1 = null;
        private PhysicsObject sphere1Body = null;
        private SceneLightPoint sphere1Light;

        private readonly Vector3 sphere2Position = Vector3.Up * 20f;
        private Model sphere2 = null;
        private PhysicsObject sphere2Body = null;
        private SceneLightPoint sphere2Light;

        private readonly Vector3 box1Position = Vector3.Up * 10f;
        private Model box1 = null;
        private PhysicsObject box1Body = null;
        private SceneLightPoint box1Light;

        private readonly Vector3 box2Position = Vector3.Up * 25f;
        private Model box2 = null;
        private PhysicsObject box2Body = null;
        private SceneLightPoint box2Light;

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
                    InitializeSpheres(),
                    InitializeBoxes(),
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
            float l = 50f;
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

            floor = await AddComponentGround<Model, ModelDescription>("Floor", "Floor", desc);
        }
        private async Task InitializeSpheres()
        {
            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            var sphere = GeometryUtil.CreateSphere(2f, 32, 32);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(sphere, mat),
                CullingVolumeType = CullingVolumeTypes.SphericVolume,
            };

            sphere1 = await AddComponent<Model, ModelDescription>("sphere1", "sphere1", desc);
            sphere2 = await AddComponent<Model, ModelDescription>("sphere2", "sphere2", desc);

            sphere1.TintColor = Color4.AdjustSaturation(Color.Red, 10f);
            sphere2.TintColor = Color4.AdjustSaturation(Color.Green, 10f);
        }
        private async Task InitializeBoxes()
        {
            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            var box = GeometryUtil.CreateBox(2f, 2f, 2f);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(box, mat),
                CullingVolumeType = CullingVolumeTypes.BoxVolume,
            };

            box1 = await AddComponent<Model, ModelDescription>("box1", "box1", desc);
            box2 = await AddComponent<Model, ModelDescription>("box2", "box2", desc);

            box1.TintColor = Color4.AdjustSaturation(Color.Blue, 20f);
            box2.TintColor = Color4.AdjustSaturation(Color.Pink, 20f);
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

            floor.Manipulator.SetRotation(0f, -0.2f, 0f);
            floorBody = new PhysicsFloor(new RigidBody(float.PositiveInfinity, floor.Manipulator.FinalTransform), floor);

            sphere1.Manipulator.SetPosition(sphere1Position);
            sphere1Body = new PhysicsObject(new RigidBody(10, sphere1.Manipulator.FinalTransform), sphere1);

            sphere2.Manipulator.SetScale(0.5f);
            sphere2.Manipulator.SetPosition(sphere2Position);
            sphere2Body = new PhysicsObject(new RigidBody(5, sphere2.Manipulator.FinalTransform), sphere2);

            box1.Manipulator.SetPosition(box1Position);
            box1Body = new PhysicsObject(new RigidBody(15, box1.Manipulator.FinalTransform), box1);

            box2.Manipulator.SetScale(2f);
            box2.Manipulator.SetPosition(box2Position);
            box2Body = new PhysicsObject(new RigidBody(20, box2.Manipulator.FinalTransform), box2);

            simulator.AddPhysicsObject(floorBody);
            simulator.AddPhysicsObject(sphere1Body);
            simulator.AddPhysicsObject(sphere2Body);
            simulator.AddPhysicsObject(box1Body);
            simulator.AddPhysicsObject(box2Body);

            sphere1Light = new SceneLightPoint(nameof(sphere1), true, sphere1.TintColor.RGB(), Color.Yellow.RGB(), true, SceneLightPointDescription.Create(Vector3.Zero, 5f, 2f));
            sphere2Light = new SceneLightPoint(nameof(sphere2), true, sphere2.TintColor.RGB(), Color.Yellow.RGB(), true, SceneLightPointDescription.Create(Vector3.Zero, 2.5f, 2f));
            box1Light = new SceneLightPoint(nameof(box1), true, box1.TintColor.RGB(), Color.Yellow.RGB(), true, SceneLightPointDescription.Create(Vector3.Zero, 2.5f, 2f));
            box2Light = new SceneLightPoint(nameof(box2), true, box2.TintColor.RGB(), Color.Yellow.RGB(), true, SceneLightPointDescription.Create(Vector3.Zero, 7.5f, 2f));

            Lights.Add(sphere1Light);
            Lights.Add(sphere2Light);
            Lights.Add(box1Light);
            Lights.Add(box2Light);

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
            UpdateBodies();

            simulator.Update(gameTime);

            sphere1Light.Position = sphere1Body.Body.Position;
            sphere2Light.Position = sphere2Body.Body.Position;
            box1Light.Position = box1Body.Body.Position;
            box2Light.Position = box2Body.Body.Position;

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

            if (Game.Input.KeyPressed(Keys.C))
            {
                Camera.MoveDown(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.Space))
            {
                Camera.MoveUp(gameTime, Game.Input.ShiftPressed);
            }
        }
        private void UpdateBodies()
        {
            if (Game.Input.KeyJustReleased(Keys.Tab))
            {
                Reset();
            }

            if (Game.Input.KeyJustReleased(Keys.D1))
            {
                sphere1Body.Reset(sphere1Position, Quaternion.Identity);
            }
            if (Game.Input.KeyJustReleased(Keys.D2))
            {
                sphere2Body.Reset(sphere2Position, Quaternion.Identity);
            }
            if (Game.Input.KeyJustReleased(Keys.D3))
            {
                box1Body.Reset(box1Position, Quaternion.Identity);
            }
            if (Game.Input.KeyJustReleased(Keys.D4))
            {
                box2Body.Reset(box2Position, Quaternion.Identity);
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

        private void Reset()
        {
            sphere1Body.Reset(sphere1Position, Quaternion.Identity);
            sphere2Body.Reset(sphere2Position, Quaternion.Identity);
            box1Body.Reset(box1Position, Quaternion.Identity);
            box2Body.Reset(box2Position, Quaternion.Identity);
        }
    }
}
