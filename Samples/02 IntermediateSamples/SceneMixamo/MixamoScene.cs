﻿using Engine;
using Engine.Animation;
using Engine.BuiltIn.Components.Models;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Linq;
using System.Threading.Tasks;

namespace IntermediateSamples.SceneMixamo
{
    public class MixamoScene : Scene
    {
        private const string resourcesAsphaltFolder = "Common/Asphalt/";
        private const string resourcesAsphaltDiffuseFile = resourcesAsphaltFolder + "d_road_asphalt_stripes_diffuse.dds";
        private const string resourcesAsphaltNormalFile = resourcesAsphaltFolder + "d_road_asphalt_stripes_normal.dds";
        private const string resourcesAsphaltSpecularFile = resourcesAsphaltFolder + "d_road_asphalt_stripes_specular.dds";

        private const string resourceModelFolder = "SceneMixamo/resources/";
        private const string resourceModelFile = "TestModel.json";

        private UITextArea title = null;
        private UITextArea runtime = null;
        private UITextArea messages = null;
        private Sprite backPanel = null;
        private UIConsole console = null;

        private Model model = null;
        private readonly Vector3 modelInitPosition = new(0, 0, 0);

        private bool uiReady = false;
        private bool gameReady = false;

        public MixamoScene(Game game) : base(game)
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
            messages = await AddComponentUI<UITextArea, UITextAreaDescription>("Messages", "Messages", new UITextAreaDescription { Font = defaultFont15, TextForeColor = Color.Orange, MaxTextLength = 256 });

            title.Text = "Mixamo Model";
            runtime.Text = "";
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
                    InitializeFloor,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
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

            var desc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(geo, mat),
                Instances = 9,
            };

            var floor = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Floor", "Floor", desc);

            int i = 0;
            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    floor[i++].Manipulator.SetPosition(x * l, h, z * l);
                }
            }
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                messages.Text = res.GetExceptions().FirstOrDefault()?.Message;
                messages.Visible = true;

                return;
            }

            StartEnvironment();

            InitializeModels();
        }
        private void StartEnvironment()
        {
            Lights.KeyLight.CastShadow = true;
            Lights.KeyLight.Direction = Vector3.Normalize(new Vector3(-0.1f, -1, 1));
            Lights.KeyLight.Enabled = true;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = false;
            Lights.HemisphericLigth = new SceneLightHemispheric("Ambient", Color.Gray.RGB(), Color.White.RGB(), true);

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 500;
            Camera.Goto(30, 25, -36f);
            Camera.LookTo(0, 10, 0);
        }

        private void InitializeModels()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeModel,
                ],
                InitializeModelsCompleted);

            LoadResources(group);
        }
        private async Task InitializeModel()
        {
            model = await AddComponent<Model, ModelDescription>(
                "TestModel",
                "TestModel",
                new()
                {
                    BlendMode = BlendModes.OpaqueTransparent,
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Content = ContentDescription.FromFile(resourceModelFolder, resourceModelFile),
                    StartsVisible = false,
                });

            model.Manipulator.SetTransform(modelInitPosition, 0, MathUtil.DegreesToRadians(-90), 0, 0.1f);

            var pDefault = new AnimationPath();
            pDefault.AddLoop("rumba");

            model.AnimationController.Start(new AnimationPlan(pDefault), 0);
        }
        private void InitializeModelsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                messages.Text = res.GetExceptions().FirstOrDefault()?.Message;
                messages.Visible = true;

                return;
            }

            model.Visible = true;

            gameReady = true;
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

            runtime.Text = Game.RuntimeText;
        }
        private void UpdateInputCamera(IGameTime gameTime)
        {
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    gameTime,
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
                Camera.MoveForward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(gameTime, Game.Input.ShiftPressed);
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
            messages.SetPosition(new Vector2(5, runtime.AbsoluteRectangle.Bottom + 3));

            backPanel.Width = Game.Form.RenderWidth;
            backPanel.Height = messages.AbsoluteRectangle.Bottom + 3 + ((messages.Height + 3) * 2);

            console.Top = backPanel.AbsoluteRectangle.Bottom;
            console.Width = Game.Form.RenderWidth;
        }
    }
}
