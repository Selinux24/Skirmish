using Engine;
using System;

namespace TerrainTest.AI
{
    using SharpDX;
    using TerrainTest.AI.Behaviors;

    class AgentManager
    {
        public static Random rnd;

        public Agent[] Agents;

        public AgentManager(Agent[] agents)
        {
            this.Agents = agents;
        }

        public void Update(GameTime gameTime)
        {
            if (rnd == null) { rnd = new Random((int)gameTime.TotalSeconds); }

            Array.ForEach(this.Agents, i => i.Update(gameTime));
        }
    }
}
