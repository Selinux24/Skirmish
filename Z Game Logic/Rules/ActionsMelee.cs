
namespace GameLogic.Rules
{
    public abstract class ActionsMelee : Actions
    {
        public Melee Melee { get; set; }

        public static ActionsMelee[] GetList(Skirmish game)
        {
            return new ActionsMelee[]
            {
                new Leave(game),
                new UseMeleeItem(game),
            };
        }

        public ActionsMelee(Skirmish game)
            : base(game)
        {

        }
    }

    public class Leave : ActionsMelee
    {
        public Leave(Skirmish game)
            : base(game)
        {
            this.Name = "Leave Combat";
        }

        public override bool Execute()
        {
            if (this.Active.LeaveMeleeTest())
            {
                this.Melee.RemoveFighter(this.Active);
            }

            return true;
        }
    }

    public class UseMeleeItem : ActionsMelee
    {
        public UseMeleeItem(Skirmish game)
            : base(game)
        {
            this.Name = "Use Item";
            this.ItemAction = true;
        }

        public override bool Execute()
        {
            this.Active.UseItemForMeleePhase();

            return true;
        }
    }
}
