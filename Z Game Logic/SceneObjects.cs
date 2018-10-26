using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.PathFinding.AStar;
using SharpDX;
using System;
using System.Collections.Generic;

namespace GameLogic
{
    using GameLogic.Rules;
    using GameLogic.Rules.Enum;

    public class SceneObjects : Scene
    {
        private const int layerHUD = 99;

        private SceneObject<TextDrawer> txtTitle = null;
        private SceneObject<TextDrawer> txtGame = null;
        private SceneObject<TextDrawer> txtTeam = null;
        private SceneObject<TextDrawer> txtSoldier = null;
        private SceneObject<TextDrawer> txtActionList = null;
        private SceneObject<TextDrawer> txtAction = null;

        private readonly string fontName = "Lucida Sans";
        private SceneObject<SpriteButton> butClose = null;
        private SceneObject<SpriteButton> butNext = null;
        private SceneObject<SpriteButton> butPrevSoldier = null;
        private SceneObject<SpriteButton> butNextSoldier = null;
        private SceneObject<SpriteButton> butPrevAction = null;
        private SceneObject<SpriteButton> butNextAction = null;

        private SceneObject<Model> cursor3D = null;

        private SceneLightSpot spotLight = null;

        private SceneObject<ModelInstanced> soldier = null;
        private readonly GridAgentType soldierAgent = null;
        private readonly Dictionary<Soldier, ModelInstance> soldierModels = new Dictionary<Soldier, ModelInstance>();
        private readonly Dictionary<Soldier, ManipulatorController> soldierControllers = new Dictionary<Soldier, ManipulatorController>();
        private readonly Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();
        private ModelInstance Current
        {
            get
            {
                return this.soldierModels[this.skirmishGame.CurrentSoldier];
            }
        }

        private SceneObject<LineListDrawer> lineDrawer = null;
        private readonly Color4 bsphColor = new Color4(Color.LightYellow.ToColor3(), 0.25f);
        private readonly Color4 frstColor = new Color4(Color.Yellow.ToColor3(), 1f);
        private readonly int bsphSlices = 50;
        private readonly int bsphStacks = 25;

        private SceneObject<Scenery> terrain = null;

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
            : base(game, SceneModes.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            GameEnvironment.Background = Color.Black;

            this.Camera.FarPlaneDistance = 1000f;
            this.Camera.Mode = CameraModes.FreeIsometric;

            this.NewGame();

            this.spotLight = new SceneLightSpot(
                "Current soldier",
                false,
                Color.White,
                Color.White,
                true,
                Vector3.Zero,
                Vector3.Down,
                15f,
                15f,
                100f);

            this.Lights.Add(this.spotLight);

            #region 3D models

            this.cursor3D = this.AddComponent<Model>(
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

            this.terrain = this.AddComponent<Scenery>(
                new GroundDescription()
                {
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources3D",
                        ModelContentFilename = "terrain.xml",
                    }
                },
                SceneObjectUsages.Ground);

            this.soldier = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Instances = this.skirmishGame.AllSoldiers.Length,
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources3D",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                },
                SceneObjectUsages.Agent);

            #endregion

            this.SetGround(this.terrain, true);

            GridInput input = new GridInput(GetTrianglesForNavigationGraph);
            GridGenerationSettings settings = new GridGenerationSettings()
            {
                NodeSize = 5f,
            };
            this.PathFinderDescription = new Engine.PathFinding.PathFinderDescription(settings, input);

            #region HUD

