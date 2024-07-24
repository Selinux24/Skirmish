using Engine;
using Engine.BuiltIn.Components.Ground;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.Components.Primitives;
using Engine.Common;
using Engine.Content;
using Engine.Physics;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace PhysicsSamples.ScenePhysics
{
    class PhysicsScene : Scene
    {
        private const float floorSize = 100f;

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea runtimeText = null;
        private UITextArea info = null;
        private GeometryColorDrawer<Line3D> lineDrawer = null;
        private Joint joint;
        private Rod rod;

        private readonly Simulator simulator = new(new BoundingBox(Vector3.One * -floorSize, Vector3.One * floorSize), 8) { Velocity = 1f, SimulationIterations = 6 };
        private readonly float bodyTime = 60f;
        private readonly float bodyDistance = floorSize * floorSize;

        private readonly ExplosionDescription explosionTemplate = ExplosionDescription.CreateExplosion();
        private readonly ExplosionDescription bigExplosionTemplate = ExplosionDescription.CreateBigExplosion();
        private Explosion lastExplosion = null;

        private readonly ConcurrentBag<ColliderData> colliders = [];
        private readonly ConcurrentBag<IContactGenerator> contactGenerators = [];

        private bool gameReady = false;

        public PhysicsScene(Game game) : base(game)
        {
            Game.VisibleMouse = true;
            Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;
        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeTexts,
                    InitializeLineDrawer,
                    InitializeTerrain,
                    InitializeSpheres,
                    InitializeBoxes,
                    InitializeCylinders,
                    InitializePyramids,
                    InitializeCapsules,
                    InitializeJoint,
                    InitializeRod,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeTexts()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Arial", 18);
            var defaultFont11 = TextDrawerDescription.FromFamily("Arial", 11);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            runtimeText = await AddComponentUI<UITextArea, UITextAreaDescription>("RuntimeText", "RuntimeText", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });
            info = await AddComponentUI<UITextArea, UITextAreaDescription>("Information", "Information", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });

            title.Text = "Physics test";
            runtimeText.Text = "";
            info.Text = "Press F1 for Help.";

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.66f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Panel", "Panel", spDesc, LayerUI - 1);
        }
        private async Task InitializeLineDrawer()
        {
            var desc = new GeometryColorDrawerDescription<Line3D>()
            {
                Count = 20000,
                DepthEnabled = true,
            };
            lineDrawer = await AddComponentEffect<GeometryColorDrawer<Line3D>, GeometryColorDrawerDescription<Line3D>>(
                "EdgeDrawer",
                "EdgeDrawer",
                desc);
        }
        private async Task InitializeTerrain()
        {
            var desc = SceneryDescription.FromFile("ScenePhysics/resources/terrain", "collisionTerrain.json");
            desc.ColliderType = ColliderTypes.Box;
            desc.CullingVolumeType = CullingVolumeTypes.BoxVolume;
            desc.BlendMode = BlendModes.Opaque;

            var terrain = await AddComponentGround<Scenery, SceneryDescription>("Terrain", "Terrain", desc);

            var rbState = new RigidBodyState { Mass = float.PositiveInfinity };
            var pTerrain = new PhysicsTerrain(new RigidBody(rbState), terrain);

            simulator.AddPhysicsObject(pTerrain);
        }
        private async Task InitializeSpheres()
        {
            var mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            int slices = 16;
            int stacks = 16;
            var sphere = GeometryUtil.CreateSphere(Topology.TriangleList, 2f, slices, stacks);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(sphere, mat),
                ColliderType = ColliderTypes.Spheric,
            };

            var sphere1Model = await AddComponent<Model, ModelDescription>("sphere1", "sphere1", desc);
            var sphere2Model = await AddComponent<Model, ModelDescription>("sphere2", "sphere2", desc);

            sphere1Model.TintColor = Color4.AdjustSaturation(Color.Red, 10f);
            sphere2Model.TintColor = Color4.AdjustSaturation(Color.Green, 10f);

            var rbState1 = new RigidBodyState
            {
                Mass = 20,
                InitialTransform = Matrix.Translation(Vector3.Up * 10f),
                Restitution = 0.95f,
                Friction = 0.5f,
            };
            var rbState2 = new RigidBodyState
            {
                Mass = 10,
                InitialTransform = Matrix.Translation(Vector3.Up * 15f),
                Restitution = 0.95f,
                Friction = 0.5f,
            };

            ColliderData sphere1 = new(rbState1, sphere1Model);
            ColliderData sphere2 = new(rbState2, sphere2Model);

            var wiredSphere = Line3D.CreateSphere(Vector3.Zero, 2f, slices * 2, stacks * 2);
            sphere1.Lines = [.. wiredSphere];
            sphere2.Lines = [.. wiredSphere];

            colliders.Add(sphere1);
            colliders.Add(sphere2);
        }
        private async Task InitializeBoxes()
        {
            var mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            var box = GeometryUtil.CreateBox(Topology.TriangleList, 2f, 2f, 2f);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(box, mat),
                ColliderType = ColliderTypes.Box,
            };

            var box1Model = await AddComponent<Model, ModelDescription>("box1", "box1", desc);
            var box2Model = await AddComponent<Model, ModelDescription>("box2", "box2", desc);

            box1Model.TintColor = Color4.AdjustSaturation(Color.Blue, 20f);
            box2Model.TintColor = Color4.AdjustSaturation(Color.Pink, 20f);

            var rbState1 = new RigidBodyState
            {
                Mass = 15,
                InitialTransform = Matrix.Translation(Vector3.Up * 20f),
                Restitution = 0.95f,
                Friction = 0.5f,
            };
            var rbState2 = new RigidBodyState
            {
                Mass = 10,
                InitialTransform = Matrix.Translation(Vector3.Up * 25f),
                Restitution = 0.95f,
                Friction = 0.5f,
            };

            ColliderData box1 = new(rbState1, box1Model);
            ColliderData box2 = new(rbState2, box2Model);

            var wiredBox = Line3D.CreateBox(Vector3.Zero, 2f, 2f, 2f);
            box1.Lines = [.. wiredBox];
            box2.Lines = [.. wiredBox];

            colliders.Add(box1);
            colliders.Add(box2);
        }
        private async Task InitializeCylinders()
        {
            var mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            float radius = 2f;
            float height = 4f;
            Vector3 center = Vector3.Zero;
            int sliceCount = 16;
            var cylinder = GeometryUtil.CreateCylinder(Topology.TriangleList, center, radius, height, sliceCount);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(cylinder, mat),
                ColliderType = ColliderTypes.Cylinder,
            };

            var cylinder1Model = await AddComponent<Model, ModelDescription>("cylinder1", "cylinder1", desc);
            var cylinder2Model = await AddComponent<Model, ModelDescription>("cylinder2", "cylinder2", desc);

            cylinder1Model.TintColor = Color4.AdjustSaturation(Color.Yellow, 20f);
            cylinder2Model.TintColor = Color4.AdjustSaturation(Color.Purple, 20f);

            var rbState1 = new RigidBodyState
            {
                Mass = 15,
                InitialTransform = Matrix.Translation(Vector3.Up * 30f),
                Restitution = 0.95f,
                Friction = 0.5f,
            };
            var rbState2 = new RigidBodyState
            {
                Mass = 10,
                InitialTransform = Matrix.Translation(Vector3.Up * 35f),
                Restitution = 0.95f,
                Friction = 0.5f,
            };

            ColliderData cylinder1 = new(rbState1, cylinder1Model);
            ColliderData cylinder2 = new(rbState2, cylinder2Model);

            var wiredCylinder = Line3D.CreateCylinder(center, radius, height, sliceCount);
            cylinder1.Lines = [.. wiredCylinder];
            cylinder2.Lines = [.. wiredCylinder];

            colliders.Add(cylinder1);
            colliders.Add(cylinder2);
        }
        private async Task InitializeCapsules()
        {
            var mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            float radius = 2f;
            float height = 8f;
            Vector3 center = Vector3.Zero;
            int sliceCount = 16;
            int stackCount = 8;
            var capsule = GeometryUtil.CreateCapsule(Topology.TriangleList, center, radius, height, sliceCount, stackCount);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(capsule, mat),
                ColliderType = ColliderTypes.Capsule,
            };

            var capsule1Model = await AddComponent<Model, ModelDescription>("capsule1", "capsule1", desc);
            var capsule2Model = await AddComponent<Model, ModelDescription>("capsule2", "capsule2", desc);

            capsule1Model.TintColor = Color4.AdjustSaturation(Color.Gray, 20f);
            capsule2Model.TintColor = Color.SandyBrown;

            var rbState1 = new RigidBodyState
            {
                Mass = 15,
                InitialTransform = Matrix.Translation(Vector3.Up * 40f),
                Restitution = 0.95f,
                Friction = 0.5f,
            };
            var rbState2 = new RigidBodyState
            {
                Mass = 10,
                InitialTransform = Matrix.Translation(Vector3.Up * 45f),
                Restitution = 0.95f,
                Friction = 0.5f,
            };

            ColliderData capsule1 = new(rbState1, capsule1Model);
            ColliderData capsule2 = new(rbState2, capsule2Model);

            var wiredCapsule = Line3D.CreateCapsule(center, radius, height, sliceCount, stackCount);
            capsule1.Lines = [.. wiredCapsule];
            capsule2.Lines = [.. wiredCapsule];

            colliders.Add(capsule1);
            colliders.Add(capsule2);
        }
        private async Task InitializePyramids()
        {
            var mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            var pyramid3d = GeometryUtil.CreatePyramid(Topology.TriangleList, Vector3.Zero, 2f, 2f, 2f);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(pyramid3d, mat),
                ColliderType = ColliderTypes.Mesh,
            };

            var pyramid1Model = await AddComponent<Model, ModelDescription>("pyramid1", "pyramid1", desc);
            var pyramid2Model = await AddComponent<Model, ModelDescription>("pyramid2", "pyramid2", desc);

            pyramid1Model.TintColor = Color4.AdjustSaturation(Color.Cyan, 20f);
            pyramid2Model.TintColor = Color4.AdjustSaturation(Color.Beige, 20f);

            var rbState1 = new RigidBodyState
            {
                Mass = 15,
                InitialTransform = Matrix.Translation(Vector3.Up * 40f),
                Restitution = 0.95f,
                Friction = 0.5f,
            };
            var rbState2 = new RigidBodyState
            {
                Mass = 10,
                InitialTransform = Matrix.Translation(Vector3.Up * 45f),
                Restitution = 0.95f,
                Friction = 0.5f,
            };

            ColliderData pyramid1 = new(rbState1, pyramid1Model);
            ColliderData pyramid2 = new(rbState2, pyramid2Model);

            var pyramid2d = Line3D.CreatePyramid(Vector3.Zero, 2f, 2f, 2f);
            pyramid1.Lines = [.. pyramid2d];
            pyramid2.Lines = [.. pyramid2d];

            colliders.Add(pyramid1);
            colliders.Add(pyramid2);
        }
        private async Task InitializeJoint()
        {
            var mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            int slices = 16;
            int stacks = 8;
            var sphere = GeometryUtil.CreateSphere(Topology.TriangleList, 0.5f, slices, stacks);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(sphere, mat),
                ColliderType = ColliderTypes.Spheric,
            };

            var jsphere1Model = await AddComponent<Model, ModelDescription>("jsphere1", "jsphere1", desc);
            var jsphere2Model = await AddComponent<Model, ModelDescription>("jsphere2", "jsphere2", desc);

            jsphere1Model.TintColor = Color4.AdjustSaturation(Color.DarkSlateGray, 10f);
            jsphere2Model.TintColor = Color4.AdjustSaturation(Color.SaddleBrown, 10f);

            var rbState1 = new RigidBodyState
            {
                Mass = 20,
                InitialTransform = Matrix.Translation(new Vector3(-15, 10, 0)),
                Restitution = 0f,
                Friction = 0.9f,
                IsStatic = true,
            };
            var rbState2 = new RigidBodyState
            {
                Mass = 50,
                InitialTransform = Matrix.Translation(new Vector3(-20, 10, 0)),
                Restitution = 0.95f,
                Friction = 0.5f,
            };

            ColliderData jsphere1 = new(rbState1, jsphere1Model);
            ColliderData jsphere2 = new(rbState2, jsphere2Model);

            var wiredSphere = Line3D.CreateSphere(Vector3.Zero, 0.5f, slices * 2, stacks * 2);
            jsphere1.Lines = [.. wiredSphere];
            jsphere2.Lines = [.. wiredSphere];

            colliders.Add(jsphere1);
            colliders.Add(jsphere2);

            var endPointOne = new BodyEndPoint(jsphere1.PhysicsObject.RigidBody, Vector3.Down * 0.5f);
            var endPointTwo = new BodyEndPoint(jsphere2.PhysicsObject.RigidBody, Vector3.Up * 0.5f);
            joint = new Joint(endPointOne, endPointTwo, 2f);

            contactGenerators.Add(joint);
        }
        private async Task InitializeRod()
        {
            var mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            int slices = 16;
            int stacks = 8;
            var sphere = GeometryUtil.CreateSphere(Topology.TriangleList, 0.5f, slices, stacks);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(sphere, mat),
                ColliderType = ColliderTypes.Spheric,
            };

            var rsphere1Model = await AddComponent<Model, ModelDescription>("rsphere1", "rsphere1", desc);
            var rsphere2Model = await AddComponent<Model, ModelDescription>("rsphere2", "rsphere2", desc);

            rsphere1Model.TintColor = Color4.AdjustSaturation(Color.DarkCyan, 10f);
            rsphere2Model.TintColor = Color4.AdjustSaturation(Color.AntiqueWhite, 10f);

            var rbState1 = new RigidBodyState
            {
                Mass = 20,
                InitialTransform = Matrix.Translation(new Vector3(15, 10, 0)),
                Restitution = 0f,
                Friction = 0.9f,
                IsStatic = true,
            };
            var rbState2 = new RigidBodyState
            {
                Mass = 50,
                InitialTransform = Matrix.Translation(new Vector3(20, 10, 0)),
                Restitution = 0.95f,
                Friction = 0.5f,
            };

            ColliderData rsphere1 = new(rbState1, rsphere1Model);
            ColliderData rsphere2 = new(rbState2, rsphere2Model);

            var wiredSphere = Line3D.CreateSphere(Vector3.Zero, 0.5f, slices * 2, stacks * 2);
            rsphere1.Lines = [.. wiredSphere];
            rsphere2.Lines = [.. wiredSphere];

            colliders.Add(rsphere1);
            colliders.Add(rsphere2);

            var endPointOne = new BodyEndPoint(rsphere1.PhysicsObject.RigidBody, Vector3.Down * 0.5f);
            var endPointTwo = new BodyEndPoint(rsphere2.PhysicsObject.RigidBody, Vector3.Up * 0.5f);
            rod = new Rod(endPointOne, endPointTwo, 2f, 0.0001f);

            contactGenerators.Add(rod);
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                var exList = res.GetExceptions();
                foreach (var ex in exList)
                {
                    Logger.WriteError(this, ex);
                }

                Game.Exit();
            }

            UpdateLayout();

            Camera.Goto(new Vector3(120, 40, 75));
            Camera.LookTo(Vector3.Zero);
            Camera.FarPlaneDistance = 300;

            colliders.ToList().ForEach(c =>
            {
                c.Initialize();
                simulator.AddPhysicsObject(c.PhysicsObject);
                Lights.Add(c.Light);
            });

            simulator.AddContactGenerators(contactGenerators);

            gameReady = true;
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
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

            UpdateText();
        }
        private void UpdateInputCamera(IGameTime gameTime)
        {
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }

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

            if (Game.Input.MouseButtonJustReleased(MouseButtons.Left))
            {
                GenerateExplosion(GetPickingRay(), Game.Input.ShiftPressed);
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
        private void UpdateStateBodies(IGameTime gameTime)
        {
            lineDrawer.Clear();

            simulator.Update(gameTime);

            float elapsed = gameTime.ElapsedSeconds;

            colliders.ToList().ForEach(c =>
            {
                c.UpdateBodyState(elapsed, bodyTime / simulator.Velocity, bodyDistance);
                c.SetLines(lineDrawer);
            });

            var lJoint = new Line3D(joint.One.PositionWorld, joint.Two.PositionWorld);
            var rJoint = new Line3D(rod.One.PositionWorld, rod.Two.PositionWorld);
            lineDrawer.AddPrimitives(Color4.White, [lJoint, rJoint]);
        }
        private void UpdateText()
        {
            runtimeText.Text = $"Explosion time: {MathF.Round(lastExplosion?.TotalElapsedTime ?? 0, 2)}; Phase: {lastExplosion?.CurrentPhase}; Active: {lastExplosion?.IsActive}";
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

        private void GenerateExplosion(PickingRay pickingRay, bool big)
        {
            if (!this.PickNearest<Triangle>(pickingRay, SceneObjectUsages.None, out var p))
            {
                return;
            }

            var explosionTmp = big ? bigExplosionTemplate : explosionTemplate;
            lastExplosion = new Explosion(p.PickingResult.Position, explosionTmp);

            simulator.AddGlobalForce(lastExplosion);
        }
    }
}
