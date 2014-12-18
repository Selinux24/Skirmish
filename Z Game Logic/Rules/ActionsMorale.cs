
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
            this.Active.TakeControlTest();

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
            this.Active.UseItemForMoralePhase();

            return true;
        }
    }
}
