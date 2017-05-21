using Engine;
using System;
using System.Collections.Generic;

namespace TerrainTest.AI
{
    public class Brain
    {
        public Ground Ground;
        public Random RandomGenerator;
        private Dictionary<int, List<AIAgent>> groups = new Dictionary<int, List<AIAgent>>();

        public Brain(Ground ground)
        {
            this.Ground = ground;
            this.RandomGenerator = new Random();
        }

        public void Update(GameTime gameTime)
        {
            foreach (var values in this.groups.Values)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    values[i].Update(gameTime);
                }
            }
        }

        public void AddAgent(int index, AIAgent agent)
        {
            if (!this.groups.ContainsKey(index))
            {
                this.groups.Add(index, new List<AIAgent>());
            }

            this.groups[index].Add(agent);
        }

        public AIAgent[] GetTargetsForAgent(AIAgent agent)
        {
            List<AIAgent> targets = new List<AIAgent>();

            foreach (var key in this.groups.Keys)
            {
                if (!this.groups[key].Contains(agent))
                {
                    targets.AddRange(this.groups[key]);
                }
            }

            return targets.ToArray();
        }
    }
}
