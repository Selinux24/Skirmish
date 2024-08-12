using Engine;
using Engine.Animation;
using Engine.BuiltIn.Components.Foliage;
using Engine.BuiltIn.Components.Geometry;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.UI;
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
        private const string resourceFoliageDiffuse1File = "grass_v.dds";
        private const string resourceFoliageDiffuse2File = "grass_d.dds";
        private const string resourceFoliageNormal2File = "grass_n.dds";
        private const string resourceFoliageDiffuse3File = "grass_p.png";
        private const string resourceFoliageMap = "mapTest.png";

        private const string noHelpText = "Press F1 for help.";
        private const string helpText = @"
F1  - Hide this help.
F2  - Show foliage map
F3  - Show foliage areas
TAB - Change control between Camera and Agent
   + Camera: WASD-Space-C to move. Left mouse click to rotate. Left mouse click + shift to change agent position.
   + Agent : WASD to move. Mouse to rotate.";

        private UITextArea title = null;
        private UITextArea runtime = null;
        private UITextArea help = null;
        private UITextArea messages = null;
        private Sprite backPanel = null;
        private UIConsole console = null;

        private const float povRadius = 1.5f;
        private const float povHeight = 6f;
        private Model pov = null;

        private const float grassStartRadius = 5f;
        private const float grassEndRadius = 50f;
        private Foliage grass = null;

        private Model map = null;

        private GeometryColorDrawer<Triangle> itemTris = null;
        private GeometryColorDrawer<Line3D> itemLines = null;
        private readonly Color gbLinesColor = new(Color.Cyan.ToColor3(), 0.5f);
        private readonly Color grTrisColor = new(Color.Blue.ToColor3(), 0.15f);
        private readonly Color grLinesColor = new(Color.Blue.ToColor3(), 0.5f);
        private readonly Color fTrisColor = new(Color.Green.ToColor3(), 0.15f);
        private readonly Color empTrisColor = new(Color.Orange.ToColor3(), 0.15f);
        private readonly Color errTrisColor = new(Color.Red.ToColor3(), 0.15f);
        private readonly Color frTrisColor = new(Color.White.ToColor3(), 0.15f);
        private readonly Color frLinesColor = new(Color.White.ToColor3(), 0.5f);
        private readonly Color crStartTrisColor = new(Color.IndianRed.ToColor3(), 0.15f);
        private readonly Color crStartLinesColor = new(Color.IndianRed.ToColor3(), 1f);
        private readonly Color crEndTrisColor = new(Color.WhiteSmoke.ToColor3(), 0.15f);
        private readonly Color crEndLinesColor = new(Color.WhiteSmoke.ToColor3(), 1f);

        private bool showHelp = false;
        private bool freeCamera = true;

        private bool uiReady = false;
        private bool gameReady = false;

        public GardenerScene(Game game) : base(game)
        {
            Game.VisibleMouse = true;
            Game.LockMouse = false;

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
            var defaultFont18 = FontDescription.FromFamily("Consolas", 18);
            var defaultFont15 = FontDescription.FromFamily("Consolas", 15);
            var defaultFont11 = FontDescription.FromFamily("Consolas", 11);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            runtime = await AddComponentUI<UITextArea, UITextAreaDescription>("Runtime", "Runtime", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });
            help = await AddComponentUI<UITextArea, UITextAreaDescription>("Help", "Help", new UITextAreaDescription { Font = defaultFont15, TextForeColor = Color.Orange, MaxTextLength = 256 });
            messages = await AddComponentUI<UITextArea, UITextAreaDescription>("Messages", "Messages", new UITextAreaDescription { Font = defaultFont15, TextForeColor = Color.Orange, MaxTextLength = 128 });

            title.Text = "Gardener";
            runtime.Text = "";
            help.Text = "";
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
                    InitializeDebug,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializePointOfView()
        {
            var geo = GeometryUtil.CreateCapsule(Topology.TriangleList, povRadius, povHeight, 32, 15);

            var mat = MaterialBlinnPhongContent.Default;

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(geo, mat),
                CastShadow = ShadowCastingAlgorihtms.All,
            };

            pov = await AddComponentAgent<Model, ModelDescription>("Capsule", "Capsule", desc);
            pov.Manipulator.SetPosition(0, h + (povHeight * 0.5f), 0);
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
            map.TintColor = new Color4(1f, 1f, 1f, 0.1f);
        }
        private async Task InitializeGrass()
        {
            float areaSize = l * dirtInstances * 0.5f;

            float windEffect = 0.3333f;
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
                NodeSize = 24,

                ChannelRed = new FoliageDescription.Channel()
                {
                    Seed = 1,
                    Instances = GroundGardenerPatchInstances.Default,
                    StartRadius = grassStartRadius,
                    EndRadius = grassEndRadius,
                    WindEffect = windEffect,
                    MinSize = minSize,
                    MaxSize = maxSize,
                    Density = 1f,
                    VegetationTextures = [resourceFoliageDiffuse1File],
                    Enabled = true,
                },
                ChannelGreen = new FoliageDescription.Channel()
                {
                    Seed = 2,
                    Instances = GroundGardenerPatchInstances.Default,
                    StartRadius = grassStartRadius,
                    EndRadius = grassEndRadius,
                    WindEffect = windEffect,
                    MinSize = minSize,
                    MaxSize = maxSize,
                    Density = 1f,
                    VegetationTextures = [resourceFoliageDiffuse2File],
                    VegetationNormalMaps = [resourceFoliageNormal2File],
                    Enabled = true,
                },
                ChannelBlue = new FoliageDescription.Channel()
                {
                    Seed = 3,
                    Instances = GroundGardenerPatchInstances.Default,
                    StartRadius = grassStartRadius,
                    EndRadius = grassEndRadius,
                    WindEffect = windEffect,
                    MinSize = minSize,
                    MaxSize = maxSize,
                    Density = 1f,
                    VegetationTextures = [resourceFoliageDiffuse3File],
                    Enabled = true,
                },
            };
            grass = await AddComponentEffect<Foliage, FoliageDescription>("Grass", "Grass", vDesc);
            grass.UseCameraAsPointOfView = false;
            grass.Visible = true;
        }
        private async Task InitializeDebug()
        {
            const string itemTrisName = nameof(itemTris);
            const string itemLinesName = nameof(itemLines);

            itemTris = await AddComponent<GeometryColorDrawer<Triangle>, GeometryColorDrawerDescription<Triangle>>(
                itemTrisName,
                itemTrisName,
                new() { Count = 5000, BlendMode = BlendModes.Alpha, StartsVisible = false });

            itemLines = await AddComponent<GeometryColorDrawer<Line3D>, GeometryColorDrawerDescription<Line3D>>(
                itemLinesName,
                itemLinesName,
                new() { Count = 1000, BlendMode = BlendModes.Alpha, StartsVisible = false });
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
                showHelp = !showHelp;
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                map.Visible = !map.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F3))
            {
                itemLines.Visible = !itemLines.Visible;
                itemTris.Visible = itemLines.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.Tab))
            {
                freeCamera = !freeCamera;
            }

            if (freeCamera)
            {
                UpdateInputCamera(gameTime);

                UpdateInputPOV();
            }
            else
            {
                UpdateInputAgent(gameTime);
            }

            UpdateHelp();
            UpdatePOV();

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
        private void UpdateInputAgent(IGameTime gameTime)
        {
            var transform = pov.Manipulator;
            var input = Game.Input;
            float vel = input.ShiftPressed ? 8 : 16;
            float velX = input.MouseXDelta * 0.1f;

            if (input.MouseButtonPressed(MouseButtons.Right))
            {
                transform.Rotate(gameTime, velX, 0, 0);
            }

            if (input.KeyPressed(Keys.A))
            {
                transform.MoveLeft(gameTime, vel);
            }

            if (input.KeyPressed(Keys.D))
            {
                transform.MoveRight(gameTime, vel);
            }

            if (input.KeyPressed(Keys.W))
            {
                transform.MoveForward(gameTime, vel);
            }

            if (input.KeyPressed(Keys.S))
            {
                transform.MoveBackward(gameTime, vel);
            }

            var position = transform.Position;
            position.Y = povHeight * 0.5f;
            transform.SetPosition(position);

            var camPosition = position;
            camPosition.Y += povHeight;
            var interest = camPosition - transform.Forward;

            Camera.SetPosition(camPosition);
            Camera.SetInterest(interest);
        }
        private void UpdateInputPOV()
        {
            if (!Game.Input.MouseButtonPressed(MouseButtons.Left))
            {
                return;
            }

            var pRay = GetPickingRay(PickingHullTypes.Perfect);
            if (!this.PickNearest<Triangle>(pRay, SceneObjectUsages.Ground, out var r))
            {
                return;
            }

            if (Game.Input.ShiftPressed)
            {
                pov.Manipulator.SetPosition(r.PickingResult.Position + (povHeight * 0.5f));
            }
            else
            {
                pov.Manipulator.LookAt(r.PickingResult.Position + (povHeight * 0.5f));
            }
        }
        private void UpdateHelp()
        {
            if (showHelp)
            {
                help.Text = helpText;
            }
            else
            {
                help.Text = noHelpText;
            }

            UpdateLayout();
        }
        private void UpdatePOV()
        {
            var position = pov.Manipulator.Position;
            var interest = position - pov.Manipulator.Forward;

            var proj = Matrix.PerspectiveFovLH(Camera.FieldOfView, Camera.AspectRelation, 0.1f, l);
            var view = Matrix.LookAtLH(position, interest, Vector3.Up);
            var frustum = new BoundingFrustum(view * proj);

            grass.PointOfView = position;
            grass.PointOfViewFrustum = frustum;

            DrawGardenerNodes(grLinesColor, grTrisColor, fTrisColor, empTrisColor, errTrisColor);
            DrawGardenerBounds(gbLinesColor);
            DrawFrustum(frustum, frLinesColor, frTrisColor);
            DrawCircle(position, grassStartRadius, crStartLinesColor, crStartTrisColor);
            DrawCircle(position, grassEndRadius, crEndLinesColor, crEndTrisColor);
        }
        private void DrawFrustum(BoundingFrustum frustum, Color4 lColor, Color4 tColor)
        {
            var corners = frustum.GetCorners();
            var topQuad = Flatten([corners[2], corners[1], corners[5], corners[6]]);

            var lines = Line3D.CreatePolygon(topQuad);
            var tris = GeometryUtil.CreatePolygon(Topology.TriangleList, topQuad);

            itemLines.SetPrimitives(lColor, lines);
            itemTris.SetPrimitives(tColor, Triangle.ComputeTriangleList(tris.Vertices, tris.Indices));
        }
        private void DrawCircle(Vector3 position, float r, Color4 lColor, Color4 tColor)
        {
            if (r <= 0)
            {
                itemLines.Clear(lColor);
                itemTris.Clear(tColor);
            }

            position.Y = h;
            var lines = Line3D.CreateCircle(position, r, 32);
            var tris = GeometryUtil.CreateCircle(Topology.TriangleList, position, r, 32);

            itemLines.SetPrimitives(lColor, lines);
            itemTris.SetPrimitives(tColor, Triangle.ComputeTriangleList(tris.Vertices, tris.Indices));
        }
        private void DrawGardenerNodes(Color4 lColor, Color4 tColor, Color4 fColor, Color4 empColor, Color4 errColor)
        {
            itemLines.Clear(lColor);
            itemTris.Clear(tColor);
            itemTris.Clear(fColor);
            itemTris.Clear(empColor);
            itemTris.Clear(errColor);

            var allNodes = grass.GetAllNodes();
            var visibleNodes = grass.GetVisibleNodes();
            var patches = grass.GetPatches();

            foreach (var node in allNodes)
            {
                var corners = node.BoundingBox.GetCorners();
                var topQuad = Flatten([corners[1], corners[0], corners[4], corners[5]]);

                var lines = Line3D.CreatePolygon(topQuad);
                itemLines.AddPrimitives(lColor, lines);

                var tris = GeometryUtil.CreatePolygon(Topology.TriangleList, topQuad);

                bool isVisible = Array.Exists(visibleNodes, n => n == node);

                var pNodes = Array.FindAll(patches, n => n.Node == node);
                bool anyError = Array.Exists(pNodes, n => n.WithData && !n.Ready);
                bool allOk = Array.Exists(pNodes, n => !(n.WithData && n.Ready)) && Array.Exists(pNodes, n => n.WithData && n.Ready);

                if (isVisible && allOk)
                {
                    itemTris.AddPrimitives(fColor, Triangle.ComputeTriangleList(tris.Vertices, tris.Indices));
                }
                else if (isVisible && anyError)
                {
                    itemTris.AddPrimitives(errColor, Triangle.ComputeTriangleList(tris.Vertices, tris.Indices));
                }
                else if (allOk)
                {
                    itemTris.AddPrimitives(empColor, Triangle.ComputeTriangleList(tris.Vertices, tris.Indices));
                }
                else if (isVisible)
                {
                    itemTris.AddPrimitives(tColor, Triangle.ComputeTriangleList(tris.Vertices, tris.Indices));
                }
            }
        }
        private void DrawGardenerBounds(Color4 lColor)
        {
            var bbox = grass.GetPlantingBounds();
            bbox.Minimum = new Vector3(bbox.Minimum.X, h, bbox.Minimum.Z);

            var lines = Line3D.CreateBox(bbox);
            itemLines.SetPrimitives(lColor, lines);
        }
        private static Vector3[] Flatten(Vector3[] points)
        {
            Vector3[] res = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                res[i] = points[i];
                res[i].Y = h;
            }

            return res;
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
            help.SetPosition(new Vector2(5, runtime.AbsoluteRectangle.Bottom + 3));
            messages.SetPosition(new Vector2(5, help.AbsoluteRectangle.Bottom + 3));

            backPanel.Width = Game.Form.RenderWidth;
            backPanel.Height = help.AbsoluteRectangle.Top + 3 + (help.Height + 3);

            console.Top = backPanel.AbsoluteRectangle.Bottom;
            console.Width = Game.Form.RenderWidth;
        }
    }
}
