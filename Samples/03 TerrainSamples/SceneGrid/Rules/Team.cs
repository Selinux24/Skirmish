using System.Collections.Generic;

namespace TerrainSamples.SceneGrid.Rules
{
    using TerrainSamples.SceneGrid.Rules.Enum;

    public class Team
    {
        private readonly List<Soldier> soldiers = new();

        public string Name { get; set; }
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
                return soldiers.ToArray();
            }
        }

        public int AirStrikeProbability { get; set; }
        public int AirStrikeRequests { get; set; }
        public int AirStrikePenetration { get; set; }
        public int AirStrikeDamage { get; set; }
        public int OrdnanceProbability { get; set; }
        public int OrdnanceRequests { get; set; }
        public int OrdnancePenetration { get; set; }
        public int OrdnanceDamage { get; set; }
        public int ReinforcementProbability { get; set; }

        public Team(string name)
        {
            Name = name;

            AirStrikeProbability = 0;
            AirStrikeRequests = 0;
            AirStrikePenetration = 0;
            AirStrikeDamage = 0;
            OrdnanceProbability = 0;
            OrdnanceRequests = 0;
            OrdnancePenetration = 0;
            OrdnanceDamage = 0;
            ReinforcementProbability = 0;
        }

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
