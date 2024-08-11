using AISamples.Common;
using AISamples.Common.Agents;
using AISamples.Common.Persistence;
using AISamples.SceneCWRVirtualWorld.Editors;
using Engine;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.UI;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AISamples.SceneCWRVirtualWorld
{
    /// <summary>
    /// Coding with Radu scene
    /// </summary>
    /// <remarks>
    /// It's a engine capacity test scene, trying to simulate a virtual world, using the Radu's course as reference:
    /// https://www.youtube.com/playlist?list=PLB0Tybl0UNfYoJE7ZwsBQoDIG4YN9ptyY
    /// https://www.youtube.com/playlist?list=PLB0Tybl0UNfZtY5IQl1aNwcoOPJNtnPEO
    /// https://github.com/gniziemazity/virtual-world
    /// https://radufromfinland.com/projects/virtualworld/
    /// </remarks>
    class VirtualWorldScene : Scene
    {
        private const int layerHUD = 99;
        private const float spaceSize = 150000f;
        private const string resourcesFolder = "SceneCWRVirtualWorld";
        private const string bestCarFileName = "bestCar.json";

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea runtimeText = null;
        private UITextArea info = null;

        private UIButton[] editorButtons;

        private Model terrain = null;

        private const string editorFont = "Consolas";
        private const int editorButtonWidth = 100;
        private const int editorButtonHeight = 25;
        private readonly Color editorButtonColor = Color.LightGray;
        private readonly Color editorButtonTextColor = Color.Black;

        private const string titleText = "A VIRTUAL WORLD";
        private const string infoText = "PRESS F1 FOR HELP";
        private const string helpText = @"F1 - CLOSE THIS HELP
F2 - TOGGLE TOOLS
F5 - ADDS A CAR TO THE WORLD
F6 - ADDS A MUTATED CAR
F7 - ADDS N CARS PRESERVING FIRST BRAIN
F8 - SAVE BEST CAR BRAIN
F - TOGGLE BEST CAR FOLLOWING
R - REMOVE BEST CAR

WASD - MOVE CAMERA
SPACE - MOVE CAMERA UP
C - MOVE CAMERA DOWN
MOUSE - ROTATE CAMERA

ESC - EXIT";
        private bool showHelp = false;

        private bool gameReady = false;
        private bool toolsReady = false;
        private bool toolsVisible = false;

        private readonly Graph graph = new([], []);
        private readonly World world;
        private readonly Tools tools;

        private bool followCar = false;
        private readonly AgentFollower carFollower = new(100, 50);

        public VirtualWorldScene(Game game) : base(game)
        {
            Game.VisibleMouse = true;
            Game.LockMouse = false;

            GameEnvironment.Background = Color.CornflowerBlue;

            Lights.SetAmbient(new SceneLightHemispheric("Ambient", Color3.White, Color3.Black, true));

            world = new(graph, 0);
            world.Generate();

            tools = new(this, world);
            tools.AddEditor<GraphEditor>(EditorModes.Graph, 0);
            tools.AddEditor<StartsEditor>(EditorModes.Start, 0);
            tools.AddEditor<TargetsEditor>(EditorModes.Target, 0);
            tools.AddEditor<StopsEditor>(EditorModes.Stops, 0);
            tools.AddEditor<YieldEditor>(EditorModes.Yields, 0);
            tools.AddEditor<LightsEditor>(EditorModes.Lights, 0);
            tools.AddEditor<CrossingsEditor>(EditorModes.Crossings, 0);
            tools.AddEditor<ParkingsEditor>(EditorModes.Parkings, 0);

            carFollower.Car = world.GetBestCar;
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
                    InitializeTitle,
                    InitializeTexts,
                    InitializeToolsButtons,
                    InitializeTerrain,
                    InitializeWorld,
                    InitializeTools,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeTitle()
        {
            var defaultFont18 = FontDescription.FromFamily("Gill Sans MT, Arial", 18);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            title.Text = titleText;

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.66f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Panel", "Panel", spDesc, LayerUI - 1);
        }
        private async Task InitializeTexts()
        {
            var defaultFont11 = FontDescription.FromFamily("Gill Sans MT, Arial", 11);

            runtimeText = await AddComponentUI<UITextArea, UITextAreaDescription>("RuntimeText", "RuntimeText", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });
            info = await AddComponentUI<UITextArea, UITextAreaDescription>("Information", "Information", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });

            runtimeText.Text = "";
            info.Text = infoText;
        }
        private async Task InitializeToolsButtons()
        {
            var buttonsFont = FontDescription.FromFamily(editorFont, 10, FontMapStyles.Regular, true);
            buttonsFont.ContentPath = resourcesFolder;

            var editorButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont);
            editorButtonDesc.ContentPath = resourcesFolder;
            editorButtonDesc.Width = editorButtonWidth;
            editorButtonDesc.Height = editorButtonHeight;
            editorButtonDesc.ColorReleased = editorButtonColor;
            editorButtonDesc.ColorPressed = new Color4(editorButtonColor.RGB() * 1.2f, 1f);
            editorButtonDesc.TextForeColor = editorButtonTextColor;
            editorButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            editorButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            List<UIButton> buttons = [];

            buttons.Add(await InitializeButton("editorLoadOSMButton", "LOAD OSM", editorButtonDesc, LoadFromOpenStreetMap));
            buttons.Add(await InitializeButton("editorLoadButton", "LOAD WORLD", editorButtonDesc, LoadWorld));
            buttons.Add(await InitializeButton("editorSaveButton", "SAVE WORLD", editorButtonDesc, SaveWorld));
            buttons.Add(null);
            foreach (var mode in tools.GetModes())
            {
                buttons.Add(await InitializeButton($"editor{mode}", $"{mode.ToString().ToUpper()} EDITOR", editorButtonDesc, () => tools.SetEditor(mode)));
            }
            buttons.Add(null);
            buttons.Add(await InitializeButton("editorClearButton", "CLEAR", editorButtonDesc, world.Clear));

            editorButtons = [.. buttons];
        }
        private async Task<UIButton> InitializeButton(string name, string caption, UIButtonDescription desc, Action callback)
        {
            var button = await AddComponentUI<UIButton, UIButtonDescription>(name, name, desc, layerHUD);
            button.MouseClick += (sender, e) =>
            {
                if (!toolsReady)
                {
                    return;
                }

                if (!e.Buttons.HasFlag(MouseButtons.Left))
                {
                    return;
                }

                callback?.Invoke();
            };
            button.Caption.Text = caption;

            return button;
        }
        private async Task InitializeTerrain()
        {
            float l = spaceSize;
            int p = 100;
            float h = 0f;
            float imgScale = 500f;

            var geo = GeometryUtil.CreateXZGrid(l, l, p, p, h, imgScale);
            geo.Uvs = geo.Uvs.Select(uv => uv * 5f);

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = Constants.TerrainDiffuseTexture;
            mat.NormalMapTexture = Constants.TerrainNormalMapTexture;
            mat.IsTransparent = false;

            var desc = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(geo, mat),
                StartsVisible = false,
            };

            terrain = await AddComponentGround<Model, ModelDescription>(nameof(terrain), nameof(terrain), desc);
            terrain.TintColor = Color.GreenYellow;
        }
        private Task InitializeWorld()
        {
            return Task.WhenAll(world.Initialize(this));
        }
        private Task InitializeTools()
        {
            return tools.Initialize();
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

                return;
            }

            UpdateLayout();

            float sp = MathF.Min(1500f, spaceSize);
            float far = sp * 1.5f;
            Camera.FarPlaneDistance = far;
            Camera.MovementDelta = sp * 0.2f;
            Camera.SlowMovementDelta = Camera.MovementDelta / 20f;

            GameEnvironment.ShadowDistanceHigh = far * 0.1f;
            GameEnvironment.ShadowDistanceMedium = far * 0.2f;
            GameEnvironment.ShadowDistanceLow = far * 0.9f;

            MoveCameraTo(Vector3.Zero);

            Lights.EnableFog(sp, Camera.FarPlaneDistance, GameEnvironment.Background);

            gameReady = true;
            toolsReady = true;

            terrain.Visible = true;

            ToggleTools();
        }
        private void MoveCameraTo(Vector3 position)
        {
            float sp = MathF.Min(1500f, spaceSize);
            float s = sp * 0.1f;
            Camera.Goto(new Vector3(0, s, s) + position);
            Camera.LookTo(position);
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (!gameReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                ToggleHelp();
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                ToggleTools();
            }

            UpdateInputCars();

            UpdateInputCamera(gameTime);

            UpdateWorld(gameTime);

            UpdateTools(gameTime);
        }
        private void ToggleHelp()
        {
            showHelp = !showHelp;

            if (showHelp)
            {
                info.Text = helpText;
            }
            else
            {
                info.Text = infoText;
            }
        }
        private void UpdateInputCamera(IGameTime gameTime)
        {
            if (!followCar || world.GetBestCar() == null)
            {
                Camera.Following = null;

                UpdateInputCameraManual(gameTime);

                return;
            }

            if (Camera.Following == null)
            {
                Camera.Following = carFollower;
            }
        }
        private void UpdateInputCameraManual(IGameTime gameTime)
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
                Camera.Move(gameTime, Vector3.Down, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.Space))
            {
                Camera.Move(gameTime, Vector3.Up, Game.Input.ShiftPressed);
            }
        }
        private void UpdateWorld(IGameTime gameTime)
        {
            world.Update(gameTime);
        }
        private void ToggleTools()
        {
            if (!toolsReady)
            {
                return;
            }

            toolsVisible = !toolsVisible;

            foreach (var button in editorButtons)
            {
                if (button != null)
                {
                    button.Visible = toolsVisible;
                }
            }

            UpdateToolsLayout();
        }
        private void UpdateTools(IGameTime gameTime)
        {
            if (!toolsReady)
            {
                return;
            }

            if (!toolsVisible)
            {
                return;
            }

            if (TopMostControl == null)
            {
                tools.Update(gameTime);
            }

            tools.Draw();
        }
        private void UpdateInputCars()
        {
            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                AddCar(false);
            }
            if (Game.Input.KeyJustReleased(Keys.F6))
            {
                AddCar(true);
            }
            if (Game.Input.KeyJustReleased(Keys.F7))
            {
                AddCars(World.MaxCarInstances);
            }
            if (Game.Input.KeyJustReleased(Keys.F8))
            {
                SaveBestCar();
            }
            if (Game.Input.KeyJustReleased(Keys.F))
            {
                ToggleFollow();
            }
            if (Game.Input.KeyJustReleased(Keys.R))
            {
                RemoveBestCar();
            }
        }
        private void AddCars(int count)
        {
            world.ClearTraffic();

            for (int i = 0; i < count; i++)
            {
                AddCar(i != 0);
            }
        }
        private void AddCar(bool mutate)
        {
            if (!world.Populated)
            {
                return;
            }

            world.CreateCar(AgentControlTypes.AI, bestCarFileName, mutate);
        }
        private void ToggleFollow()
        {
            followCar = !followCar;
        }
        private void SaveBestCar()
        {
            var bestCar = world.GetBestCar();
            if (bestCar == null)
            {
                return;
            }

            bestCar.Brain.Save(bestCarFileName);
        }
        private void RemoveBestCar()
        {
            var bestCar = world.GetBestCar();
            if (bestCar != null)
            {
                world.RemoveCar(bestCar);
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

            float panelBottom = runtimeText.Top + runtimeText.Height;
            panel.Width = Game.Form.RenderWidth;
            panel.Height = panelBottom;

            info.SetPosition(new Vector2(5, panelBottom + 3));

            UpdateToolsLayout();
        }
        private void UpdateToolsLayout()
        {
            //Show the editor buttons centered at screen bottom
            if (!toolsReady)
            {
                return;
            }

            UIControlExtensions.LocateButtons(Game.Form, editorButtons, editorButtonWidth, editorButtonHeight, editorButtons.Length);
        }

        private void LoadWorld()
        {
            using System.Windows.Forms.OpenFileDialog dlg = new()
            {
                Filter = "World files (*.world)|*.world",
                FilterIndex = 1,
                RestoreDirectory = true,
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var worldFile = SerializationHelper.DeserializeJsonFromFile<WorldFile>(dlg.FileName);
                world.LoadFromWorldFile(worldFile);
                var (start, _) = world.GetStart();
                MoveCameraTo(new Vector3(start.X, 0, start.Y));
            }
        }
        private void SaveWorld()
        {
            using System.Windows.Forms.SaveFileDialog dlg = new()
            {
                FileName = "newworld.world",
                Filter = "World files (*.world)|*.world",
                FilterIndex = 1,
                RestoreDirectory = true,
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var worldFile = World.FromWorld(world);
                SerializationHelper.SerializeJsonToFile(worldFile, dlg.FileName);
            }
        }
        private void LoadFromOpenStreetMap()
        {
            using System.Windows.Forms.OpenFileDialog dlg = new()
            {
                Filter = "Open Street Map data (*.json)|*.json",
                FilterIndex = 1,
                RestoreDirectory = true,
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var osmGraph = Osm.ParseRoads(dlg.FileName, 10f);
                world.GenerateFromGraph(osmGraph);
                var (start, _) = world.GetStart();
                MoveCameraTo(new Vector3(start.X, 0, start.Y));
            }
        }
    }
}
