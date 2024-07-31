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
        private const float spaceSize = 1000f;
        private const string resourcesFolder = "SceneCWRVirtualWorld";

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
        private const string helpText = @"F1 - HELP
WASD - MOVE CAMERA
SPACE - MOVE CAMERA UP
C - MOVE CAMERA DOWN
MOUSE - ROTATE CAMERA
ESC - EXIT";
        private bool showHelp = false;

        private bool gameReady = false;
        private bool toolsReady = false;

        private readonly Graph graph = new([], []);
        private readonly World world;
        private readonly Tools tools;

        public VirtualWorldScene(Game game) : base(game)
        {
            Game.VisibleMouse = true;
            Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;

            world = new(graph, 0);
            tools = new(this, world);
            tools.AddEditor<GraphEditor>(EditorModes.Graph, 0);
            tools.AddEditor<StartsEditor>(EditorModes.Start, 0);
            tools.AddEditor<TargetsEditor>(EditorModes.Target, 0);
            tools.AddEditor<StopsEditor>(EditorModes.Stops, 0);
            tools.AddEditor<YieldEditor>(EditorModes.Yields, 0);
            tools.AddEditor<LightsEditor>(EditorModes.Lights, 0);
            tools.AddEditor<CrossingsEditor>(EditorModes.Crossings, 0);
            tools.AddEditor<ParkingsEditor>(EditorModes.Parkings, 0);
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
                    InitializeEditorButtons,
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
        private async Task InitializeEditorButtons()
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

            buttons.Add(await InitializeButton("editorLoadButton", "LOAD GRAPH", editorButtonDesc, LoadGraph));
            buttons.Add(await InitializeButton("editorSaveButton", "SAVE GRAPH", editorButtonDesc, SaveGraph));
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
            float h = 0f;

            var geo = GeometryUtil.CreatePlane(l, h, Vector3.Up);
            geo.Uvs = geo.Uvs.Select(uv => uv * 5f);

            var mat = MaterialBlinnPhongContent.Default;

            var desc = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(geo, mat),
            };

            terrain = await AddComponentGround<Model, ModelDescription>(nameof(terrain), nameof(terrain), desc);
            terrain.TintColor = Color.MediumSpringGreen;
        }
        private Task InitializeWorld()
        {
            return world.Initialize(this);
        }
        private async Task InitializeTools()
        {
            await tools.Initialize();
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

            float s = spaceSize * 0.6f;
            Camera.Goto(new Vector3(0, s, -s));
            Camera.LookTo(Vector3.Zero);
            Camera.FarPlaneDistance = spaceSize * 1.5f;
            Camera.MovementDelta = spaceSize * 0.2f;
            Camera.SlowMovementDelta = Camera.MovementDelta / 20f;

            gameReady = true;

            OpenEditor();
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

            UpdateInputCamera(gameTime);

            UpdateWorld(gameTime);

            if (!toolsReady)
            {
                return;
            }

            if (TopMostControl == null)
            {
                tools.Update(gameTime);
            }

            tools.Draw();
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
        }

        private void UpdateWorld(IGameTime gameTime)
        {
            world.Update(gameTime);
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

            UpdateEditorLayout();
        }
        private void UpdateEditorLayout()
        {
            //Show the editor buttons centered at screen bottom
            if (!toolsReady)
            {
                return;
            }

            UIControlExtensions.LocateButtons(Game.Form, editorButtons, editorButtonWidth, editorButtonHeight, editorButtons.Length);
        }

        private void LoadGraph()
        {
            using System.Windows.Forms.OpenFileDialog dlg = new()
            {
                Filter = "Graph files (*.graph)|*.graph",
                FilterIndex = 1,
                RestoreDirectory = true,
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                graph.LoadFromFile(dlg.FileName);
            }
        }
        private void SaveGraph()
        {
            using System.Windows.Forms.SaveFileDialog dlg = new()
            {
                FileName = "newgraph.graph",
                Filter = "Graph files (*.graph)|*.graph",
                FilterIndex = 1,
                RestoreDirectory = true,
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                graph.SaveToFile(dlg.FileName);
            }
        }
        private void OpenEditor()
        {
            toolsReady = true;

            foreach (var button in editorButtons)
            {
                if (button != null)
                {
                    button.Visible = true;
                }
            }

            UpdateEditorLayout();
        }
    }
}
