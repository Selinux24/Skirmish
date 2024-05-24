using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IntermediateSamples.SceneGardener
{
    public class GardenerScene : Scene
    {
        private const int dirtInstances = 3;
        private const float s = 10f;
        private const float l = 10f * s;
        private const float h = 0f;

        private const string resourceString = "SceneGardener/Resources/";
        private const string resourceDirtDiffuseString = "dirt002.dds";
        private const string resourceDirtNormalString = "normal001.dds";
        private const string resourceFoliageString = "SceneGardener/Resources/Foliage/";
        private const string resourceFoliageMap = "mapTest.png";

        private UITextArea title = null;
        private UITextArea runtime = null;
        private UITextArea messages = null;
        private Sprite backPanel = null;
        private UIConsole console = null;

        private Model pov = null;
        private Model map = null;

        private bool uiReady = false;
        private bool gameReady = false;

        public GardenerScene(Game game) : base(game)
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
            runtime = await AddComponentUI<UITextArea, UITextAreaDescription>("Runtime", "Runtime", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow });
            messages = await AddComponentUI<UITextArea, UITextAreaDescription>("Messages", "Messages", new UITextAreaDescription { Font = defaultFont15, TextForeColor = Color.Orange });

            title.Text = "Gardener";
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
                    InitializePointOfView,
                    InitializeDirt,
                    InitializeFoliageMap,
                    InitializeGrass,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializePointOfView()
        {
            var geo = GeometryUtil.CreateSphere(Topology.TriangleList, 0.5f, 32, 15);

            var mat = MaterialBlinnPhongContent.Default;

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(geo, mat),
                CastShadow = ShadowCastingAlgorihtms.All,
            };

            pov = await AddComponentAgent<Model, ModelDescription>("Sphere", "Sphere", desc);
            pov.Manipulator.SetPosition(0, h, 0);
        }
        private async Task InitializeDirt()
        {
            var geo = GeometryUtil.CreatePlane(l, h, Vector3.Up);

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = resourceString + resourceDirtDiffuseString;
            mat.NormalMapTexture = resourceString + resourceDirtNormalString;

            var desc = new ModelInstancedDescription()
            {
                Content = ContentDescription.FromContentData(geo, mat),
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Instances = dirtInstances * dirtInstances,
            };

            var dirt = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Dirt", "Dirt", desc);

            int m = (int)MathF.Truncate(dirtInstances * 0.5f);
            int from = m - dirtInstances + 1;
            int to = dirtInstances - m;

            int i = 0;
            for (int x = from; x < to; x++)
            {
                for (int z = from; z < to; z++)
                {
                    dirt[i++].Manipulator.SetPosition(x * l, h, z * l);
                }
            }
        }
        private async Task InitializeFoliageMap()
        {
            var geo = GeometryUtil.CreatePlane(l * dirtInstances, h + 0.1f, Vector3.Up);

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = resourceFoliageString + resourceFoliageMap;
            mat.IsTransparent = true;

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(geo, mat),
                BlendMode = BlendModes.Alpha,
                UseAnisotropicFiltering = true,
                StartsVisible = false,
            };

            map = await AddComponentEffect<Model, ModelDescription>("Map", "Map", desc);
            map.TintColor = new Color4(1f, 1f, 1f, 0.5f);
        }
        private async Task InitializeGrass()
        {
            float areaSize = l * dirtInstances * 0.5f;

            float startRadius = 0f;
            float endRadius = 500f;
            float windEffect = 0f;
            Vector2 minSize = new(3f, 3f);
            Vector2 maxSize = new(5f, 5f);

            var vDesc = new FoliageDescription()
            {
                ContentPath = resourceFoliageString,
                VegetationMap = resourceFoliageMap,
                PlantingArea = new(new(-areaSize), new(areaSize)),
                BlendMode = BlendModes.Opaque,
                ColliderType = ColliderTypes.None,
                PathFindingHull = PickingHullTypes.None,
                PickingHull = PickingHullTypes.None,
                CullingVolumeType = CullingVolumeTypes.None,
                CastShadow = ShadowCastingAlgorihtms.None,

                ChannelRed = new FoliageDescription.Channel()
                {
                    Seed = 1,
                    Instances = GroundGardenerPatchInstances.Default,
                    StartRadius = startRadius,
                    EndRadius = endRadius,
                    WindEffect = windEffect,
                    MinSize = minSize,
                    MaxSize = maxSize,
                    Density = 1f,
                    VegetationTextures = ["grass_v.dds"],
                    Enabled = true,
                },
                ChannelGreen = new FoliageDescription.Channel()
                {
                    Seed = 2,
                    Instances = GroundGardenerPatchInstances.Default,
                    StartRadius = startRadius,
                    EndRadius = endRadius,
                    WindEffect = windEffect,
                    MinSize = minSize,
                    MaxSize = maxSize,
                    Density = 1f,
                    VegetationTextures = ["grass_d.dds"],
                    VegetationNormalMaps = ["grass_n.dds"],
                    Enabled = true,
                },
                ChannelBlue = new FoliageDescription.Channel()
                {
                    Seed = 3,
                    Instances = GroundGardenerPatchInstances.Default,
                    StartRadius = startRadius,
                    EndRadius = endRadius,
                    WindEffect = windEffect,
                    MinSize = minSize,
                    MaxSize = maxSize,
                    Density = 1f,
                    VegetationTextures = ["grass_p.png"],
                    Enabled = true,
                },
            };
            var grass = await AddComponentEffect<Foliage, FoliageDescription>("Grass", "Grass", vDesc);
            grass.Visible = true;
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                messages.Text = res.GetExceptions().FirstOrDefault()?.Message;
                messages.Visible = true;

                return;
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

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 1000;
            Camera.Goto(60, 50 + h, -70);
            Camera.LookTo(0, 10 + h, 0);
            Camera.MovementDelta = 40f;
            Camera.SlowMovementDelta = 10f;
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

            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                map.Visible = !map.Visible;
            }

            UpdateInputCamera(gameTime);

            runtime.Text = Game.RuntimeText;
        }
        private void UpdateInputCamera(IGameTime gameTime)
        {
            var camera = Camera;
            var input = Game.Input;
            bool slow = input.ShiftPressed;

            if (input.MouseButtonPressed(MouseButtons.Right))
            {
                camera.RotateMouse(
                    gameTime,
                    input.MouseXDelta,
                    input.MouseYDelta);
            }

            if (input.KeyPressed(Keys.A))
            {
                camera.MoveLeft(gameTime, slow);
            }

            if (input.KeyPressed(Keys.D))
            {
                camera.MoveRight(gameTime, slow);
            }

            if (input.KeyPressed(Keys.W))
            {
                camera.MoveForward(gameTime, slow);
            }

            if (input.KeyPressed(Keys.S))
            {
                camera.MoveBackward(gameTime, slow);
            }

            if (input.KeyPressed(Keys.Space))
            {
                camera.MoveUp(gameTime, slow);
            }

            if (input.KeyPressed(Keys.C))
            {
                camera.MoveDown(gameTime, slow);
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
