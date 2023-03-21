using Engine;
using Engine.Common;
using Engine.Content;
using Engine.Physics;
using Engine.UI;
using SharpDX;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Physics
{
    class TestScene : Scene
    {
        private const float floorSize = 50f;

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea runtimeText = null;
        private UITextArea info = null;
        private PrimitiveListDrawer<Line3D> lineDrawer = null;

        private readonly Simulator simulator = new() { Velocity = 1f };
        private readonly float bodyTime = 20f;
        private readonly float bodyDistance = floorSize * floorSize;

        private readonly ConcurrentBag<ColliderData> colliders = new();

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
                    InitializeLineDrawer(),
                    InitializeFloor(),
                    InitializeSpheres(),
                    InitializeBoxes(),
                    InitializeCylinders(),
                    InitializePyramids(),
                    InitializeCapsules(),
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
        private async Task InitializeLineDrawer()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 20000,
                DepthEnabled = true,
            };
            lineDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "EdgeDrawer",
                "EdgeDrawer",
                desc);
        }
        private async Task InitializeFloor()
        {
            float l = floorSize;
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

            var floor = await AddComponentGround<Model, ModelDescription>("Floor", "Floor", desc);

            var floorTrn = Matrix.RotationYawPitchRoll(0f, -0.2f, 0f);
            var rbState = new RigidBodyState { Mass = float.PositiveInfinity, InitialTransform = floorTrn };
            var floorBody = new PhysicsFloor(new RigidBody(rbState), floor);

            simulator.AddPhysicsObject(floorBody);
        }
        private async Task InitializeSpheres()
        {
            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            int slices = 16;
            int stacks = 16;
            var sphere = GeometryUtil.CreateSphere(2f, slices, stacks);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(sphere, mat),
                CullingVolumeType = CullingVolumeTypes.SphericVolume,
            };

            ColliderData sphere1 = new(20, Matrix.Translation(Vector3.Up * 10f));
            ColliderData sphere2 = new(10, Matrix.Translation(Vector3.Up * 15f));

            sphere1.Model = await AddComponent<Model, ModelDescription>("sphere1", "sphere1", desc);
            sphere2.Model = await AddComponent<Model, ModelDescription>("sphere2", "sphere2", desc);

            sphere1.Model.TintColor = Color4.AdjustSaturation(Color.Red, 10f);
            sphere2.Model.TintColor = Color4.AdjustSaturation(Color.Green, 10f);

            sphere1.Lines = Line3D.CreateWiredSphere(sphere1.Model.GetBoundingSphere(), slices * 2, stacks * 2);
            sphere2.Lines = Line3D.CreateWiredSphere(sphere2.Model.GetBoundingSphere(), slices * 2, stacks * 2);

            colliders.Add(sphere1);
            colliders.Add(sphere2);
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

            ColliderData box1 = new(15, Matrix.Translation(Vector3.Up * 20f));
            ColliderData box2 = new(10, Matrix.Translation(Vector3.Up * 25f));

            box1.Model = await AddComponent<Model, ModelDescription>("box1", "box1", desc);
            box2.Model = await AddComponent<Model, ModelDescription>("box2", "box2", desc);

            box1.Model.TintColor = Color4.AdjustSaturation(Color.Blue, 20f);
            box2.Model.TintColor = Color4.AdjustSaturation(Color.Pink, 20f);

            box1.Lines = Line3D.CreateWiredBox(box1.Model.GetOrientedBoundingBox());
            box2.Lines = Line3D.CreateWiredBox(box2.Model.GetOrientedBoundingBox());

            colliders.Add(box1);
            colliders.Add(box2);
        }
        private async Task InitializeCylinders()
        {
            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            float radius = 2f;
            float height = 4f;
            Vector3 center = Vector3.Zero;
            int stackCount = 16;

            var cylinder = GeometryUtil.CreateCylinder(center, radius, height, stackCount);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(cylinder, mat),
                CullingVolumeType = CullingVolumeTypes.CylinderVolume,
            };

            ColliderData cylinder1 = new(15, Matrix.Translation(Vector3.Up * 30f));
            ColliderData cylinder2 = new(10, Matrix.Translation(Vector3.Up * 35f));

            cylinder1.Model = await AddComponent<Model, ModelDescription>("cylinder1", "cylinder1", desc);
            cylinder2.Model = await AddComponent<Model, ModelDescription>("cylinder2", "cylinder2", desc);

            cylinder1.Model.TintColor = Color4.AdjustSaturation(Color.Yellow, 20f);
            cylinder2.Model.TintColor = Color4.AdjustSaturation(Color.Purple, 20f);

            cylinder1.Lines = Line3D.CreateWiredCylinder(center, radius, height, stackCount);
            cylinder2.Lines = Line3D.CreateWiredCylinder(center, radius, height, stackCount);

            colliders.Add(cylinder1);
            colliders.Add(cylinder2);
        }
        private async Task InitializeCapsules()
        {
            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            float radius = 2f;
            float height = 4f;
            Vector3 center = Vector3.Zero;
            int sliceCount = 8;
            int stackCount = 16;

            var capsule = GeometryUtil.CreateCapsule(center, radius, height + radius + radius, sliceCount, stackCount);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(capsule, mat),
                CullingVolumeType = CullingVolumeTypes.CapsuleVolume,
            };

            ColliderData capsule1 = new(15, Matrix.Translation(Vector3.Up * 40f));
            ColliderData capsule2 = new(10, Matrix.Translation(Vector3.Up * 45f));

            capsule1.Model = await AddComponent<Model, ModelDescription>("capsule1", "capsule1", desc);
            capsule2.Model = await AddComponent<Model, ModelDescription>("capsule2", "capsule2", desc);

            capsule1.Model.TintColor = Color4.AdjustSaturation(Color.Orchid, 20f);
            capsule2.Model.TintColor = Color4.AdjustSaturation(Color.AliceBlue, 20f);

            capsule1.Lines = Line3D.CreateWiredCylinder(center, radius, height, stackCount);
            capsule2.Lines = Line3D.CreateWiredCylinder(center, radius, height, stackCount);

            colliders.Add(capsule1);
            colliders.Add(capsule2);
        }
        private async Task InitializePyramids()
        {
            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            var pyramid = GeometryUtil.CreatePyramid(Vector3.Zero, 2f, 2f, 2f);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(pyramid, mat),
                CullingVolumeType = CullingVolumeTypes.None,
            };

            ColliderData pyramid1 = new(15, Matrix.Translation(Vector3.Up * 40f));
            ColliderData pyramid2 = new(10, Matrix.Translation(Vector3.Up * 45f));

            pyramid1.Model = await AddComponent<Model, ModelDescription>("pyramid1", "pyramid1", desc);
            pyramid2.Model = await AddComponent<Model, ModelDescription>("pyramid2", "pyramid2", desc);

            pyramid1.Model.TintColor = Color4.AdjustSaturation(Color.Cyan, 20f);
            pyramid2.Model.TintColor = Color4.AdjustSaturation(Color.Beige, 20f);

            pyramid1.Lines = Line3D.CreateWiredPyramid(pyramid1.Model.GetPoints());
            pyramid2.Lines = Line3D.CreateWiredPyramid(pyramid2.Model.GetPoints());

            colliders.Add(pyramid1);
            colliders.Add(pyramid2);
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

            colliders.ToList().ForEach(c =>
            {
                c.Initialize();
                simulator.AddPhysicsObject(c.PhysicsObject);
                Lights.Add(c.Light);
            });

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

            UpdateInputCamera(gameTime);
            UpdateInputBodies();

            UpdateStateBodies(gameTime);

            base.Update(gameTime);
        }
        private void UpdateInputCamera(GameTime gameTime)
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
                Vector3 fwd = new(Camera.Forward.X, 0, Camera.Forward.Z);
                fwd.Normalize();
                Camera.Move(gameTime, fwd, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Vector3 bwd = new(Camera.Backward.X, 0, Camera.Backward.Z);
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
        private void UpdateInputBodies()
        {
            if (Game.Input.KeyJustReleased(Keys.Tab))
            {
                colliders.ToList().ForEach(c =>
                {
                    c.Reset();
                });
            }

            if (Game.Input.KeyJustReleased(Keys.D1))
            {
                colliders.ElementAtOrDefault(0)?.Reset();
            }
            if (Game.Input.KeyJustReleased(Keys.D2))
            {
                colliders.ElementAtOrDefault(1)?.Reset();
            }
            if (Game.Input.KeyJustReleased(Keys.D3))
            {
                colliders.ElementAtOrDefault(2)?.Reset();
            }
            if (Game.Input.KeyJustReleased(Keys.D4))
            {
                colliders.ElementAtOrDefault(3)?.Reset();
            }
            if (Game.Input.KeyJustReleased(Keys.D5))
            {
                colliders.ElementAtOrDefault(4)?.Reset();
            }
            if (Game.Input.KeyJustReleased(Keys.D6))
            {
                colliders.ElementAtOrDefault(5)?.Reset();
            }
        }
        private void UpdateStateBodies(GameTime gameTime)
        {
            lineDrawer.Clear();

            simulator.Update(gameTime);

            float elapsed = gameTime.ElapsedSeconds;

            colliders.ToList().ForEach(c =>
            {
                c.UpdateBodyState(elapsed, bodyTime / simulator.Velocity, bodyDistance);
                c.SetLines(lineDrawer);
            });
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
}
