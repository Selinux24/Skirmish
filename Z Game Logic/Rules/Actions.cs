using System.Collections.Generic;

namespace GameLogic.Rules
{
    public abstract class Actions
    {
        public string Name { get; set; }
        public Soldier Active { get; set; }
        public Soldier Passive { get; set; }
        public Item Item { get; set; }
        public bool Automatic { get; set; }

        public static Actions[] GetActions(Phase phase, Team team, Soldier soldier, ActionTypes actionType = ActionTypes.All)
        {
            List<Actions> actions = new List<Actions>();

            Actions[] teamActions = team.GetActions(phase, soldier, actionType);

            if (teamActions.Length > 0) actions.AddRange(teamActions);

            Actions[] soldierActions = soldier.GetActions(phase, actionType);

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
