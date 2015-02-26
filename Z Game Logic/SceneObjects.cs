using System;
using System.Collections.Generic;
using Engine;
using Engine.Common;
using Engine.PathFinding;
using SharpDX;

namespace GameLogic
{
    using GameLogic.Rules;

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

        private ModelInstanced model = null;
        private Dictionary<Soldier, ModelInstance> soldierModels = new Dictionary<Soldier, ModelInstance>();
        private ModelInstance current
        {
            get
            {
                return this.soldierModels[this.skirmishGame.CurrentSoldier];
            }
        }

        private LineListDrawer lineDrawer = null;
        private Color4 bsphColor = new Color4(Color.LightYellow.ToColor3(), 1f / 4f);
        private Color4 frstColor = new Color4(Color.Yellow.ToColor3(), 1f);
        private int bsphSlices = 50;
        private int bsphStacks = 25;

        private Terrain terrain = null;
        private Minimap minimap = null;

        private int actionIndex = 0;
        private Actions[] currentActions = null;
        private Skirmish skirmishGame = null;
        private bool gameFinished = false;
        private Actions currentAction
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

        #endregion

        public SceneObjects(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.FarPlaneDistance = 1000f;
            this.Camera.Mode = CameraModes.FreeIsometric;

            this.NewGame();

            this.cursor3D = this.AddModel("Resources3D", "cursor.dae");
            this.terrain = this.AddTerrain(new TerrainDescription()
            {
                ModelFileName = "terrain.dae",
                ContentPath = "Resources3D",
                UsePathFinding = true,
                PathNodeSize = 5f,
            });
            this.model = this.AddInstancingModel("Resources3D", "soldier.dae", this.skirmishGame.AllSoldiers.Length);

            #region HUD

            int minimapWidth = this.Game.Form.RenderWidth / 4;
            int minimapHeight = this.Game.Form.RenderHeight / 4;

            this.minimap = this.AddMinimap(new MinimapDescription()
            {
                Left = this.Game.Form.RenderWidth - minimapWidth - 5,
                Top = this.Game.Form.RenderHeight - minimapHeight - 5,
                Width = minimapWidth,
                Height = minimapHeight,
                Terrain = this.terrain,
            },
            100);

            this.txtTitle = this.AddText("Tahoma", 24, Color.White, Color.Gray);
            this.txtGame = this.AddText("Lucida Casual", 12, Color.LightBlue, Color.DarkBlue);
            this.txtTeam = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.txtSoldier = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.txtActionList = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.txtAction = this.AddText("Lucida Casual", 12, Color.Yellow);

            BackgroundDescription bkDesc = new BackgroundDescription()
            {
                ContentPath = "Resources",
                Textures = new[] { "HUD.png" },
            };
            this.sprHUD = this.AddBackgroud(bkDesc, 99);

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
            this.butNext.Click += (sender, eventArgs) => { this.Next(); };
            this.butPrevSoldier.Click += (sender, eventArgs) => { this.PrevSoldier(true); };
            this.butNextSoldier.Click += (sender, eventArgs) => { this.NextSoldier(true); };
            this.butPrevAction.Click += (sender, eventArgs) => { this.PrevAction(); };
            this.butNextAction.Click += (sender, eventArgs) => { this.NextAction(); };

            this.txtTitle.Text = "Game Logic";

            #endregion

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

            if (this.Game.Input.KeyJustReleased(this.keyExit))
            {
                this.Game.Exit();
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey) || this.Game.Input.KeyPressed(Keys.RShiftKey);

            Ray cursorRay = this.GetPickingRay();
            Vector3 position;
            Triangle triangle;
            bool picked = this.terrain.PickNearest(ref cursorRay, out position, out triangle);

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

            if (this.Game.Input.KeyJustReleased(Keys.N))
            {
                this.NextSoldier(!shift);
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.PrevSoldier(!shift);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                this.NextAction();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                this.PrevAction();
            }

            if (this.Game.Input.KeyJustReleased(Keys.E))
            {
                this.Next();
            }

            #endregion

            #region 3D

            if (picked)
            {
                this.cursor3D.Manipulator.SetPosition(position);
            }

            if (this.Game.Input.KeyJustReleased(Keys.PageDown))
            {
                this.Camera.NextIsometricAxis();
            }

            if (this.Game.Input.KeyJustReleased(Keys.PageUp))
            {
                this.Camera.PreviousIsometricAxis();
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, shift);
            }

            if (this.Game.Input.RightMouseButtonPressed)
            {
                if (picked)
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

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.GoToSoldier(this.skirmishGame.CurrentSoldier);
            }

            #endregion

            #region Actions

