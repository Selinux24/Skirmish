using System;
using System.Collections.Generic;
using Engine;
using Engine.Common;
using SharpDX;

namespace GameLogic
{
    using GameLogic.Rules;

    public class SceneObjects : Scene3D
    {
        private Model cursor3D = null;

        private Terrain terrain = null;
        private Minimap minimap = null;

        private ModelInstanced model = null;
        private Dictionary<Soldier, ModelInstance> soldierModels = new Dictionary<Soldier, ModelInstance>();
        private ModelInstance current
        {
            get
            {
                return this.soldierModels[Program.SkirmishGame.CurrentSoldier];
            }
        }

        private LineListDrawer lineDrawer = null;
        private Color4 bsphColor = new Color4(Color.LightYellow.ToColor3(), 1f / 4f);
        private Color4 frstColor = new Color4(Color.Yellow.ToColor3(), 1f);
        private int bsphSlices = 50;
        private int bsphStacks = 25;

        public SceneObjects(Game game)
            : base(game)
        {
            this.ContentPath = "Resources3D";
        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.FarPlaneDistance = 1000f;
            this.Camera.Mode = CameraModes.FreeIsometric;

            this.cursor3D = this.AddModel("cursor.dae");

            this.terrain = this.AddTerrain("terrain.dae", new TerrainDescription() { });

            this.minimap = this.AddMinimap(new MinimapDescription()
            {
                Width = 100,
                Height = 100,
                Left = 0,
                Top = 0,
                Terrain = this.terrain,
            });

            BoundingBox bbox = this.terrain.GetBoundingBox();

            float terrainHeight = bbox.Maximum.Z - bbox.Minimum.Z;
            float gameWidth = terrainHeight / (Program.SkirmishGame.Teams.Length + 1);
            float teamSeparation = terrainHeight / (Program.SkirmishGame.Teams.Length);

            this.model = this.AddInstancingModel("soldier.dae", Program.SkirmishGame.AllSoldiers.Length);

            float soldierSeparation = 10f;
            int instanceIndex = 0;
            int teamIndex = 0;
            foreach (Team team in Program.SkirmishGame.Teams)
            {
                float teamWidth = team.Soldiers.Length * soldierSeparation;

                int soldierIndex = 0;
                foreach (Soldier soldier in team.Soldiers)
                {
                    ModelInstance instance = this.model.Instances[instanceIndex++];

                    instance.TextureIndex = teamIndex;

                    Vector2 point = new Vector2(
                        (soldierIndex * soldierSeparation) - (teamWidth * 0.5f),
                        (teamIndex * teamSeparation) - (gameWidth * 0.5f));

                    Vector3 position;
                    if (this.terrain.FindGroundPosition(point, out position))
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

            this.lineDrawer = this.AddLineListDrawer(5000);
            this.lineDrawer.Visible = false;

            this.GoToSoldier(Program.SkirmishGame.CurrentSoldier);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey) || this.Game.Input.KeyPressed(Keys.RShiftKey);

            Ray cursorRay = this.GetPickingRay();
            Vector3 position;
            Triangle triangle;
            bool picked = this.terrain.Pick(cursorRay, out position, out triangle);

            if (picked)
            {
                this.cursor3D.Manipulator.SetPosition(position);
            }

            #region DEBUG

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                this.SetFrustum();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.lineDrawer.Visible = !this.lineDrawer.Visible;
            }

            #endregion

            #region Camera

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

            #endregion

            #region Navigation

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.GoToSoldier(Program.SkirmishGame.CurrentSoldier);
            }

            #endregion

            #region Actions

            if (Program.CurrentAction is Move)
            {
                if (this.Game.Input.LeftMouseButtonJustReleased)
                {
                    this.Move(Program.SkirmishGame.CurrentSoldier, position);
                }
            }
            else if (Program.CurrentAction is Assault)
            {
                if (this.Game.Input.LeftMouseButtonJustReleased)
                {
                    Soldier active = Program.SkirmishGame.CurrentSoldier;
                    Soldier passive = null;
                    Vector3 pos = this.current.Manipulator.Position;
                    float d = float.MaxValue;

                    foreach (Team team in Program.SkirmishGame.EnemyOf(Program.SkirmishGame.CurrentTeam))
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
        }

        public void GoToSoldier(Soldier soldier)
        {
            BoundingSphere bsph = this.soldierModels[soldier].GetBoundingSphere();

            this.Camera.LookTo(bsph.Center, CameraTranslations.Quick);
            this.lineDrawer.SetLines(this.bsphColor, GeometryUtil.CreateWiredSphere(bsph, this.bsphSlices, this.bsphStacks));
        }
        public void UpdateSoldierStates()
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

        public void Move(Soldier active, Vector3 destination)
        {
            Move action = new Move(Program.SkirmishGame)
            {
                Active = active,
                WastedPoints = active.CurrentMovingCapacity,
                Destination = destination,
            };

            if (action.Execute())
            {
                this.soldierModels[active].Manipulator.SetPosition(destination, true);

                this.GoToSoldier(active);
            }
        }
        public void Assault(Soldier passive, Soldier active)
        {
            //TODO: This test must be repeated many times
            if (passive.CurrentHealth != HealthStates.Disabled)
            {
                Assault action = new Assault(Program.SkirmishGame)
                {
                    Active = active,
                    WastedPoints = active.CurrentMovingCapacity,
                    Passive = passive,
                };

                if (action.Execute())
                {
                    Manipulator3D passiveMan = this.soldierModels[passive].Manipulator;
                    Manipulator3D activeMan = this.soldierModels[active].Manipulator;

                    Vector3 dir = Vector3.Normalize(activeMan.Position - passiveMan.Position);

                    activeMan.SetPosition(passiveMan.Position + (dir * 3f), true);
                    activeMan.LookAt(passiveMan.Position);

                    this.GoToSoldier(active);
                }
            }
        }

        private void SetFrustum()
        {
            this.lineDrawer.SetLines(this.frstColor, GeometryUtil.CreateWiredPyramid(this.Camera.Frustum));

            this.Camera.Mode = CameraModes.Free;
        }
    }
}
