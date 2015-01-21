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
        private ModelInstanced model = null;
        private Terrain terrain = null;

        private Color bsphColor = Color.LightYellow;
        private int bsphSlices = 20;
        private int bsphStacks = 10;
        private LineListDrawer bsphMeshesDrawer = null;
        private ModelInstance current
        {
            get
            {
                return this.soldiers[Program.SkirmishGame.CurrentSoldier];
            }
        }

        private Dictionary<Soldier, ModelInstance> soldiers = new Dictionary<Soldier, ModelInstance>();

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
            this.terrain.Manipulator.SetScale(2);
            this.terrain.Update(new GameTime());
            this.terrain.ComputeVolumes(this.terrain.Manipulator.LocalTransform);

            this.model = this.AddInstancingModel("soldier.dae", Program.SkirmishGame.AllSoldiers.Length);

            float delta = 10f;
            int instanceIndex = 0;
            int teamIndex = 0;
            foreach (Team team in Program.SkirmishGame.Teams)
            {
                int soldierIndex = 0;
                foreach (Soldier soldier in team.Soldiers)
                {
                    ModelInstance instance = this.model.Instances[instanceIndex++];

                    instance.TextureIndex = teamIndex;

                    Vector3 position;
                    if (this.terrain.FindGroundPosition(soldierIndex * delta, teamIndex * delta, out position))
                    {
                        instance.Manipulator.SetPosition(position);
                    }
                    else
                    {
                        throw new Exception("Bad position");
                    }
                    
                    instance.Manipulator.SetScale(3);

                    if (teamIndex > 0)
                    {
                        instance.Manipulator.SetRotation(MathUtil.DegreesToRadians(180), 0, 0);
                    }

                    this.soldiers.Add(soldier, instance);

                    soldierIndex++;
                }

                teamIndex++;
            }
            this.model.Update(new GameTime());

            this.bsphMeshesDrawer = this.AddLineListDrawer(GeometryUtil.CreateWiredSphere(this.model.BoundingSphere, this.bsphSlices, this.bsphStacks), this.bsphColor);

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
            this.model.ComputeVolumes(this.current.Manipulator.LocalTransform);

            this.Camera.LookTo(this.model.BoundingSphere.Center, true);
            this.bsphMeshesDrawer.SetLines(GeometryUtil.CreateWiredSphere(this.model.BoundingSphere, this.bsphSlices, this.bsphStacks), this.bsphColor);
        }
    }
}
