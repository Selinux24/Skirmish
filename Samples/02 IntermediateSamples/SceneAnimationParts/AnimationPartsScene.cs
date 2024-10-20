﻿using Engine;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.Components.Primitives;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Threading.Tasks;

namespace IntermediateSamples.SceneAnimationParts
{
    class AnimationPartsScene : Scene
    {
        private const string resourcesLeopardFolder = "Common/Leopard/";
        private const string resourcesLeopardFile = "Leopard.json";

        private const string resourcesAsphaltFolder = "Common/Asphalt/";
        private const string resourcesAsphaltDiffuseFile = resourcesAsphaltFolder + "d_road_asphalt_stripes_diffuse.dds";
        private const string resourcesAsphaltNormalFile = resourcesAsphaltFolder + "d_road_asphalt_stripes_normal.dds";
        private const string resourcesAsphaltSpecularFile = resourcesAsphaltFolder + "d_road_asphalt_stripes_specular.dds";

        private UITextArea title = null;
        private UIPanel backPanel = null;
        private UIConsole console = null;

        private PrimitiveListDrawer<Triangle> itemTris = null;
        private PrimitiveListDrawer<Line3D> itemLines = null;
        private readonly Color sphTrisColor = new(Color.Red.ToColor3(), 0.25f);
        private readonly Color sphLinesColor = new(Color.Red.ToColor3(), 1f);
        private readonly Color boxTrisColor = new(Color.Green.ToColor3(), 0.25f);
        private readonly Color boxLinesColor = new(Color.Green.ToColor3(), 1f);
        private readonly Color obbTrisColor = new(Color.Blue.ToColor3(), 0.25f);
        private readonly Color obbLinesColor = new(Color.Blue.ToColor3(), 1f);

        private Model tank;

        private bool uiReady = false;
        private bool gameReady = false;

        public AnimationPartsScene(Game game) : base(game)
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

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });

            title.Text = "Model Parts Test";

            backPanel = await AddComponentUI<UIPanel, UIPanelDescription>("Backpanel", "Backpanel", UIPanelDescription.Default(new Color4(0, 0, 0, 0.75f)), LayerUI - 1);

            var consoleDesc = UIConsoleDescription.Default(new Color4(0.35f, 0.35f, 0.35f, 1f));
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
                    InitializeTank,
                    InitializeFloor,
                    InitializeDebug,
                ],
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    StartEnvironment();

                    gameReady = true;
                });

            LoadResources(group);
        }
        private async Task InitializeTank()
        {
            var tDesc = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Optimize = false,
                Content = ContentDescription.FromFile(resourcesLeopardFolder, resourcesLeopardFile),
                TransformNames =
                [
                    "Hull-mesh",
                    "Turret-mesh",
                    "Barrel-mesh"
                ],
                TransformDependences =
                [
                    -1,
                    0,
                    1
                ],
            };

            tank = await AddComponentAgent<Model, ModelDescription>("Tanks", "Tanks", tDesc);
            tank.Manipulator.SetScaling(0.5f);
        }
        private async Task InitializeFloor()
        {
            float l = 100f;
            float h = 0f;

            var geo = GeometryUtil.CreatePlane(l, h, Vector3.Up);

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
        private async Task InitializeDebug()
        {
            itemTris = await AddComponent<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>(
                "DebugItemTris",
                "DebugItemTris",
                new PrimitiveListDrawerDescription<Triangle>() { Count = 5000, BlendMode = BlendModes.Alpha });
            itemTris.Visible = false;

            itemLines = await AddComponent<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "DebugItemLines",
                "DebugItemLines",
                new PrimitiveListDrawerDescription<Line3D>() { Count = 1000, BlendMode = BlendModes.Alpha });
            itemLines.Visible = false;
        }
        private void StartEnvironment()
        {
            Lights.KeyLight.CastShadow = true;
            Lights.KeyLight.Direction = Vector3.Normalize(new Vector3(-0.1f, -1, 1));
            Lights.KeyLight.Enabled = true;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = false;
            Lights.HemisphericLigth = new SceneLightHemispheric("Ambient", Color.Gray.RGB(), Color.White.RGB(), true);

            BoundingBox bbox = tank.GetBoundingBox();
            float playerHeight = bbox.Maximum.Y - bbox.Minimum.Y;
            Vector3 lookDir = Vector3.Normalize(new Vector3(0, playerHeight, -playerHeight * 2f)) * 50;

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 500;
            Camera.Goto(lookDir);
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
            UpdateInputTank(gameTime);
            UpdateInputDebug();

            UpdateDebugData();
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
        private void UpdateInputTank(IGameTime gameTime)
        {
            if (Game.Input.ShiftPressed)
            {
                UpdateInputHull(gameTime);
            }
            else
            {
                UpdateInputTurret(gameTime);
            }
        }
        private void UpdateInputHull(IGameTime gameTime)
        {
            if (Game.Input.KeyPressed(Keys.J))
            {
                tank.Manipulator.YawLeft(gameTime);
            }
            if (Game.Input.KeyPressed(Keys.L))
            {
                tank.Manipulator.YawRight(gameTime);
            }

            if (Game.Input.KeyPressed(Keys.I))
            {
                tank.Manipulator.MoveForward(gameTime, 10);
            }
            if (Game.Input.KeyPressed(Keys.K))
            {
                tank.Manipulator.MoveBackward(gameTime, 10);
            }
        }
        private void UpdateInputTurret(IGameTime gameTime)
        {
            if (Game.Input.KeyPressed(Keys.J))
            {
                tank.GetModelPartByName("Turret-mesh").Manipulator.YawLeft(gameTime);
            }
            if (Game.Input.KeyPressed(Keys.L))
            {
                tank.GetModelPartByName("Turret-mesh").Manipulator.YawRight(gameTime);
            }

            if (Game.Input.KeyPressed(Keys.I))
            {
                tank.GetModelPartByName("Barrel-mesh").Manipulator.PitchUp(gameTime);
            }
            if (Game.Input.KeyPressed(Keys.K))
            {
                tank.GetModelPartByName("Barrel-mesh").Manipulator.PitchDown(gameTime);
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
        }

        private void UpdateDebugData()
        {
            var sph = tank.GetBoundingSphere();
            var box = tank.GetBoundingBox();
            var obb = tank.GetOrientedBoundingBox();

            itemTris.SetPrimitives(sphTrisColor, Triangle.ComputeTriangleList(sph, 20, 20));
            itemTris.SetPrimitives(boxTrisColor, Triangle.ComputeTriangleList(box));
            itemTris.SetPrimitives(obbTrisColor, Triangle.ComputeTriangleList(obb));
            itemTris.Active = itemTris.Visible = true;

            itemLines.SetPrimitives(sphLinesColor, Line3D.CreateSphere(sph, 20, 20));
            itemLines.SetPrimitives(boxLinesColor, Line3D.CreateBox(box));
            itemLines.SetPrimitives(obbLinesColor, Line3D.CreateBox(obb));
            itemLines.Active = itemLines.Visible = true;
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

            backPanel.Width = Game.Form.RenderWidth;
            backPanel.Height = title.AbsoluteRectangle.Bottom + 3;

            console.Top = backPanel.AbsoluteRectangle.Bottom;
            console.Width = Game.Form.RenderWidth;
        }
    }
}
