using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.PathFinding.AStar;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameLogic
{
    using GameLogic.Rules;
    using GameLogic.Rules.Enum;

    public class SceneObjects : Scene
    {
        private const int layerHUD = 99;

        private UITextArea txtTitle = null;
        private UITextArea txtGame = null;
        private UITextArea txtTeam = null;
        private UITextArea txtSoldier = null;
        private UITextArea txtActionList = null;
        private UITextArea txtAction = null;

        private readonly string titleFontFileName = "ARMY RUST.TTF";
        private readonly string fontFileName = "JOYSTIX.TTF";
        private readonly int fontSize = 12;
        private UIButton butClose = null;
        private UIButton butNext = null;
        private UIButton butPrevSoldier = null;
        private UIButton butNextSoldier = null;
        private UIButton butPrevAction = null;
        private UIButton butNextAction = null;

        private Model cursor3D = null;

        private SceneLightSpot spotLight = null;

        private ModelInstanced troops = null;
        private readonly GridAgentType soldierAgent = null;
        private readonly Dictionary<Soldier, ModelInstance> soldierModels = new Dictionary<Soldier, ModelInstance>();
        private readonly Dictionary<Soldier, ManipulatorController> soldierControllers = new Dictionary<Soldier, ManipulatorController>();
        private readonly Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        private PrimitiveListDrawer<Line3D> lineDrawer = null;
        private readonly Color4 bsphColor = new Color4(Color.LightYellow.ToColor3(), 0.25f);
        private readonly Color4 frstColor = new Color4(Color.Yellow.ToColor3(), 1f);
        private readonly int bsphSlices = 50;
        private readonly int bsphStacks = 25;

        private Scenery terrain = null;

        private int actionIndex = 0;
        private ActionSpecification[] currentActions = null;
        private Skirmish skirmishGame = null;
        private bool gameFinished = false;
        private ActionSpecification CurrentAction
        {
            get
            {
                if (currentActions != null && currentActions.Length > 0)
                {
                    return currentActions[actionIndex];
                }

                return null;
            }
        }

        private bool gameReady = false;

        #region Keys

        private readonly Keys keyExit = Keys.Escape;
        private readonly Keys keyChangeMode = Keys.R;

        private readonly Keys keyDebugFrustum = Keys.Space;
        private readonly Keys keyDebugVolumes = Keys.F1;

        private readonly Keys keyHUDNextSoldier = Keys.N;
        private readonly Keys keyHUDPrevSoldier = Keys.P;
        private readonly Keys keyHUDNextAction = Keys.Right;
        private readonly Keys keyHUDPrevAction = Keys.Left;
        private readonly Keys keyHUDNextPhase = Keys.E;

        private readonly Keys keyCAMNextIsometric = Keys.PageDown;
        private readonly Keys keyCAMPrevIsometric = Keys.PageUp;
        private readonly Keys keyCAMMoveLeft = Keys.A;
        private readonly Keys keyCAMMoveForward = Keys.W;
        private readonly Keys keyCAMMoveBackward = Keys.S;
        private readonly Keys keyCAMMoveRight = Keys.D;
        private readonly Keys keyCAMCenterSoldier = Keys.Home;

        #endregion

        public SceneObjects(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            GameEnvironment.Background = Color.Black;

            Camera.FarPlaneDistance = 1000f;
            Camera.Mode = CameraModes.FreeIsometric;

            NewGame();

            spotLight = new SceneLightSpot(
                "Current soldier",
                false,
                Color.White,
                Color.White,
                true,
                SceneLightSpotDescription.Create(Vector3.Zero, Vector3.Down, 15f, 15f, 100f));

            Lights.Add(spotLight);

            await LoadResourcesAsync(
                new[]
                {
                    InitializeModels(),
                    InitializeHUD(),
                    InitializeDebug()
                },
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    UpdateLayout();

                    InitializeAnimations();

                    InitializePositions();

                    GoToSoldier(skirmishGame.CurrentSoldier);

                    Task.WhenAll(UpdateNavigationGraph());

                    gameReady = true;
                });
        }
        private async Task InitializeModels()
        {
            cursor3D = await this.AddComponentModel(
                new ModelDescription()
                {
                    CastShadow = false,
                    DepthEnabled = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources3D",
                        ModelContentFilename = "cursor.xml",
                    }
                },
                SceneObjectUsages.UI,
                layerHUD);

            troops = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Instances = skirmishGame.AllSoldiers.Length,
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources3D",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                },
                SceneObjectUsages.Agent);

            terrain = await this.AddComponentScenery(
                GroundDescription.FromFile("Resources3D", "terrain.xml"),
                SceneObjectUsages.Ground);

            int minimapHeight = (Game.Form.RenderHeight / 4) - 8;
            int minimapWidth = minimapHeight;
            var topLeft = new Vector2(593, 443);
            var bottomRight = new Vector2(789, 590);
            var tRes = new Vector2(800, 600);
            var wRes = new Vector2(Game.Form.RenderWidth, Game.Form.RenderHeight);
            var pTopLeft = wRes / tRes * topLeft;
            var pBottomRight = wRes / tRes * bottomRight;
            var q = pTopLeft + ((pBottomRight - pTopLeft - new Vector2(minimapWidth, minimapHeight)) * 0.5f);

            await this.AddComponentUIMinimap(
                new UIMinimapDescription()
                {
                    Top = (int)q.Y,
                    Left = (int)q.X,
                    Width = minimapWidth,
                    Height = minimapHeight,
                    Drawables = new IDrawable[]
                    {
                        troops,
                    },
                    BackColor = Color.LightSlateGray,
                    MinimapArea = terrain.GetBoundingBox(),
                },
                layerHUD);

            SetGround(terrain, true);

            GridInput input = new GridInput(GetTrianglesForNavigationGraph);
            GridGenerationSettings settings = new GridGenerationSettings()
            {
                NodeSize = 5f,
            };
            PathFinderDescription = new Engine.PathFinding.PathFinderDescription(settings, input);
        }
        private async Task InitializeHUD()
        {
            BackgroundDescription bkDesc = new BackgroundDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "HUD.png" },
                BaseColor = new Color4(1f, 1f, 1f, 1f),
                EventsEnabled = false,
            };
            await this.AddComponentSprite(bkDesc, SceneObjectUsages.UI, layerHUD - 1);

            var titleFont = new TextDrawerDescription
            {
                FontFileName = titleFontFileName,
                FontSize = fontSize * 3,
                ForeColor = Color.White,
                ShadowColor = Color.Gray,
            };
            var gameFont = new TextDrawerDescription()
            {
                FontFileName = fontFileName,
                FontSize = (int)(fontSize * 1.25f),
                ForeColor = Color.LightBlue,
                ShadowColor = Color.DarkBlue,
            };
            var textFont = new TextDrawerDescription()
            {
                FontFileName = fontFileName,
                FontSize = fontSize,
                ForeColor = Color.Yellow,
            };
            var buttonsFont = new TextDrawerDescription()
            {
                FontFileName = fontFileName,
                FontSize = fontSize,
                ForeColor = Color.Yellow,
                HorizontalAlign = HorizontalTextAlign.Center,
                VerticalAlign = VerticalTextAlign.Middle,
            };

            txtTitle = await this.AddComponentUITextArea(new UITextAreaDescription { Font = titleFont }, layerHUD);
            txtGame = await this.AddComponentUITextArea(new UITextAreaDescription { Font = gameFont }, layerHUD);
            txtTeam = await this.AddComponentUITextArea(new UITextAreaDescription { Font = textFont }, layerHUD);
            txtSoldier = await this.AddComponentUITextArea(new UITextAreaDescription { Font = textFont }, layerHUD);
            txtActionList = await this.AddComponentUITextArea(new UITextAreaDescription { Font = textFont }, layerHUD);
            txtAction = await this.AddComponentUITextArea(new UITextAreaDescription { Font = textFont }, layerHUD);

            var butCloseDesc = UIButtonDescription.DefaultTwoStateButton("button_on.png", "button_off.png", UITextAreaDescription.Default(buttonsFont, "Exit"));
            butCloseDesc.Width = 60;
            butCloseDesc.Height = 20;

            butClose = await this.AddComponentUIButton(butCloseDesc, layerHUD);

            butNext = await this.AddComponentUIButton(new UIButtonDescription()
            {
                TwoStateButton = true,
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 120,
                Height = 20,
                Caption = UITextAreaDescription.Default(buttonsFont, "Next Phase"),
            }, layerHUD);

            butPrevSoldier = await this.AddComponentUIButton(new UIButtonDescription()
            {
                TwoStateButton = true,
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 150,
                Height = 20,
                Caption = UITextAreaDescription.Default(buttonsFont, "Prev.Soldier"),
            }, layerHUD);

            butNextSoldier = await this.AddComponentUIButton(new UIButtonDescription()
            {
                TwoStateButton = true,
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 150,
                Height = 20,
                Caption = UITextAreaDescription.Default(buttonsFont, "Next Soldier"),
            }, layerHUD);

            butPrevAction = await this.AddComponentUIButton(new UIButtonDescription()
            {
                TwoStateButton = true,
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 150,
                Height = 20,
                Caption = UITextAreaDescription.Default(buttonsFont, "Prev.Action"),
            }, layerHUD);

            butNextAction = await this.AddComponentUIButton(new UIButtonDescription()
            {
                TwoStateButton = true,
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 150,
                Height = 20,
                Caption = UITextAreaDescription.Default(buttonsFont, "Next Action"),
            }, layerHUD);

            butClose.JustReleased += (sender, eventArgs) => { Game.Exit(); };
            butNext.JustReleased += (sender, eventArgs) => { NextPhase(); };
            butPrevSoldier.JustReleased += (sender, eventArgs) => { PrevSoldier(true); };
            butNextSoldier.JustReleased += (sender, eventArgs) => { NextSoldier(true); };
            butPrevAction.JustReleased += (sender, eventArgs) => { PrevAction(); };
            butNextAction.JustReleased += (sender, eventArgs) => { NextAction(); };

            txtTitle.Text = "Game Logic";
        }
        private async Task InitializeDebug()
        {
            lineDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(new PrimitiveListDrawerDescription<Line3D>() { Count = 5000 });
            lineDrawer.Visible = false;
        }

        public override void NavigationGraphUpdated()
        {
            gameReady = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            foreach (var soldierC in soldierControllers.Keys)
            {
                soldierControllers[soldierC].UpdateManipulator(gameTime, soldierModels[soldierC].Manipulator);
            }

            if (Game.Input.KeyJustReleased(keyExit))
            {
                Game.Exit();

                return;
            }

            if (Game.Input.KeyJustReleased(keyChangeMode))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);

                return;
            }

            bool shift = Game.Input.KeyPressed(Keys.LShiftKey) || Game.Input.KeyPressed(Keys.RShiftKey);

            Ray cursorRay = GetPickingRay();
            bool picked = PickNearest(cursorRay, RayPickingParams.Default, SceneObjectUsages.Ground, out PickingResult<Triangle> r);

            //DEBUG
            UpdateDebug();

            //HUD
            UpdateHUD(shift);

            if (UICaptured)
            {
                return;
            }

            //3D
            Update3D(gameTime, shift, cursorRay, picked, r);

            //Actions
            UpdateActions(cursorRay, picked, r);
        }
        private void UpdateDebug()
        {
            if (Game.Input.KeyJustReleased(keyDebugFrustum))
            {
                SetFrustum();
            }

            if (Game.Input.KeyJustReleased(keyDebugVolumes))
            {
                lineDrawer.Visible = !lineDrawer.Visible;
            }
        }
        private void UpdateHUD(bool shift)
        {
            if (Game.Input.KeyJustReleased(keyHUDNextSoldier))
            {
                NextSoldier(!shift);
            }

            if (Game.Input.KeyJustReleased(keyHUDPrevSoldier))
            {
                PrevSoldier(!shift);
            }

            if (Game.Input.KeyJustReleased(keyHUDNextAction))
            {
                NextAction();
            }

            if (Game.Input.KeyJustReleased(keyHUDPrevAction))
            {
                PrevAction();
            }

            if (Game.Input.KeyJustReleased(keyHUDNextPhase))
            {
                NextPhase();
            }

            if (!gameFinished)
            {
                txtGame.Text = string.Format("{0}", skirmishGame);
                txtTeam.Text = string.Format("{0}", skirmishGame.CurrentTeam);
                txtSoldier.Text = string.Format("{0}", skirmishGame.CurrentSoldier);
                txtActionList.Text = string.Format("{0}", currentActions.Join(" ++ "));
                txtAction.Text = string.Format("{0}", CurrentAction);
            }
        }
        private void Update3D(GameTime gameTime, bool shift, Ray cursorRay, bool picked, PickingResult<Triangle> r)
        {
            if (picked)
            {
                cursor3D.Manipulator.SetPosition(r.Position);
            }

            if (Game.Input.KeyJustReleased(keyCAMNextIsometric))
            {
                Camera.NextIsometricAxis();
            }

            if (Game.Input.KeyJustReleased(keyCAMPrevIsometric))
            {
                Camera.PreviousIsometricAxis();
            }

            if (Game.Input.KeyPressed(keyCAMMoveLeft))
            {
                Camera.MoveLeft(gameTime, shift);
            }

            if (Game.Input.KeyPressed(keyCAMMoveRight))
            {
                Camera.MoveRight(gameTime, shift);
            }

            if (Game.Input.KeyPressed(keyCAMMoveForward))
            {
                Camera.MoveForward(gameTime, shift);
            }

            if (Game.Input.KeyPressed(keyCAMMoveBackward))
            {
                Camera.MoveBackward(gameTime, shift);
            }

            if (Game.Input.RightMouseButtonJustReleased)
            {
                DoGoto(cursorRay, picked, r.Position);
            }

            if (Game.Input.MouseWheelDelta > 0)
            {
                Camera.ZoomIn(gameTime, shift);
            }

            if (Game.Input.MouseWheelDelta < 0)
            {
                Camera.ZoomOut(gameTime, shift);
            }

            if (Game.Input.KeyJustReleased(keyCAMCenterSoldier))
            {
                GoToSoldier(skirmishGame.CurrentSoldier);
            }

            spotLight.Position = soldierModels[skirmishGame.CurrentSoldier].Manipulator.Position + (Vector3.UnitY * 10f);
        }
        private void DoGoto(Ray cursorRay, bool picked, Vector3 pickedPosition)
        {
            Soldier pickedSoldier = PickSoldier(cursorRay, false);
            if (pickedSoldier != null)
            {
                if (pickedSoldier.Team == skirmishGame.CurrentTeam)
                {
                    skirmishGame.CurrentSoldier = pickedSoldier;
                }

                GoToSoldier(pickedSoldier);
            }
            else if (picked)
            {
                Camera.LookTo(pickedPosition, CameraTranslations.UseDelta);
            }
        }
        private void UpdateActions(Ray cursorRay, bool picked, PickingResult<Triangle> r)
        {
            if (CurrentAction != null)
            {
                bool selectorDone = false;
                Area area = null;

                if (Game.Input.LeftMouseButtonJustReleased)
                {
                    if (CurrentAction.Selector == Selectors.Goto)
                    {
                        //TODO: Show goto selector
                        selectorDone = picked;
                    }
                    else if (CurrentAction.Selector == Selectors.Area)
                    {
                        //TODO: Show area selector
                        area = new Area();

                        selectorDone = picked;
                    }
                }

                if (selectorDone)
                {
                    UpdateSelected(cursorRay, r.Position, area);
                }
            }
        }
        private void UpdateSelected(Ray cursorRay, Vector3 position, Area area)
        {
            if (CurrentAction.Action == Actions.Move)
            {
                Move(skirmishGame.CurrentSoldier, position);
            }
            else if (CurrentAction.Action == Actions.Run)
            {
                Run(skirmishGame.CurrentSoldier, position);
            }
            else if (CurrentAction.Action == Actions.Crawl)
            {
                Crawl(skirmishGame.CurrentSoldier, position);
            }
            else if (CurrentAction.Action == Actions.Assault)
            {
                Soldier active = skirmishGame.CurrentSoldier;
                Soldier passive = PickSoldierNearestToPosition(cursorRay, true);
                if (passive != null)
                {
                    Assault(active, passive);
                }
            }
            else if (CurrentAction.Action == Actions.CoveringFire)
            {
                Soldier active = skirmishGame.CurrentSoldier;

                CoveringFire(active, active.CurrentShootingWeapon, area);
            }
            else if (CurrentAction.Action == Actions.Reload)
            {
                Soldier active = skirmishGame.CurrentSoldier;

                Reload(active, active.CurrentShootingWeapon);
            }
            else if (CurrentAction.Action == Actions.Repair)
            {
                Soldier active = skirmishGame.CurrentSoldier;

                Repair(active, active.CurrentShootingWeapon);
            }
            else if (CurrentAction.Action == Actions.Inventory)
            {
                Inventory(skirmishGame.CurrentSoldier);
            }
            else if (CurrentAction.Action == Actions.UseMovementItem)
            {
                UseMovementItem(skirmishGame.CurrentSoldier);
            }
            else if (CurrentAction.Action == Actions.Communications)
            {
                Communications(skirmishGame.CurrentSoldier);
            }
        }
        public override void GameGraphicsResized()
        {
            UpdateLayout();
        }
        private void UpdateLayout()
        {
            txtTitle.SetPosition(new Vector2(5, 0));
            txtGame.SetPosition(new Vector2(10, txtTitle.Top + txtTitle.Height + 1));
            txtTeam.SetPosition(new Vector2(txtGame.Left, txtGame.Top + txtGame.Height + 1));

            butClose.Top = 1;
            butClose.Left = Game.Form.RenderWidth - 60 - 1;

            butNext.Top = (int)(Game.Form.RenderHeight * 0.85f);
            butNext.Left = 10;
            butPrevSoldier.Top = butNext.Top;
            butPrevSoldier.Left = butNext.Left + butNext.Width + 25;
            butNextSoldier.Top = butNext.Top;
            butNextSoldier.Left = butPrevSoldier.Left + butPrevSoldier.Width + 10;
            butPrevAction.Top = butNext.Top;
            butPrevAction.Left = butNextSoldier.Left + butNextSoldier.Width + 25;
            butNextAction.Top = butNext.Top;
            butNextAction.Left = butPrevAction.Left + butPrevAction.Width + 10;

            txtSoldier.SetPosition(new Vector2(10, butNext.Top + butNext.Height + 1));
            txtActionList.SetPosition(new Vector2(txtSoldier.Left, txtSoldier.Top + txtSoldier.Height + 1));
            txtAction.SetPosition(new Vector2(txtSoldier.Left, txtActionList.Top + txtActionList.Height + 1));
        }

        private void InitializeAnimations()
        {
            AddAnimation("idle", "idle");
            AddAnimation("stand", "stand");
            AddAnimation("walk", "walk");
            AddAnimation("run", "run");
        }
        private void AddAnimation(string clipName, string animationName)
        {
            AnimationPath p = new AnimationPath();
            p.AddLoop(clipName);
            animations.Add(animationName, new AnimationPlan(p));
        }
        private void InitializePositions()
        {
            BoundingBox bbox = terrain.GetBoundingBox();

            float terrainHeight = bbox.Maximum.Z - bbox.Minimum.Z;
            float teamSeparation = terrainHeight / (skirmishGame.Teams.Length);

            float soldierSeparation = 12f;
            int instanceIndex = 0;
            uint teamIndex = 0;
            foreach (Team team in skirmishGame.Teams)
            {
                float teamWidth = team.Soldiers.Length * soldierSeparation;

                int soldierIndex = 0;
                foreach (var soldierC in team.Soldiers)
                {
                    var soldier = troops[instanceIndex++];

                    soldier.TextureIndex = teamIndex;
                    soldier.AnimationController.AddPath(animations["stand"]);
                    soldier.AnimationController.Start(soldierIndex);
                    soldier.AnimationController.TimeDelta = 0.20f;

                    float x = (soldierIndex * soldierSeparation) - (teamWidth * 0.5f);
                    float z = (teamIndex * teamSeparation) - (teamSeparation * 0.5f);

                    if (FindTopGroundPosition(x, z, out PickingResult<Triangle> r))
                    {
                        soldier.Manipulator.SetPosition(r.Position, true);
                    }
                    else
                    {
                        throw new GameLogicException("Bad position");
                    }

                    if (teamIndex == 0)
                    {
                        soldier.Manipulator.SetRotation(MathUtil.DegreesToRadians(180), 0, 0, true);
                    }

                    soldierModels.Add(soldierC, soldier);
                    var controller = new BasicManipulatorController();
                    controller.PathEnd += Controller_PathEnd;
                    soldierControllers.Add(soldierC, controller);

                    soldierIndex++;
                }

                teamIndex++;
            }
        }

        private void Controller_PathEnd(object sender, EventArgs e)
        {
            var instance = sender as ManipulatorController;

            foreach (var item in soldierControllers)
            {
                if (item.Value == instance)
                {
                    soldierModels[item.Key].AnimationController.ContinuePath(animations["stand"]);
                }
            }
        }

        private void SetFrustum()
        {
            lineDrawer.SetPrimitives(frstColor, Line3D.CreateWiredFrustum(Camera.Frustum));
        }

        protected void NewGame()
        {
            skirmishGame = new Skirmish();
            skirmishGame.AddTeam("Team1", "Reds", TeamRoles.Defense, 5, 1, 1);
            skirmishGame.AddTeam("Team2", "Blues", TeamRoles.Assault, 5, 1, 1);
            skirmishGame.Start();
            currentActions = skirmishGame.GetActions();
        }
        protected void LoadGame()
        {
            //TODO: Load game from file
        }
        protected void SaveGame()
        {
            //TODO: Save game to file
        }

        protected void NextSoldier(bool selectIdle)
        {
            skirmishGame.NextSoldier(selectIdle);

            currentActions = skirmishGame.GetActions();

            GoToSoldier(skirmishGame.CurrentSoldier);
        }
        protected void PrevSoldier(bool selectIdle)
        {
            skirmishGame.PrevSoldier(selectIdle);

            currentActions = skirmishGame.GetActions();

            GoToSoldier(skirmishGame.CurrentSoldier);
        }
        protected void NextAction()
        {
            if (currentActions != null && currentActions.Length > 0)
            {
                actionIndex++;

                if (actionIndex > currentActions.Length - 1)
                {
                    actionIndex = 0;
                }
            }
        }
        protected void PrevAction()
        {
            if (currentActions != null && currentActions.Length > 0)
            {
                actionIndex--;

                if (actionIndex < 0)
                {
                    actionIndex = currentActions.Length - 1;
                }
            }
        }
        protected void RefreshActions()
        {
            currentActions = skirmishGame.GetActions();

            UpdateSoldierStates();
        }
        protected void NextPhase()
        {
            skirmishGame.NextPhase();

            //Do automatic actions
            foreach (var soldierC in skirmishGame.Soldiers)
            {
                Melee melee = skirmishGame.GetMelee(soldierC);

                ActionSpecification[] actions = ActionsManager.GetActions(skirmishGame.CurrentPhase, soldierC.Team, soldierC, melee != null, ActionTypes.Automatic);
                if (actions.Length > 0)
                {
                    foreach (ActionSpecification ac in actions)
                    {
                        //TODO: Need of standar method
                        if (ac.Action == Actions.FindCover)
                        {
                            Vector3 point = GetRandomPoint();

                            FindCover(soldierC, point);
                        }
                        else if (ac.Action == Actions.RunAway)
                        {
                            Vector3 point = GetRandomPoint();

                            RunAway(soldierC, point);
                        }
                        else if (ac.Action == Actions.TakeControl)
                        {
                            TakeControl(soldierC);
                        }
                    }
                }
            }

            skirmishGame.NextSoldier(true);

            UpdateSoldierStates();

            currentActions = skirmishGame.GetActions();
            actionIndex = 0;

            GoToSoldier(skirmishGame.CurrentSoldier);

            Victory v = skirmishGame.IsFinished();
            if (v != null)
            {
                gameFinished = true;

                txtGame.Text = string.Format("{0}", v);
                txtTeam.Text = "";
                txtSoldier.Text = "";
                txtActionList.Text = "";
                txtAction.Text = "";
            }
        }

        protected void GoToSoldier(Soldier soldier)
        {
            BoundingSphere bsph = soldierModels[soldier].GetBoundingSphere();

            Camera.LookTo(bsph.Center, CameraTranslations.Quick);
            lineDrawer.SetPrimitives(bsphColor, Line3D.CreateWiredSphere(bsph, bsphSlices, bsphStacks));
        }
        protected void UpdateSoldierStates()
        {
            foreach (var soldierC in soldierModels.Keys)
            {
                if (soldierC.CurrentHealth == HealthStates.Disabled)
                {
                    soldierModels[soldierC].Active = false;
                    soldierModels[soldierC].Visible = false;
                }
                else
                {
                    soldierModels[soldierC].Active = true;
                    soldierModels[soldierC].Visible = true;
                }
            }
        }

        protected void Move(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Move(active, active.CurrentMovingCapacity))
            {
                var model = soldierModels[active];
                var controller = soldierControllers[active];

                //Run 3d actions
                var path = SetPath(soldierAgent, model.Manipulator.Position, destination);
                if (path != null)
                {
                    //Set move animation clip
                    model.AnimationController.SetPath(animations["walk"]);

                    //Folow
                    controller.Follow(path);
                    controller.MaximumSpeed = 1.4f;

                    GoToSoldier(active);
                }

                RefreshActions();
            }
        }
        protected void Crawl(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Crawl(active, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                var path = SetPath(soldierAgent, soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set crawl animation clip
                    soldierControllers[active].Follow(path);

                    GoToSoldier(active);
                }

                RefreshActions();
            }
        }
        protected void Run(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Run(active, active.CurrentMovingCapacity))
            {
                var model = soldierModels[active];
                var controller = soldierControllers[active];

                //Run 3d actions
                var path = SetPath(soldierAgent, model.Manipulator.Position, destination);
                if (path != null)
                {
                    //Set move animation clip
                    model.AnimationController.SetPath(animations["run"]);

                    //Folow
                    controller.Follow(path);
                    controller.MaximumSpeed = 4.0f;

                    GoToSoldier(active);
                }

                RefreshActions();
            }
        }
        protected void Assault(Soldier active, Soldier passive)
        {
            skirmishGame.JoinMelee(active, passive);

            if (ActionsManager.Assault(active, passive, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                Manipulator3D passiveMan = soldierModels[passive].Manipulator;
                Manipulator3D activeMan = soldierModels[active].Manipulator;

                Vector3 dir = Vector3.Normalize(activeMan.Position - passiveMan.Position);
                Vector3 destination = passiveMan.Position + (dir * 3f);

                var path = SetPath(soldierAgent, activeMan.Position, destination);
                if (path != null)
                {
                    //TODO: Set assault animation clip
                    soldierControllers[active].Follow(path);

                    GoToSoldier(active);
                }

                RefreshActions();
            }
        }
        protected void CoveringFire(Soldier active, Weapon weapon, Area area)
        {
            if (ActionsManager.CoveringFire(active, weapon, area, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set covering fire animation clip

                RefreshActions();
            }
        }
        protected void Reload(Soldier active, Weapon weapon)
        {
            if (ActionsManager.Reload(active, weapon, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set reload animation clip

                RefreshActions();
            }
        }
        protected void Repair(Soldier active, Weapon weapon)
        {
            if (ActionsManager.Repair(active, weapon, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set repair animation clip

                RefreshActions();
            }
        }
        protected void Inventory(Soldier active)
        {
            if (ActionsManager.Inventory(active, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set inventory animation clip

                //TODO: Show inventory screen

                RefreshActions();
            }
        }
        protected void UseMovementItem(Soldier active)
        {
            if (ActionsManager.UseMovementItem(active, active.CurrentItem, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                RefreshActions();
            }
        }
        protected void Communications(Soldier active)
        {
            if (ActionsManager.Communications(active))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                RefreshActions();
            }
        }
        protected void FindCover(Soldier active, Vector3 destination)
        {
            if (ActionsManager.FindCover(active))
            {
                //Run 3d actions
                var path = SetPath(soldierAgent, soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    soldierControllers[active].Follow(path);

                    GoToSoldier(active);
                }

                RefreshActions();
            }
        }
        protected void RunAway(Soldier active, Vector3 destination)
        {
            if (ActionsManager.RunAway(active))
            {
                //Run 3d actions
                var path = SetPath(soldierAgent, soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    soldierControllers[active].Follow(path);

                    GoToSoldier(active);
                }

                RefreshActions();
            }
        }
        protected void Shoot(Soldier active, Weapon weapon, Soldier passive)
        {
            Manipulator3D passiveMan = soldierModels[passive].Manipulator;
            Manipulator3D activeMan = soldierModels[active].Manipulator;

            float distance = Vector3.Distance(passiveMan.Position, activeMan.Position);

            if (ActionsManager.Shoot(active, weapon, distance, passive, active.CurrentActionPoints))
            {
                //Run 3d actions

                //TODO: Set shooting animation clip for active

                if (passive.CurrentHealth == HealthStates.Disabled)
                {
                    //Set impacted and killed animation clip for passive
                    passive.AnimateKill(weapon);
                }
                else
                {
                    //Set impacted animation clip for passive
                    passive.AnimateHurt(weapon);
                }

                RefreshActions();
            }
        }
        protected void SupressingFire(Soldier active, Weapon weapon, Area area)
        {
            if (ActionsManager.SupressingFire(active, weapon, area, active.CurrentActionPoints))
            {
                //Run 3d actions
                //...
                //TODO: Set supressing fire animation clip

                RefreshActions();
            }
        }
        protected void Support(Soldier active)
        {
            if (ActionsManager.Support(active))
            {
                //Run 3d actions
                //...
                //TODO: Set support animation clip

                RefreshActions();
            }
        }
        protected void UseShootingItem(Soldier active)
        {
            if (ActionsManager.UseShootingItem(active, active.CurrentItem, active.CurrentActionPoints))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                RefreshActions();
            }
        }
        protected void FirstAid(Soldier active, Soldier passive)
        {
            if (ActionsManager.FirstAid(active, passive, active.CurrentActionPoints))
            {
                //Run 3d actions
                //...
                //TODO: Set support animation clip

                RefreshActions();
            }
        }
        protected void LeaveCombat(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Leave(active))
            {
                Melee melee = skirmishGame.GetMelee(active);
                melee.RemoveFighter(active);

                //Run 3d actions
                var path = SetPath(soldierAgent, soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //Set run animation clip
                    soldierControllers[active].Follow(path);

                    GoToSoldier(active);
                }

                RefreshActions();
            }
        }
        protected void UseMeleeItem(Soldier active)
        {
            if (ActionsManager.UseMeleeItem(active, active.CurrentItem))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                RefreshActions();
            }
        }
        protected void TakeControl(Soldier active)
        {
            if (ActionsManager.TakeControl(active))
            {
                //Run 3d actions

                //TODO: Set take control animation clip

                RefreshActions();
            }
        }
        protected void UseMoraleItem(Soldier active)
        {
            if (ActionsManager.UseMoraleItem(active, active.CurrentItem))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                RefreshActions();
            }
        }

        private IControllerPath SetPath(GridAgentType agentType, Vector3 origin, Vector3 destination)
        {
            var path = FindPath(agentType, origin, destination);
            if (path != null)
            {
                return new SegmentPath(origin, path.Positions);
            }

            return null;
        }
        private Soldier PickSoldier(Ray cursorRay, bool enemyOnly)
        {
            return PickSoldierNearestToPosition(cursorRay, enemyOnly);
        }
        private Soldier PickSoldierNearestToPosition(Ray cursorRay, bool enemyOnly)
        {
            Team[] teams = enemyOnly ? skirmishGame.EnemyOf(skirmishGame.CurrentTeam) : skirmishGame.Teams;
            if (!teams.Any())
            {
                return null;
            }

            //Select nearest picked soldier
            float d = float.MaxValue;
            Soldier res = null;
            foreach (Team team in teams)
            {
                foreach (var soldierC in team.Soldiers)
                {
                    var picked = soldierModels[soldierC].PickNearest(cursorRay, out var r);
                    if (!picked)
                    {
                        continue;
                    }

                    if (r.Distance < d)
                    {
                        d = r.Distance;
                        res = soldierC;
                    }
                }
            }

            return res;
        }
        private Vector3 GetRandomPoint()
        {
            return Vector3.Zero;
        }
    }
}
