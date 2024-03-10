using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntermediateSamples.SceneSimpleAnimation
{
    public class SimpleAnimationScene : Scene
    {
        private const string SceneResourceDoors = "SceneSimpleAnimation/Resources/Doors";
        private const string SceneResourceLadder = "SceneSimpleAnimation/Resources/Ladder";
        private const string SceneResourceSoldier = "SceneSimpleAnimation/Resources/Soldier";
        private const string SceneResourceRat = "SceneSimpleAnimation/Resources/Rat";

        private const string DefaultString = "default";
        private const string OpenString = "open";
        private const string CloseString = "close";
        private const string RepString = "rep";
        private const string Idle1String = "idle1";
        private const string Idle2String = "idle2";
        private const string StandString = "stand";
        private const string PullString = "pull";
        private const string PushString = "push";
        private const string WalkString = "walk";
        private const string RunString = "run";

        private UITextArea title = null;
        private UITextArea runtime = null;
        private UITextArea animText = null;
        private UITextArea messages = null;
        private Sprite backPanel = null;
        private UIConsole console = null;

        private PrimitiveListDrawer<Triangle> itemTris = null;
        private PrimitiveListDrawer<Line3D> itemLines = null;
        private readonly Color itemTrisColor = new(Color.Yellow.ToColor3(), 0.6f);
        private readonly Color itemLinesColor = new(Color.Red.ToColor3(), 1f);
        private bool showItemDEBUG = false;
        private bool showItem = true;
        private int itemIndex = 0;

        private static Action<ModelInstanced> StartAnimation()
        {
            return item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    item[i].AnimationController.Start(0);
                }
            };
        }
        private static Action<ModelInstanced> PauseAnimation()
        {
            return item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    item[i].AnimationController.Pause();
                }
            };
        }
        private static Action<ModelInstanced> ResumeAnimation()
        {
            return item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    item[i].AnimationController.Resume();
                }
            };
        }
        private static Action<ModelInstanced> IncreaseDelta()
        {
            return item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    var controller = item[i].AnimationController;

                    controller.TimeDelta += 0.1f;
                    controller.TimeDelta = Math.Min(5, controller.TimeDelta);
                }
            };
        }
        private static Action<ModelInstanced> DecreaseDelta()
        {
            return item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    var controller = item[i].AnimationController;

                    controller.TimeDelta -= 0.1f;
                    controller.TimeDelta = Math.Max(0, controller.TimeDelta);
                }
            };
        }

        private readonly List<ModelInstanced> animObjects = [];

        private readonly Dictionary<string, AnimationPlan> soldierPaths = [];
        private readonly Dictionary<string, AnimationPlan> ratPaths = [];

        private bool uiReady = false;
        private bool gameReady = false;

        public SimpleAnimationScene(Game game)
            : base(game)
        {
#if DEBUG
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif

            GameEnvironment.Background = Color.CornflowerBlue;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            InitializeUI();
        }

        private void InitializeUI()
        {
            LoadResourcesAsync(
                InitializeUITitle(),
                InitializeUICompleted);
        }
        private async Task InitializeUITitle()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Consolas", 18);
            var defaultFont15 = TextDrawerDescription.FromFamily("Consolas", 15);
            var defaultFont11 = TextDrawerDescription.FromFamily("Consolas", 11);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            runtime = await AddComponentUI<UITextArea, UITextAreaDescription>("Runtime", "Runtime", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow });
            animText = await AddComponentUI<UITextArea, UITextAreaDescription>("AnimText", "AnimText", new UITextAreaDescription { Font = defaultFont15, TextForeColor = Color.Orange });
            messages = await AddComponentUI<UITextArea, UITextAreaDescription>("Messages", "Messages", new UITextAreaDescription { Font = defaultFont15, TextForeColor = Color.Orange });

            title.Text = "Animation test";
            runtime.Text = "";
            animText.Text = "";
            messages.Text = "";

            backPanel = await AddComponentUI<Sprite, SpriteDescription>("Backpanel", "Backpanel", SpriteDescription.Default(new Color4(0, 0, 0, 0.75f)), LayerUI - 1);

            var consoleDesc = UIConsoleDescription.Default(new Color4(0.35f, 0.35f, 0.35f, 1f));
            consoleDesc.LogFilterFunc = (l) => l.LogLevel > LogLevel.Trace || (l.LogLevel == LogLevel.Trace && l.CallerTypeName == nameof(AnimationController));
            console = await AddComponentUI<UIConsole, UIConsoleDescription>("Console", "Console", consoleDesc, LayerUI + 1);
            console.Visible = false;

            uiReady = true;
        }
        private void InitializeUICompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            UpdateLayout();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            LoadResourcesAsync(
                [
                    InitializeLadder(),
                    InitializeLadder2(),
                    InitializeSoldier(),
                    InitializeRat(),
                    InitializeDoors(),
                    InitializeJails(),
                    InitializeFloor(),
                    InitializeDebug()
                ],
                InitializeComponentsCompleted);
        }
        private async Task InitializeFloor()
        {
            float l = 15f;
            float h = 0f;

            VertexData[] vertices =
            [
                new (){ Position = new (-l, h, -l), Normal = Vector3.Up, Texture = new (0.0f, 0.0f) },
                new (){ Position = new (-l, h, +l), Normal = Vector3.Up, Texture = new (0.0f, 1.0f) },
                new (){ Position = new (+l, h, -l), Normal = Vector3.Up, Texture = new (1.0f, 0.0f) },
                new (){ Position = new (+l, h, +l), Normal = Vector3.Up, Texture = new (1.0f, 1.0f) },
            ];

            uint[] indices =
            [
                0, 1, 2,
                1, 3, 2,
            ];

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = "SceneSimpleAnimation/resources/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SceneSimpleAnimation/resources/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "SceneSimpleAnimation/resources/d_road_asphalt_stripes_specular.dds";

            var desc = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(vertices, indices, mat),
            };

            await AddComponent<Model, ModelDescription>("Floor", "Floor", desc);
        }
        private async Task InitializeLadder()
        {
            var ladder = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                "Ladder",
                "Ladder",
                new ModelInstancedDescription()
                {
                    Instances = 2,
                    CastShadow = ShadowCastingAlgorihtms.All,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceLadder, "Dn_Anim_Ladder.json"),
                });

            ladder[0].Manipulator.SetPosition(-4f, 1, 0);
            ladder[1].Manipulator.SetPosition(-4.5f, 1, 5);

            ladder[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            ladder[1].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);

            var def = new AnimationPath();
            def.Add(DefaultString);
            var pull = new AnimationPath();
            pull.Add(PullString);
            var push = new AnimationPath();
            push.Add(PushString);

            var ladderPaths = new Dictionary<string, AnimationPlan>
            {
                { DefaultString, new AnimationPlan(def) },
                { PullString, new AnimationPlan(pull) },
                { PushString, new AnimationPlan(push) }
            };

            ladder[0].AnimationController.AppendPlan(ladderPaths[PullString]);
            ladder[1].AnimationController.AppendPlan(ladderPaths[PullString]);

            animObjects.Add(ladder);
        }
        private async Task InitializeLadder2()
        {
            var ladder = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                "Ladder2",
                "Ladder2",
                new ModelInstancedDescription()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceLadder, "Dn_Anim_Ladder_2.json"),
                });

            var ladder2 = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                "Ladder22",
                "Ladder22",
                new ModelInstancedDescription()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceLadder, "Dn_Anim_Ladder_22.json"),
                });

            ladder[0].Manipulator.SetPosition(-3f, 1, 0);
            ladder[1].Manipulator.SetPosition(-3.5f, 1, 5);

            ladder[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            ladder[1].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);

            ladder2[0].Manipulator.SetPosition(-2f, 1, 0);
            ladder2[1].Manipulator.SetPosition(-2.5f, 1, 5);

            ladder2[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            ladder2[1].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);

            var def = new AnimationPath();
            def.Add(DefaultString);
            var pull = new AnimationPath();
            pull.Add(PullString);
            var push = new AnimationPath();
            push.Add(PushString);

            var ladder2Paths = new Dictionary<string, AnimationPlan>
            {
                { DefaultString, new AnimationPlan(def) },
                { PullString, new AnimationPlan(pull) },
                { PushString, new AnimationPlan(push) }
            };

            ladder[0].AnimationController.AppendPlan(ladder2Paths[PullString]);
            ladder[1].AnimationController.AppendPlan(ladder2Paths[PullString]);

            ladder2[0].AnimationController.AppendPlan(ladder2Paths[PushString]);
            ladder2[1].AnimationController.AppendPlan(ladder2Paths[PushString]);

            animObjects.Add(ladder);
            animObjects.Add(ladder2);
        }
        private async Task InitializeSoldier()
        {
            var soldier = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                "Soldier",
                "Soldier",
                new ModelInstancedDescription()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceSoldier, "soldier_anim2.json"),
                });

            soldier[0].Manipulator.SetPosition(0, 0, 0);
            soldier[1].Manipulator.SetPosition(0.5f, 0, 5);

            soldier[0].AnimationController.PlanEnding += SoldierControllerPathEnding;
            soldier[1].AnimationController.PlanEnding += SoldierControllerPathEnding;

            var p1 = new AnimationPath();
            p1.Add(Idle1String);

            var p2 = new AnimationPath();
            p2.Add(Idle2String);

            var p3 = new AnimationPath();
            p3.Add(StandString);

            var p4 = new AnimationPath();
            p4.Add(WalkString);

            var p5 = new AnimationPath();
            p5.Add(RunString);

            soldierPaths.Add(Idle1String, new AnimationPlan(p1));
            soldierPaths.Add(Idle2String, new AnimationPlan(p2));
            soldierPaths.Add(StandString, new AnimationPlan(p3));
            soldierPaths.Add(WalkString, new AnimationPlan(p4));
            soldierPaths.Add(RunString, new AnimationPlan(p5));

            soldier[0].AnimationController.AppendPlan(soldierPaths[Idle1String]);
            soldier[1].AnimationController.AppendPlan(soldierPaths[Idle1String]);

            animObjects.Add(soldier);
        }
        private async Task InitializeRat()
        {
            var rat = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                "Rat",
                "Rat",
                new ModelInstancedDescription()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceRat, "rat.json"),
                });

            rat[0].Manipulator.SetPosition(2, 0, 0);
            rat[1].Manipulator.SetPosition(2.5f, 0, 5);

            var p0 = new AnimationPath();
            p0.AddLoop(WalkString);

            ratPaths.Add(WalkString, new AnimationPlan(p0));

            rat[0].AnimationController.AppendPlan(ratPaths[WalkString]);
            rat[1].AnimationController.AppendPlan(ratPaths[WalkString]);

            animObjects.Add(rat);
        }
        private async Task InitializeDoors()
        {
            var doors = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                "Doors",
                "Doors",
                new ModelInstancedDescription()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceDoors, "Dn_Doors.json"),
                });

            var walls = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                "DoorWalls",
                "DoorWalls",
                new ModelInstancedDescription()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceDoors, "Wall1.json"),
                });

            doors[0].Manipulator.SetPosition(-10, 0, 8);
            doors[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            doors[0].Manipulator.SetScaling(2.5f);

            walls[0].Manipulator.SetPosition(-10, 0, 8);
            walls[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            walls[0].Manipulator.SetScaling(2.5f);

            var def = new AnimationPath();
            def.Add(DefaultString);
            var open = new AnimationPath();
            open.Add(OpenString);
            var close = new AnimationPath();
            close.Add(CloseString);
            var rep = new AnimationPath();
            rep.Add(OpenString);
            rep.Add(CloseString);

            var doorsPaths = new Dictionary<string, AnimationPlan>
            {
                { DefaultString, new AnimationPlan(def) },
                { OpenString, new AnimationPlan(open) },
                { CloseString, new AnimationPlan(close) },
                { RepString, new AnimationPlan(rep) }
            };

            doors[0].AnimationController.AppendPlan(doorsPaths[RepString]);

            animObjects.Add(doors);
        }
        private async Task InitializeJails()
        {
            var walls = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                "Walls",
                "Walls",
                new ModelInstancedDescription()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceDoors, "Wall2.json"),
                });

            walls[0].Manipulator.SetPosition(10, 0, 8);
            walls[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            walls[0].Manipulator.SetScaling(2.5f);

            var doors = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                "Jails",
                "Jails",
                new ModelInstancedDescription()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceDoors, "Dn_Jails.json"),
                });

            doors[0].Manipulator.SetPosition(10, 0, 8);
            doors[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            doors[0].Manipulator.SetScaling(2.5f);

            var def = new AnimationPath();
            def.Add(DefaultString);
            var open = new AnimationPath();
            open.Add(OpenString);
            var close = new AnimationPath();
            close.Add(CloseString);
            var rep = new AnimationPath();
            rep.Add(OpenString);
            rep.Add(CloseString);

            var jailsPaths = new Dictionary<string, AnimationPlan>
            {
                { DefaultString, new AnimationPlan(def) },
                { OpenString, new AnimationPlan(open) },
                { CloseString, new AnimationPlan(close) },
                { RepString, new AnimationPlan(rep) }
            };

            doors[0].AnimationController.AppendPlan(jailsPaths[RepString]);

            animObjects.Add(doors);
        }
        private async Task InitializeDebug()
        {
            itemTris = await AddComponent<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>(
                "DebugItemTris",
                "DebugItemTris",
                new PrimitiveListDrawerDescription<Triangle>() { Count = 5000, Color = itemTrisColor });

            itemLines = await AddComponent<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "DebugItemLines",
                "DebugItemLines",
                new PrimitiveListDrawerDescription<Line3D>() { Count = 1000, Color = itemLinesColor });
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                messages.Text = res.GetErrorMessage();
                messages.Visible = true;

                res.ThrowExceptions();
            }

            InitializeEnvironment();

            gameReady = true;
        }

        private void InitializeEnvironment()
        {
            Lights.KeyLight.CastShadow = true;
            Lights.KeyLight.Direction = Vector3.Normalize(new Vector3(-0.1f, -1, 1));
            Lights.KeyLight.Enabled = true;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = false;
            Lights.HemisphericLigth = new SceneLightHemispheric("Ambient", Color.Gray.RGB(), Color.White.RGB(), true);

            var bbox = new BoundingBox();
            animObjects.ForEach(item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    bbox = SharpDX.BoundingBox.Merge(bbox, item[i].GetBoundingBox());
                }
            });
            float playerHeight = bbox.Maximum.Y - bbox.Minimum.Y;

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 500;
            Camera.Goto(0, playerHeight, -12f);
            Camera.LookTo(0, playerHeight * 0.6f, 0);
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (!uiReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.Oem5))
            {
                console.Toggle();
            }

            if (!gameReady)
            {
                return;
            }

            UpdateInputCamera(gameTime);
            UpdateInputAnimation();
            UpdateInputDebug();

            UpdateDebugData();

            var itemController = animObjects[itemIndex][0].AnimationController;

            runtime.Text = Game.RuntimeText;
            animText.Text = $"Paths: {itemController}";
        }
        private void UpdateInputCamera(IGameTime gameTime)
        {
#if DEBUG
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    gameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                gameTime,
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
                Camera.MoveForward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(gameTime, Game.Input.ShiftPressed);
            }
        }
        private void UpdateInputAnimation()
        {
            if (Game.Input.KeyJustReleased(Keys.Left))
            {
                animObjects.ForEach(DecreaseDelta());
            }

            if (Game.Input.KeyJustReleased(Keys.Right))
            {
                animObjects.ForEach(IncreaseDelta());
            }

            if (Game.Input.KeyJustReleased(Keys.Up))
            {
                if (Game.Input.KeyPressed(Keys.ShiftKey))
                {
                    animObjects.ForEach(StartAnimation());
                }
                else
                {
                    animObjects.ForEach(ResumeAnimation());
                }
            }

            if (Game.Input.KeyJustReleased(Keys.Down))
            {
                animObjects.ForEach(PauseAnimation());
            }
        }
        private void UpdateInputDebug()
        {
            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            if (Game.Input.KeyJustReleased(Keys.C))
            {
                Lights.DirectionalLights[0].CastShadow = !Lights.DirectionalLights[0].CastShadow;
            }

            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                showItemDEBUG = !showItemDEBUG;
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                itemIndex--;
            }

            if (Game.Input.KeyJustReleased(Keys.F3))
            {
                itemIndex++;
            }

            itemIndex %= animObjects.Count;
            if (itemIndex < 0) itemIndex = animObjects.Count - 1;

            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                showItem = !showItem;
            }
        }
        private void UpdateDebugData()
        {
            var selectedItem = animObjects[itemIndex][0];

            animObjects.ForEach(item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    item[i].Visible = !showItemDEBUG || showItem || (item[i] != selectedItem);
                }
            });

            if (showItemDEBUG)
            {
                var tris = selectedItem.GetGeometry();
                var bbox = selectedItem.GetBoundingBox();

                itemTris.SetPrimitives(itemTrisColor, tris);
                itemLines.SetPrimitives(itemLinesColor, Line3D.CreateFromVertices(GeometryUtil.CreateBox(Topology.LineList, bbox)));

                itemTris.Active = itemTris.Visible = true;
                itemLines.Active = itemLines.Visible = true;
            }
            else
            {
                if (itemTris != null)
                {
                    itemTris.Active = itemTris.Visible = false;
                }

                if (itemLines != null)
                {
                    itemLines.Active = itemLines.Visible = false;
                }
            }
        }

        private void SoldierControllerPathEnding(object sender, AnimationControllerEventArgs e)
        {
            if (sender is AnimationController controller)
            {
                var transitionPlan = controller.GetTransitionPlan();
                var currentPlan = controller.GetCurrentPlan();

                if (!e.IsTransition && (transitionPlan?.Active ?? true))
                {
                    //Current plan ends, transition plan continues
                    return;
                }

                if (e.IsTransition && currentPlan.Active)
                {
                    //Transition ends, current plan continues
                    return;
                }

                var keys = soldierPaths.Keys.ToArray();

                while (true)
                {
                    int index = Helper.RandomGenerator.Next(0, 100) % keys.Length;

                    string planName = keys[index];
                    var newPlan = soldierPaths[planName];

                    if (newPlan.CurrentPath?.CurrentItem?.ClipName != currentPlan.CurrentPath?.CurrentItem?.ClipName)
                    {
                        controller.TransitionToPlan(newPlan);

                        break;
                    }
                }
            }
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            if (!uiReady)
            {
                return;
            }

            title.SetPosition(Vector2.Zero);
            runtime.SetPosition(new Vector2(5, title.AbsoluteRectangle.Bottom + 3));
            animText.SetPosition(new Vector2(5, runtime.AbsoluteRectangle.Bottom + 3));
            messages.SetPosition(new Vector2(5, animText.AbsoluteRectangle.Bottom + 3));

            backPanel.Width = Game.Form.RenderWidth;
            backPanel.Height = messages.AbsoluteRectangle.Bottom + 3;

            console.Top = backPanel.AbsoluteRectangle.Bottom;
            console.Width = Game.Form.RenderWidth;
        }
    }
}
