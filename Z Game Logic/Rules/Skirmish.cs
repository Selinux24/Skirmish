using System.Collections.Generic;
using System;

namespace GameLogic.Rules
{
    public class Skirmish
    {
        private List<Team> teams = new List<Team>();
        private Dictionary<Team, bool> turnInfo = new Dictionary<Team, bool>();

        private int currentTurn = 0;
        private Phase currentPhase = Phase.Movement;
        private int currentSoldier = 0;
        public Team CurrentTeam
        {
            get
            {
                Team[] idleTeams = this.IdleTeams;

                return idleTeams.Length > 0 ? idleTeams[0] : null;
            }
        }
        public Team[] Teams
        {
            get
            {
                return this.teams.ToArray();
            }
        }
        public Team[] IdleTeams
        {
            get
            {
                List<Team> idleTeams = new List<Team>();

                foreach (Team team in this.turnInfo.Keys)
                {
                    if (this.turnInfo[team] == true)
                    {
                        idleTeams.Add(team);
                    }
                }

                return idleTeams.ToArray();
            }
        }
        public Soldier CurrentSoldier
        {
            get
            {
                return this.Soldiers[this.currentSoldier];
            }
            set
            {
                if (value != null)
                {
                    this.currentSoldier = Array.IndexOf(this.Soldiers, value);
                }
            }
        }
        public Soldier[] Soldiers
        {
            get
            {
                return this.CurrentTeam.Soldiers;
            }
        }
        public Soldier[] IdleSoldiers
        {
            get
            {
                return Array.FindAll(this.CurrentTeam.Soldiers, s => s.IdleForPhase(this.currentPhase));
            }
        }

        public Skirmish()
        {

        }

        public void AddTeam(string name, string faction, TeamRole role, int soldiers)
        {
            List<Soldier> soldierList = new List<Soldier>(soldiers);

            for (int i = 0; i < soldiers; i++)
            {
                soldierList.Add(new Soldier() { Name = string.Format("John Smith {0:00} of {1}", i + 1, name) });
            }

            Team team = new Team()
            {
                Name = name,
                Faction = faction,
                Role = role,
                Leader = soldierList[0],
                Soldiers = soldierList.ToArray(),
            };

            this.teams.Add(team);
        }

        public void Start()
        {
            this.currentTurn = 1;
            this.currentPhase = 0;

            this.turnInfo.Clear();
            foreach (Team team in this.teams)
            {
                this.turnInfo.Add(team, true);
            }

            this.currentSoldier = 0;
        }

        public Soldier NextSoldier(bool selectIdle)
        {
            Soldier current = this.CurrentSoldier;

            Soldier[] selectables = selectIdle ? this.IdleSoldiers : this.Soldiers;
            if (selectables.Length > 0)
            {
                int index = Array.IndexOf(selectables, current);
                if (index >= 0)
                {
                    this.currentSoldier = index + 1;
                }
                else
                {
                    this.currentSoldier = Array.IndexOf(this.Soldiers, selectables[0]);
                }

                if (this.currentSoldier > this.Soldiers.Length - 1)
                {
                    this.currentSoldier = 0;
                }
            }

            return this.CurrentSoldier;
        }
        public Soldier PrevSoldier(bool selectIdle)
        {
            Soldier current = this.CurrentSoldier;

            Soldier[] selectables = selectIdle ? this.IdleSoldiers : this.Soldiers;
            if (selectables.Length > 0)
            {
                int index = Array.IndexOf(selectables, current);
                if (index >= 0)
                {
                    this.currentSoldier = index - 1;
                }
                else
                {
                    this.currentSoldier = Array.IndexOf(this.Soldiers, selectables[0]);
                }

                if (this.currentSoldier < 0)
                {
                    this.currentSoldier = this.Soldiers.Length - 1;
                }
            }

            return this.CurrentSoldier;
        }

        public SoldierAction[] GetActions()
        {
            return SoldierAction.GetActions(this.currentPhase, this.CurrentTeam, this.CurrentSoldier);
        }
        public void DoAction(SoldierAction action)
        {
            if (SoldierAction.DoAction(action, this.currentPhase, this.CurrentTeam, this.CurrentSoldier))
            {
                this.NextSoldier(true);
            }
        }

        public void NextPhase()
        {
            if (this.currentPhase == Phase.End)
            {
                //All phases done for this team. Select next team

                //Get current
                Team currentTeam = this.CurrentTeam;

                //Mark actions done
                this.turnInfo[currentTeam] = false;

                //Get new current (next idle)
                currentTeam = this.CurrentTeam;
                if (currentTeam == null)
                {
                    //Next turn
                    this.currentTurn++;

                    //Mark all teams idle
                    this.turnInfo.Clear();
                    foreach (Team team in this.teams)
                    {
                        this.turnInfo.Add(team, true);
                    }
                }

                this.currentPhase = 0;

                //Select first idle soldier for this phase
                this.currentSoldier = 0;
            }
            else
            {
                //Done with current phase
                this.currentPhase++;

                //Select first idle soldier for this phase
                this.currentSoldier = 0;
            }
        }

        public override string ToString()
        {
            return string.Format("Battle -> {0} teams. Turn {0} | Phase {1}", this.currentTurn, this.currentPhase, this.teams.Count);
        }
    }
}
