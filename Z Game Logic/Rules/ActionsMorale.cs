
namespace GameLogic.Rules
{
    public abstract class ActionsMorale : Actions
    {
        public static ActionsMorale[] List
        {
            get
            {
                return new ActionsMorale[]
                {
                    new TakeControl(),
                    new UseMoraleItem(),
                };
            }
        }
    }

    public class TakeControl : ActionsMorale
    {
        public TakeControl()
        {
            this.Name = "Take Control";
            this.Automatic = true;
        }

        public override bool Execute()
        {
            this.Active.IdleForMorale = false;

            return true;
        }
    }

    public class UseMoraleItem : ActionsMorale
    {
        public UseMoraleItem()
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
