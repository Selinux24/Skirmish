using System;
using System.Collections.Generic;
using System.Linq;

namespace TerrainSamples.SceneGrid.Rules
{
    using TerrainSamples.SceneGrid.Rules.Enum;

    public class Skirmish
    {
        private readonly List<Team> teams = new();
        private readonly Dictionary<Team, bool> turnInfo = new();
        private readonly List<Melee> melees = new();

        private int currentTurn = 0;
        private int currentSoldier = 0;

        public Phase CurrentPhase { get; private set; } = Phase.Movement;
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
                var idleTeams = new List<Team>();

                foreach (Team team in this.turnInfo.Keys)
                {
                    if (this.turnInfo[team])
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
                return Array.FindAll(this.CurrentTeam.Soldiers, s => s.IdleForPhase(this.CurrentPhase));
            }
        }
        public Soldier[] AllSoldiers
        {
            get
            {
                var soldiers = new List<Soldier>();

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

        public void AddTeam(string name, string faction, TeamRoles role, int soldiers, int heavy, int docs, bool addLeader = true)
        {
            var team = new Team(name)
            {
                Faction = faction,
                Role = role,
            };

            if (addLeader)
            {
                team.AddSoldier(string.Format("Hannibal Smith of {0}", name), SoldierClasses.Line);
            }

            for (int i = 0; i < soldiers; i++)
            {
                team.AddSoldier(string.Format("John Smith {0:00} of {1}", i + 1, name), SoldierClasses.Line);
            }

            for (int i = 0; i < heavy; i++)
            {
                team.AddSoldier(string.Format("Puro Smith {0:00} of {1}", i + 1, name), SoldierClasses.Heavy);
            }

            for (int i = 0; i < docs; i++)
            {
                team.AddSoldier(string.Format("Doc Smith {0:00} of {1}", i + 1, name), SoldierClasses.Medic);
            }

            this.teams.Add(team);
        }
        public void Start()
        {
            this.currentTurn = 1;
            this.CurrentPhase = 0;

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
                t.Role != TeamRoles.Neutral &&
                Array.Exists(t.Soldiers, s => s.CurrentHealth != HealthStates.Disabled)).ToArray();

            string[] factions = activeTeams.Select((a) => { return a.Faction; }).Distinct().ToArray();
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
                        if (this.CurrentSoldier.IdleForPhase(this.CurrentPhase))
                        {
                            this.NextSoldierIndex();
                        }

                        while (!this.CurrentSoldier.IdleForPhase(this.CurrentPhase))
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
                    this.PrevSoldierIndex();
                }
                else
                {
                    this.currentSoldier = Array.IndexOf(this.Soldiers, selectables[0]);
                    this.PrevSoldierIndex();
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
                this.CurrentPhase,
                this.CurrentTeam,
                this.CurrentSoldier,
                melee != null,
                ActionTypes.Manual);
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
            if (this.CurrentPhase == Phase.End)
            {
                //All phases done for this team. Select next team
                this.EndPhase();
            }
            else
            {
                //Done with current phase, next phase
                this.OnePhase();
            }
        }
        private void EndPhase()
        {
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

                this.CurrentPhase = 0;
                this.currentSoldier = 0;

                #endregion
            }
            else
            {
                #region Next team

                this.CurrentPhase = 0;
                this.currentSoldier = 0;

                #endregion
            }
        }
        private void OnePhase()
        {
            if (this.CurrentPhase == Phase.Melee)
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

                this.melees.RemoveAll(m => m.Done);
            }

            this.CurrentPhase++;
            this.currentSoldier = 0;
        }

        public Team[] EnemyOf(Team team)
        {
            return teams.FindAll(t => t.Faction != team.Faction && t.Role != TeamRoles.Neutral).ToArray();
        }
        public Team[] FriendOf(Team team)
        {
            return teams.FindAll(t => t.Faction == team.Faction || t.Role == TeamRoles.Neutral).ToArray();
        }

        public override string ToString()
        {
            return string.Format(
                "Battle -> {0} teams. Turn {1} ++ Phase {2}. Melees {3}",
                this.teams.Count,
                this.currentTurn,
                this.CurrentPhase,
                this.melees.Count);
        }
    }
}
