using System.Collections.Generic;
using Engine;
using SharpDX;

namespace GameLogic
{
    using GameLogic.Rules;
    using Engine.Common;

    public class SceneObjects : Scene3D
    {
        private Vector3? cameraPoint = null;

        private ModelInstanced model = null;

        private Color bsphColor = Color.LightYellow;
        private int bsphSlices = 20;
        private int bsphStacks = 10;
        private LineListDrawer bsphMeshesDrawer = null;

        public Dictionary<Team, ModelInstanced> Teams = new Dictionary<Team, ModelInstanced>();
        public Dictionary<Soldier, int> Soldiers = new Dictionary<Soldier, int>();

        public SceneObjects(Game game)
            : base(game)
        {
            this.ContentPath = "Resources3D";
        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.Mode = CameraModes.FreeIsometric;

            this.model = this.AddInstancingModel("soldier.dae", Program.SkirmishGame.AllSoldiers.Length);

            int instanceIndex = 0;

            int teamIndex = 0;
            foreach (Team team in Program.SkirmishGame.Teams)
            {
                this.Teams.Add(team, this.model);

                float delta = 10;

                int soldierIndex = 0;
                foreach (Soldier soldier in team.Soldiers)
                {
                    this.model.Manipulators[instanceIndex].SetPosition(soldierIndex * delta, 0, teamIndex * delta);
                    this.model.Manipulators[instanceIndex].SetScale(3);

                    if (teamIndex > 0)
                    {
                        this.model.Manipulators[instanceIndex].SetRotation(MathUtil.DegreesToRadians(180), 0, 0);
                    }

                    this.Soldiers.Add(soldier, instanceIndex++);

                    soldierIndex++;
                }

                teamIndex++;
            }

            this.bsphMeshesDrawer = this.AddLineListDrawer(GeometryUtil.CreateWiredSphere(this.model.BoundingSphere, this.bsphSlices, this.bsphStacks), this.bsphColor);

            this.model.Update(new GameTime());

            this.GoToSoldier(Program.SkirmishGame.CurrentSoldier);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this.UpdateInput(gameTime);

            this.UpdateCamera(gameTime);
        }
        private void UpdateInput(GameTime gameTime)
        {
            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey) || this.Game.Input.KeyPressed(Keys.RShiftKey);

            if (this.Game.Input.KeyPressed(Keys.C))
            {
                this.GoToSoldier(Program.SkirmishGame.CurrentSoldier);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, shift);

                this.cameraPoint = null;
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, shift);

                this.cameraPoint = null;
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, shift);

                this.cameraPoint = null;
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, shift);

                this.cameraPoint = null;
            }

            if (this.Game.Input.MouseWheelDelta > 0)
            {
                this.Camera.ZoomIn(gameTime, shift);

                this.cameraPoint = null;
            }

            if (this.Game.Input.MouseWheelDelta < 0)
            {
                this.Camera.ZoomOut(gameTime, shift);

                this.cameraPoint = null;
            }
        }
        private void UpdateCamera(GameTime gameTime)
        {
            if (this.cameraPoint.HasValue)
            {
                if (!Vector3.NearEqual(this.Camera.Interest, this.cameraPoint.Value, new Vector3(0.1f, 0.1f, 0.1f)))
                {
                    this.Camera.FineTranslation(gameTime, this.cameraPoint.Value, 10f, false);
                }
                else
                {
                    this.cameraPoint = null;
                }
            }
        }
        public void GoToSoldier(Soldier soldier)
        {
            Manipulator3D manipulator = this.model.Manipulators[this.Soldiers[soldier]];

            this.model.ComputeVolumes(manipulator.LocalTransform);

            this.cameraPoint = this.model.BoundingSphere.Center;
            this.bsphMeshesDrawer.SetLines(GeometryUtil.CreateWiredSphere(this.model.BoundingSphere, this.bsphSlices, this.bsphStacks), this.bsphColor);
        }
    }
}
