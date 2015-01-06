using System.Collections.Generic;
using Engine;
using SharpDX;
using SharpDX.DirectInput;

namespace GameLogic
{
    using GameLogic.Rules;

    public class SceneObjects : Scene3D
    {
        public Dictionary<Team, ModelInstanced> teams = new Dictionary<Team, ModelInstanced>();
        public Dictionary<Soldier, Manipulator3D> soldiers = new Dictionary<Soldier, Manipulator3D>();

        public SceneObjects(Game game)
            : base(game)
        {
            this.ContentPath = "Resources3D";
        }

        public override void Initialize()
        {
            base.Initialize();

            int teamIndex = 0;
            foreach (Team team in Program.SkirmishGame.Teams)
            {
                ModelInstanced model = this.AddInstancingModel("Poly.dae", team.Soldiers.Length);

                this.teams.Add(team, model);

                float delta = 10;

                int soldierIndex = 0;
                foreach (Soldier soldier in team.Soldiers)
                {
                    Manipulator3D man = model[soldierIndex++];

                    man.SetPosition(soldierIndex * delta, 0, teamIndex * delta);

                    this.soldiers.Add(soldier, man);
                }

                teamIndex++;
            }

            Vector3 pos = this.soldiers[Program.SkirmishGame.CurrentSoldier].Position;

            this.Camera.Mode = CameraModes.FreeIsometric;
            this.Camera.LookTo(pos);
            //this.Camera.Position = pos + (Vector3.One * 10f);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool shift = this.Game.Input.KeyPressed(Key.LeftShift) || this.Game.Input.KeyPressed(Key.RightShift);

            if (this.Game.Input.KeyPressed(Key.A))
            {
                this.Camera.MoveLeft(this.Game.GameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Key.D))
            {
                this.Camera.MoveRight(this.Game.GameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Key.W))
            {
                this.Camera.MoveForward(this.Game.GameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Key.S))
            {
                this.Camera.MoveBackward(this.Game.GameTime, shift);
            }
        }
    }
}
