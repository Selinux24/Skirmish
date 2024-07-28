using Engine;
using Engine.Animation;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.Components.Primitives;
using Engine.BuiltIn.UI;
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
        private const string resourcesAsphaltFolder = "Common/Asphalt/";
        private const string resourcesAsphaltDiffuseFile = resourcesAsphaltFolder + "d_road_asphalt_stripes_diffuse.dds";
        private const string resourcesAsphaltNormalFile = resourcesAsphaltFolder + "d_road_asphalt_stripes_normal.dds";
        private const string resourcesAsphaltSpecularFile = resourcesAsphaltFolder + "d_road_asphalt_stripes_specular.dds";

        private const string SceneResource = "Common/";
        private const string SceneResourceDoors = SceneResource + "Doors/";
        private const string SceneResourceLadder = SceneResource + "Ladder/";
        private const string SceneResourceSoldier = SceneResource + "Soldier/";
        private const string SceneResourceRat = SceneResource + "Rat/";

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

        private const string laddersAString = "LadderA";
        private const string laddersBString = "LadderB";
        private const string laddersCString = "LadderC";
        private const string soldiersString = "Soldiers";
        private const string ratsString = "Rats";
        private const string doorsString = "Doors";
        private const string doorWallsString = "DoorWalls";
        private const string jailsString = "Jails";
        private const string jailWallsString = "JailWalls";

        private UITextArea title = null;
        private UITextArea runtime = null;
        private UITextArea animText = null;
        private UITextArea messages = null;
        private Sprite backPanel = null;
        private UIConsole console = null;

        private ModelInstanced laddersA;
        private ModelInstanced laddersB;
        private ModelInstanced laddersC;
        private ModelInstanced soldiers;
        private ModelInstanced rats;
        private ModelInstanced doors;
        private ModelInstanced jails;

        private GeometryColorDrawer<Triangle> itemTris = null;
        private GeometryColorDrawer<Line3D> itemLines = null;
        private readonly Color itemTrisColor = new(Color.Yellow.ToColor3(), 0.6f);
        private readonly Color itemLinesColor = new(Color.Red.ToColor3(), 1f);
        private bool showItemDEBUG = false;
        private bool showItem = true;
        private int itemIndex = 0;

        private static Action<ModelInstance> StartAnimation()
        {
            return item =>
            {
                item.AnimationController.Start(0);
            };
        }
        private static Action<ModelInstance> PauseAnimation()
        {
            return item =>
            {
                item.AnimationController.Pause();
            };
        }
        private static Action<ModelInstance> ResumeAnimation()
        {
            return item =>
            {
                item.AnimationController.Resume();
            };
        }
        private static Action<ModelInstance> IncreaseDelta()
        {
            return item =>
            {
                var controller = item.AnimationController;

                controller.TimeDelta += 0.1f;
                controller.TimeDelta = MathF.Min(5f, controller.TimeDelta);
            };
        }
        private static Action<ModelInstance> DecreaseDelta()
        {
            return item =>
            {
                var controller = item.AnimationController;

                controller.TimeDelta -= 0.1f;
                controller.TimeDelta = MathF.Max(0f, controller.TimeDelta);
            };
        }

        private readonly List<ModelInstance> animObjects = [];

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

        public override void Initialize()
        {
            base.Initialize();

            InitializeUI();
        }

        private void InitializeUI()
        {
            var group = LoadResourceGroup.FromTasks(
                InitializeUITitle,
                InitializeUICompleted);

            LoadResources(group);
        }
        private async Task InitializeUITitle()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Consolas", 18);
            var defaultFont15 = TextDrawerDescription.FromFamily("Consolas", 15);
            var defaultFont11 = TextDrawerDescription.FromFamily("Consolas", 11);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            runtime = await AddComponentUI<UITextArea, UITextAreaDescription>("Runtime", "Runtime", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });
            animText = await AddComponentUI<UITextArea, UITextAreaDescription>("AnimText", "AnimText", new UITextAreaDescription { Font = defaultFont15, TextForeColor = Color.Orange, MaxTextLength = 128 });
            messages = await AddComponentUI<UITextArea, UITextAreaDescription>("Messages", "Messages", new UITextAreaDescription { Font = defaultFont15, TextForeColor = Color.Orange, MaxTextLength = 256 });

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
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeLadder,
                    InitializeSoldier,
                    InitializeRat,
                    InitializeDoors,
                    InitializeJails,
                    InitializeFloor,
                    InitializeDebug,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeFloor()
        {
            float l = 15f;
            float h = 0f;

            var geo = GeometryUtil.CreatePlane(l * 2, h, Vector3.Up);

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = resourcesAsphaltDiffuseFile;
            mat.NormalMapTexture = resourcesAsphaltNormalFile;
            mat.SpecularTexture = resourcesAsphaltSpecularFile;

            var desc = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(geo, mat),
            };

            await AddComponent<Model, ModelDescription>("Floor", "Floor", desc);
        }
        private async Task InitializeLadder()
        {
            var def = new AnimationPath();
            def.Add(DefaultString);
            var pull = new AnimationPath();
            pull.Add(PullString);
            var push = new AnimationPath();
            push.Add(PushString);

            var ladderPaths = new Dictionary<string, AnimationPlan>
            {
                { DefaultString, new(def) },
                { PullString, new(pull) },
                { PushString, new(push) }
            };

            laddersA = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                laddersAString,
                laddersAString,
                new()
                {
                    Instances = 2,
                    CastShadow = ShadowCastingAlgorihtms.All,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceLadder, "Dn_Anim_Ladder.json"),
                });

            laddersA[0].Manipulator.SetPosition(-4f, 1, 0);
            laddersA[1].Manipulator.SetPosition(-4.5f, 1, 5);

            laddersA[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            laddersA[1].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);

            laddersA[0].AnimationController.AppendPlan(ladderPaths[PullString]);
            laddersA[1].AnimationController.AppendPlan(ladderPaths[PullString]);

            laddersB = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                laddersBString,
                laddersBString,
                new()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceLadder, "Dn_Anim_Ladder_2.json"),
                });

            laddersB[0].Manipulator.SetPosition(-3f, 1, 0);
            laddersB[1].Manipulator.SetPosition(-3.5f, 1, 5);

            laddersB[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            laddersB[1].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);

            laddersB[0].AnimationController.AppendPlan(ladderPaths[PullString]);
            laddersB[1].AnimationController.AppendPlan(ladderPaths[PullString]);

            laddersC = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                laddersCString,
                laddersCString,
                new()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceLadder, "Dn_Anim_Ladder_22.json"),
                });

            laddersC[0].Manipulator.SetPosition(-2f, 1, 0);
            laddersC[1].Manipulator.SetPosition(-2.5f, 1, 5);

            laddersC[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            laddersC[1].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);

            laddersC[0].AnimationController.AppendPlan(ladderPaths[PushString]);
            laddersC[1].AnimationController.AppendPlan(ladderPaths[PushString]);
        }
        private async Task InitializeSoldier()
        {
            soldiers = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                soldiersString,
                soldiersString,
                new()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceSoldier, "soldier_anim2.json"),
                });

            soldiers[0].Manipulator.SetPosition(0, 0, 0);
            soldiers[1].Manipulator.SetPosition(0.5f, 0, 5);

            soldiers[0].AnimationController.PlanEnding += SoldierControllerPathEnding;
            soldiers[1].AnimationController.PlanEnding += SoldierControllerPathEnding;

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

            soldierPaths.Add(Idle1String, new(p1));
            soldierPaths.Add(Idle2String, new(p2));
            soldierPaths.Add(StandString, new(p3));
            soldierPaths.Add(WalkString, new(p4));
            soldierPaths.Add(RunString, new(p5));

            soldiers[0].AnimationController.AppendPlan(soldierPaths[Idle1String]);
            soldiers[1].AnimationController.AppendPlan(soldierPaths[Idle1String]);
        }
        private async Task InitializeRat()
        {
            rats = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                ratsString,
                ratsString,
                new()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceRat, "rat.json"),
                });

            rats[0].Manipulator.SetPosition(2, 0, 0);
            rats[1].Manipulator.SetPosition(2.5f, 0, 5);

            var p0 = new AnimationPath();
            p0.AddLoop(WalkString);

            ratPaths.Add(WalkString, new(p0));

            rats[0].AnimationController.AppendPlan(ratPaths[WalkString]);
            rats[1].AnimationController.AppendPlan(ratPaths[WalkString]);
        }
        private async Task InitializeDoors()
        {
            doors = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                doorsString,
                doorsString,
                new()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceDoors, "Dn_Doors.json"),
                });

            var walls = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                doorWallsString,
                doorWallsString,
                new()
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
                { DefaultString, new(def) },
                { OpenString, new(open) },
                { CloseString, new(close) },
                { RepString, new(rep) }
            };

            doors[0].AnimationController.AppendPlan(doorsPaths[RepString]);
        }
        private async Task InitializeJails()
        {
            jails = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                jailsString,
                jailsString,
                new()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceDoors, "Dn_Jails.json"),
                });

            jails[0].Manipulator.SetPosition(10, 0, 8);
            jails[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            jails[0].Manipulator.SetScaling(2.5f);

            var walls = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                jailWallsString,
                jailWallsString,
                new()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(SceneResourceDoors, "Wall2.json"),
                });

            walls[0].Manipulator.SetPosition(10, 0, 8);
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

            var jailsPaths = new Dictionary<string, AnimationPlan>
            {
                { DefaultString, new(def) },
                { OpenString, new(open) },
                { CloseString, new(close) },
                { RepString, new(rep) }
            };

            jails[0].AnimationController.AppendPlan(jailsPaths[RepString]);
        }
        private async Task InitializeDebug()
        {
            const string dbItemTrisString = "DebugItemTris";
            const string dbItemLinesString = "DebugItemLines";

            itemTris = await AddComponentEffect<GeometryColorDrawer<Triangle>, GeometryColorDrawerDescription<Triangle>>(
                dbItemTrisString,
                dbItemTrisString,
                new() { Count = 5000, Color = itemTrisColor, DepthEnabled = false });

            itemLines = await AddComponentEffect<GeometryColorDrawer<Line3D>, GeometryColorDrawerDescription<Line3D>>(
                dbItemLinesString,
                dbItemLinesString,
                new() { Count = 1000, Color = itemLinesColor, DepthEnabled = false });
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                messages.Text = res.GetErrorMessage();
                messages.Visible = true;

                return;
            }

            animObjects.AddRange(laddersA?.GetInstances() ?? []);
            animObjects.AddRange(laddersB?.GetInstances() ?? []);
            animObjects.AddRange(laddersC?.GetInstances() ?? []);
            animObjects.AddRange(soldiers?.GetInstances() ?? []);
            animObjects.AddRange(rats?.GetInstances() ?? []);
            animObjects.AddRange(doors?.GetInstances() ?? []);
            animObjects.AddRange(jails?.GetInstances() ?? []);

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
                bbox = SharpDX.BoundingBox.Merge(bbox, item.GetBoundingBox());
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

            var item = animObjects[itemIndex];
            var itemController = item.AnimationController;

            runtime.Text = Game.RuntimeText;
            animText.Text = $"{item} - Paths: {itemController}";
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
            var selectedItem = animObjects[itemIndex];

            animObjects.ForEach(item =>
            {
                item.Visible = !showItemDEBUG || showItem || (item != selectedItem);
            });

            if (showItemDEBUG)
            {
                var tris = selectedItem.GetGeometry();
                var bbox = selectedItem.GetBoundingBox();

                itemTris.SetPrimitives(itemTrisColor, tris);
                itemLines.SetPrimitives(itemLinesColor, Line3D.CreateBox(bbox));

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
