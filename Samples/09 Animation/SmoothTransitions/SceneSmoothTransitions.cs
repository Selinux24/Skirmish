using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.Tween;
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

        private Model soldier = null;
        private readonly Dictionary<string, AnimationPlan> soldierAnimationPlans = new Dictionary<string, AnimationPlan>();

        private PrimitiveListDrawer<Triangle> itemTris = null;
        private readonly Color itemTrisColor = new Color(Color.Yellow.ToColor3(), 0.25f);

        private IControllerPath soldierPath;
        private float soldierPathStartTime;
        private float soldierPathTotalTime;
        private int pathIndex = 0;
        private readonly float soldierSpeed = 5f;
        private readonly float globalTimeDelta = 1f;

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
                        InitializeSoldier(),
                        InitializeDebug(),
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

            var desc = new ModelInstancedDescription()
            {
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(vertices, indices, mat),
                Instances = 9,
            };

            var floor = await this.AddComponentModelInstanced("Floor", "Floor", desc);

            int i = 0;
            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    floor[i++].Manipulator.SetPosition(x * l * 2f, h, z * l * 2f);
                }
            }

            SetGround(floor, true);
        }
        private async Task InitializeSoldier()
        {
            soldier = await this.AddComponentModel(
                "Soldier",
                "Soldier",
                new ModelDescription()
                {
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile("SmoothTransitions/Resources/Soldier", "soldier_anim2.json"),
                    TextureIndex = 1,
                });

            soldier.AnimationController.PathEnding += SoldierControllerPathEnding;

            soldier.Manipulator.SetPosition(0, 0, 5, true);

            AnimationPath pIdle = new AnimationPath();
            pIdle.Add("idle1", 2f);
            pIdle.Add("idle1");

            AnimationPath pWalk = new AnimationPath();
            pWalk.Add("walk");

            soldierAnimationPlans.Add("idle", new AnimationPlan(pIdle));
            soldierAnimationPlans.Add("walk", new AnimationPlan(pWalk));

            soldier.AnimationController.AddPath(soldierAnimationPlans["idle"]);
            soldier.AnimationController.Start(0);
        }
        private async Task InitializeDebug()
        {
            itemTris = await this.AddComponentPrimitiveListDrawer("DebugItemTris", "DebugItemTris", new PrimitiveListDrawerDescription<Triangle>() { Count = 100000, Color = itemTrisColor });
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
            Camera.Goto(10, 5, -12f);
            Camera.LookTo(0, 5 * 0.6f, 0);

            Game.VisibleMouse = true;
            Game.LockMouse = false;
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
            UpdateInputAnimation(gameTime);
            UpdateInputDebug();

            base.Update(gameTime);

            UpdateSoldier(gameTime);

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
        private void UpdateInputAnimation(GameTime gameTime)
        {
            if (Game.Input.MouseButtonJustReleased(MouseButtons.Left))
            {
                var pRay = GetPickingRay();
                var rayPParams = RayPickingParams.FacingOnly | RayPickingParams.Perfect;

                if (PickNearest<Triangle>(pRay, rayPParams, out var r))
                {
                    MoveSoldierTo(gameTime, r.Position);
                }
            }

            if (Game.Input.KeyJustReleased(Keys.M))
            {
                Vector3 to = soldier.Manipulator.Position - (Vector3.ForwardLH * 20f);

                MoveSoldierTo(gameTime, to);
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
        private void UpdateSoldier(GameTime gameTime)
        {
            if (soldierPath == null)
            {
                return;
            }

            float currTime = (gameTime.TotalSeconds - soldierPathStartTime) * globalTimeDelta;
            float gradient = ScaleFuncs.SineEaseInOut(currTime / soldierPathTotalTime);
            float pathTime = gradient * soldierPath.Length;

            var pathPosition = soldierPath.GetPosition(pathTime);

            soldier.Manipulator.SetPosition(pathPosition);
            soldier.Manipulator.LookAt(soldierPath.Last, Axis.Y, 0.5f);

            if (currTime >= pathIndex || soldierPath.Last == pathPosition)
            {
                var color = GetColor(pathTime, soldierPathTotalTime);

                var tris = soldier.GetTriangles();
                itemTris.AddPrimitives(color, tris);

                pathIndex++;
            }

            if (Vector3.Distance(soldierPath.Last, pathPosition) < 0.1f)
            {
                soldierPath = null;
            }
        }

        /// <summary>
        /// Gets a gradient color
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="max">Maximum value</param>
        private Color4 GetColor(float value, float max)
        {
            var colorFrom = new Color(0f, 1f, 0f, 0.25f);
            var colorMiddle = new Color(Color.LightYellow.ToColor3(), 0.25f);
            var colorTo = new Color(1f, 0f, 0f, 0.25f);

            float gradient = value / max;

            if (gradient < 0.5f)
            {
                return Color4.Lerp(colorFrom, colorMiddle, gradient * 2f);
            }
            else
            {
                return Color4.Lerp(colorMiddle, colorTo, (gradient - 0.5f) * 2f);
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
            backPanel.Height = messages.AbsoluteRectangle.Bottom + 3;

            console.Top = backPanel.AbsoluteRectangle.Bottom;
            console.Width = Game.Form.RenderWidth;
        }

        /// <summary>
        /// Fires when the soldier animation controller ends a plan
        /// </summary>
        private void SoldierControllerPathEnding(object sender, EventArgs e)
        {
            if (sender is AnimationController controller)
            {
                controller.SetPath(soldierAnimationPlans["idle"]);
                controller.Start(0);
            }
        }
        /// <summary>
        /// Moves the soldier to the specified position
        /// </summary>
        /// <param name="to">Position</param>
        private void MoveSoldierTo(GameTime gameTime, Vector3 to)
        {
            soldier.AnimationController.TimeDelta = globalTimeDelta;

            Vector3 from = soldier.Manipulator.Position;

            soldierPath = CalcPath(from, to);
            soldierPathStartTime = gameTime.TotalSeconds;
            var soldierAnim = CalcAnimation(from, to, soldierSpeed);

            //Sets the plan to the animation controller
            soldier.AnimationController.SetPath(soldierAnim);
            soldier.AnimationController.Start();

            pathIndex = 0;
            itemTris.Clear();
        }
        /// <summary>
        /// Calculates a controller path
        /// </summary>
        /// <param name="from">Position from</param>
        /// <param name="to">Position to</param>
        /// <returns></returns>
        private IControllerPath CalcPath(Vector3 from, Vector3 to)
        {
            return new SegmentPath(from, to);
        }
        /// <summary>
        /// Calculates an animation plan
        /// </summary>
        /// <param name="from">Position from</param>
        /// <param name="to">Position to</param>
        /// <param name="speed">Speed</param>
        private AnimationPlan CalcAnimation(Vector3 from, Vector3 to, float speed)
        {
            float distance = Vector3.Distance(from, to);
            soldierPathTotalTime = distance / speed;

            //Calculates the plan
            var dd = soldier.GetDrawingData(LevelOfDetail.High);
            return soldier.AnimationController.CalcPath(dd.SkinningData, "walk", soldierPathTotalTime);
        }
    }
}
