using Engine;
using System;

namespace TerrainTest.AI
{
    using SharpDX;
    using TerrainTest.AI.Behaviors;

    class AgentManager
    {
        public static Random rnd;

        public Agent[] Team1;
        public Agent[] Team2;
        public Ground Ground;

        public AgentManager(Agent[] team1, Agent[] team2, Ground ground)
        {
            this.Team1 = team1;
            this.Team2 = team2;
            this.Ground = ground;
        }

        public void Update(GameTime gameTime)
        {
            if (rnd == null) { rnd = new Random((int)gameTime.TotalSeconds); }

            Array.ForEach(this.Team1, i => this.UpdateAgent(i, this.Team2, gameTime));
            Array.ForEach(this.Team2, i => this.UpdateAgent(i, this.Team1, gameTime));
        }

        private void UpdateAgent(Agent agent, Agent[] targets, GameTime gameTime)
        {
            agent.Update(gameTime);

            if (agent.Life > 0)
            {
                if (agent.CurrentBehavior is AttackBehavior)
                {
                    if (((AttackBehavior)agent.CurrentBehavior).Target.Life <= 0)
                    {
                        var checkPoints = new Vector3[]
                        {
                            new Vector3(+60, 0, -60),
                            new Vector3(-60, 0, -60),
                            new Vector3(+60, 0, +60),
                            new Vector3(-70, 0, +70),
                        };

                        agent.DoPatrol(checkPoints, 5f, 15f, this.Ground);
                    }
                }
                else if (agent.CurrentBehavior == null || agent.CurrentBehavior is PatrolBehavior)
                {
                    var tList = Array.FindAll(targets, target =>
                    {
                        if (target.Life > 0)
                        {
                            var p1 = agent.Model.Manipulator.Position;
                            var p2 = target.Model.Manipulator.Position;

                            return Vector3.Distance(p1, p2) < 20;
                        }

                        return false;
                    });

                    if (tList.Length > 0)
                    {
                        agent.DoAttack(tList[0]);

                        agent.Model.Manipulator.Stop();
                    }
                }
            }
        }
    }
}
