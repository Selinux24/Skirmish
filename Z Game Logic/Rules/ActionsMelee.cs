
namespace GameLogic.Rules
{
    public abstract class ActionsMelee : Actions
    {
        public Melee Melee { get; set; }

        public static ActionsMelee[] List
        {
            get
            {
                return new ActionsMelee[]
                {
                    new Fight(),
                    new Leave(),
                    new UseMeleeItem(),
                };
            }
        }
    }

    public class Fight : ActionsMelee
    {
        public Fight()
        {
            this.Name = "Fight";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class Leave : ActionsMelee
    {
        public Leave()
        {
            this.Name = "Leave Combat";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            this.Melee.RemoveFighter(this.Active);

            return true;
        }
    }

    public class UseMeleeItem : ActionsMelee
    {
        public UseMeleeItem()
        {
            this.Name = "Use Item";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            this.Item.Use();

            return true;
        }
    }
}
