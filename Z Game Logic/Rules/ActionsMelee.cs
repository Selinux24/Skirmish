
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
                    new Leave(),
                    new UseMeleeItem(),
                };
            }
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
            if (this.Active.LeaveMeleeTest())
            {
                this.Melee.RemoveFighter(this.Active);
            }

            return true;
        }
    }

    public class UseMeleeItem : ActionsMelee
    {
        public UseMeleeItem()
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
