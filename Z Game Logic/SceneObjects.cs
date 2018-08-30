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

        private string fontName = "Lucida Sans";
        private SceneObject<Sprite> sprHUD = null;
        private SceneObject<SpriteButton> butClose = null;
        private SceneObject<SpriteButton> butNext = null;
        private SceneObject<SpriteButton> butPrevSoldier = null;
        private SceneObject<SpriteButton> butNextSoldier = null;
        private SceneObject<SpriteButton> butPrevAction = null;
        private SceneObject<SpriteButton> butNextAction = null;

        private SceneObject<Model> cursor3D = null;

        private SceneLightSpot spotLight = null;

        private SceneObject<ModelInstanced> soldier = null;
        private GridAgentType soldierAgent = null;
        private Dictionary<Soldier, ModelInstance> soldierModels = new Dictionary<Soldier, ModelInstance>();
        private Dictionary<Soldier, ManipulatorController> soldierControllers = new Dictionary<Soldier, ManipulatorController>();
        private Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();
        private ModelInstance current
        {
            get
            {
                return this.soldierModels[this.skirmishGame.CurrentSoldier];
            }
        }

        private SceneObject<LineListDrawer> lineDrawer = null;
        private Color4 bsphColor = new Color4(Color.LightYellow.ToColor3(), 0.25f);
        private Color4 frstColor = new Color4(Color.Yellow.ToColor3(), 1f);
        private int bsphSlices = 50;
        private int bsphStacks = 25;

        private SceneObject<Scenery> terrain = null;
        private SceneObject<Minimap> minimap = null;

        private int actionIndex = 0;
        private ActionSpecification[] currentActions = null;
        private Skirmish skirmishGame = null;
        private bool gameFinished = false;
        private ActionSpecification currentAction
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

        private Keys keyExit = Keys.Escape;
        private Keys keyChangeMode = Keys.R;

        private Keys keyDebugFrustum = Keys.Space;
        private Keys keyDebugVolumes = Keys.F1;

        private Keys keyHUDNextSoldier = Keys.N;
        private Keys keyHUDPrevSoldier = Keys.P;
        private Keys keyHUDNextAction = Keys.Right;
        private Keys keyHUDPrevAction = Keys.Left;
        private Keys keyHUDNextPhase = Keys.E;

        private Keys keyCAMNextIsometric = Keys.PageDown;
        private Keys keyCAMPrevIsometric = Keys.PageUp;
        private Keys keyCAMMoveLeft = Keys.A;
        private Keys keyCAMMoveForward = Keys.W;
        private Keys keyCAMMoveBackward = Keys.S;
        private Keys keyCAMMoveRight = Keys.D;
        private Keys keyCAMCenterSoldier = Keys.Home;

        #endregion

        public SceneObjects(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
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
                SceneObjectUsageEnum.UI,
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
                SceneObjectUsageEnum.Ground);

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
                SceneObjectUsageEnum.Agent);

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
            this.sprHUD = this.AddComponent<Sprite>(bkDesc, SceneObjectUsageEnum.UI, layerHUD - 1);

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
            this.minimap = this.AddComponent<Minimap>(minimapDesc, SceneObjectUsageEnum.UI, layerHUD);

            this.txtTitle = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 24, Color.White, Color.Gray), SceneObjectUsageEnum.UI, layerHUD);
            this.txtGame = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate(this.fontName, 12, Color.LightBlue, Color.DarkBlue), SceneObjectUsageEnum.UI, layerHUD);
            this.txtTeam = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate(this.fontName, 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.txtSoldier = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate(this.fontName, 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.txtActionList = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate(this.fontName, 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.txtAction = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate(this.fontName, 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);

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
            }, SceneObjectUsageEnum.UI, layerHUD);

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
            }, SceneObjectUsageEnum.UI, layerHUD);

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
            }, SceneObjectUsageEnum.UI, layerHUD);

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
            }, SceneObjectUsageEnum.UI, layerHUD);

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
            }, SceneObjectUsageEnum.UI, layerHUD);

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
            }, SceneObjectUsageEnum.UI, layerHUD);

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

            foreach (var soldier in this.soldierControllers.Keys)
            {
                this.soldierControllers[soldier].UpdateManipulator(gameTime, this.soldierModels[soldier].Manipulator);
            }

            if (!this.CapturedControl)
            {
                if (this.Game.Input.KeyJustReleased(this.keyExit))
                {
                    this.Game.Exit();
                }

                if (this.Game.Input.KeyJustReleased(this.keyChangeMode))
                {
                    this.SetRenderMode(this.GetRenderMode() == SceneModesEnum.ForwardLigthning ?
                        SceneModesEnum.DeferredLightning :
                        SceneModesEnum.ForwardLigthning);
                }

                bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey) || this.Game.Input.KeyPressed(Keys.RShiftKey);

                Ray cursorRay = this.GetPickingRay();
                bool picked = this.PickNearest(ref cursorRay, true, SceneObjectUsageEnum.Ground, out PickingResult<Triangle> r);

                #region DEBUG

                if (this.Game.Input.KeyJustReleased(this.keyDebugFrustum))
                {
                    this.SetFrustum();
                }

                if (this.Game.Input.KeyJustReleased(this.keyDebugVolumes))
                {
                    this.lineDrawer.Visible = !this.lineDrawer.Visible;
                }

                #endregion

                #region HUD

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

                #endregion

                #region 3D

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
                    Soldier pickedSoldier = this.PickSoldier(ref cursorRay, false);
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

                #endregion

                #region Actions

                if (this.currentAction != null)
                {
                    bool selectorDone = false;
                    Area area = null;

                    if (this.currentAction.Selector == SelectorEnum.Goto)
                    {
                        //TODO: Show goto selector
                        if (this.Game.Input.LeftMouseButtonJustReleased)
                        {
                            selectorDone = true;
                        }
                    }
                    else if (this.currentAction.Selector == SelectorEnum.Area)
                    {
                        //TODO: Show area selector
                        if (this.Game.Input.LeftMouseButtonJustReleased)
                        {
                            area = new Area();

                            selectorDone = true;
                        }
                    }

                    if (selectorDone)
                    {
                        if (this.currentAction.Action == ActionsEnum.Move)
                        {
                            this.Move(this.skirmishGame.CurrentSoldier, r.Position);
                        }
                        else if (this.currentAction.Action == ActionsEnum.Run)
                        {
                            this.Run(this.skirmishGame.CurrentSoldier, r.Position);
                        }
                        else if (this.currentAction.Action == ActionsEnum.Crawl)
                        {
                            this.Crawl(this.skirmishGame.CurrentSoldier, r.Position);
                        }
                        else if (this.currentAction.Action == ActionsEnum.Assault)
                        {
                            Soldier active = this.skirmishGame.CurrentSoldier;
                            Vector3 pos = this.current.Manipulator.Position;
                            Soldier passive = this.PickSoldierNearestToPosition(ref cursorRay, ref pos, true);
                            if (passive != null)
                            {
                                this.Assault(active, passive);
                            }
                        }
                        else if (this.currentAction.Action == ActionsEnum.CoveringFire)
                        {
                            Soldier active = this.skirmishGame.CurrentSoldier;

                            this.CoveringFire(active, active.CurrentShootingWeapon, area);
                        }
                        else if (this.currentAction.Action == ActionsEnum.Reload)
                        {
                            Soldier active = this.skirmishGame.CurrentSoldier;

                            this.Reload(active, active.CurrentShootingWeapon);
                        }
                        else if (this.currentAction.Action == ActionsEnum.Repair)
                        {
                            Soldier active = this.skirmishGame.CurrentSoldier;

                            this.Repair(active, active.CurrentShootingWeapon);
                        }
                        else if (this.currentAction.Action == ActionsEnum.Inventory)
                        {
                            this.Inventory(this.skirmishGame.CurrentSoldier);
                        }
                        else if (this.currentAction.Action == ActionsEnum.UseMovementItem)
                        {
                            this.UseMovementItem(this.skirmishGame.CurrentSoldier);
                        }
                        else if (this.currentAction.Action == ActionsEnum.Communications)
                        {
                            this.Communications(this.skirmishGame.CurrentSoldier);
                        }
                    }
                }

                #endregion

                if (!this.gameFinished)
                {
                    this.txtGame.Instance.Text = string.Format("{0}", this.skirmishGame);
                    this.txtTeam.Instance.Text = string.Format("{0}", this.skirmishGame.CurrentTeam);
                    this.txtSoldier.Instance.Text = string.Format("{0}", this.skirmishGame.CurrentSoldier);
                    this.txtActionList.Instance.Text = string.Format("{0}", this.currentActions.Join(" | "));
                    this.txtAction.Instance.Text = string.Format("{0}", this.currentAction);
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
                foreach (Soldier soldier in team.Soldiers)
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
                        throw new Exception("Bad position");
                    }

                    if (teamIndex == 0)
                    {
                        instance.Manipulator.SetRotation(MathUtil.DegreesToRadians(180), 0, 0, true);
                    }

                    this.soldierModels.Add(soldier, instance);
                    var controller = new BasicManipulatorController();
                    controller.PathEnd += Controller_PathEnd;
                    this.soldierControllers.Add(soldier, controller);

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

        private void NewGame()
        {
            this.skirmishGame = new Skirmish();
            this.skirmishGame.AddTeam("Team1", "Reds", TeamRoleEnum.Defense, 5, 1, 1);
            this.skirmishGame.AddTeam("Team2", "Blues", TeamRoleEnum.Assault, 5, 1, 1);
            //this.SkirmishGame.AddTeam("Team3", "Gray", TeamRole.Neutral, 2, 0, 0, false);
            this.skirmishGame.Start();
            this.currentActions = this.skirmishGame.GetActions();
        }
        private void LoadGame()
        {
            //TODO: Load game from file
            this.NewGame();
        }
        private void SaveGame()
        {
            //TODO: Save game to file
        }

        private void NextSoldier(bool selectIdle)
        {
            this.skirmishGame.NextSoldier(selectIdle);

            this.currentActions = this.skirmishGame.GetActions();

            this.GoToSoldier(this.skirmishGame.CurrentSoldier);
        }
        private void PrevSoldier(bool selectIdle)
        {
            this.skirmishGame.PrevSoldier(selectIdle);

            this.currentActions = this.skirmishGame.GetActions();

            this.GoToSoldier(this.skirmishGame.CurrentSoldier);
        }
        private void NextAction()
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
        private void PrevAction()
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
        private void RefreshActions()
        {
            this.currentActions = this.skirmishGame.GetActions();

            this.UpdateSoldierStates();
        }
        private void NextPhase()
        {
            this.skirmishGame.NextPhase();

            //Do automatic actions
            foreach (Soldier soldier in this.skirmishGame.Soldiers)
            {
                Melee melee = this.skirmishGame.GetMelee(soldier);

                ActionSpecification[] actions = ActionsManager.GetActions(this.skirmishGame.CurrentPhase, soldier.Team, soldier, melee != null, ActionTypeEnum.Automatic);
                if (actions.Length > 0)
                {
                    foreach (ActionSpecification ac in actions)
                    {
                        //TODO: Need of standar method
                        if (ac.Action == ActionsEnum.FindCover)
                        {
                            Vector3 point = this.GetRandomPoint();

                            this.FindCover(soldier, point);
                        }
                        else if (ac.Action == ActionsEnum.RunAway)
                        {
                            Vector3 point = this.GetRandomPoint();

                            this.RunAway(soldier, point);
                        }
                        else if (ac.Action == ActionsEnum.TakeControl)
                        {
                            this.TakeControl(soldier);
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

        private void GoToSoldier(Soldier soldier)
        {
            BoundingSphere bsph = this.soldierModels[soldier].GetBoundingSphere();

            this.Camera.LookTo(bsph.Center, CameraTranslations.Quick);
            this.lineDrawer.Instance.SetLines(this.bsphColor, Line3D.CreateWiredSphere(bsph, this.bsphSlices, this.bsphStacks));
        }
        private void UpdateSoldierStates()
        {
            foreach (Soldier soldier in this.soldierModels.Keys)
            {
                if (soldier.CurrentHealth == HealthStateEnum.Disabled)
                {
                    this.soldierModels[soldier].Active = false;
                    this.soldierModels[soldier].Visible = false;
                }
                else
                {
                    this.soldierModels[soldier].Active = true;
                    this.soldierModels[soldier].Visible = true;
                }
            }
        }

        private void Move(Soldier active, Vector3 destination)
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
        private void Crawl(Soldier active, Vector3 destination)
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
        private void Run(Soldier active, Vector3 destination)
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
        private void Assault(Soldier active, Soldier passive)
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
        private void CoveringFire(Soldier active, Weapon weapon, Area area)
        {
            if (ActionsManager.CoveringFire(this.skirmishGame, active, weapon, area, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set covering fire animation clip

                this.RefreshActions();
            }
        }
        private void Reload(Soldier active, Weapon weapon)
        {
            if (ActionsManager.Reload(this.skirmishGame, active, weapon, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set reload animation clip

                this.RefreshActions();
            }
        }
        private void Repair(Soldier active, Weapon weapon)
        {
            if (ActionsManager.Repair(this.skirmishGame, active, weapon, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set repair animation clip

                this.RefreshActions();
            }
        }
        private void Inventory(Soldier active)
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
        private void UseMovementItem(Soldier active)
        {
            if (ActionsManager.UseMovementItem(this.skirmishGame, active, active.CurrentItem, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                this.RefreshActions();
            }
        }
        private void Communications(Soldier active)
        {
            if (ActionsManager.Communications(this.skirmishGame, active))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                this.RefreshActions();
            }
        }
        private void FindCover(Soldier active, Vector3 destination)
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
        private void RunAway(Soldier active, Vector3 destination)
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
        private void Shoot(Soldier active, Weapon weapon, Soldier passive)
        {
            Manipulator3D passiveMan = this.soldierModels[passive].Manipulator;
            Manipulator3D activeMan = this.soldierModels[active].Manipulator;

            float distance = Vector3.Distance(passiveMan.Position, activeMan.Position);

            if (ActionsManager.Shoot(this.skirmishGame, active, weapon, distance, passive, active.CurrentActionPoints))
            {
                //Run 3d actions

                //TODO: Set shooting animation clip for active

                if (passive.CurrentHealth == HealthStateEnum.Disabled)
                {
                    //TODO: Set impacted and killed animation clip for passive
                }
                else
                {
                    //TODO: Set impacted animation clip for passive
                }

                this.RefreshActions();
            }
        }
        private void SupressingFire(Soldier active, Weapon weapon, Area area)
        {
            if (ActionsManager.SupressingFire(this.skirmishGame, active, weapon, area, active.CurrentActionPoints))
            {
                //Run 3d actions
                //...
                //TODO: Set supressing fire animation clip

                this.RefreshActions();
            }
        }
        private void Support(Soldier active)
        {
            if (ActionsManager.Support(this.skirmishGame, active))
            {
                //Run 3d actions
                //...
                //TODO: Set support animation clip

                this.RefreshActions();
            }
        }
        private void UseShootingItem(Soldier active)
        {
            if (ActionsManager.UseShootingItem(this.skirmishGame, active, active.CurrentItem, active.CurrentActionPoints))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                this.RefreshActions();
            }
        }
        private void FirstAid(Soldier active, Soldier passive)
        {
            if (ActionsManager.FirstAid(this.skirmishGame, active, passive, active.CurrentActionPoints))
            {
                //Run 3d actions
                //...
                //TODO: Set support animation clip

                this.RefreshActions();
            }
        }
        private void LeaveCombat(Soldier active, Vector3 destination)
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
        private void UseMeleeItem(Soldier active)
        {
            if (ActionsManager.UseMeleeItem(this.skirmishGame, active, active.CurrentItem))
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                this.RefreshActions();
            }
        }
        private void TakeControl(Soldier active)
        {
            if (ActionsManager.TakeControl(this.skirmishGame, active))
            {
                //Run 3d actions

                //TODO: Set take control animation clip

                this.RefreshActions();
            }
        }
        private void UseMoraleItem(Soldier active)
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
        private Soldier PickSoldier(ref Ray cursorRay, bool enemyOnly)
        {
            Vector3 position = cursorRay.Position;
            return PickSoldierNearestToPosition(ref cursorRay, ref position, enemyOnly);
        }
        private Soldier PickSoldierNearestToPosition(ref Ray cursorRay, ref Vector3 position, bool enemyOnly)
        {
            Soldier res = null;
            float d = float.MaxValue;

            Team[] teams = enemyOnly ? this.skirmishGame.EnemyOf(this.skirmishGame.CurrentTeam) : this.skirmishGame.Teams;

            foreach (Team team in teams)
            {
                foreach (Soldier soldier in team.Soldiers)
                {
                    if (this.soldierModels[soldier].PickNearest(ref cursorRay, true, out PickingResult<Triangle> r))
                    {
                        if (r.Distance < d)
                        {
                            //Select nearest picked soldier
                            res = soldier;
                            d = r.Distance;
                        }
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
