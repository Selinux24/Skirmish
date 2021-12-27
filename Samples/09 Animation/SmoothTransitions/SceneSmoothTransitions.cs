using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private readonly Vector3 soldierInitPosition = new Vector3(0, 0, 5);

        private PrimitiveListDrawer<Triangle> itemTris = null;
        private readonly Color itemTrisColor = new Color(Color.Yellow.ToColor3(), 0.25f);

        private IControllerPath soldierPath;
        private readonly float soldierPathArrival = 3.333f;
        private float soldierPathStartTime;
        private float globalTimeDelta = 1f;
        private readonly float pathStep = 0.5f;
        private float pathIndex = 0f;
        private readonly float soldierSpeed = 5.3333f;
        private readonly StringBuilder animData = new StringBuilder();
        private readonly SteeringAgent steeringAgent = new SteeringAgent();

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

            soldier.AnimationController.PathUpdated += SoldierControllerPathUpdated;
            soldier.AnimationController.PathEnding += SoldierControllerPathEnding;
            soldier.AnimationController.PathChanged += SoldierControllerPathChanged;
            soldier.AnimationController.AnimationOffsetChanged += SoldierControllerAnimationOffsetChanged;

            soldier.Manipulator.SetPosition(soldierInitPosition, true);

            AnimationPath pIdle = new AnimationPath();
            pIdle.Add("idle1");
            pIdle.AddLoop("idle2");

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
            itemTris.Visible = false;
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
            UpdateDebug();

            runtime.Text = Game.RuntimeText;
        }
        private void UpdateInputCamera(GameTime gameTime)
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
        private void UpdateInputAnimation(GameTime gameTime)
        {
            if (Game.Input.MouseButtonJustReleased(MouseButtons.Left))
            {
                var pRay = GetPickingRay();
                var rayPParams = RayPickingParams.FacingOnly | RayPickingParams.Perfect;

                if (PickNearest<Triangle>(pRay, rayPParams, out var r))
                {
                    MoveSoldierTo(gameTime, r.Position, Game.Input.KeyPressed(Keys.ShiftKey));
                }
            }

            if (Game.Input.KeyJustReleased(Keys.M))
            {
                soldier.Manipulator.SetPosition(soldierInitPosition, true);
                Vector3 to = soldierInitPosition - (Vector3.ForwardLH * 31f);

                MoveSoldierTo(gameTime, to, Game.Input.KeyPressed(Keys.ShiftKey));
            }

            if (Game.Input.KeyJustReleased(Keys.U))
            {
                soldier.AnimationController.TimeDelta = Game.Input.KeyPressed(Keys.ShiftKey) ? 0.1f : 1f;
                soldier.AnimationController.SetPath(soldierAnimationPlans["walk"]);
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

            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                itemTris.Visible = !itemTris.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.L))
            {
                File.WriteAllText(@"AnimData.txt", animData.ToString());
            }
        }

        private void UpdateSoldier(GameTime gameTime)
        {
            if (soldierPath == null)
            {
                return;
            }

            steeringAgent.Update(gameTime.ElapsedSeconds * globalTimeDelta);
            var pathPosition = steeringAgent.Position;

            soldier.Manipulator.SetPosition(pathPosition);
            soldier.Manipulator.LookAt(soldierPath.Last, Axis.Y, 0.5f);

            float currTime = (gameTime.TotalSeconds - soldierPathStartTime) * globalTimeDelta;
            if (currTime >= pathIndex || soldierPath.Last == pathPosition)
            {
                float distToEnd = Vector3.Distance(pathPosition, soldierPath.Last);

                var color = GetColor(distToEnd / soldierPath.Length);
                var tris = soldier.GetTriangles();
                itemTris.AddPrimitives(color, tris);

                pathIndex += pathStep;
            }

            if (Vector3.Distance(soldierPath.Last, pathPosition) < 0.1f)
            {
                soldierPath = null;
            }
        }
        private void UpdateDebug()
        {
            messages.Text = $"{soldier.AnimationController.CurrentPath}";
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

        /// <summary>
        /// Moves the soldier to the specified position
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="to">Position</param>
        /// <param name="slow">Slow motion</param>
        private void MoveSoldierTo(GameTime gameTime, Vector3 to, bool slow)
        {
            globalTimeDelta = slow ? 0.1f : 1f;

            Vector3 from = soldier.Manipulator.Position;

            soldierPath = CalcPath(from, to);
            soldierPathStartTime = gameTime.TotalSeconds;

            steeringAgent.Reset();
            steeringAgent.Position = from;
            steeringAgent.MaxSpeed = soldierSpeed;
            steeringAgent.MaxForce = 0.25f;
            steeringAgent.WaitTime = 0.25f;
            steeringAgent.Arrival(soldierPath.Last, soldierPathArrival);

            //Sets the plan to the animation controller
            var soldierAnim = CalcAnimation(from, to, soldierSpeed);
            soldier.AnimationController.SetPath(soldierAnim);
            soldier.AnimationController.TimeDelta = globalTimeDelta;

            pathIndex = 0;
            itemTris.Clear();
            animData.Clear();
        }
        /// <summary>
        /// Calculates a controller path
        /// </summary>
        /// <param name="from">Position from</param>
        /// <param name="to">Position to</param>
        /// <returns>Returns the path</returns>
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
            float soldierPathTotalTime = distance / speed;

            //Calculates the plan
            return soldier.AnimationController.CalcAnimationPath("walk", soldierPathTotalTime);
        }
        /// <summary>
        /// Gets a gradient color
        /// </summary>
        /// <param name="value">Value</param>
        private Color4 GetColor(float value)
        {
            var colorFrom = new Color4(0f, 1f, 0f, 0.25f);
            var colorMiddle = new Color4(Color.Orange.ToColor3(), 0.25f);
            var colorTo = new Color4(1f, 0f, 0f, 0.25f);

            if (value < 0.5f)
            {
                float v = value * 2f;

                return Color4.Lerp(colorFrom, colorMiddle, v);
            }
            else
            {
                float v = (value - 0.5f) * 2f;

                return Color4.Lerp(colorMiddle, colorTo, v);
            }
        }

        private void SoldierControllerPathEnding(object sender, AnimationControllerEventArgs e)
        {
            if (sender is AnimationController controller)
            {
                controller.SetPath(soldierAnimationPlans["idle"]);

                if (soldierPath == null)
                {
                    return;
                }

                animData.AppendLine($"PathEnding   : {e.CurrentPath}");
            }
        }
        private void SoldierControllerPathUpdated(object sender, AnimationControllerEventArgs e)
        {
            if (sender is AnimationController)
            {
                if (soldierPath == null)
                {
                    return;
                }

                animData.AppendLine($"PathUpdated  : {e.CurrentPath}");

                var tris = soldier.GetTriangles();
                itemTris.AddPrimitives(new Color(Color.MediumPurple.ToColor3(), 0.5f), tris);
            }
        }
        private void SoldierControllerPathChanged(object sender, AnimationControllerEventArgs e)
        {
            if (sender is AnimationController)
            {
                if (soldierPath == null)
                {
                    return;
                }

                animData.AppendLine($"PathChanged  : {e.CurrentPath}");
            }
        }
        private void SoldierControllerAnimationOffsetChanged(object sender, AnimationControllerEventArgs e)
        {
            if (sender is AnimationController)
            {
                if (soldierPath == null)
                {
                    return;
                }

                animData.AppendLine($"OffsetChanged: {e.CurrentPath}");
            }
        }
    }
}
