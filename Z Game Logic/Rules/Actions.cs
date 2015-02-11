using System.Collections.Generic;

namespace GameLogic.Rules
{
    public abstract class Actions
    {
        protected Skirmish Game { get; private set; }

        public string Name { get; set; }
        public Soldier Active { get; set; }
        public Soldier Passive { get; set; }
        public Area Area { get; set; }
        public bool Automatic { get; set; }
        public bool ItemAction { get; set; }
        public bool LeadersOnly { get; set; }
        public bool NeedsCommunicator { get; set; }
        public SoldierClasses Classes { get; set; }

        public Actions(Skirmish game)
        {
            this.Game = game;

            this.Automatic = false;
            this.ItemAction = false;
            this.LeadersOnly = false;
            this.NeedsCommunicator = false;
            this.Classes = SoldierClasses.Line | SoldierClasses.Heavy | SoldierClasses.Support | SoldierClasses.Medic;
        }

        public static Actions[] GetActions(Skirmish game, Phase phase, Team team, Soldier soldier, bool onMelee, ActionTypes actionType = ActionTypes.All)
        {
            List<Actions> actions = new List<Actions>();

            Actions[] teamActions = team.GetActions(game, phase, soldier, onMelee, actionType);

            if (teamActions.Length > 0) actions.AddRange(teamActions);

            Actions[] soldierActions = soldier.GetActions(game, phase, onMelee, actionType);

            if (soldierActions.Length > 0) actions.AddRange(soldierActions);

            return actions.ToArray();
        }

        public abstract bool Execute();

        public override string ToString()
        {
            return string.Format("{0}", this.Name);
        }
    }
}
