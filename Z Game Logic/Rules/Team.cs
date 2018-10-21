using System.Collections.Generic;

namespace GameLogic.Rules
{
    using GameLogic.Rules.Enum;

    public class Team
    {
        private readonly List<Soldier> soldiers = new List<Soldier>();

        public string Name { get; set; }
        public string Faction { get; set; }
        public TeamRoles Role { get; set; }
        public Soldier Leader
        {
            get
            {
                return this.soldiers[0];
            }
        }
        public Soldier[] Soldiers
        {
            get
            {
                return this.soldiers.ToArray();
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
            this.Name = name;

            this.AirStrikeProbability = 0;
            this.AirStrikeRequests = 0;
            this.AirStrikePenetration = 0;
            this.AirStrikeDamage = 0;
            this.OrdnanceProbability = 0;
            this.OrdnanceRequests = 0;
            this.OrdnancePenetration = 0;
            this.OrdnanceDamage = 0;
            this.ReinforcementProbability = 0;
        }

        public void NextTurn()
        {
            foreach (Soldier soldier in this.soldiers)
            {
                soldier.NextTurn();
            }
        }

        public void AddSoldier(string name, SoldierClasses soldierClass)
        {
            this.soldiers.Add(new Soldier(name, soldierClass, this));
        }

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2} -> Soldiers {3}", this.Name, this.Faction, this.Role, this.soldiers.Count);
        }
    }
}
