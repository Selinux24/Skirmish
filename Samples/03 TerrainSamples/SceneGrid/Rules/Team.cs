using System.Collections.Generic;

namespace TerrainSamples.SceneGrid.Rules
{
    using TerrainSamples.SceneGrid.Rules.Enum;

    public class Team(string name)
    {
        private readonly List<Soldier> soldiers = [];

        public string Name { get; set; } = name;
        public string Faction { get; set; }
        public TeamRoles Role { get; set; }
        public Soldier Leader
        {
            get
            {
                return soldiers[0];
            }
        }
        public Soldier[] Soldiers
        {
            get
            {
                return [..soldiers];
            }
        }

        public int AirStrikeProbability { get; set; } = 0;
        public int AirStrikeRequests { get; set; } = 0;
        public int AirStrikePenetration { get; set; } = 0;
        public int AirStrikeDamage { get; set; } = 0;
        public int OrdnanceProbability { get; set; } = 0;
        public int OrdnanceRequests { get; set; } = 0;
        public int OrdnancePenetration { get; set; } = 0;
        public int OrdnanceDamage { get; set; } = 0;
        public int ReinforcementProbability { get; set; } = 0;

        public void NextTurn()
        {
            foreach (Soldier soldier in soldiers)
            {
                soldier.NextTurn();
            }
        }

        public void AddSoldier(string name, SoldierClasses soldierClass)
        {
            soldiers.Add(new Soldier(name, soldierClass, this));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} ++ {Faction} ++ {Role} -> Soldiers {soldiers.Count}";
        }
    }
}
