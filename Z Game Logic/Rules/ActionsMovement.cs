using SharpDX;

namespace GameLogic.Rules
{
    public abstract class ActionsMovement : Actions
    {
        public Vector3 Destination { get; set; }
        public int WastedPoints { get; set; }

        public static ActionsMovement[] GetList(Skirmish game)
        {
            return new ActionsMovement[]
            {
                new Move(game),
                new Run(game),
                new Crawl(game),
                new Assault(game),
                new CoveringFire(game),
                new Reload(game),
                new Repair(game),
                new Inventory(game),
                new UseMovementItem(game),
                new Communications(game),
                new FindCover(game),
                new RunAway(game),
            };
        }

        public ActionsMovement(Skirmish game)
            : base(game)
        {

        }
    }

    public class Move : ActionsMovement
    {
        public Move(Skirmish game)
            : base(game)
        {
            this.Name = "Move";
        }

        public override bool Execute()
        {
            if (this.Active.IdleForMovement)
            {
                this.Active.Move(this.WastedPoints);

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class Run : ActionsMovement
    {
        public Run(Skirmish game)
            : base(game)
        {
            this.Name = "Run";
        }

        public override bool Execute()
        {
            if (this.Active.IdleForMovement)
            {
                this.Active.Run(this.WastedPoints);

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class Crawl : ActionsMovement
    {
        public Crawl(Skirmish game)
            : base(game)
        {
            this.Name = "Crawl";
        }

        public override bool Execute()
        {
            if (this.Active.IdleForMovement)
            {
                this.Active.Crawl(this.WastedPoints);

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class Assault : ActionsMovement
    {
        public Assault(Skirmish game)
            : base(game)
        {
            this.Name = "Assault";
        }

        public override bool Execute()
        {
            this.Active.Assault(this.WastedPoints);
            this.Passive.Assault(0);

            return true;
        }
    }

    public class CoveringFire : ActionsMovement
    {
        public CoveringFire(Skirmish game)
            : base(game)
        {
            this.Name = "Covering Fire";
        }

        public override bool Execute()
        {
            this.Active.SetState(SoldierStates.CoveringFire, this.Area);

            return true;
        }
    }

    public class Reload : ActionsMovement
    {
        public Reload(Skirmish game)
            : base(game)
        {
            this.Name = "Reload";
        }

        public override bool Execute()
        {
            this.Active.ReloadTest(this.WastedPoints);

            return true;
        }
    }

    public class Repair : ActionsMovement
    {
        public Repair(Skirmish game)
            : base(game)
        {
            this.Name = "Repair";
        }

        public override bool Execute()
        {
            this.Active.RepairTest(this.WastedPoints);

            return true;
        }
    }

    public class Inventory : ActionsMovement
    {
        public Inventory(Skirmish game)
            : base(game)
        {
            this.Name = "Inventory";
        }

        public override bool Execute()
        {
            this.Active.Inventory(this.WastedPoints);

            return true;
        }
    }

    public class UseMovementItem : ActionsMovement
    {
        public UseMovementItem(Skirmish game)
            : base(game)
        {
            this.Name = "Use Item";
            this.ItemAction = true;
        }

        public override bool Execute()
        {
            this.Active.UseItemForMovementPhase(this.WastedPoints);

            return true;
        }
    }

    public class Communications : ActionsMovement
    {
        public Communications(Skirmish game)
            : base(game)
        {
            this.Name = "Communications";
            this.LeadersOnly = true;
        }

        public override bool Execute()
        {
            this.Active.CommunicationsTest();

            return true;
        }
    }

    public class FindCover : ActionsMovement
    {
        public FindCover(Skirmish game)
            : base(game)
        {
            this.Name = "Find Cover";
            this.Automatic = true;
        }

        public override bool Execute()
        {
            this.Active.FindCover();

            return true;
        }
    }

    public class RunAway : ActionsMovement
    {
        public RunAway(Skirmish game)
            : base(game)
        {
            this.Name = "Run Away";
            this.Automatic = true;
        }

        public override bool Execute()
        {
            this.Active.RunAway();

            return true;
        }
    }
}
