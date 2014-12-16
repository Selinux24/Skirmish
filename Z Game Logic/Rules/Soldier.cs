
namespace GameLogic.Rules
{
    public class Soldier
    {
        public string Name { get; set; }
        public bool IdleForMovement { get; set; }
        public bool IdleForShooting { get; set; }
        public bool IdleForMelee { get; set; }
        public bool IdleForMorale { get; set; }

        public Soldier()
        {
            this.IdleForMovement = true;
            this.IdleForShooting = true;
            this.IdleForMelee = true;
            this.IdleForMorale = true;
        }

        public bool IdleForPhase(Phase phase)
        {
            if (phase == Phase.Movement) return this.IdleForMovement;
            else if (phase == Phase.Shooting) return this.IdleForShooting;
            else if (phase == Phase.Melee) return this.IdleForMelee;
            else if (phase == Phase.Morale) return this.IdleForMorale;
            else return false;
        }
        public void NextTurn()
        {
            this.IdleForMovement = true;
            this.IdleForShooting = true;
            this.IdleForMelee = true;
            this.IdleForMorale = true;
        }

        public override string ToString()
        {
            return string.Format(
                "{0} -> {1}, {2}, {3}, {4}", 
                this.Name, 
                this.IdleForMovement ? "Can move" : "He moved",
                this.IdleForShooting ? "Can shoot" : "He shoot",
                this.IdleForMelee ? "Can fight" : "He fought",
                this.IdleForMorale ? "Has to pass morale" : "Passed");
        }
    }
}