            if (this.currentAction is Move)
            {
                if (this.Game.Input.LeftMouseButtonJustReleased)
                {
                    this.Move(this.skirmishGame.CurrentSoldier, position);
                }
            }
            else if (this.currentAction is Assault)
            {
                if (this.Game.Input.LeftMouseButtonJustReleased)
                {
                    Soldier active = this.skirmishGame.CurrentSoldier;
                    Soldier passive = null;
                    Vector3 pos = this.current.Manipulator.Position;
                    float d = float.MaxValue;

                    foreach (Team team in this.skirmishGame.EnemyOf(this.skirmishGame.CurrentTeam))
                    {
                        foreach (Soldier soldier in team.Soldiers)
                        {
                            Vector3 soldierPosition;
                            Triangle trianglePosition;
                            if (this.soldierModels[soldier].Pick(cursorRay, out soldierPosition, out trianglePosition))
                            {
                                float nd = Vector3.DistanceSquared(pos, this.soldierModels[soldier].Manipulator.Position);

                                if (nd < d)
                                {
                                    //Select nearest picked passive
                                    passive = soldier;
                                    d = nd;
                                }
                            }
                        }
                    }

                    if (passive != null)
                    {
                        this.Assault(passive, active);
                    }
                }
            }

            #endregion

            if (!this.gameFinished)
            {
                this.txtGame.Text = string.Format("{0}", this.skirmishGame);
                this.txtTeam.Text = string.Format("{0}", this.skirmishGame.CurrentTeam);
                this.txtSoldier.Text = string.Format("{0}", this.skirmishGame.CurrentSoldier);
                this.txtActionList.Text = string.Format("{0}", this.currentActions.ToStringList());
                this.txtAction.Text = string.Format("{0}", this.currentAction);
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
                    ModelInstance instance = this.model.Instances[instanceIndex++];

                    instance.TextureIndex = teamIndex;

                    float x = (soldierIndex * soldierSeparation) - (teamWidth * 0.5f);
                    float z = (teamIndex * teamSeparation) - (gameWidth * 0.5f);

                    Vector3 position;
                    if (this.terrain.FindTopGroundPosition(x, z, out position))
                    {
                        instance.Manipulator.SetPosition(position, true);
                    }
                    else
                    {
                        throw new Exception("Bad position");
                    }

                    instance.Manipulator.SetScale(3, true);

                    if (teamIndex > 0)
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
            this.lineDrawer.SetLines(this.frstColor, GeometryUtil.CreateWiredPyramid(this.Camera.Frustum));
        }

