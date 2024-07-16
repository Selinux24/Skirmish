using Engine;
using Engine.BuiltIn.Components.Models;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
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
        private UIButton editorAddRandomPointButton = null;
        private UIButton editorAddRandomSegmentButton = null;
        private UIButton editorRemoveRandomSegmentButton = null;
        private UIButton editorRemoveRandomPointButton = null;
        private UIButton editorClearButton = null;

        private Model terrain = null;

        private const string editorFont = "Gill Sans MT, Consolas";
        private const int editorButtonWidth = 200;
        private const int editorButtonHeight = 25;
        private readonly Color editorButtonColor = Color.WhiteSmoke;
        private readonly Color editorButtonTextColor = Color.Black;
        private const float graphPointRadius = 10f;
        private const float graphLineRadius = 1f;
        private const float graphSelectThreshold = 25f;

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
        private bool editorReady = false;

        private readonly Graph graph = new([], []);
        private readonly Editor editor = new();

        public VirtualWorldScene(Game game) : base(game)
        {
            Game.VisibleMouse = true;
            Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;
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
                    InitializeEditor,
                    InitializeTerrain,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeTitle()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Gill Sans MT, Arial", 18);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            title.Text = titleText;

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.66f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Panel", "Panel", spDesc, LayerUI - 1);
        }
        private async Task InitializeTexts()
        {
            var defaultFont11 = TextDrawerDescription.FromFamily("Gill Sans MT, Arial", 11);

            runtimeText = await AddComponentUI<UITextArea, UITextAreaDescription>("RuntimeText", "RuntimeText", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });
            info = await AddComponentUI<UITextArea, UITextAreaDescription>("Information", "Information", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });

            runtimeText.Text = "";
            info.Text = infoText;
        }
        private async Task InitializeEditorButtons()
        {
            var buttonsFont = TextDrawerDescription.FromFamily(editorFont, 10, FontMapStyles.Regular, true);
            buttonsFont.ContentPath = resourcesFolder;

            var editorButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont);
            editorButtonDesc.ContentPath = resourcesFolder;
            editorButtonDesc.Width = editorButtonWidth;
            editorButtonDesc.Height = editorButtonHeight;
            editorButtonDesc.ColorReleased = new Color4(editorButtonColor.RGB(), 0.8f);
            editorButtonDesc.ColorPressed = new Color4(editorButtonColor.RGB() * 1.2f, 0.9f);
            editorButtonDesc.TextForeColor = editorButtonTextColor;
            editorButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            editorButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            editorAddRandomPointButton = await InitializeButton(nameof(editorAddRandomPointButton), "ADD RANDOM POINT", editorButtonDesc);
            editorAddRandomSegmentButton = await InitializeButton(nameof(editorAddRandomSegmentButton), "ADD RANDOM SEGMENT", editorButtonDesc);
            editorRemoveRandomSegmentButton = await InitializeButton(nameof(editorRemoveRandomSegmentButton), "REMOVE RANDOM SEGMENT", editorButtonDesc);
            editorRemoveRandomPointButton = await InitializeButton(nameof(editorRemoveRandomPointButton), "REMOVE RANDOM POINT", editorButtonDesc);
            editorClearButton = await InitializeButton(nameof(editorClearButton), "CLEAR", editorButtonDesc);

            editorButtons =
            [
                editorAddRandomPointButton,
                editorAddRandomSegmentButton,
                editorRemoveRandomSegmentButton,
                editorRemoveRandomPointButton,
                editorClearButton,
            ];
        }
        private async Task<UIButton> InitializeButton(string name, string caption, UIButtonDescription desc)
        {
            var button = await AddComponentUI<UIButton, UIButtonDescription>(name, name, desc, layerHUD);
            button.MouseClick += SceneButtonClick;
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
        private Task InitializeEditor()
        {
            return editor.InitializeEditorDrawer(this);
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

            UpdateEditor(gameTime);
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

        private void UpdateEditor(IGameTime gameTime)
        {
            if (!editorReady)
            {
                return;
            }

            if (TopMostControl == null)
            {
                editor.UpdateInputEditor(this, graph, gameTime, graphSelectThreshold);
            }

            editor.DrawGraph(graph, 0, graphPointRadius, graphLineRadius);
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
            if (!editorReady)
            {
                return;
            }

            UIControlExtensions.LocateButtons(Game.Form, editorButtons, editorButtonWidth, editorButtonHeight, 6);
        }

        private void SceneButtonClick(IUIControl sender, MouseEventArgs e)
        {
            if (!editorReady)
            {
                return;
            }

            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            if (sender == editorAddRandomPointButton) AddRandonPoint();
            if (sender == editorAddRandomSegmentButton) AddRandomSegment();
            if (sender == editorRemoveRandomSegmentButton) RemoveRandomSegment();
            if (sender == editorRemoveRandomPointButton) RemoveRandomPoint();
            if (sender == editorClearButton) graph.Clear();
        }
        private void AddRandonPoint()
        {
            float size = spaceSize * 0.3f;

            var point = new Vector2(
                 Helper.RandomGenerator.NextFloat(-size, size),
                 Helper.RandomGenerator.NextFloat(-size, size));

            graph.TryAddPoint(point);
        }
        private void AddRandomSegment()
        {
            int count = graph.GetPointCount();
            if (count == 0)
            {
                return;
            }

            int index1 = Helper.RandomGenerator.Next(0, count);
            int index2 = Helper.RandomGenerator.Next(0, count);
            if (index1 == index2)
            {
                return;
            }

            graph.TryAddSegment(new(graph.GetPoint(index1), graph.GetPoint(index2)));
        }
        private void RemoveRandomSegment()
        {
            int count = graph.GetSegmentCount();
            if (count == 0)
            {
                return;
            }

            int index = Helper.RandomGenerator.Next(0, count);
            if (index < 0)
            {
                return;
            }

            graph.RemoveSegment(graph.GetSegment(index));
        }
        private void RemoveRandomPoint()
        {
            int count = graph.GetPointCount();
            if (count == 0)
            {
                return;
            }

            int index = Helper.RandomGenerator.Next(0, count);
            if (index < 0)
            {
                return;
            }

            graph.RemovePoint(graph.GetPoint(index));
        }
        private void OpenEditor()
        {
            editorReady = true;

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
