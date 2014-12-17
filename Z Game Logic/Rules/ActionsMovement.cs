using SharpDX;

namespace GameLogic.Rules
{
    public abstract class ActionsMovement : Actions
    {
        public int Distance { get; set; }

        public static ActionsMovement[] List
        {
            get
            {
                return new ActionsMovement[]
                {
                    new Move(),
                    new Run(),
                    new Crawl(),
                    new Assault(),
                    new CoveringFire(),
                    new Reload(),
                    new Repair(),
                    new Inventory(),
                    new UseMovementItem(),
                    new Communications(),
                    new FindCover(),
                    new RunAway(),
                };
            }
        }
    }

    public class Move : ActionsMovement
    {
        public Move()
        {
            this.Name = "Move";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class Run : ActionsMovement
    {
        public Run()
        {
            this.Name = "Run";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class Crawl : ActionsMovement
    {
        public Crawl()
        {
            this.Name = "Crawl";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class Assault : ActionsMovement
    {
        public Assault()
        {
            this.Name = "Assault";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;

            return true;
        }
    }

    public class CoveringFire : ActionsMovement
    {
        public CoveringFire()
        {
            this.Name = "Covering Fire";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class Reload : ActionsMovement
    {
        public Reload()
        {
            this.Name = "Reload";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = true;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class Repair : ActionsMovement
    {
        public Repair()
        {
            this.Name = "Repair";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = true;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class Inventory : ActionsMovement
    {
        public Inventory()
        {
            this.Name = "Inventory";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = true;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class UseMovementItem : ActionsMovement
    {
        public UseMovementItem()
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

    public class Communications : ActionsMovement
    {
        public Communications()
        {
            this.Name = "Communications";
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class FindCover : ActionsMovement
    {
        public FindCover()
        {
            this.Name = "Find Cover";
            this.Automatic = true;
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }

    public class RunAway : ActionsMovement
    {
        public RunAway()
        {
            this.Name = "Run Away";
            this.Automatic = true;
        }

        public override bool Execute()
        {
            this.Active.IdleForMovement = false;
            this.Active.IdleForShooting = false;
            this.Active.IdleForMelee = false;

            return true;
        }
    }
}