        private void NewGame()
        {
            this.skirmishGame = new Skirmish();
            this.skirmishGame.AddTeam("Team1", "Reds", TeamRole.Defense, 5, 1, 1);
            this.skirmishGame.AddTeam("Team2", "Blues", TeamRole.Assault, 5, 1, 1);
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
        private void ShootingActions(ActionsShooting action)
        {
            if (action is Shoot)
            {
                //Needs target
                action.Active = this.skirmishGame.CurrentSoldier;
                action.Passive = this.GetRandomEnemy();
                action.WastedPoints = this.skirmishGame.CurrentSoldier.CurrentActionPoints;
            }
            else if (action is SupressingFire)
            {
                //Needs area
                action.Active = this.skirmishGame.CurrentSoldier;
                action.Area = new Area();
            }
            else if (action is Support)
            {
                //None
                action.Active = this.skirmishGame.CurrentSoldier;
            }
            else if (action is UseShootingItem)
            {
                //None
                action.Active = this.skirmishGame.CurrentSoldier;
                action.WastedPoints = this.skirmishGame.CurrentSoldier.CurrentActionPoints;
            }
            else if (action is FirstAid)
            {
                //Needs target
                action.Active = this.skirmishGame.CurrentSoldier;
                action.Passive = this.GetRandomFriend();
                action.WastedPoints = this.skirmishGame.CurrentSoldier.CurrentActionPoints;
            }
        }
        private void MeleeActions(ActionsMelee action)
        {
            if (action is Leave)
            {
                //None
                action.Active = this.skirmishGame.CurrentSoldier;
                action.Melee = this.skirmishGame.GetMelee(this.skirmishGame.CurrentSoldier);
            }
            else if (action is UseMeleeItem)
            {
                //None
                action.Active = this.skirmishGame.CurrentSoldier;
            }
        }
        private void MoraleActions(ActionsMorale action)
        {
            if (action is UseMoraleItem)
            {
                //None
                action.Active = this.skirmishGame.CurrentSoldier;
            }
        }
        private void Next()
        {
            this.skirmishGame.Next();
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
            this.lineDrawer.SetLines(this.bsphColor, GeometryUtil.CreateWiredSphere(bsph, this.bsphSlices, this.bsphStacks));
        }
        private void UpdateSoldierStates()
        {
            foreach (Soldier soldier in this.soldierModels.Keys)
            {
                if (soldier.CurrentHealth == HealthStates.Disabled)
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
            Move action = new Move(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
                Destination = destination,
            };

            if (action.Execute())
            {
                //Run 3d actions
                Path path = this.terrain.FindPath(this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set move animation clip
                    this.soldierModels[active].Manipulator.Follow(path.GenerateCurve());

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        private void Crawl(Soldier active, Vector3 destination)
        {
            Crawl action = new Crawl(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
                Destination = destination,
            };

            if (action.Execute())
            {
                //Run 3d actions
                Path path = this.terrain.FindPath(this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set crawl animation clip
                    this.soldierModels[active].Manipulator.Follow(path.GenerateCurve());

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        private void Run(Soldier active, Vector3 destination)
        {
            Run action = new Run(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
                Destination = destination,
            };

            if (action.Execute())
            {
                //Run 3d actions
                Path path = this.terrain.FindPath(this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    this.soldierModels[active].Manipulator.Follow(path.GenerateCurve());

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        private void Assault(Soldier passive, Soldier active)
        {
            Assault action = new Assault(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
                Passive = passive,
            };

            if (action.Execute())
            {
                //Run 3d actions
                Manipulator3D passiveMan = this.soldierModels[passive].Manipulator;
                Manipulator3D activeMan = this.soldierModels[active].Manipulator;

                Vector3 dir = Vector3.Normalize(activeMan.Position - passiveMan.Position);
                Vector3 destination = passiveMan.Position + (dir * 3f);

                Path path = this.terrain.FindPath(activeMan.Position, destination);
                if (path != null)
                {
                    //TODO: Set assault animation clip
                    this.soldierModels[active].Manipulator.Follow(path.GenerateCurve());

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        private void CoveringFire(Soldier active, Area area)
        {
            CoveringFire action = new CoveringFire(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
                Area = area,
            };

            if (action.Execute())
            {
                //Run 3d actions
                //...
                //TODO: Set covering fire animation clip

                this.RefreshActions();
            }
        }
        private void Reload(Soldier active)
        {
            Reload action = new Reload(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
            };

            if (action.Execute())
            {
                //Run 3d actions
                //...
                //TODO: Set reload animation clip

                this.RefreshActions();
            }
        }
        private void Repair(Soldier active)
        {
            Repair action = new Repair(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
            };

            if (action.Execute())
            {
                //Run 3d actions
                //...
                //TODO: Set repair animation clip

                this.RefreshActions();
            }
        }
        private void Inventory(Soldier active)
        {
            Inventory action = new Inventory(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
            };

            if (action.Execute())
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
            UseMovementItem action = new UseMovementItem(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
            };

            if (action.Execute())
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                this.RefreshActions();
            }
        }
        private void Communications(Soldier active)
        {
            Communications action = new Communications(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
            };

            if (action.Execute())
            {
                //Run 3d actions
                //...
                //TODO: Set use item animation clip

                this.RefreshActions();
            }
        }
        private void FindCover(Soldier active, Vector3 destination)
        {
            FindCover action = new FindCover(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
                Destination = destination,
            };

            if (action.Execute())
            {
                //Run 3d actions
                Path path = this.terrain.FindPath(this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    this.soldierModels[active].Manipulator.Follow(path.GenerateCurve());

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }
        private void RunAway(Soldier active, Vector3 destination)
        {
            RunAway action = new RunAway(this.skirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
                Destination = destination,
            };

            if (action.Execute())
            {
                //Run 3d actions
                Path path = this.terrain.FindPath(this.soldierModels[active].Manipulator.Position, destination);
                if (path != null)
                {
                    //TODO: Set run animation clip
                    this.soldierModels[active].Manipulator.Follow(path.GenerateCurve());

                    this.GoToSoldier(active);
                }

                this.RefreshActions();
            }
        }

        private Soldier GetRandomEnemy()
        {
            Random rnd = new Random();

            Team[] enemyTeams = this.skirmishGame.EnemyOf(this.skirmishGame.CurrentTeam);
            if (enemyTeams.Length > 0)
            {
                int t = rnd.Next(enemyTeams.Length - 1);

                Team team = enemyTeams[t];

                Soldier[] aliveSoldiers = Array.FindAll(team.Soldiers, w => w.CurrentHealth != HealthStates.Disabled);

                int s = rnd.Next(aliveSoldiers.Length - 1);

                Soldier soldier = aliveSoldiers[s];

                return soldier;
            }

            return null;
        }
        private Soldier GetRandomFriend()
        {
            Random rnd = new Random();

            Team[] friendlyTeams = this.skirmishGame.FriendOf(this.skirmishGame.CurrentTeam);
            if (friendlyTeams.Length > 0)
            {
                int t = rnd.Next(friendlyTeams.Length - 1);

                Team team = friendlyTeams[t];

                Soldier[] woundedSoldiers = Array.FindAll(team.Soldiers, w => w.CurrentHealth != HealthStates.Healthy);

                int s = rnd.Next(woundedSoldiers.Length - 1);

                Soldier soldier = woundedSoldiers[s];

                return soldier;
            }

            return null;
        }
    }
}
