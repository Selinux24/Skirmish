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
        private Terrain terrain = null;

        private ModelInstanced model = null;
        private Dictionary<Soldier, ModelInstance> soldiers = new Dictionary<Soldier, ModelInstance>();
        private ModelInstance current
        {
            get
            {
                return this.soldiers[Program.SkirmishGame.CurrentSoldier];
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

            this.terrain = this.AddTerrain("terrain.dae", new TerrainDescription() { });
            this.terrain.Manipulator.SetScale(2, true);

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

                    this.soldiers.Add(soldier, instance);

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

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.GoToSoldier(Program.SkirmishGame.CurrentSoldier);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                this.SetFrustum();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.lineDrawer.Visible = !this.lineDrawer.Visible;
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

            if (this.Game.Input.MouseWheelDelta > 0)
            {
                this.Camera.ZoomIn(gameTime, shift);
            }

            if (this.Game.Input.MouseWheelDelta < 0)
            {
                this.Camera.ZoomOut(gameTime, shift);
            }
        }

        public void GoToSoldier(Soldier soldier)
        {
            BoundingSphere bsph = this.soldiers[soldier].GetBoundingSphere();

            this.Camera.LookTo(bsph.Center, CameraTranslations.Quick);
            this.lineDrawer.SetLines(this.bsphColor, GeometryUtil.CreateWiredSphere(bsph, this.bsphSlices, this.bsphStacks));
        }
        public void UpdateSoldierStates()
        {
            foreach (Soldier soldier in this.soldiers.Keys)
            {
                if (soldier.CurrentHealth == HealthStates.Disabled)
                {
                    this.soldiers[soldier].Active = false;
                    this.soldiers[soldier].Visible = false;
                }
                else
                {
                    this.soldiers[soldier].Active = true;
                    this.soldiers[soldier].Visible = true;
                }
            }
        }

        public void Move(Soldier soldier, Vector3 destination)
        {
            this.soldiers[soldier].Manipulator.SetPosition(destination, true);
        }
        public void Melee(Soldier passive, Soldier active)
        {
            Manipulator3D passiveMan = this.soldiers[passive].Manipulator;
            Manipulator3D activeMan = this.soldiers[active].Manipulator;

            Vector3 dir = Vector3.Normalize(activeMan.Position - passiveMan.Position);

            activeMan.SetPosition(passiveMan.Position + (dir * 3f), true);
            activeMan.LookAt(passiveMan.Position);

            this.GoToSoldier(active);
        }

        private void SetFrustum()
        {
            this.lineDrawer.SetLines(this.frstColor, GeometryUtil.CreateWiredFrustum(this.Camera.Frustum));

            this.Camera.Mode = CameraModes.Free;
        }
    }
}
