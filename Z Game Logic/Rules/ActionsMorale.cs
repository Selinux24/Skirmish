
namespace GameLogic.Rules
{
    public abstract class ActionsMorale : Actions
    {
        public static ActionsMorale[] GetList(Skirmish game)
        {
            return new ActionsMorale[]
            {
                new TakeControl(game),
                new UseMoraleItem(game),
            };
        }

        public ActionsMorale(Skirmish game)
            : base(game)
        {

        }
    }

    public class TakeControl : ActionsMorale
    {
        public TakeControl(Skirmish game)
            : base(game)
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
        public UseMoraleItem(Skirmish game)
            : base(game)
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
