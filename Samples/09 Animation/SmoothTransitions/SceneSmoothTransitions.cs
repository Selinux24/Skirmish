using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Animation.SmoothTransitions
{
    public class SceneSmoothTransitions : Scene
    {
        private UITextArea title = null;
        private UITextArea runtime = null;
        private UITextArea messages = null;
        private Sprite backPanel = null;
        private UIConsole console = null;

        private PrimitiveListDrawer<Triangle> itemTris = null;
        private PrimitiveListDrawer<Line3D> itemLines = null;
        private readonly Color itemTrisColor = new Color(Color.Yellow.ToColor3(), 0.6f);
        private readonly Color itemLinesColor = new Color(Color.Red.ToColor3(), 1f);

        private readonly Dictionary<string, AnimationPlan> soldierPaths = new Dictionary<string, AnimationPlan>();
       
        private bool uiReady = false;
        private bool gameReady = false;

        public SceneSmoothTransitions(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
            await InitializeUI();

            UpdateLayout();

            try
            {
                await LoadResourcesAsync(
                    new[]
                    {
                        InitializeFloor(),
                        InitializeDebug()
                    },
                    (res) =>
                    {
                        if (!res.Completed)
                        {
                            res.ThrowExceptions();
                        }

                        InitializeEnvironment();

                        gameReady = true;
                    });
            }
            catch (Exception ex)
            {
                messages.Text = ex.Message;
                messages.Visible = true;
            }
        }
        private async Task InitializeUI()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Consolas", 18);
            var defaultFont15 = TextDrawerDescription.FromFamily("Consolas", 15);
            var defaultFont11 = TextDrawerDescription.FromFamily("Consolas", 11);

            title = await this.AddComponentUITextArea("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            runtime = await this.AddComponentUITextArea("Runtime", "Runtime", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow });
            messages = await this.AddComponentUITextArea("Messages", "Messages", new UITextAreaDescription { Font = defaultFont15, TextForeColor = Color.Orange });

            title.Text = "Smooth Transitions";
            runtime.Text = "";
            messages.Text = "";

            backPanel = await this.AddComponentSprite("Backpanel", "Backpanel", SpriteDescription.Default(new Color4(0, 0, 0, 0.75f)), SceneObjectUsages.UI, LayerUI - 1);

            var consoleDesc = UIConsoleDescription.Default(new Color4(0.35f, 0.35f, 0.35f, 1f));
            consoleDesc.LogFilterFunc = (l) => l.LogLevel > LogLevel.Trace || (l.LogLevel == LogLevel.Trace && l.CallerTypeName == nameof(AnimationController));
            console = await this.AddComponentUIConsole("Console", "Console", consoleDesc, LayerUI + 1);
            console.Visible = false;

            uiReady = true;
        }
        private async Task InitializeFloor()
        {
            float l = 15f;
            float h = 0f;

            VertexData[] vertices = new VertexData[]
            {
                    new VertexData{ Position = new Vector3(-l, h, -l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 0.0f) },
                    new VertexData{ Position = new Vector3(-l, h, +l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 1.0f) },
                    new VertexData{ Position = new Vector3(+l, h, -l), Normal = Vector3.Up, Texture = new Vector2(1.0f, 0.0f) },
                    new VertexData{ Position = new Vector3(+l, h, +l), Normal = Vector3.Up, Texture = new Vector2(1.0f, 1.0f) },
            };

            uint[] indices = new uint[]
            {
                    0, 1, 2,
                    1, 3, 2,
            };

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = "SmoothTransitions/resources/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SmoothTransitions/resources/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "SmoothTransitions/resources/d_road_asphalt_stripes_specular.dds";

            var desc = new ModelDescription()
            {
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(vertices, indices, mat),
            };

            await this.AddComponentModel("Floor", "Floor", desc);
        }
        private async Task InitializeSoldier()
        {
            var soldier = await this.AddComponentModelInstanced(
                "Soldier",
                "Soldier",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile("SmoothTransitions/Resources/Soldier", "soldier_anim2.json"),
                });

            soldier[0].Manipulator.SetPosition(0, 0, 0, true);
            soldier[1].Manipulator.SetPosition(0.5f, 0, 5, true);

            AnimationPath p0 = new AnimationPath();
            p0.Add("idle1");
            p0.AddRepeat("idle2", 5);
            p0.Add("idle1");
            p0.Add("stand");
            p0.Add("idle1");
            p0.AddRepeat("walk", 5);
            p0.AddRepeat("run", 10);
            p0.AddRepeat("walk", 1);
            p0.AddRepeat("idle2", 5);
            p0.Add("idle1");

            AnimationPath p1 = new AnimationPath();
            p1.Add("idle1");

            AnimationPath p2 = new AnimationPath();
            p2.AddRepeat("idle2", 2);

            AnimationPath p3 = new AnimationPath();
            p3.AddRepeat("stand", 5);

            soldierPaths.Add("complex", new AnimationPlan(p0));
            soldierPaths.Add("idle1", new AnimationPlan(p1));
            soldierPaths.Add("idle2", new AnimationPlan(p2));
            soldierPaths.Add("stand", new AnimationPlan(p3));

            soldier[0].AnimationController.AddPath(soldierPaths["complex"]);
            soldier[1].AnimationController.AddPath(soldierPaths["complex"]);
        }
        private async Task InitializeDebug()
        {
            itemTris = await this.AddComponentPrimitiveListDrawer("DebugItemTris", "DebugItemTris", new PrimitiveListDrawerDescription<Triangle>() { Count = 5000, Color = itemTrisColor });
            itemLines = await this.AddComponentPrimitiveListDrawer("DebugItemLines", "DebugItemLines", new PrimitiveListDrawerDescription<Line3D>() { Count = 1000, Color = itemLinesColor });
        }

        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            Lights.KeyLight.CastShadow = true;
            Lights.KeyLight.Direction = Vector3.Normalize(new Vector3(-0.1f, -1, 1));
            Lights.KeyLight.Enabled = true;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = false;
            Lights.HemisphericLigth = new SceneLightHemispheric("Ambient", Color.Gray.RGB(), Color.White.RGB(), true);

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 500;
            Camera.Goto(0, 1, -12f);
            Camera.LookTo(0, 1 * 0.6f, 0);
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<Start.SceneStart>();
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
            UpdateInputDebug();

            base.Update(gameTime);

            UpdateDebugData();

            runtime.Text = Game.RuntimeText;
        }
        private void UpdateInputCamera(GameTime gameTime)
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
            backPanel.Height = messages.AbsoluteRectangle.Bottom + 3;

            console.Top = backPanel.AbsoluteRectangle.Bottom;
            console.Width = Game.Form.RenderWidth;
        }
    }
}
