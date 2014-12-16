
namespace GameLogic.Rules
{
    public abstract class SoldierAction
    {
        public string Name { get; set; }
        public Soldier Active { get; set; }
        public Soldier Passive { get; set; }

        public abstract bool Execute();

        internal static SoldierAction[] GetActions(Phase phase, Team team, Soldier soldier)
        {
            if (phase == Phase.Movement)
            {
                return new SoldierAction[] 
                {
                    new Move(),
                };
            }
            else if (phase == Phase.Shooting)
            {
                return new SoldierAction[] 
                {
                    new Fire(),
                };
            }
            else if (phase == Phase.Melee)
            {
                return new SoldierAction[] 
                {
                    new Melee(),
                };
            }
            else if (phase == Phase.Morale)
            {
                return new SoldierAction[] 
                {
                    new Morale(),
                };
            }
            else
            {
                return new SoldierAction[] { };
            }
        }
        internal static bool DoAction(SoldierAction action, Phase phase, Team team, Soldier soldier)
        {
            return action.Execute();
        }
    }

    public class Move : SoldierAction
    {
        public override bool Execute()
        {
            this.Active.IdleForMovement = false;

            return true;
        }
    }

    public class Fire : SoldierAction
    {
        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class Melee : SoldierAction
    {
        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class Morale : SoldierAction
    {
        public override bool Execute()
        {
            this.Active.IdleForMorale = false;

            return true;
        }
    }
}
