
namespace GameLogic.Rules
{
    public abstract class ActionsShooting : Actions
    {
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
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            if (this.Passive.Health == HealthStates.Healthy)
            {
                this.Passive.Health = HealthStates.Wounded;
            }
            else if (this.Passive.Health == HealthStates.Wounded)
            {
                this.Passive.Health = HealthStates.Disabled;
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
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class Support : ActionsShooting
    {
        public Support()
        {
            this.Name = "Support";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class UseShootingItem : ActionsShooting
    {
        public UseShootingItem()
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

    public class FirstAid : ActionsShooting
    {
        public FirstAid()
        {
            this.Name = "First Aid";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            if (this.Passive.Health == HealthStates.Disabled)
            {
                this.Passive.Health = HealthStates.Wounded;
            }
            else if (this.Passive.Health == HealthStates.Wounded)
            {
                this.Passive.Health = HealthStates.Healthy;
            }

            return true;
        }
    }
}
