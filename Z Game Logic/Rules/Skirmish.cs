using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Rules
{
    using GameLogic.Rules.Enum;

    public class Skirmish
    {
        private List<Team> teams = new List<Team>();
        private Dictionary<Team, bool> turnInfo = new Dictionary<Team, bool>();
        private List<Melee> melees = new List<Melee>();

        private int currentTurn = 0;
        private Phase currentPhase = Phase.Movement;
        private int currentSoldier = 0;
        public Phase CurrentPhase
        {
            get
            {
                return this.currentPhase;
            }
        }
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
        public Soldier[] AllSoldiers
        {
            get
            {
                List<Soldier> soldiers = new List<Soldier>();

                foreach (Team team in this.Teams)
                {
                    soldiers.AddRange(team.Soldiers);
                }

                return soldiers.ToArray();
            }
        }

        public Skirmish()
        {

        }

        public void AddTeam(string name, string faction, TeamRoleEnum role, int soldiers, int heavy, int docs, bool addLeader = true)
        {
            Team team = new Team(name)
            {
                Faction = faction,
                Role = role,
            };

            if (addLeader)
            {
                team.AddSoldier(string.Format("Hannibal Smith of {0}", name), SoldierClassEnum.Line);
            }

            for (int i = 0; i < soldiers; i++)
            {
                team.AddSoldier(string.Format("John Smith {0:00} of {1}", i + 1, name), SoldierClassEnum.Line);
            }

            for (int i = 0; i < heavy; i++)
            {
                team.AddSoldier(string.Format("Puro Smith {0:00} of {1}", i + 1, name), SoldierClassEnum.Heavy);
            }

            for (int i = 0; i < docs; i++)
            {
                team.AddSoldier(string.Format("Doc Smith {0:00} of {1}", i + 1, name), SoldierClassEnum.Medic);
            }

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
        public Victory IsFinished()
        {
            //Victory conditions...
            Team[] activeTeams = teams.FindAll(t =>
                t.Role != TeamRoleEnum.Neutral &&
                Array.Exists(t.Soldiers, s => s.CurrentHealth != HealthStateEnum.Disabled)).ToArray();

            string[] factions = activeTeams.Distinct((a) => { return a.Faction; }).ToArray();
            if (factions.Length == 1)
            {
                return new Victory()
                {
                    Text = factions[0] + " wins!",
                };
            }
            else if (factions.Length == 0)
            {
                return new Victory()
                {
                    Text = "Draw...",
                };
            }
            else
            {
                return null;
            }
        }

        public Soldier NextSoldier(bool selectIdle)
        {
            Soldier[] soldiers = this.Soldiers;
            if (soldiers.Length > 0)
            {
                if (selectIdle)
                {
                    Soldier[] idles = this.IdleSoldiers;
                    if (idles.Length > 0)
                    {
                        if (this.CurrentSoldier.IdleForPhase(this.currentPhase))
                        {
                            this.NextSoldierIndex();
                        }

                        while (!this.CurrentSoldier.IdleForPhase(this.currentPhase))
                        {
                            this.NextSoldierIndex();
                        }
                    }
                }
                else
                {
                    this.NextSoldierIndex();
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
        private void NextSoldierIndex()
        {
            this.currentSoldier++;

            if (this.currentSoldier > this.Soldiers.Length - 1)
            {
                this.currentSoldier = 0;
            }
        }
        private void PrevSoldierIndex()
        {
            this.currentSoldier--;

            if (this.currentSoldier < 0)
            {
                this.currentSoldier = this.Soldiers.Length - 1;
            }
        }

        public ActionSpecification[] GetActions()
        {
            Melee melee = this.GetMelee(this.CurrentSoldier);

            return ActionsManager.GetActions(
                this.currentPhase,
                this.CurrentTeam,
                this.CurrentSoldier,
                melee != null,
                ActionTypeEnum.Manual);
        }
        public Melee GetMelee(Soldier soldier)
        {
            return this.melees.Find(m => m.ContainsSoldier(soldier));
        }
        public void JoinMelee(Soldier active, Soldier passive)
        {
            Melee melee = this.GetMelee(passive);
            if (melee == null)
            {
                melee = new Melee();

                this.melees.Add(melee);

                melee.AddFighter(passive);
            }

            melee.AddFighter(active);
        }

        public void NextPhase()
        {
            if (this.currentPhase == Phase.End)
            {
                #region All phases done for this team. Select next team

                //Get current
                Team currentTeam = this.CurrentTeam;

                //Mark actions done
                this.turnInfo[currentTeam] = false;

                //Get new current (next idle)
                currentTeam = this.CurrentTeam;
                if (currentTeam == null)
                {
                    #region No idle teams, next turn

                    this.currentTurn++;

                    //Mark all teams idle
                    this.turnInfo.Clear();
                    foreach (Team team in this.teams)
                    {
                        team.NextTurn();

                        this.turnInfo.Add(team, true);
                    }

                    this.currentPhase = 0;
                    this.currentSoldier = 0;

                    #endregion
                }
                else
                {
                    #region Next team

                    this.currentPhase = 0;
                    this.currentSoldier = 0;

                    #endregion
                }

                #endregion
            }
            else
            {
                #region Done with current phase, next phase

                if (this.currentPhase == Phase.Melee)
                {
                    //Resolve melees
                    foreach (Melee melee in this.melees)
                    {
                        melee.Resolve();

                        if (melee.Done)
                        {
                            melee.Disolve();
                        }
                    }

                    this.melees.RemoveAll(m => m.Done == true);
                }

                this.currentPhase++;
                this.currentSoldier = 0;

                #endregion
            }
        }

        public Team[] EnemyOf(Team team)
        {
            return teams.FindAll(t => t.Faction != team.Faction && t.Role != TeamRoleEnum.Neutral).ToArray();
        }
        public Team[] FriendOf(Team team)
        {
            return teams.FindAll(t => t.Faction == team.Faction || t.Role == TeamRoleEnum.Neutral).ToArray();
        }

        public override string ToString()
        {
            return string.Format(
                "Battle -> {0} teams. Turn {1} | Phase {2}. Melees {3}",
                this.teams.Count,
                this.currentTurn,
                this.currentPhase,
                this.melees.Count);
        }
    }
}
