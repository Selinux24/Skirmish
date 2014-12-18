
namespace GameLogic.Rules
{
    public abstract class ActionsShooting : Actions
    {
        public int WastedPoints { get; set; }

        public static ActionsShooting[] List
        {
            get
            {
                return new ActionsShooting[]
                {
                    new Shoot(),
                    new SupressingFire(),
                    new Support(),
                    new UseShootingItem(),
                    new FirstAid(),
                };
            }
        }
    }

    public class Shoot : ActionsShooting
    {
        public Shoot()
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
        public SupressingFire()
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
        public Support()
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
        public UseShootingItem()
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
        public FirstAid()
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
