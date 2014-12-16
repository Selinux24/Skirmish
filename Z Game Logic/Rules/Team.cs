using System.Collections.Generic;

namespace GameLogic.Rules
{
    public class Team
    {
        private List<Soldier> soldiers = new List<Soldier>();

        public string Name { get; set; }
        public string Faction { get; set; }
        public TeamRole Role { get; set; }
        public Soldier Leader { get; set; }
        public Soldier[] Soldiers
        {
            get
            {
                return this.soldiers.ToArray();
            }
            set
            {
                this.soldiers.Clear();

                if(value != null && value.Length >0)
                {
                    this.soldiers.AddRange(value);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2} -> Soldiers {3}", this.Name, this.Faction, this.Role, this.soldiers.Count);
        }
    }
}
