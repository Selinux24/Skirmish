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
using System.IO;
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
        private const string samplesFolder = "SceneCWRVirtualWorld/worlds";
        private const string sampleCarFileName = $"{samplesFolder}/sample_car.json";
        private const string sampleWorldFileName = $"{samplesFolder}/sample_world.world";
        private const int fileDialogWidth = 600;
        private const int fileDialogHeight = 350;
        private const int fileButtonsCount = 10;
        private const string worldSearchPattern = "*.world";
        private const string osmSearchPattern = "*.osm";

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea runtimeText = null;
        private UITextArea info = null;

        private UIButton[] editorButtons;

        private UIDialog fileDialog = null;
        private UIButton[] fileButtons;
        private UITextArea fileSelectedText = null;
        private UITextArea fileFolderText = null;
        private UIButton filePageUpButton = null;
        private UIButton filePageDownButton = null;
        private MapFileTypes fileType = MapFileTypes.None;

        private Model terrain = null;

        private const string editorFont = "Consolas";
        private const int editorButtonWidth = 100;
        private const int editorButtonHeight = 25;
        private readonly Color editorBackgroundColor = Color.WhiteSmoke;
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
        private bool fileDlgVisible = false;

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
                    InitializeFileDialog,
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

            var titleDesc = new UITextAreaDescription
            {
                Font = defaultFont18,
                TextForeColor = Color.White,
                Text = titleText,
                StartsVisible = false,
            };
            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", titleDesc);

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.66f));
            spDesc.StartsVisible = false;
            panel = await AddComponentUI<Sprite, SpriteDescription>("Panel", "Panel", spDesc, LayerUI - 1);
        }
        private async Task InitializeTexts()
        {
            var defaultFont11 = FontDescription.FromFamily("Gill Sans MT, Arial", 11);

            var runtimeTextDesc = new UITextAreaDescription
            {
                Font = defaultFont11,
                TextForeColor = Color.Yellow,
                MaxTextLength = 256,
                Text = string.Empty,
                StartsVisible = false,
            };
            runtimeText = await AddComponentUI<UITextArea, UITextAreaDescription>("RuntimeText", "RuntimeText", runtimeTextDesc);

            var infoDesc = new UITextAreaDescription
            {
                Font = defaultFont11,
                TextForeColor = Color.Yellow,
                MaxTextLength = 256,
                Text = infoText,
                StartsVisible = false,
            };
            info = await AddComponentUI<UITextArea, UITextAreaDescription>("Information", "Information", infoDesc);
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
            editorButtonDesc.StartsVisible = false;

            List<UIButton> buttons = [];
            buttons.Add(await InitializeToolButton("editorLoadOSMButton", "LOAD OSM", editorButtonDesc, LoadFromOpenStreetMap));
            buttons.Add(await InitializeToolButton("editorLoadButton", "LOAD WORLD", editorButtonDesc, LoadWorldFromFile));
            buttons.Add(await InitializeToolButton("editorSaveButton", "SAVE WORLD", editorButtonDesc, SaveWorldToFile));
            buttons.Add(null);
            foreach (var mode in tools.GetModes())
            {
                buttons.Add(await InitializeToolButton($"editor{mode}", $"{mode.ToString().ToUpper()} EDITOR", editorButtonDesc, () => tools.SetEditor(mode)));
            }
            buttons.Add(null);
            buttons.Add(await InitializeToolButton("editorClearButton", "CLEAR", editorButtonDesc, world.Clear));

            editorButtons = [.. buttons];
        }
        private async Task<UIButton> InitializeToolButton(string name, string caption, UIButtonDescription desc, Action callback)
        {
            var button = await AddComponentUI<UIButton, UIButtonDescription>(name, name, desc, layerHUD);

            button.Caption.Text = caption;
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

            return button;
        }
        private async Task InitializeFileDialog()
        {
            var textFont = FontDescription.FromFamily(editorFont, 16);
            textFont.ContentPath = resourcesFolder;

            var fileTextDesc = UITextAreaDescription.Default(textFont);
            fileTextDesc.TextForeColor = editorButtonTextColor;
            fileTextDesc.StartsVisible = false;
            fileSelectedText = await AddComponentUI<UITextArea, UITextAreaDescription>(nameof(fileSelectedText), nameof(fileSelectedText), fileTextDesc);
            fileFolderText = await AddComponentUI<UITextArea, UITextAreaDescription>(nameof(fileFolderText), nameof(fileFolderText), fileTextDesc);

            var dlgButtonsFont = FontDescription.FromFamily(editorFont, 18);
            dlgButtonsFont.ContentPath = resourcesFolder;

            var fileDlgButtonDesc = UIButtonDescription.DefaultTwoStateButton(dlgButtonsFont);
            fileDlgButtonDesc.ContentPath = resourcesFolder;
            fileDlgButtonDesc.Width = 150;
            fileDlgButtonDesc.Height = 20;
            fileDlgButtonDesc.ColorReleased = editorButtonColor;
            fileDlgButtonDesc.ColorPressed = new Color4(editorButtonColor.RGB() * 1.2f, 1f);
            fileDlgButtonDesc.TextForeColor = editorButtonTextColor;
            fileDlgButtonDesc.StartsVisible = false;

            var fileDialogDesc = UIDialogDescription.Default(fileDialogWidth, fileDialogHeight);
            fileDialogDesc.Padding = 10;
            fileDialogDesc.TextArea = fileTextDesc;
            fileDialogDesc.Buttons = fileDlgButtonDesc;
            fileDialogDesc.Background = UIPanelDescription.Default(editorBackgroundColor);
            fileDialogDesc.StartsVisible = false;

            fileDialog = await AddComponentUI<UIDialog, UIDialogDescription>(nameof(fileDialog), nameof(fileDialog), fileDialogDesc, layerHUD);
            fileDialog.OnAcceptHandler += OnDialogAccept;
            fileDialog.OnCancelHandler += OnDialogCancel;

            var buttonsFont = FontDescription.FromFamily(editorFont, 14);
            buttonsFont.ContentPath = resourcesFolder;

            var fileButtonDesc = UIButtonDescription.Default(buttonsFont);
            fileButtonDesc.ContentPath = resourcesFolder;
            fileButtonDesc.Width = fileDialogWidth * 0.8f;
            fileButtonDesc.Height = 20;
            fileButtonDesc.ColorReleased = editorButtonColor;
            fileButtonDesc.TextForeColor = editorButtonTextColor;
            fileButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Left;
            fileButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            fileButtonDesc.StartsVisible = false;

            List<UIButton> buttons = [];
            for (int i = 0; i < fileButtonsCount; i++)
            {
                buttons.Add(await InitializeFileButton($"file_{i}", string.Empty, fileButtonDesc));
            }

            fileButtons = [.. buttons];

            var filePageButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont);
            filePageButtonDesc.ContentPath = resourcesFolder;
            filePageButtonDesc.ColorReleased = editorButtonColor;
            filePageButtonDesc.ColorPressed = new Color4(editorButtonColor.RGB() * 1.2f, 1f);
            filePageButtonDesc.TextForeColor = editorButtonTextColor;
            filePageButtonDesc.StartsVisible = false;

            filePageUpButton = await AddComponentUI<UIButton, UIButtonDescription>(nameof(filePageUpButton), nameof(filePageUpButton), filePageButtonDesc, layerHUD + 1);
            filePageUpButton.Caption.Text = "U";
            filePageUpButton.MouseClick += (sender, e) =>
            {
                if (!e.Buttons.HasFlag(MouseButtons.Left))
                {
                    return;
                }

                if (FolderNavigator.PageUp())
                {
                    LoadFolder(fileFolderText.TooltipText, worldSearchPattern);
                }
            };

            filePageDownButton = await AddComponentUI<UIButton, UIButtonDescription>(nameof(filePageDownButton), nameof(filePageDownButton), filePageButtonDesc, layerHUD + 1);
            filePageDownButton.Caption.Text = "D";
            filePageDownButton.MouseClick += (sender, e) =>
            {
                if (!e.Buttons.HasFlag(MouseButtons.Left))
                {
                    return;
                }

                if (FolderNavigator.PageDown())
                {
                    LoadFolder(fileFolderText.TooltipText, worldSearchPattern);
                }
            };
        }
        private async Task<UIButton> InitializeFileButton(string name, string caption, UIButtonDescription desc)
        {
            var button = await AddComponentUI<UIButton, UIButtonDescription>(name, name, desc, layerHUD + 1);

            button.Caption.Text = caption;
            button.MouseClick += (sender, e) =>
            {
                if (!e.Buttons.HasFlag(MouseButtons.Left))
                {
                    return;
                }

                if (sender is not UIButton button)
                {
                    return;
                }

                string fileName = button.Caption.Text;
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                string path = button.TooltipText;

                if (FolderNavigatorPath.FileNameIsPrevFolder(fileName) || FolderNavigatorPath.FileNameIsFolder(fileName))
                {
                    FolderNavigator.PageIndex = 0;

                    LoadFolder(path, worldSearchPattern);
                }
                else
                {
                    fileSelectedText.Text = fileName;
                    fileSelectedText.TooltipText = path;
                }
            };

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
            MoveCameraTo(Vector3.Zero);

            GameEnvironment.ShadowDistanceHigh = far * 0.1f;
            GameEnvironment.ShadowDistanceMedium = far * 0.2f;
            GameEnvironment.ShadowDistanceLow = far * 0.9f;

            Lights.EnableFog(sp, Camera.FarPlaneDistance, GameEnvironment.Background);

            LoadWorld(sampleWorldFileName);

            gameReady = true;
            toolsReady = true;

            title.Visible = true;
            panel.Visible = true;
            info.Visible = true;
            runtimeText.Visible = true;
            terrain.Visible = true;
            world.Visible = true;

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

            if (fileDlgVisible)
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

            string carFileName = File.Exists(bestCarFileName) ? bestCarFileName : sampleCarFileName;

            world.CreateCar(AgentControlTypes.AI, carFileName, mutate);
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

        private void LoadFromOpenStreetMap()
        {
            fileDialog.ShowDialog("Load World from Open Street Map data", () =>
            {
                FolderNavigator.PageIndex = 0;
                FolderNavigator.ItemsPerPage = fileButtonsCount;

                fileType = MapFileTypes.OSM;

                LoadFolder(samplesFolder, osmSearchPattern);

                ShowDialog();
            });
        }
        private void LoadWorldFromFile()
        {
            fileDialog.ShowDialog("Load World from file", () =>
            {
                FolderNavigator.PageIndex = 0;
                FolderNavigator.ItemsPerPage = fileButtonsCount;

                fileType = MapFileTypes.World;

                LoadFolder(samplesFolder, worldSearchPattern);

                ShowDialog();
            });
        }
        private void SaveWorldToFile()
        {
            SaveWorld("newworld.world");
        }

        private bool LoadFolder(string folder, string searchPattern)
        {
            if (!FolderNavigator.LoadFolder(folder, searchPattern, out var paths))
            {
                return false;
            }

            fileFolderText.Text = FormatFolderName(FolderNavigator.SelectedFolder.Path, 40);
            fileFolderText.TooltipText = FolderNavigator.SelectedFolder.Path;

            fileSelectedText.Text = null;
            fileSelectedText.TooltipText = null;

            for (int i = 0; i < fileButtons.Length; i++)
            {
                if (i >= paths.Length)
                {
                    fileButtons[i].TooltipText = string.Empty;
                    fileButtons[i].Caption.Text = string.Empty;

                    continue;
                }

                var data = paths[i];

                fileButtons[i].TooltipText = data.Path;
                fileButtons[i].Caption.Text = data.GetFileName();
            }

            return true;
        }
        private static string FormatFolderName(string folderName, int length)
        {
            if (folderName?.Length <= length)
            {
                return folderName;
            }

            return $"...{folderName.Substring(folderName.Length - length, length)}";
        }
        private void ShowDialog()
        {
            ToggleTools();

            fileDlgVisible = fileDialog.Visible = true;

            var first = fileButtons[0];
            var last = fileButtons[^1];
            var renderArea = fileDialog.GetRenderArea(true);
            var buttonWidth = renderArea.Width - first.Height;
            var buttonHeight = first.Height;
            float x = renderArea.Left;
            float y = renderArea.Top + buttonHeight + 5;

            fileFolderText.SetPosition(x, y);
            fileFolderText.Width = buttonWidth;
            fileFolderText.Visible = true;
            y += fileFolderText.Height + 1;

            foreach (var button in fileButtons)
            {
                button.SetPosition(x, y);
                button.Width = buttonWidth;
                button.Visible = true;
                y += button.Height + 1;
            }

            filePageUpButton.SetPosition(first.Left + first.Width + 1, first.Top);
            filePageUpButton.Width = first.Height;
            filePageUpButton.Height = first.Height;
            filePageUpButton.Visible = true;

            filePageDownButton.SetPosition(last.Left + last.Width + 1, last.Top);
            filePageDownButton.Width = last.Height;
            filePageDownButton.Height = last.Height;
            filePageDownButton.Visible = true;

            fileSelectedText.SetPosition(x, y);
            fileSelectedText.Visible = true;
        }
        private void HideDialog()
        {
            ToggleTools();

            fileDlgVisible = fileDialog.Visible = false;
            fileFolderText.Visible = false;
            foreach (var button in fileButtons)
            {
                button.Caption.Text = string.Empty;
                button.TooltipText = string.Empty;
                button.Visible = false;
            }
            fileSelectedText.Visible = false;
            filePageUpButton.Visible = false;
            filePageDownButton.Visible = false;
        }

        private void OnDialogAccept(object sender, EventArgs e)
        {
            string fileName = fileSelectedText.TooltipText;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            if (fileType == MapFileTypes.World)
            {
                LoadWorld(fileName);
            }
            else if (fileType == MapFileTypes.OSM)
            {
                LoadOSM(fileName);
            }

            fileType = MapFileTypes.None;

            fileDialog.CloseDialog(HideDialog);
        }
        private void OnDialogCancel(object sender, EventArgs e)
        {
            fileType = MapFileTypes.None;

            fileDialog.CloseDialog(HideDialog);
        }

        private void LoadOSM(string fileName)
        {
            var osmGraph = Osm.ParseRoads(fileName, 10f);
            world.GenerateFromGraph(osmGraph);
            var (start, _) = world.GetStart();
            MoveCameraTo(new Vector3(start.X, 0, start.Y));
        }
        private void LoadWorld(string fileName)
        {
            var worldFile = SerializationHelper.DeserializeJsonFromFile<WorldFile>(fileName);
            world.LoadFromWorldFile(worldFile);
            var (start, _) = world.GetStart();
            MoveCameraTo(new Vector3(start.X, 0, start.Y));
        }
        private void SaveWorld(string fileName)
        {
            var worldFile = World.FromWorld(world);
            SerializationHelper.SerializeJsonToFile(worldFile, fileName);
        }
    }
}
