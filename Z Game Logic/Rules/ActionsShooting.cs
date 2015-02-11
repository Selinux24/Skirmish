
namespace GameLogic.Rules
{
    public abstract class ActionsShooting : Actions
    {
        public int WastedPoints { get; set; }

        public static ActionsShooting[] GetList(Skirmish game)
        {
            return new ActionsShooting[]
            {
                new Shoot(game),
                new SupressingFire(game),
                new Support(game),
                new UseShootingItem(game),
                new FirstAid(game),
            };
        }

        public ActionsShooting(Skirmish game)
            : base(game)
        {

        }
    }

    public class Shoot : ActionsShooting
    {
        public Shoot(Skirmish game)
            : base(game)
        {
            this.Name = "Shoot";
        }

        public override bool Execute()
        {
            if (this.Active.ShootingTest(this.WastedPoints))
            {
                this.Passive.HitTest(this.Active.CurrentShootingWeapon);
            }

            return true;
        }
    }

    public class SupressingFire : ActionsShooting
    {
        public SupressingFire(Skirmish game)
            : base(game)
        {
            this.Name = "Supressing Fire";
        }

        public override bool Execute()
        {
            this.Active.SetState(SoldierStates.SupressingFire, this.Area);

            return true;
        }
    }

    public class Support : ActionsShooting
    {
        public Support(Skirmish game)
            : base(game)
        {
            this.Name = "Support";
            this.NeedsCommunicator = true;
        }

        public override bool Execute()
        {
            this.Active.SupportTest();

            return true;
        }
    }

    public class UseShootingItem : ActionsShooting
    {
        public UseShootingItem(Skirmish game)
            : base(game)
        {
            this.Name = "Use Item";
            this.ItemAction = true;
        }

        public override bool Execute()
        {
            this.Active.UseItemForShootingPhase(this.WastedPoints);

            return true;
        }
    }

    public class FirstAid : ActionsShooting
    {
        public FirstAid(Skirmish game)
            : base(game)
        {
            this.Name = "First Aid";
            this.Classes = SoldierClasses.Medic;
        }

        public override bool Execute()
        {
            if (this.Active.FirstAidTest(this.WastedPoints))
            {
                this.Passive.HealingTest(10);
            }

            return true;
        }
    }
}