            SpriteBackgroundDescription bkDesc = new SpriteBackgroundDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "HUD.png" },
                Color = new Color4(1f, 1f, 1f, 1f),
            };
            this.AddComponent<Sprite>(bkDesc, SceneObjectUsages.UI, layerHUD - 1);

            int minimapHeight = (this.Game.Form.RenderHeight / 4) - 8;
            int minimapWidth = minimapHeight;

            var topLeft = new Vector2(593, 443);
            var bottomRight = new Vector2(789, 590);
            var tRes = new Vector2(800, 600);
            var wRes = new Vector2(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);

            var pTopLeft = wRes / tRes * topLeft;
            var pBottomRight = wRes / tRes * bottomRight;

            var q = pTopLeft + ((pBottomRight - pTopLeft - new Vector2(minimapWidth, minimapHeight)) * 0.5f);

            MinimapDescription minimapDesc = new MinimapDescription()
            {
                Top = (int)q.Y,
                Left = (int)q.X,
                Width = minimapWidth,
                Height = minimapHeight,
                Drawables = new SceneObject[]
                {
                    this.terrain,
                    this.soldier,
                },
                MinimapArea = this.terrain.Instance.GetBoundingBox(),
            };
            this.AddComponent<Minimap>(minimapDesc, SceneObjectUsages.UI, layerHUD);

            this.txtTitle = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 24, Color.White, Color.Gray), SceneObjectUsages.UI, layerHUD);
            this.txtGame = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate(this.fontName, 12, Color.LightBlue, Color.DarkBlue), SceneObjectUsages.UI, layerHUD);
            this.txtTeam = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate(this.fontName, 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.txtSoldier = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate(this.fontName, 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.txtActionList = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate(this.fontName, 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.txtAction = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate(this.fontName, 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);

            this.butClose = this.AddComponent<SpriteButton>(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 60,
                Height = 20,
                TextDescription = new TextDrawerDescription()
                {
                    Font = this.fontName,
                    FontSize = 12,
                    TextColor = Color.Yellow,
                    ShadowColor = Color.Orange,
                },
                Text = "EXIT",
            }, SceneObjectUsages.UI, layerHUD);

            this.butNext = this.AddComponent<SpriteButton>(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 60,
                Height = 20,
                TextDescription = new TextDrawerDescription()
                {
                    Font = this.fontName,
                    FontSize = 10,
                    TextColor = Color.Yellow,
                },
                Text = "Next",
            }, SceneObjectUsages.UI, layerHUD);

            this.butPrevSoldier = this.AddComponent<SpriteButton>(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                TextDescription = new TextDrawerDescription()
                {
                    Font = this.fontName,
                    FontSize = 10,
                    TextColor = Color.Yellow,
                },
                Text = "Prev.Soldier",
            }, SceneObjectUsages.UI, layerHUD);

            this.butNextSoldier = this.AddComponent<SpriteButton>(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                TextDescription = new TextDrawerDescription()
                {
                    Font = this.fontName,
                    FontSize = 10,
                    TextColor = Color.Yellow,
                },
                Text = "Next Soldier",
            }, SceneObjectUsages.UI, layerHUD);

            this.butPrevAction = this.AddComponent<SpriteButton>(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                TextDescription = new TextDrawerDescription()
                {
                    Font = this.fontName,
                    FontSize = 10,
                    TextColor = Color.Yellow,
                },
                Text = "Prev.Action",
            }, SceneObjectUsages.UI, layerHUD);

            this.butNextAction = this.AddComponent<SpriteButton>(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                TextDescription = new TextDrawerDescription()
                {
                    Font = this.fontName,
                    FontSize = 10,
                    TextColor = Color.Yellow,
                },
                Text = "Next Action",
            }, SceneObjectUsages.UI, layerHUD);

            this.butClose.Instance.Click += (sender, eventArgs) => { this.Game.Exit(); };
            this.butNext.Instance.Click += (sender, eventArgs) => { this.NextPhase(); };
            this.butPrevSoldier.Instance.Click += (sender, eventArgs) => { this.PrevSoldier(true); };
            this.butNextSoldier.Instance.Click += (sender, eventArgs) => { this.NextSoldier(true); };
            this.butPrevAction.Instance.Click += (sender, eventArgs) => { this.PrevAction(); };
            this.butNextAction.Instance.Click += (sender, eventArgs) => { this.NextAction(); };

            this.txtTitle.Instance.Text = "Game Logic";

            #endregion

            this.UpdateLayout();

            this.InitializeAnimations();

            this.InitializePositions();

            #region DEBUG

            this.lineDrawer = this.AddComponent<LineListDrawer>(new LineListDrawerDescription() { Count = 5000 });
            this.lineDrawer.Visible = false;

            #endregion

            this.GoToSoldier(this.skirmishGame.CurrentSoldier);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            foreach (var soldierC in this.soldierControllers.Keys)
            {
                this.soldierControllers[soldierC].UpdateManipulator(gameTime, this.soldierModels[soldierC].Manipulator);
            }

            if (!this.CapturedControl)
            {
                if (this.Game.Input.KeyJustReleased(this.keyExit))
                {
                    this.Game.Exit();
                }

                if (this.Game.Input.KeyJustReleased(this.keyChangeMode))
                {
                    this.SetRenderMode(this.GetRenderMode() == SceneModes.ForwardLigthning ?
                        SceneModes.DeferredLightning :
                        SceneModes.ForwardLigthning);
                }

                bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey) || this.Game.Input.KeyPressed(Keys.RShiftKey);

                Ray cursorRay = this.GetPickingRay();
                bool picked = this.PickNearest(ref cursorRay, true, SceneObjectUsages.Ground, out PickingResult<Triangle> r);

                //DEBUG
                this.UpdateDebug();

                //HUD
                this.UpdateHUD(shift);

                //3D
                this.Update3D(gameTime, shift, cursorRay, picked, r);

                //Actions
                this.UpdateActions(cursorRay, picked, r);
            }
        }
        private void UpdateDebug()
        {
            if (this.Game.Input.KeyJustReleased(this.keyDebugFrustum))
            {
                this.SetFrustum();
            }

            if (this.Game.Input.KeyJustReleased(this.keyDebugVolumes))
            {
                this.lineDrawer.Visible = !this.lineDrawer.Visible;
            }
        }
        private void UpdateHUD(bool shift)
        {
            if (this.Game.Input.KeyJustReleased(this.keyHUDNextSoldier))
            {
                this.NextSoldier(!shift);
            }

            if (this.Game.Input.KeyJustReleased(this.keyHUDPrevSoldier))
            {
                this.PrevSoldier(!shift);
            }

            if (this.Game.Input.KeyJustReleased(this.keyHUDNextAction))
            {
                this.NextAction();
            }

            if (this.Game.Input.KeyJustReleased(this.keyHUDPrevAction))
            {
                this.PrevAction();
            }

            if (this.Game.Input.KeyJustReleased(this.keyHUDNextPhase))
            {
                this.NextPhase();
            }

            if (!this.gameFinished)
            {
                this.txtGame.Instance.Text = string.Format("{0}", this.skirmishGame);
                this.txtTeam.Instance.Text = string.Format("{0}", this.skirmishGame.CurrentTeam);
                this.txtSoldier.Instance.Text = string.Format("{0}", this.skirmishGame.CurrentSoldier);
                this.txtActionList.Instance.Text = string.Format("{0}", this.currentActions.Join(" | "));
                this.txtAction.Instance.Text = string.Format("{0}", this.CurrentAction);
            }
        }
        private void Update3D(GameTime gameTime, bool shift, Ray cursorRay, bool picked, PickingResult<Triangle> r)
        {
            if (picked)
            {
                this.cursor3D.Transform.SetPosition(r.Position);
            }

            if (this.Game.Input.KeyJustReleased(this.keyCAMNextIsometric))
            {
                this.Camera.NextIsometricAxis();
            }

            if (this.Game.Input.KeyJustReleased(this.keyCAMPrevIsometric))
            {
                this.Camera.PreviousIsometricAxis();
            }

            if (this.Game.Input.KeyPressed(this.keyCAMMoveLeft))
            {
                this.Camera.MoveLeft(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(this.keyCAMMoveRight))
            {
                this.Camera.MoveRight(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(this.keyCAMMoveForward))
            {
                this.Camera.MoveForward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(this.keyCAMMoveBackward))
            {
                this.Camera.MoveBackward(gameTime, shift);
            }

            if (this.Game.Input.RightMouseButtonJustReleased)
            {
                Soldier pickedSoldier = this.PickSoldier(cursorRay, false);
                if (pickedSoldier != null)
                {
                    if (pickedSoldier.Team == this.skirmishGame.CurrentTeam)
                    {
                        this.skirmishGame.CurrentSoldier = pickedSoldier;
                    }

                    this.GoToSoldier(pickedSoldier);
                }
                else if (picked)
                {
                    this.Camera.LookTo(r.Position, CameraTranslations.UseDelta);
                }
            }

            if (this.Game.Input.MouseWheelDelta > 0)
            {
                this.Camera.ZoomIn(gameTime, shift);
            }

            if (this.Game.Input.MouseWheelDelta < 0)
            {
                this.Camera.ZoomOut(gameTime, shift);
            }

            if (this.Game.Input.KeyJustReleased(this.keyCAMCenterSoldier))
            {
                this.GoToSoldier(this.skirmishGame.CurrentSoldier);
            }

            this.spotLight.Position = this.soldierModels[this.skirmishGame.CurrentSoldier].Manipulator.Position + (Vector3.UnitY * 10f);
        }
        private void UpdateActions(Ray cursorRay, bool picked, PickingResult<Triangle> r)
        {
            if (this.CurrentAction != null)
            {
                bool selectorDone = false;
                Area area = null;

                if (this.Game.Input.LeftMouseButtonJustReleased)
                {
                    if (this.CurrentAction.Selector == Selectors.Goto)
                    {
                        //TODO: Show goto selector
                        selectorDone = picked;
                    }
                    else if (this.CurrentAction.Selector == Selectors.Area)
                    {
                        //TODO: Show area selector
                        area = new Area();

                        selectorDone = picked;
                    }
                }

                if (selectorDone)
                {
                    if (this.CurrentAction.Action == Actions.Move)
                    {
                        this.Move(this.skirmishGame.CurrentSoldier, r.Position);
                    }
                    else if (this.CurrentAction.Action == Actions.Run)
                    {
                        this.Run(this.skirmishGame.CurrentSoldier, r.Position);
                    }
                    else if (this.CurrentAction.Action == Actions.Crawl)
                    {
                        this.Crawl(this.skirmishGame.CurrentSoldier, r.Position);
                    }
                    else if (this.CurrentAction.Action == Actions.Assault)
                    {
                        Soldier active = this.skirmishGame.CurrentSoldier;
                        Vector3 pos = this.Current.Manipulator.Position;
                        Soldier passive = this.PickSoldierNearestToPosition(cursorRay, pos, true);
                        if (passive != null)
                        {
                            this.Assault(active, passive);
                        }
                    }
                    else if (this.CurrentAction.Action == Actions.CoveringFire)
                    {
                        Soldier active = this.skirmishGame.CurrentSoldier;

                        this.CoveringFire(active, active.CurrentShootingWeapon, area);
                    }
                    else if (this.CurrentAction.Action == Actions.Reload)
                    {
                        Soldier active = this.skirmishGame.CurrentSoldier;

                        this.Reload(active, active.CurrentShootingWeapon);
                    }
                    else if (this.CurrentAction.Action == Actions.Repair)
                    {
                        Soldier active = this.skirmishGame.CurrentSoldier;

                        this.Repair(active, active.CurrentShootingWeapon);
                    }
                    else if (this.CurrentAction.Action == Actions.Inventory)
                    {
                        this.Inventory(this.skirmishGame.CurrentSoldier);
                    }
                    else if (this.CurrentAction.Action == Actions.UseMovementItem)
                    {
                        this.UseMovementItem(this.skirmishGame.CurrentSoldier);
                    }
                    else if (this.CurrentAction.Action == Actions.Communications)
                    {
                        this.Communications(this.skirmishGame.CurrentSoldier);
                    }
                }
            }
        }
        protected override void Resized(object sender, EventArgs e)
        {
            this.UpdateLayout();
        }
        private void UpdateLayout()
        {
            this.txtTitle.Instance.Top = 0;
            this.txtTitle.Instance.Left = 5;
            this.txtGame.Instance.Top = this.txtTitle.Instance.Top + this.txtTitle.Instance.Height + 1;
            this.txtGame.Instance.Left = 10;
            this.txtTeam.Instance.Top = this.txtGame.Instance.Top + this.txtGame.Instance.Height + 1;
            this.txtTeam.Instance.Left = this.txtGame.Instance.Left;

            this.butClose.Instance.Top = 1;
            this.butClose.Instance.Left = this.Game.Form.RenderWidth - 60 - 1;

            this.butNext.Instance.Top = (int)((float)this.Game.Form.RenderHeight * 0.85f);
            this.butNext.Instance.Left = 10;
            this.butPrevSoldier.Instance.Top = this.butNext.Instance.Top;
            this.butPrevSoldier.Instance.Left = this.butNext.Instance.Left + this.butNext.Instance.Width + 25;
            this.butNextSoldier.Instance.Top = this.butNext.Instance.Top;
            this.butNextSoldier.Instance.Left = this.butPrevSoldier.Instance.Left + this.butPrevSoldier.Instance.Width + 10;
            this.butPrevAction.Instance.Top = this.butNext.Instance.Top;
            this.butPrevAction.Instance.Left = this.butNextSoldier.Instance.Left + this.butNextSoldier.Instance.Width + 25;
            this.butNextAction.Instance.Top = this.butNext.Instance.Top;
            this.butNextAction.Instance.Left = this.butPrevAction.Instance.Left + this.butPrevAction.Instance.Width + 10;

            this.txtSoldier.Instance.Top = this.butNext.Instance.Top + this.butNext.Instance.Height + 1;
            this.txtSoldier.Instance.Left = 10;
            this.txtActionList.Instance.Top = this.txtSoldier.Instance.Top + this.txtSoldier.Instance.Height + 1;
            this.txtActionList.Instance.Left = this.txtSoldier.Instance.Left;
            this.txtAction.Instance.Top = this.txtActionList.Instance.Top + this.txtActionList.Instance.Height + 1;
            this.txtAction.Instance.Left = this.txtSoldier.Instance.Left;
        }

        private void InitializeAnimations()
        {
            {
                AnimationPath p = new AnimationPath();
                p.AddLoop("idle");

                this.animations.Add("idle", new AnimationPlan(p));
            }
            {
                AnimationPath p = new AnimationPath();
                p.AddLoop("stand");

                this.animations.Add("stand", new AnimationPlan(p));
            }
            {
                AnimationPath p = new AnimationPath();
                p.AddLoop("walk");
                this.animations.Add("walk", new AnimationPlan(p));
            }
            {
                AnimationPath p = new AnimationPath();
                p.AddLoop("run");
                this.animations.Add("run", new AnimationPlan(p));
            }
        }
        private void InitializePositions()
        {
            BoundingBox bbox = this.terrain.Instance.GetBoundingBox();

            float terrainHeight = bbox.Maximum.Z - bbox.Minimum.Z;
            float gameWidth = terrainHeight / (this.skirmishGame.Teams.Length + 1);
            float teamSeparation = terrainHeight / (this.skirmishGame.Teams.Length);

            float soldierSeparation = 10f;
            int instanceIndex = 0;
            uint teamIndex = 0;
            foreach (Team team in this.skirmishGame.Teams)
            {
                float teamWidth = team.Soldiers.Length * soldierSeparation;

                int soldierIndex = 0;
                foreach (var soldierC in team.Soldiers)
                {
                    ModelInstance instance = this.soldier.Instance[instanceIndex++];

                    instance.TextureIndex = teamIndex;
                    instance.AnimationController.AddPath(this.animations["stand"]);
                    instance.AnimationController.Start(soldierIndex);
                    instance.AnimationController.TimeDelta = 0.20f;

                    float x = (soldierIndex * soldierSeparation) - (teamWidth * 0.5f);
                    float z = (teamIndex * teamSeparation) - (gameWidth * 0.5f);

                    if (this.FindTopGroundPosition(x, z, out PickingResult<Triangle> r))
                    {
                        instance.Manipulator.SetPosition(r.Position, true);
                    }
                    else
                    {
                        throw new GameLogicException("Bad position");
                    }

                    if (teamIndex == 0)
                    {
                        instance.Manipulator.SetRotation(MathUtil.DegreesToRadians(180), 0, 0, true);
                    }

                    this.soldierModels.Add(soldierC, instance);
                    var controller = new BasicManipulatorController();
                    controller.PathEnd += Controller_PathEnd;
                    this.soldierControllers.Add(soldierC, controller);

                    soldierIndex++;
                }

                teamIndex++;
            }
        }

        private void Controller_PathEnd(object sender, EventArgs e)
        {
            var instance = sender as ManipulatorController;

            foreach (var item in this.soldierControllers)
            {
                if (item.Value == instance)
                {
                    this.soldierModels[item.Key].AnimationController.ContinuePath(this.animations["stand"]);
                }
            }
        }

        private void SetFrustum()
        {
            this.lineDrawer.Instance.SetLines(this.frstColor, Line3D.CreateWiredFrustum(this.Camera.Frustum));
        }

        protected void NewGame()
        {
            this.skirmishGame = new Skirmish();
            this.skirmishGame.AddTeam("Team1", "Reds", TeamRoles.Defense, 5, 1, 1);
            this.skirmishGame.AddTeam("Team2", "Blues", TeamRoles.Assault, 5, 1, 1);
            this.skirmishGame.Start();
            this.currentActions = this.skirmishGame.GetActions();
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
            this.skirmishGame.NextSoldier(selectIdle);

            this.currentActions = this.skirmishGame.GetActions();

            this.GoToSoldier(this.skirmishGame.CurrentSoldier);
        }
        protected void PrevSoldier(bool selectIdle)
        {
            this.skirmishGame.PrevSoldier(selectIdle);

            this.currentActions = this.skirmishGame.GetActions();

            this.GoToSoldier(this.skirmishGame.CurrentSoldier);
        }
        protected void NextAction()
        {
            if (this.currentActions != null && this.currentActions.Length > 0)
            {
                this.actionIndex++;

                if (this.actionIndex > this.currentActions.Length - 1)
                {
                    this.actionIndex = 0;
                }
            }
        }
        protected void PrevAction()
        {
            if (this.currentActions != null && this.currentActions.Length > 0)
            {
                this.actionIndex--;

                if (this.actionIndex < 0)
                {
                    this.actionIndex = this.currentActions.Length - 1;
                }
            }
        }
        protected void RefreshActions()
        {
            this.currentActions = this.skirmishGame.GetActions();

            this.UpdateSoldierStates();
        }
        protected void NextPhase()
        {
            this.skirmishGame.NextPhase();

            //Do automatic actions
            foreach (var soldierC in this.skirmishGame.Soldiers)
            {
                Melee melee = this.skirmishGame.GetMelee(soldierC);

                ActionSpecification[] actions = ActionsManager.GetActions(this.skirmishGame.CurrentPhase, soldierC.Team, soldierC, melee != null, ActionTypes.Automatic);
                if (actions.Length > 0)
                {
                    foreach (ActionSpecification ac in actions)
                    {
                        //TODO: Need of standar method
                        if (ac.Action == Actions.FindCover)
                        {
                            Vector3 point = this.GetRandomPoint();

                            this.FindCover(soldierC, point);
                        }
                        else if (ac.Action == Actions.RunAway)
                        {
                            Vector3 point = this.GetRandomPoint();

                            this.RunAway(soldierC, point);
                        }
                        else if (ac.Action == Actions.TakeControl)
                        {
                            this.TakeControl(soldierC);
                        }
                    }
                }
            }

            this.skirmishGame.NextSoldier(true);

            this.UpdateSoldierStates();

            this.currentActions = this.skirmishGame.GetActions();
            this.actionIndex = 0;

            this.GoToSoldier(this.skirmishGame.CurrentSoldier);

            Victory v = this.skirmishGame.IsFinished();
            if (v != null)
            {
                this.gameFinished = true;

                this.txtGame.Instance.Text = string.Format("{0}", v);
                this.txtTeam.Instance.Text = "";
                this.txtSoldier.Instance.Text = "";
                this.txtActionList.Instance.Text = "";
                this.txtAction.Instance.Text = "";
            }
        }

        protected void GoToSoldier(Soldier soldier)
        {
            BoundingSphere bsph = this.soldierModels[soldier].GetBoundingSphere();

            this.Camera.LookTo(bsph.Center, CameraTranslations.Quick);
            this.lineDrawer.Instance.SetLines(this.bsphColor, Line3D.CreateWiredSphere(bsph, this.bsphSlices, this.bsphStacks));
        }
        protected void UpdateSoldierStates()
        {
            foreach (var soldierC in this.soldierModels.Keys)
            {
                if (soldierC.CurrentHealth == HealthStates.Disabled)
                {
                    this.soldierModels[soldierC].Active = false;
                    this.soldierModels[soldierC].Visible = false;
                }
                else
                {
                    this.soldierModels[soldierC].Active = true;
                    this.soldierModels[soldierC].Visible = true;
                }
            }
        }

        protected void Move(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Move(this.skirmishGame, active, active.CurrentMovingCapacity))
            {
                var model = this.soldierModels[active];
                var controller = this.soldierControllers[active];

                //Run 3d actions
                var path = this.SetPath(this.soldierAgent, model.Manipulator.Position, destination);
                if (path != null)
                {
                    //Set move animation clip
                    model.AnimationController.SetPath(this.animations["walk"]);

                    //Folow
                    controller.Follow(path);
                    controller.MaximumSpeed = 3;

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        protected void Crawl(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Crawl(this.skirmishGame, active, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                var path = this.SetPath(this.soldierAgent, this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set crawl animation clip
                    this.soldierControllers[active].Follow(path);

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        protected void Run(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Run(this.skirmishGame, active, active.CurrentMovingCapacity))
            {
                var model = this.soldierModels[active];
                var controller = this.soldierControllers[active];

                //Run 3d actions
                var path = this.SetPath(this.soldierAgent, model.Manipulator.Position, destination);
                if (path != null)
                {
                    //Set move animation clip
                    model.AnimationController.SetPath(this.animations["run"]);

                    //Folow
                    controller.Follow(path);
                    controller.MaximumSpeed = 8;

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        protected void Assault(Soldier active, Soldier passive)
        {
            if (ActionsManager.Assault(this.skirmishGame, active, passive, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                Manipulator3D passiveMan = this.soldierModels[passive].Manipulator;
                Manipulator3D activeMan = this.soldierModels[active].Manipulator;

                Vector3 dir = Vector3.Normalize(activeMan.Position - passiveMan.Position);
                Vector3 destination = passiveMan.Position + (dir * 3f);

                var path = this.SetPath(this.soldierAgent, activeMan.Position, destination);
                if (path != null)
                {
                    //TODO: Set assault animation clip
                    this.soldierControllers[active].Follow(path);

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        protected void CoveringFire(Soldier active, Weapon weapon, Area area)
        {
            if (ActionsManager.CoveringFire(this.skirmishGame, active, weapon, area, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set covering fire animation clip

                this.RefreshActions();
            }
        }
        protected void Reload(Soldier active, Weapon weapon)
        {
            if (ActionsManager.Reload(this.skirmishGame, active, weapon, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set reload animation clip

                this.RefreshActions();
            }
        }
        protected void Repair(Soldier active, Weapon weapon)
        {
            if (ActionsManager.Repair(this.skirmishGame, active, weapon, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set repair animation clip

                this.RefreshActions();
            }
        }
        protected void Inventory(Soldier active)
        {
            if (ActionsManager.Inventory(this.skirmishGame, active, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set inventory animation clip

                //TODO: Show inventory screen

                this.RefreshActions();
            }
        }
        protected void UseMovementItem(Soldier active)
        {
            if (ActionsManager.UseMovementItem(this.skirmishGame, active, active.CurrentItem, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                this.RefreshActions();
            }
        }
        protected void Communications(Soldier active)
        {
            if (ActionsManager.Communications(this.skirmishGame, active))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                this.RefreshActions();
            }
        }
        protected void FindCover(Soldier active, Vector3 destination)
        {
            if (ActionsManager.FindCover(this.skirmishGame, active))
            {
                //Run 3d actions
                var path = this.SetPath(this.soldierAgent, this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    this.soldierControllers[active].Follow(path);

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        protected void RunAway(Soldier active, Vector3 destination)
        {
            if (ActionsManager.RunAway(this.skirmishGame, active))
            {
                //Run 3d actions
                var path = this.SetPath(this.soldierAgent, this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    this.soldierControllers[active].Follow(path);

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        protected void Shoot(Soldier active, Weapon weapon, Soldier passive)
        {
            Manipulator3D passiveMan = this.soldierModels[passive].Manipulator;
            Manipulator3D activeMan = this.soldierModels[active].Manipulator;

            float distance = Vector3.Distance(passiveMan.Position, activeMan.Position);

            if (ActionsManager.Shoot(this.skirmishGame, active, weapon, distance, passive, active.CurrentActionPoints))
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

                this.RefreshActions();
            }
        }
        protected void SupressingFire(Soldier active, Weapon weapon, Area area)
        {
            if (ActionsManager.SupressingFire(this.skirmishGame, active, weapon, area, active.CurrentActionPoints))
            {
                //Run 3d actions
                //...
                //TODO: Set supressing fire animation clip

                this.RefreshActions();
            }
        }
        protected void Support(Soldier active)
        {
            if (ActionsManager.Support(this.skirmishGame, active))
            {
                //Run 3d actions
                //...
                //TODO: Set support animation clip

                this.RefreshActions();
            }
        }
        protected void UseShootingItem(Soldier active)
        {
            if (ActionsManager.UseShootingItem(this.skirmishGame, active, active.CurrentItem, active.CurrentActionPoints))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                this.RefreshActions();
            }
        }
        protected void FirstAid(Soldier active, Soldier passive)
        {
            if (ActionsManager.FirstAid(this.skirmishGame, active, passive, active.CurrentActionPoints))
            {
                //Run 3d actions
                //...
                //TODO: Set support animation clip

                this.RefreshActions();
            }
        }
        protected void LeaveCombat(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Leave(this.skirmishGame, active))
            {
                //Run 3d actions
                var path = this.SetPath(this.soldierAgent, this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    this.soldierControllers[active].Follow(path);

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        protected void UseMeleeItem(Soldier active)
        {
            if (ActionsManager.UseMeleeItem(this.skirmishGame, active, active.CurrentItem))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                this.RefreshActions();
            }
        }
        protected void TakeControl(Soldier active)
        {
            if (ActionsManager.TakeControl(this.skirmishGame, active))
            {
                //Run 3d actions

                //TODO: Set take control animation clip

                this.RefreshActions();
            }
        }
        protected void UseMoraleItem(Soldier active)
        {
            if (ActionsManager.UseMoraleItem(this.skirmishGame, active, active.CurrentItem))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                this.RefreshActions();
            }
        }

        private IControllerPath SetPath(GridAgentType agentType, Vector3 origin, Vector3 destination)
        {
            var path = this.FindPath(agentType, origin, destination);
            if (path != null)
            {
                return new SegmentPath(origin, path.ReturnPath.ToArray());
            }

            return null;
        }
        private Soldier PickSoldier(Ray cursorRay, bool enemyOnly)
        {
            Vector3 position = cursorRay.Position;
            return PickSoldierNearestToPosition(cursorRay, position, enemyOnly);
        }
        private Soldier PickSoldierNearestToPosition(Ray cursorRay, Vector3 position, bool enemyOnly)
        {
            Soldier res = null;
            float d = float.MaxValue;

            Team[] teams = enemyOnly ? this.skirmishGame.EnemyOf(this.skirmishGame.CurrentTeam) : this.skirmishGame.Teams;

            foreach (Team team in teams)
            {
                foreach (var soldierC in team.Soldiers)
                {
                    var picked = this.soldierModels[soldierC].PickAll(ref cursorRay, true, out PickingResult<Triangle>[] r);
                    if (picked && r?.Length > 0)
                    {
                        if (r.Length > 1)
                        {
                            Array.Sort(r, (r1, r2) =>
                            {
                                var d1 = Vector3.DistanceSquared(r1.Position, position);
                                var d2 = Vector3.DistanceSquared(r2.Position, position);

                                return d1.CompareTo(d2);
                            });
                        }

                        //Select nearest picked soldier
                        res = soldierC;
                        d = r[0].Distance;
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
