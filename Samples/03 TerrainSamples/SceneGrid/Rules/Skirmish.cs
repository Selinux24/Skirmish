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
                Team[] idleTeams = IdleTeams;

                return idleTeams.Length > 0 ? idleTeams[0] : null;
            }
        }
        public Team[] Teams
        {
            get
            {
                return teams.ToArray();
            }
        }
        public Team[] IdleTeams
        {
            get
            {
                var idleTeams = new List<Team>();

                foreach (Team team in turnInfo.Keys)
                {
                    if (turnInfo[team])
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
                return Soldiers[currentSoldier];
            }
            set
            {
                if (value != null)
                {
                    currentSoldier = Array.IndexOf(Soldiers, value);
                }
            }
        }
        public Soldier[] Soldiers
        {
            get
            {
                return CurrentTeam.Soldiers;
            }
        }
        public Soldier[] IdleSoldiers
        {
            get
            {
                return Array.FindAll(CurrentTeam.Soldiers, s => s.IdleForPhase(CurrentPhase));
            }
        }
        public Soldier[] AllSoldiers
        {
            get
            {
                var soldiers = new List<Soldier>();

                foreach (Team team in Teams)
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

            teams.Add(team);
        }
        public void Start()
        {
            currentTurn = 1;
            CurrentPhase = 0;

            turnInfo.Clear();
            foreach (Team team in teams)
            {
                turnInfo.Add(team, true);
            }

            currentSoldier = 0;
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
            Soldier[] soldiers = Soldiers;
            if (soldiers.Length > 0)
            {
                if (selectIdle)
                {
                    Soldier[] idles = IdleSoldiers;
                    if (idles.Length > 0)
                    {
                        if (CurrentSoldier.IdleForPhase(CurrentPhase))
                        {
                            NextSoldierIndex();
                        }

                        while (!CurrentSoldier.IdleForPhase(CurrentPhase))
                        {
                            NextSoldierIndex();
                        }
                    }
                }
                else
                {
                    NextSoldierIndex();
                }
            }

            return CurrentSoldier;
        }
        public Soldier PrevSoldier(bool selectIdle)
        {
            Soldier current = CurrentSoldier;

            Soldier[] selectables = selectIdle ? IdleSoldiers : Soldiers;
            if (selectables.Length > 0)
            {
                int index = Array.IndexOf(selectables, current);
                if (index >= 0)
                {
                    PrevSoldierIndex();
                }
                else
                {
                    currentSoldier = Array.IndexOf(Soldiers, selectables[0]);
                    PrevSoldierIndex();
                }
            }

            return CurrentSoldier;
        }
        private void NextSoldierIndex()
        {
            currentSoldier++;

            if (currentSoldier > Soldiers.Length - 1)
            {
                currentSoldier = 0;
            }
        }
        private void PrevSoldierIndex()
        {
            currentSoldier--;

            if (currentSoldier < 0)
            {
                currentSoldier = Soldiers.Length - 1;
            }
        }

        public ActionSpecification[] GetActions()
        {
            Melee melee = GetMelee(CurrentSoldier);

            return ActionsManager.GetActions(
                CurrentPhase,
                CurrentTeam,
                CurrentSoldier,
                melee != null,
                ActionTypes.Manual);
        }
        public Melee GetMelee(Soldier soldier)
        {
            return melees.Find(m => m.ContainsSoldier(soldier));
        }
        public void JoinMelee(Soldier active, Soldier passive)
        {
            Melee melee = GetMelee(passive);
            if (melee == null)
            {
                melee = new Melee();

                melees.Add(melee);

                melee.AddFighter(passive);
            }

            melee.AddFighter(active);
        }

        public void NextPhase()
        {
            if (CurrentPhase == Phase.End)
            {
                //All phases done for this team. Select next team
                EndPhase();
            }
            else
            {
                //Done with current phase, next phase
                OnePhase();
            }
        }
        private void EndPhase()
        {
            //Get current
            Team currentTeam = CurrentTeam;

            //Mark actions done
            turnInfo[currentTeam] = false;

            //Get new current (next idle)
            currentTeam = CurrentTeam;
            if (currentTeam == null)
            {
                #region No idle teams, next turn

                currentTurn++;

                //Mark all teams idle
                turnInfo.Clear();
                foreach (Team team in teams)
                {
                    team.NextTurn();

                    turnInfo.Add(team, true);
                }

                CurrentPhase = 0;
                currentSoldier = 0;

                #endregion
            }
            else
            {
                #region Next team

                CurrentPhase = 0;
                currentSoldier = 0;

                #endregion
            }
        }
        private void OnePhase()
        {
            if (CurrentPhase == Phase.Melee)
            {
                //Resolve melees
                foreach (Melee melee in melees)
                {
                    melee.Resolve();

                    if (melee.Done)
                    {
                        melee.Disolve();
                    }
                }

                melees.RemoveAll(m => m.Done);
            }

            CurrentPhase++;
            currentSoldier = 0;
        }

        public Team[] EnemyOf(Team team)
        {
            return teams.FindAll(t => t.Faction != team.Faction && t.Role != TeamRoles.Neutral).ToArray();
        }
        public Team[] FriendOf(Team team)
        {
            return teams.FindAll(t => t.Faction == team.Faction || t.Role == TeamRoles.Neutral).ToArray();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Battle -> {teams.Count} teams. Turn {currentTurn} ++ Phase {CurrentPhase}. Melees {melees.Count}";
        }
    }
}
