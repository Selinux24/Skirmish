using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Linq;
using System.Threading.Tasks;

namespace IntermediateSamples.SceneTransforms
{
    public class TransformsScene : Scene
    {
        private const float s = 10f;
        private const float l = 5f * s;
        private const float h = 0f;

        private const string resourcesTerrainFolder = "Common/Terrain/";
        private const string resourcesTerrainDiffuseTexture = resourcesTerrainFolder + "terrain.png";
        private const string resourcesTerrainNormalMapTexture = resourcesTerrainFolder + "terrain_nmap.png";

        private const string resourcesTreeFolder = "Common/Trees/";
        private const string resourcesTreeFile = "STree1.json";

        private UITextArea title = null;
        private UITextArea runtime = null;
        private UITextArea messages = null;
        private Sprite backPanel = null;
        private UIConsole console = null;

        private Model tree = null;
        private ModelInstance marker = null;

        private bool uiReady = false;
        private bool gameReady = false;

        public TransformsScene(Game game) : base(game)
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

            title.Text = "Transforms";
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
                    InitializeTree,
                    InitializeMarkers,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeFloor()
        {
            var geo = GeometryUtil.CreatePlane(l * 2, h, Vector3.Up);

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = resourcesTerrainDiffuseTexture;
            mat.NormalMapTexture = resourcesTerrainNormalMapTexture;

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
                    floor[i++].Manipulator.SetPosition(x * l * 2f, h, z * l * 2f);
                }
            }
        }
        private async Task InitializeTree()
        {
            const string modelName = "Tree1";

            var tDesc = new ModelDescription()
            {
                Content = ContentDescription.FromFile(resourcesTreeFolder, resourcesTreeFile),
                Optimize = true,
                PickingHull = PickingHullTypes.Hull,
                CastShadow = ShadowCastingAlgorihtms.Directional | ShadowCastingAlgorihtms.Spot,
                StartsVisible = true,
            };

            tree = await AddComponent<Model, ModelDescription>(modelName, modelName, tDesc, SceneObjectUsages.Object);
            tree.Manipulator.SetPosition(0, h, 0);
            tree.Manipulator.SetScaling(s);
        }
        private async Task InitializeMarkers()
        {
            float radius = 0.5f;
            int sliceCount = 32;
            int stackCount = 15;

            var geom = GeometryUtil.CreateSphere(Topology.TriangleList, radius, sliceCount, stackCount);

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseColor = Color.White;

            var desc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.None,
                Content = ContentDescription.FromContentData(geom, mat),
                Instances = 6,
            };

            var markers = await AddComponent<ModelInstanced, ModelInstancedDescription>("Marker", "Marker", desc, SceneObjectUsages.None);

            markers[0].Manipulator.SetPosition(0, h, 0);
            markers[1].Manipulator.SetPosition(+1 * l, h, +1 * l);
            markers[2].Manipulator.SetPosition(+1 * l, h, -1 * l);
            markers[3].Manipulator.SetPosition(-1 * l, h, -1 * l);
            markers[4].Manipulator.SetPosition(-1 * l, h, +1 * l);

            for (int i = 0; i < 5; i++)
            {
                markers[i].TintColor = Color.Yellow;
            }

            marker = markers[5];
            marker.TintColor = Color.Red;
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

            TreeController.Update(gameTime);

            UpdateInputCamera(gameTime);

            UpdateInputMarker(gameTime);

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
        private void UpdateInputMarker(IGameTime gameTime)
        {
            var pRay = GetPickingRay(PickingHullTypes.Perfect);

            if (!this.PickNearest(pRay, SceneObjectUsages.Object | SceneObjectUsages.Ground, out ScenePickingResult<Triangle> r))
            {
                return;
            }

            var position = r.PickingResult.Position;

            marker.Manipulator.SetPosition(position);

            if (Game.Input.MouseButtonJustReleased(MouseButtons.Left))
            {
                var treePos = tree.Manipulator.Position;
                treePos.Y = position.Y;
                var collisionVector = Vector3.Normalize(treePos - position);

                TreeController.AddFallingTree(tree, collisionVector, gameTime.TotalSeconds);
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
