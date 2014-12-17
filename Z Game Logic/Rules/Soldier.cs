using System;

namespace GameLogic.Rules
{
    public class Soldier
    {
        private int baseMovingCapacity = 1;
        private int baseActionPoints = 1;
        private int baseMelee = 1;
        private int baseSmallWeapons = 1;
        private int baseBigWeapons = 1;
        private int baseStrength = 1;
        private int baseAgility = 1;
        private int baseEndurance = 1;
        private int baseHealth = 1;
        private int baseInitiative = 1;

        public Team Team { get; set; }
        public string Name { get; set; }
        public SoldierClasses SoldierClass { get; set; }
        public HealthStates Health { get; set; }
        public MoraleStates Morale { get; set; }
        public bool IdleForMovement { get; set; }
        public bool IdleForShooting { get; set; }
        public bool IdleForMelee { get; set; }
        public bool IdleForMorale { get; set; }
        public int Initiative
        {
            get
            {
                return this.baseInitiative;
            }
        }

        public Soldier(string name, SoldierClasses soldierClass)
        {
            this.Name = name;
            this.SoldierClass = soldierClass;

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
        public Actions[] GetActions(Phase phase, ActionTypes actionType = ActionTypes.All)
        {
            Actions[] actions = new Actions[] { };

            if (phase == Phase.Movement && this.IdleForMovement)
            {
                actions = ActionsMovement.List;
                if (actions.Length > 0)
                {
                    Array.ForEach(actions, a => a.Active = this);
                }
            }
            else if (phase == Phase.Shooting && this.IdleForShooting)
            {
                actions = ActionsShooting.List;
                if (actions.Length > 0)
                {
                    Array.ForEach(actions, a => a.Active = this);
                }
            }
            else if (phase == Phase.Melee && this.IdleForMelee)
            {
                actions = ActionsMelee.List;
                if (actions.Length > 0)
                {
                    Array.ForEach(actions, a => a.Active = this);
                }
            }
            else if (phase == Phase.Morale && this.IdleForMorale)
            {
                actions = ActionsMorale.List;
                if (actions.Length > 0)
                {
                    Array.ForEach(actions, a => a.Active = this);
                }
            }

            if (actionType == ActionTypes.All)
            {
                return actions;
            }
            else
            {
                return Array.FindAll(actions, a => a.Automatic == (actionType == ActionTypes.Automatic));
            }
        }

        public void HitTest(Soldier soldier)
        {
            if (this.Health == HealthStates.Healthy)
            {
                this.Health = HealthStates.Wounded;
            }
            else if (this.Health == HealthStates.Wounded)
            {
                this.Health = HealthStates.Disabled;
            }
        }
        public void FightTest(Soldier soldier)
        {
            if (this.Health == HealthStates.Healthy)
            {
                this.Health = HealthStates.Wounded;
            }
            else if (this.Health == HealthStates.Wounded)
            {
                this.Health = HealthStates.Disabled;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} [{1}][{2}] -> {3}, {4}, {5}, {6}",
                this.Name,
                this.Health,
                this.Morale,
                this.IdleForMovement ? "Can move" : "He moved",
                this.IdleForShooting ? "Can shoot" : "He shoot",
                this.IdleForMelee ? "Can fight" : "He fought",
                this.IdleForMorale ? "Has to pass morale" : "Passed");
        }
    }
}
