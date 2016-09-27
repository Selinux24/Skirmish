using Engine;
using Engine.Common;
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
        private TextDrawer txtTitle = null;
        private TextDrawer txtGame = null;
        private TextDrawer txtTeam = null;
        private TextDrawer txtSoldier = null;
        private TextDrawer txtActionList = null;
        private TextDrawer txtAction = null;

        private Sprite sprHUD = null;
        private SpriteButton butClose = null;
        private SpriteButton butNext = null;
        private SpriteButton butPrevSoldier = null;
        private SpriteButton butNextSoldier = null;
        private SpriteButton butPrevAction = null;
        private SpriteButton butNextAction = null;

        private Model cursor3D = null;

        private SceneLightPoint pointLight = null;

        private ModelInstanced soldier = null;
        private GridAgent soldierAgent = null;
        private Dictionary<Soldier, ModelInstance> soldierModels = new Dictionary<Soldier, ModelInstance>();
        private ModelInstance current
        {
            get
            {
                return this.soldierModels[this.skirmishGame.CurrentSoldier];
            }
        }

        private LineListDrawer lineDrawer = null;
        private Color4 bsphColor = new Color4(Color.LightYellow.ToColor3(), 0.25f);
        private Color4 frstColor = new Color4(Color.Yellow.ToColor3(), 1f);
        private int bsphSlices = 50;
        private int bsphStacks = 25;

        private Scenery terrain = null;
        private Minimap minimap = null;

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
            : base(game, SceneModesEnum.DeferredLightning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            GameEnvironment.Background = Color.Black;

            this.Camera.FarPlaneDistance = 1000f;
            this.Camera.Mode = CameraModes.FreeIsometric;

            this.NewGame();

            this.pointLight = new SceneLightPoint()
            {
                Name = "Current soldier",
                Enabled = true,
                LightColor = Color.White,
                Position = Vector3.Zero,
                AmbientIntensity = 0f,
                DiffuseIntensity = 1f,
                Radius = 3f,
            };

            this.Lights.Add(this.pointLight);

            #region 3D models

            this.cursor3D = this.AddModel(new ModelDescription()
            {
                ContentPath = "Resources3D",
                ModelFileName = "cursor.dae",
            });
            this.terrain = this.AddScenery(new GroundDescription()
            {
                Model = new GroundDescription.ModelDescription()
                {
                    ModelFileName = "terrain.dae",
                },
                ContentPath = "Resources3D",
                PathFinder = new GroundDescription.PathFinderDescription()
                {
                    Settings = new GridGenerationSettings()
                    {
                        NodeSize = 5f,
                    },
                },
                Opaque = true,
            });
            this.soldier = this.AddInstancingModel(new ModelInstancedDescription()
            {
                ContentPath = "Resources3D",
                ModelFileName = "soldier.dae",
                Instances = this.skirmishGame.AllSoldiers.Length,
                Opaque = true,
            });

            #endregion

            #region HUD

            BackgroundDescription bkDesc = new BackgroundDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "HUD.png" },
            };
            this.sprHUD = this.AddBackgroud(bkDesc);

            int minimapWidth = this.Game.Form.RenderWidth / 4;
            int minimapHeight = this.Game.Form.RenderHeight / 4;

            MinimapDescription minimapDesc = new MinimapDescription()
            {
                Left = this.Game.Form.RenderWidth - minimapWidth - (this.Game.Form.RenderWidth / 100),
                Top = this.Game.Form.RenderHeight - minimapHeight - (this.Game.Form.RenderHeight / 100),
                Width = minimapWidth,
                Height = minimapHeight,
                Drawables = new Drawable[]
                {
                    this.terrain,
                    this.soldier,
                },
                MinimapArea = this.terrain.GetBoundingBox(),
            };
            this.minimap = this.AddMinimap(minimapDesc);

            this.txtTitle = this.AddText("Tahoma", 24, Color.White, Color.Gray);
            this.txtGame = this.AddText("Lucida Casual", 12, Color.LightBlue, Color.DarkBlue);
            this.txtTeam = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.txtSoldier = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.txtActionList = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.txtAction = this.AddText("Lucida Casual", 12, Color.Yellow);

            this.butClose = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 60,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 12,
                TextColor = Color.Yellow,
                TextShadowColor = Color.Orange,
                Text = "EXIT",
            });

            this.butNext = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 60,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Next",
            });

            this.butPrevSoldier = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Prev.Soldier",
            });

            this.butNextSoldier = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Next Soldier",
            });

            this.butPrevAction = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Prev.Action",
            });

            this.butNextAction = this.AddSpriteButton(new SpriteButtonDescription()
            {
                TextureReleased = "button_on.png",
                TexturePressed = "button_off.png",
                Width = 90,
                Height = 20,
                Font = "Lucida Casual",
                FontSize = 10,
                TextColor = Color.Yellow,
                Text = "Next Action",
            });

            this.butClose.Click += (sender, eventArgs) => { this.Game.Exit(); };
            this.butNext.Click += (sender, eventArgs) => { this.NextPhase(); };
            this.butPrevSoldier.Click += (sender, eventArgs) => { this.PrevSoldier(true); };
            this.butNextSoldier.Click += (sender, eventArgs) => { this.NextSoldier(true); };
            this.butPrevAction.Click += (sender, eventArgs) => { this.PrevAction(); };
            this.butNextAction.Click += (sender, eventArgs) => { this.NextAction(); };

            this.txtTitle.Text = "Game Logic";

            #endregion

            this.SceneVolume = this.terrain.GetBoundingSphere();

            this.UpdateLayout();

            this.IntializePositions();

            #region DEBUG

            this.lineDrawer = this.AddLineListDrawer(5000);
            this.lineDrawer.Visible = false;

            #endregion

            this.GoToSoldier(this.skirmishGame.CurrentSoldier);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!this.CapturedControl)
            {
                if (this.Game.Input.KeyJustReleased(this.keyExit))
                {
                    this.Game.Exit();
                }

                bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey) || this.Game.Input.KeyPressed(Keys.RShiftKey);

                Ray cursorRay = this.GetPickingRay();
                Vector3 position;
                Triangle triangle;
                float distance;
                bool picked = this.terrain.PickNearest(ref cursorRay, true, out position, out triangle, out distance);

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
                    this.cursor3D.Manipulator.SetPosition(position);
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
                        this.Camera.LookTo(position, CameraTranslations.UseDelta);
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

                this.pointLight.Position = this.soldierModels[this.skirmishGame.CurrentSoldier].Manipulator.Position;

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
                            this.Move(this.skirmishGame.CurrentSoldier, position);
                        }
                        else if (this.currentAction.Action == ActionsEnum.Run)
                        {
                            this.Run(this.skirmishGame.CurrentSoldier, position);
                        }
                        else if (this.currentAction.Action == ActionsEnum.Crawl)
                        {
                            this.Crawl(this.skirmishGame.CurrentSoldier, position);
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
                    this.txtGame.Text = string.Format("{0}", this.skirmishGame);
                    this.txtTeam.Text = string.Format("{0}", this.skirmishGame.CurrentTeam);
                    this.txtSoldier.Text = string.Format("{0}", this.skirmishGame.CurrentSoldier);
                    this.txtActionList.Text = string.Format("{0}", this.currentActions.Join(" | "));
                    this.txtAction.Text = string.Format("{0}", this.currentAction);
                }
            }
        }
        protected override void Resized(object sender, EventArgs e)
        {
            this.UpdateLayout();
        }
        private void UpdateLayout()
        {
            this.txtTitle.Top = 0;
            this.txtTitle.Left = 5;
            this.txtGame.Top = this.txtTitle.Top + this.txtTitle.Height + 1;
            this.txtGame.Left = 10;
            this.txtTeam.Top = this.txtGame.Top + this.txtGame.Height + 1;
            this.txtTeam.Left = this.txtGame.Left;

            this.butClose.Top = 1;
            this.butClose.Left = this.Game.Form.RenderWidth - 60 - 1;

            this.butNext.Top = (int)((float)this.Game.Form.RenderHeight * 0.85f);
            this.butNext.Left = 10;
            this.butPrevSoldier.Top = this.butNext.Top;
            this.butPrevSoldier.Left = this.butNext.Left + this.butNext.Width + 25;
            this.butNextSoldier.Top = this.butNext.Top;
            this.butNextSoldier.Left = this.butPrevSoldier.Left + this.butPrevSoldier.Width + 10;
            this.butPrevAction.Top = this.butNext.Top;
            this.butPrevAction.Left = this.butNextSoldier.Left + this.butNextSoldier.Width + 25;
            this.butNextAction.Top = this.butNext.Top;
            this.butNextAction.Left = this.butPrevAction.Left + this.butPrevAction.Width + 10;

            this.txtSoldier.Top = this.butNext.Top + this.butNext.Height + 1;
            this.txtSoldier.Left = 10;
            this.txtActionList.Top = this.txtSoldier.Top + this.txtSoldier.Height + 1;
            this.txtActionList.Left = this.txtSoldier.Left;
            this.txtAction.Top = this.txtActionList.Top + this.txtActionList.Height + 1;
            this.txtAction.Left = this.txtSoldier.Left;
        }
        private void IntializePositions()
        {
            BoundingBox bbox = this.terrain.GetBoundingBox();

            float terrainHeight = bbox.Maximum.Z - bbox.Minimum.Z;
            float gameWidth = terrainHeight / (this.skirmishGame.Teams.Length + 1);
            float teamSeparation = terrainHeight / (this.skirmishGame.Teams.Length);

            float soldierSeparation = 10f;
            int instanceIndex = 0;
            int teamIndex = 0;
            foreach (Team team in this.skirmishGame.Teams)
            {
                float teamWidth = team.Soldiers.Length * soldierSeparation;

                int soldierIndex = 0;
                foreach (Soldier soldier in team.Soldiers)
                {
                    ModelInstance instance = this.soldier.Instances[instanceIndex++];

                    instance.TextureIndex = teamIndex;
                    instance.AnimationController.AddClip(0, true, float.MaxValue);
                    instance.AnimationController.Time = soldierIndex;

                    float x = (soldierIndex * soldierSeparation) - (teamWidth * 0.5f);
                    float z = (teamIndex * teamSeparation) - (gameWidth * 0.5f);

                    Vector3 position;
                    Triangle triangle;
                    float distance;
                    if (this.terrain.FindTopGroundPosition(x, z, out position, out triangle, out distance))
                    {
                        instance.Manipulator.SetPosition(position, true);
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

                    soldierIndex++;
                }

                teamIndex++;
            }
        }
        private void SetFrustum()
        {
            this.lineDrawer.SetLines(this.frstColor, Line3.CreateWiredFrustum(this.Camera.Frustum));
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

                this.txtGame.Text = string.Format("{0}", v);
                this.txtTeam.Text = "";
                this.txtSoldier.Text = "";
                this.txtActionList.Text = "";
                this.txtAction.Text = "";
            }
        }

        private void GoToSoldier(Soldier soldier)
        {
            BoundingSphere bsph = this.soldierModels[soldier].GetBoundingSphere();

            this.Camera.LookTo(bsph.Center, CameraTranslations.Quick);
            this.lineDrawer.SetLines(this.bsphColor, Line3.CreateWiredSphere(bsph, this.bsphSlices, this.bsphStacks));
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
                //Run 3d actions
                var path = this.terrain.FindPath(this.soldierAgent, this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set move animation clip
                    this.soldierModels[active].Manipulator.Follow(path.ReturnPath.ToArray(), 0.2f, this.terrain);

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
                var path = this.terrain.FindPath(this.soldierAgent, this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set crawl animation clip
                    this.soldierModels[active].Manipulator.Follow(path.ReturnPath.ToArray(), 0.2f, this.terrain);

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        private void Run(Soldier active, Vector3 destination)
        {
            if (ActionsManager.Run(this.skirmishGame, active, active.CurrentMovingCapacity))
            {
                //Run 3d actions
                var path = this.terrain.FindPath(this.soldierAgent, this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    this.soldierModels[active].Manipulator.Follow(path.ReturnPath.ToArray(), 0.2f, this.terrain);

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

                var path = this.terrain.FindPath(this.soldierAgent, activeMan.Position, destination);
                if (path != null)
                {
                    //TODO: Set assault animation clip
                    this.soldierModels[active].Manipulator.Follow(path.ReturnPath.ToArray(), 0.2f, this.terrain);

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
                var path = this.terrain.FindPath(this.soldierAgent, this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    this.soldierModels[active].Manipulator.Follow(path.ReturnPath.ToArray(), 0.2f, this.terrain);

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
                var path = this.terrain.FindPath(this.soldierAgent, this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    this.soldierModels[active].Manipulator.Follow(path.ReturnPath.ToArray(), 0.2f, this.terrain);

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
                var path = this.terrain.FindPath(this.soldierAgent, this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    this.soldierModels[active].Manipulator.Follow(path.ReturnPath.ToArray(), 0.2f, this.terrain);

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
                    Vector3 soldierPosition;
                    Triangle trianglePosition;
                    float distanceToPosition;
                    if (this.soldierModels[soldier].PickNearest(ref cursorRay, true, out soldierPosition, out trianglePosition, out distanceToPosition))
                    {
                        if (distanceToPosition < d)
                        {
                            //Select nearest picked soldier
                            res = soldier;
                            d = distanceToPosition;
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
