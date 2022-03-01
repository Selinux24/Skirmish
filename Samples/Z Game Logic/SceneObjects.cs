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
        private AnimationPlan soldierCrawl;
        private AnimationPlan soldierWalk;
        private AnimationPlan soldierRun;
        private AnimationPlan soldierAssault;
        private AnimationPlan soldierStand;
        private AnimationPlan soldierDefendAssault;
        private AnimationPlan soldierRandomFire;
        private AnimationPlan soldierReload;
        private AnimationPlan soldierUseItem;
        private AnimationPlan soldierMoraleRestored;
        private readonly float soldierCrawlSpeed = 1f;
        private readonly float soldierWalkSpeed = 4.5f;
        private readonly float soldierRunSpeed = 9f;
        private readonly float soldierAssaultSpeed = 6f;

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

        public SelectorTypes CurrentSelector { get; set; } = SelectorTypes.Default;

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
            GameEnvironment.Background = Color.Black;

            Camera.FarPlaneDistance = 1000f;
            Camera.Mode = CameraModes.FreeIsometric;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            NewGame();

            spotLight = new SceneLightSpot(
                "Current soldier",
                false,
                Color3.White,
                Color3.White,
                true,
                SceneLightSpotDescription.Create(Vector3.Zero, Vector3.Down, 15f, 15f, 100f));

            Lights.Add(spotLight);

            InitializeResources();
        }
        private void InitializeResources()
        {
            LoadResourcesAsync(
                new[]
                {
                    InitializeModels(),
                    InitializeHUD(),
                    InitializeDebug()
                },
                InitializeResourcesCompleted);
        }
        private async Task InitializeModels()
        {
            cursor3D = await AddComponentUI<Model, ModelDescription>(
                "Cursor3D",
                "Cursor3D",
                new ModelDescription()
                {
                    CastShadow = false,
                    DepthEnabled = false,
                    Content = ContentDescription.FromFile("Resources3D", "cursor.json"),
                },
                LayerEffects);

            troops = await AddComponentAgent<ModelInstanced, ModelInstancedDescription>(
                "Troops",
                "Troops",
                new ModelInstancedDescription()
                {
                    Instances = skirmishGame.AllSoldiers.Length,
                    CastShadow = true,
                    Content = ContentDescription.FromFile("Resources3D", "soldier_anim2.json"),
                });

            terrain = await AddComponentGround<Scenery, GroundDescription>(
                "Terrain",
                "Terrain",
                GroundDescription.FromFile("Resources3D", "terrain.json"));

            int minimapHeight = (Game.Form.RenderHeight / 4) - 8;
            int minimapWidth = minimapHeight;
            var topLeft = new Vector2(593, 443);
            var bottomRight = new Vector2(789, 590);
            var tRes = new Vector2(800, 600);
            var wRes = new Vector2(Game.Form.RenderWidth, Game.Form.RenderHeight);
            var pTopLeft = wRes / tRes * topLeft;
            var pBottomRight = wRes / tRes * bottomRight;
            var q = pTopLeft + ((pBottomRight - pTopLeft - new Vector2(minimapWidth, minimapHeight)) * 0.5f);

            await AddComponentUI<UIMinimap, UIMinimapDescription>(
                "Minimap",
                "Minimap",
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
                });

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
            SpriteDescription bkDesc = SpriteDescription.Background("Resources/HUD.png");
            await AddComponentUI<Sprite, SpriteDescription>("HUD", "HUD", bkDesc, LayerUI - 1);

            var titleFont = TextDrawerDescription.FromFamily(titleFontFileName, fontSize * 3, true);
            var gameFont = TextDrawerDescription.FromFile(fontFileName, (int)(fontSize * 1.25f));
            var textFont = TextDrawerDescription.FromFile(fontFileName, fontSize);
            var buttonsFont = TextDrawerDescription.FromFile(fontFileName, fontSize);

            txtTitle = await AddComponentUI<UITextArea, UITextAreaDescription>("txtTitle", "txtTitle", UITextAreaDescription.Default(titleFont));
            txtTitle.TextForeColor = Color.White;
            txtTitle.TextShadowColor = Color.Gray;

            txtGame = await AddComponentUI<UITextArea, UITextAreaDescription>("txtGame", "txtGame", UITextAreaDescription.Default(gameFont));
            txtGame.TextForeColor = Color.LightBlue;
            txtGame.TextShadowColor = Color.DarkBlue;

            txtTeam = await AddComponentUI<UITextArea, UITextAreaDescription>("txtTeam", "txtTeam", UITextAreaDescription.Default(textFont));
            txtTeam.TextForeColor = Color.Yellow;

            txtSoldier = await AddComponentUI<UITextArea, UITextAreaDescription>("txtSoldier", "txtSoldier", UITextAreaDescription.Default(textFont));
            txtSoldier.TextForeColor = Color.Yellow;

            txtActionList = await AddComponentUI<UITextArea, UITextAreaDescription>("txtActionList", "txtActionList", UITextAreaDescription.Default(textFont));
            txtActionList.TextForeColor = Color.Yellow;

            txtAction = await AddComponentUI<UITextArea, UITextAreaDescription>("txtAction", "txtAction", UITextAreaDescription.Default(textFont));
            txtAction.TextForeColor = Color.Yellow;

            var butCloseDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "button_on.png", "button_off.png");
            butCloseDesc.Width = 60;
            butCloseDesc.Height = 20;
            butCloseDesc.TextForeColor = Color.Yellow;
            butCloseDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            butCloseDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            butCloseDesc.Text = "Exit";

            butClose = await AddComponentUI<UIButton, UIButtonDescription>("butClose", "butClose", butCloseDesc);

            butNext = await AddComponentUI<UIButton, UIButtonDescription>("butNext", "butNext", new UIButtonDescription()
            {
                TwoStateButton = true,
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 120,
                Height = 20,
                Font = buttonsFont,
                Text = "Next Phase",
            });
            butNext.Caption.TextForeColor = Color.Yellow;

            butPrevSoldier = await AddComponentUI<UIButton, UIButtonDescription>("butPrevSoldier", "butPrevSoldier", new UIButtonDescription()
            {
                TwoStateButton = true,
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 150,
                Height = 20,
                Font = buttonsFont,
                Text = "Prev.Soldier",
            });
            butPrevSoldier.Caption.TextForeColor = Color.Yellow;

            butNextSoldier = await AddComponentUI<UIButton, UIButtonDescription>("butNextSoldier", "butNextSoldier", new UIButtonDescription()
            {
                TwoStateButton = true,
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 150,
                Height = 20,
                Font = buttonsFont,
                Text = "Next Soldier",
            });
            butNextSoldier.Caption.TextForeColor = Color.Yellow;

            butPrevAction = await AddComponentUI<UIButton, UIButtonDescription>("butPrevAction", "butPrevAction", new UIButtonDescription()
            {
                TwoStateButton = true,
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 150,
                Height = 20,
                Font = buttonsFont,
                Text = "Prev.Action",
            });
            butPrevAction.Caption.TextForeColor = Color.Yellow;

            butNextAction = await AddComponentUI<UIButton, UIButtonDescription>("butNextAction", "butNextAction", new UIButtonDescription()
            {
                TwoStateButton = true,
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 150,
                Height = 20,
                Font = buttonsFont,
                Text = "Next Action",
            });
            butNextAction.Caption.TextForeColor = Color.Yellow;

            butClose.MouseClick += (sender, eventArgs) => { if (eventArgs.Buttons.HasFlag(MouseButtons.Left)) Game.Exit(); };
            butNext.MouseClick += (sender, eventArgs) => { if (eventArgs.Buttons.HasFlag(MouseButtons.Left)) NextPhase(); };
            butPrevSoldier.MouseClick += (sender, eventArgs) => { if (eventArgs.Buttons.HasFlag(MouseButtons.Left)) PrevSoldier(true); };
            butNextSoldier.MouseClick += (sender, eventArgs) => { if (eventArgs.Buttons.HasFlag(MouseButtons.Left)) NextSoldier(true); };
            butPrevAction.MouseClick += (sender, eventArgs) => { if (eventArgs.Buttons.HasFlag(MouseButtons.Left)) PrevAction(); };
            butNextAction.MouseClick += (sender, eventArgs) => { if (eventArgs.Buttons.HasFlag(MouseButtons.Left)) NextAction(); };

            txtTitle.Text = "Game Logic";
        }
        private async Task InitializeDebug()
        {
            lineDrawer = await AddComponent<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "DebugLineDrawer",
                "DebugLineDrawer",
                new PrimitiveListDrawerDescription<Line3D>() { Count = 5000 });

            lineDrawer.Visible = false;
        }
        private async Task InitializeResourcesCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            UpdateLayout();

            InitializeAnimations();

            InitializePositions();

            GoToSoldier(skirmishGame.CurrentSoldier);

            await UpdateNavigationGraph();

            gameReady = true;
        }
        private void InitializeAnimations()
        {
            soldierCrawl = AnimationPlan.CreateLoop("crawl");
            soldierWalk = AnimationPlan.CreateLoop("walk");
            soldierRun = AnimationPlan.CreateLoop("run");
            soldierAssault = AnimationPlan.CreateLoop("assault");
            soldierStand = AnimationPlan.CreateLoop("stand");
            soldierDefendAssault = AnimationPlan.CreateLoop("defendAssault");
            soldierRandomFire = AnimationPlan.CreateLoop("randomFire");
            soldierReload = AnimationPlan.Create("reload");
            soldierUseItem = AnimationPlan.Create("useItem");
            soldierMoraleRestored = AnimationPlan.Create("moraleRestored");
        }
        private void InitializePositions()
        {
            BoundingBox bbox = terrain.GetBoundingBox();

            float terrainHeight = bbox.Maximum.Z - bbox.Minimum.Z;
            float teamSeparation = terrainHeight / (skirmishGame.Teams.Length);

            float soldierSeparation = 12f;
            int instanceIndex = 0;
            uint teamIndex = 0;
            foreach (var teamSoldiers in skirmishGame.Teams.Select(t => t.Soldiers))
            {
                float teamWidth = teamSoldiers.Length * soldierSeparation;

                int soldierIndex = 0;
                foreach (var soldierC in teamSoldiers)
                {
                    var soldier = troops[instanceIndex++];

                    soldier.TextureIndex = teamIndex;
                    soldier.AnimationController.Start(soldierStand, soldierIndex);

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

            var cursorRay = GetPickingRay();
            bool picked = this.PickNearest(cursorRay, SceneObjectUsages.Ground, out ScenePickingResult<Triangle> r);

            //DEBUG
            UpdateDebug();

            //HUD
            UpdateHUD();

            if (TopMostControl != null)
            {
                return;
            }

            //3D
            Update3D(gameTime, cursorRay, picked, r);

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
        private void UpdateHUD()
        {
            if (Game.Input.KeyJustReleased(keyHUDNextSoldier))
            {
                NextSoldier(!Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyJustReleased(keyHUDPrevSoldier))
            {
                PrevSoldier(!Game.Input.ShiftPressed);
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
        private void Update3D(GameTime gameTime, PickingRay cursorRay, bool picked, ScenePickingResult<Triangle> r)
        {
            if (picked)
            {
                cursor3D.Manipulator.SetPosition(r.PickingResult.Position);
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
                Camera.MoveLeft(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(keyCAMMoveRight))
            {
                Camera.MoveRight(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(keyCAMMoveForward))
            {
                Camera.MoveForward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(keyCAMMoveBackward))
            {
                Camera.MoveBackward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.MouseButtonJustReleased(MouseButtons.Right))
            {
                DoGoto(cursorRay, picked, r.PickingResult.Position);
            }

            if (Game.Input.MouseWheelDelta > 0)
            {
                Camera.ZoomIn(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.MouseWheelDelta < 0)
            {
                Camera.ZoomOut(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyJustReleased(keyCAMCenterSoldier))
            {
                GoToSoldier(skirmishGame.CurrentSoldier);
            }

            spotLight.Position = soldierModels[skirmishGame.CurrentSoldier].Manipulator.Position + (Vector3.UnitY * 10f);
        }
        private void DoGoto(PickingRay cursorRay, bool picked, Vector3 pickedPosition)
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
        private void UpdateActions(PickingRay cursorRay, bool picked, ScenePickingResult<Triangle> r)
        {
            if (CurrentAction != null)
            {
                bool selectorDone = false;
                Area area = null;

                if (Game.Input.MouseButtonJustReleased(MouseButtons.Left))
                {
                    if (CurrentAction.Selector == Selectors.Goto)
                    {
                        selectorDone = picked;
                        SetSelector(SelectorTypes.Goto);
                    }
                    else if (CurrentAction.Selector == Selectors.Area)
                    {
                        area = new Area();

                        selectorDone = picked;
                        SetSelector(SelectorTypes.Area);
                    }
                }

                if (selectorDone)
                {
                    UpdateSelected(cursorRay, r.PickingResult.Position, area);
                }
            }
        }
        private void UpdateSelected(PickingRay cursorRay, Vector3 position, Area area)
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
        private void SetSelector(SelectorTypes selector)
        {
            CurrentSelector = selector;
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

        private void Controller_PathEnd(object sender, EventArgs e)
        {
            var instance = sender as ManipulatorController;

            foreach (var item in soldierControllers)
            {
                if (item.Value == instance)
                {
                    soldierModels[item.Key].AnimationController.ReplacePlan(soldierStand);
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
            throw new NotImplementedException();
        }
        protected void SaveGame()
        {
            throw new NotImplementedException();
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
                    foreach (var action in actions.Select(a => a.Action))
                    {
                        if (action == Actions.FindCover)
                        {
                            Vector3 point = GetRandomPoint();

                            FindCover(soldierC, point);
                        }
                        else if (action == Actions.RunAway)
                        {
                            Vector3 point = GetRandomPoint();

                            RunAway(soldierC, point);
                        }
                        else if (action == Actions.TakeControl)
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
                MoveToAction(active, destination, soldierWalkSpeed, soldierWalk);
            }
        }
        protected void Crawl(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Crawl(active, active.CurrentMovingCapacity))
            {
                MoveToAction(active, destination, soldierCrawlSpeed, soldierCrawl);
            }
        }
        protected void Run(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Run(active, active.CurrentMovingCapacity))
            {
                MoveToAction(active, destination, soldierRunSpeed, soldierRun);
            }
        }
        protected void Assault(Soldier active, Soldier passive)
        {
            skirmishGame.JoinMelee(active, passive);

            if (ActionsManager.Assault(active, passive, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                var passiveModel = soldierModels[passive];
                var activeModel = soldierModels[active];

                Vector3 dir = Vector3.Normalize(activeModel.Manipulator.Position - passiveModel.Manipulator.Position);
                Vector3 destination = passiveModel.Manipulator.Position + (dir * 3f);

                MoveToAction(active, destination, soldierAssaultSpeed, soldierAssault);
                InPlaceAction(passive, soldierDefendAssault);
            }
        }
        protected void CoveringFire(Soldier active, Weapon weapon, Area area)
        {
            if (ActionsManager.CoveringFire(active, weapon, area, active.CurrentMovingCapacity))
            {
                InPlaceAction(active, soldierRandomFire);
            }
        }
        protected void Reload(Soldier active, Weapon weapon)
        {
            if (ActionsManager.Reload(active, weapon, active.CurrentMovingCapacity))
            {
                InPlaceAction(active, soldierReload);
            }
        }
        protected void Repair(Soldier active, Weapon weapon)
        {
            if (ActionsManager.Repair(active, weapon, active.CurrentMovingCapacity))
            {
                InPlaceAction(active, soldierUseItem);
            }
        }
        protected void Inventory(Soldier active)
        {
            if (ActionsManager.Inventory(active, active.CurrentMovingCapacity))
            {
                InPlaceAction(active, soldierUseItem);

                //Show inventory screen
                ShowInventory(active);
            }
        }
        protected void UseMovementItem(Soldier active)
        {
            if (ActionsManager.UseMovementItem(active, active.CurrentItem, active.CurrentMovingCapacity))
            {
                InPlaceAction(active, soldierUseItem);
            }
        }
        protected void Communications(Soldier active)
        {
            if (ActionsManager.Communications(active))
            {
                InPlaceAction(active, soldierUseItem);
            }
        }
        protected void FindCover(Soldier active, Vector3 destination)
        {
            if (ActionsManager.FindCover(active))
            {
                MoveToAction(active, destination, soldierRunSpeed, soldierRun);
            }
        }
        protected void RunAway(Soldier active, Vector3 destination)
        {
            if (ActionsManager.RunAway(active))
            {
                MoveToAction(active, destination, soldierRunSpeed, soldierRun);
            }
        }
        protected void Shoot(Soldier active, Weapon weapon, Soldier passive)
        {
            var passiveModel = soldierModels[passive];
            var activeModel = soldierModels[active];

            Manipulator3D passiveMan = passiveModel.Manipulator;
            Manipulator3D activeMan = activeModel.Manipulator;

            float distance = Vector3.Distance(passiveMan.Position, activeMan.Position);

            if (ActionsManager.Shoot(active, weapon, distance, passive, active.CurrentActionPoints))
            {
                //Run 3d actions

                //Set shooting animation clip for active
                activeModel.AnimationController.ReplacePlan(soldierRandomFire);

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
                InPlaceAction(active, soldierRandomFire);
            }
        }
        protected void Support(Soldier active)
        {
            if (ActionsManager.Support(active))
            {
                InPlaceAction(active, soldierReload);
            }
        }
        protected void UseShootingItem(Soldier active)
        {
            if (ActionsManager.UseShootingItem(active, active.CurrentItem, active.CurrentActionPoints))
            {
                InPlaceAction(active, soldierUseItem);
            }
        }
        protected void FirstAid(Soldier active, Soldier passive)
        {
            if (ActionsManager.FirstAid(active, passive, active.CurrentActionPoints))
            {
                InPlaceAction(active, soldierUseItem);
            }
        }
        protected void LeaveCombat(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Leave(active))
            {
                Melee melee = skirmishGame.GetMelee(active);
                melee.RemoveFighter(active);

                MoveToAction(active, destination, soldierRunSpeed, soldierRun);
            }
        }
        protected void UseMeleeItem(Soldier active)
        {
            if (ActionsManager.UseMeleeItem(active, active.CurrentItem))
            {
                InPlaceAction(active, soldierUseItem);
            }
        }
        protected void TakeControl(Soldier active)
        {
            if (ActionsManager.TakeControl(active))
            {
                InPlaceAction(active, soldierMoraleRestored);
            }
        }
        protected void UseMoraleItem(Soldier active)
        {
            if (ActionsManager.UseMoraleItem(active, active.CurrentItem))
            {
                InPlaceAction(active, soldierUseItem);
            }
        }

        private void MoveToAction(Soldier active, Vector3 destination, float speed, AnimationPlan animation)
        {
            var model = soldierModels[active];
            var controller = soldierControllers[active];

            //Run 3d actions
            var path = SetPath(soldierAgent, model.Manipulator.Position, destination);
            if (path != null)
            {
                //Set animation clip
                model.AnimationController.ReplacePlan(animation);

                //Folow
                controller.Follow(path);
                controller.MaximumSpeed = speed;

                GoToSoldier(active);
            }

            RefreshActions();
        }
        private void InPlaceAction(Soldier active, AnimationPlan animation)
        {
            //Run 3d actions
            var activeModel = soldierModels[active];

            //Set animation clip
            activeModel.AnimationController.ReplacePlan(animation);

            RefreshActions();
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
        private Soldier PickSoldier(PickingRay cursorRay, bool enemyOnly)
        {
            return PickSoldierNearestToPosition(cursorRay, enemyOnly);
        }
        private Soldier PickSoldierNearestToPosition(PickingRay cursorRay, bool enemyOnly)
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

        private void ShowInventory(Soldier active)
        {
            throw new NotImplementedException();
        }
    }
}
