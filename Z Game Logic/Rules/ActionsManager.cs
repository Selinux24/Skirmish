using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Rules
{
    using GameLogic.Rules.Enum;

    public static class ActionsManager
    {
        public static ActionSpecification[] GetActions(Phase phase, Team team, Soldier soldier, bool onMelee, ActionTypes actionType = ActionTypes.All)
        {
            var teamActions = GetListForTeam();

            teamActions = FilterTeamActions(teamActions, phase, actionType);

            IEnumerable<ActionSpecification> soldierActions = new ActionSpecification[] { };

            if (phase == Phase.Movement)
            {
                soldierActions = GetListForMovement();
            }
            else if (phase == Phase.Shooting)
            {
                soldierActions = GetListForShooting();
            }
            else if (phase == Phase.Melee)
            {
                soldierActions = GetListForMelee();
            }
            else if (phase == Phase.Morale)
            {
                soldierActions = GetListForMorale();
            }

            soldierActions = FilterSoldierActions(soldierActions, phase, soldier, onMelee, actionType);

            List<ActionSpecification> actions = new List<ActionSpecification>();

            if (teamActions.Any()) actions.AddRange(teamActions);
            if (soldierActions.Any()) actions.AddRange(soldierActions);

            return actions.ToArray();
        }

        private static IEnumerable<ActionSpecification> FilterTeamActions(IEnumerable<ActionSpecification> actions, Phase phase, ActionTypes actionType)
        {
            if (phase != Phase.End && actionType.HasFlag(ActionTypes.Automatic))
            {
                return actions.Where(a => a.Automatic).ToArray();
            }
            else if (phase != Phase.End && actionType.HasFlag(ActionTypes.Manual))
            {
                return actions.Where(a => !a.Automatic).ToArray();
            }

            return actions;
        }

        private static IEnumerable<ActionSpecification> FilterSoldierActions(IEnumerable<ActionSpecification> actions, Phase phase, Soldier soldier, bool onMelee, ActionTypes actionType)
        {
            //Filter by phase
            var filteredActions = FilterByPhase(actions, phase, soldier);

            filteredActions = filteredActions.Where(a => !a.LeadersOnly || (a.LeadersOnly && soldier.IsLeader));

            filteredActions = filteredActions.Where(a => ((a.Classes & soldier.SoldierClass) == soldier.SoldierClass));

            if (onMelee)
            {
                filteredActions = filteredActions.Where(a => onMelee && a.MeleeOnly);
            }
            else
            {
                filteredActions = filteredActions.Where(a => !onMelee && !a.MeleeOnly);
            }

            if (actionType != ActionTypes.All)
            {
                filteredActions = filteredActions.Where(a => a.Automatic == (actionType == ActionTypes.Automatic));
            }

            return filteredActions.ToArray();
        }

        private static IEnumerable<ActionSpecification> FilterByPhase(IEnumerable<ActionSpecification> actions, Phase phase, Soldier soldier)
        {
            if (phase == Phase.Movement && soldier.IdleForMovement)
            {
                return actions.Where(a => !a.ItemAction || a.ItemAction && soldier.HasItemsForMovingPhase);
            }
            else if (phase == Phase.Shooting && soldier.IdleForShooting)
            {
                return actions.Where(a => !a.ItemAction || a.ItemAction && soldier.HasItemsForShootingPhase);
            }
            else if (phase == Phase.Melee && soldier.IdleForMelee)
            {
                return actions.Where(a => !a.ItemAction || a.ItemAction && soldier.HasItemsForMeleePhase);
            }
            else if (phase == Phase.Morale)
            {
                return actions.Where(a => !a.ItemAction || a.ItemAction && soldier.HasItemsForMoralePhase);
            }

            return actions;
        }

        #region Team actions

        private static IEnumerable<ActionSpecification> GetListForTeam()
        {
            return new ActionSpecification[] { };
        }

        #endregion

        #region Movement

        private static IEnumerable<ActionSpecification> GetListForMovement()
        {
            return new[]
            {
                new ActionSpecification() { Action = Actions.Move,              Name = "Move" },
                new ActionSpecification() { Action = Actions.Run,               Name = "Run" },
                new ActionSpecification() { Action = Actions.Crawl,             Name = "Crawl" },
                new ActionSpecification() { Action = Actions.Assault,           Name = "Assault" },
                new ActionSpecification() { Action = Actions.CoveringFire,      Name = "Covering Fire" },
                new ActionSpecification() { Action = Actions.Reload,            Name = "Reload" },
                new ActionSpecification() { Action = Actions.Repair,            Name = "Repair" },
                new ActionSpecification() { Action = Actions.Inventory,         Name = "Inventory" },
                new ActionSpecification() { Action = Actions.UseMovementItem,   Name = "Use Item",          ItemAction = true },
                new ActionSpecification() { Action = Actions.Communications,    Name = "Communications",    LeadersOnly = true },
                new ActionSpecification() { Action = Actions.FindCover,         Name = "Find Cover",        Automatic = true },
                new ActionSpecification() { Action = Actions.RunAway,           Name = "Run Away",          Automatic = true },
            };
        }

        public static bool Move(Skirmish game, Soldier active, int wastedPoints)
        {
            if (active.IdleForMovement)
            {
                active.Move(wastedPoints);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Run(Skirmish game, Soldier active, int wastedPoints)
        {
            if (active.IdleForMovement)
            {
                active.Run(wastedPoints);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Crawl(Skirmish game, Soldier active, int wastedPoints)
        {
            if (active.IdleForMovement)
            {
                active.Crawl(wastedPoints);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Assault(Skirmish game, Soldier active, Soldier passive, int wastedPoints)
        {
            //TODO: This test must be repeated many times
            if (passive.CurrentHealth != HealthStates.Disabled)
            {
                game.JoinMelee(active, passive);

                active.Assault(wastedPoints);
                passive.Assault(0);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CoveringFire(Skirmish game, Soldier active, Weapon weapon, Area area, int wastedPoints)
        {
            active.SetState(SoldierStates.CoveringFire, weapon, area);

            return true;
        }

        public static bool Reload(Skirmish game, Soldier active, Weapon weapon, int wastedPoints)
        {
            active.ReloadTest(weapon, wastedPoints);

            return true;
        }

        public static bool Repair(Skirmish game, Soldier active, Weapon weapon, int wastedPoints)
        {
            active.RepairTest(weapon, wastedPoints);

            return true;
        }

        public static bool Inventory(Skirmish game, Soldier active, int wastedPoints)
        {
            active.Inventory(wastedPoints);

            return true;
        }

        public static bool UseMovementItem(Skirmish game, Soldier active, Item item, int wastedPoints)
        {
            active.UseItemForMovementPhase(item, wastedPoints);

            return true;
        }

        public static bool Communications(Skirmish game, Soldier active)
        {
            active.CommunicationsTest();

            return true;
        }

        public static bool FindCover(Skirmish game, Soldier active)
        {
            active.FindCover();

            return true;
        }

        public static bool RunAway(Skirmish game, Soldier active)
        {
            active.RunAway();

            return true;
        }

        #endregion

        #region Shooting

        private static IEnumerable<ActionSpecification> GetListForShooting()
        {
            return new[]
            {
                new ActionSpecification() { Action = Actions.Shoot,             Name = "Shoot",  },
                new ActionSpecification() { Action = Actions.SupressingFire,    Name = "Supressing Fire",  },
                new ActionSpecification() { Action = Actions.Support,           Name = "Support",           NeedsCommunicator = true, },
                new ActionSpecification() { Action = Actions.UseShootingItem,   Name = "Use Item",          ItemAction = true },
                new ActionSpecification() { Action = Actions.FirstAid,          Name = "First Aid",         Classes = SoldierClasses.Medic },
            };
        }

        public static bool Shoot(Skirmish game, Soldier active, Weapon weapon, float distance, Soldier passive, int wastedPoints)
        {
            if (active.ShootingTest(weapon, distance, wastedPoints))
            {
                passive.HitTest(weapon);
            }

            return true;
        }

        public static bool SupressingFire(Skirmish game, Soldier active, Weapon weapon, Area area, int wastedPoints)
        {
            active.SetState(SoldierStates.SupressingFire, weapon, area);

            return true;
        }

        public static bool Support(Skirmish game, Soldier active)
        {
            active.SupportTest();

            return true;
        }

        public static bool UseShootingItem(Skirmish game, Soldier active, Item item, int wastedPoints)
        {
            active.UseItemForShootingPhase(item, wastedPoints);

            return true;
        }

        public static bool FirstAid(Skirmish game, Soldier active, Soldier passive, int wastedPoints)
        {
            if (active.FirstAidTest(wastedPoints))
            {
                passive.HealingTest(active.CurrentFirstAidHability);
            }

            return true;
        }

        #endregion

        #region Melee

        private static IEnumerable<ActionSpecification> GetListForMelee()
        {
            return new[]
            {
                new ActionSpecification() { Action = Actions.LeaveCombat,      Name = "Leave Combat" },
                new ActionSpecification() { Action = Actions.UseMeleeItem,     Name = "Use Item",      ItemAction = true },
            };
        }

        public static bool Leave(Skirmish game, Soldier active)
        {
            if (active.LeaveMeleeTest())
            {
                Melee melee = game.GetMelee(active);

                melee.RemoveFighter(active);
            }

            return true;
        }

        public static bool UseMeleeItem(Skirmish game, Soldier active, Item item)
        {
            active.UseItemForMeleePhase(item);

            return true;
        }

        #endregion

        #region Morale

        private static IEnumerable<ActionSpecification> GetListForMorale()
        {
            return new[]
            {
                new ActionSpecification() { Action = Actions.TakeControl,     Name = "Take Control",  Automatic = true },
                new ActionSpecification() { Action = Actions.UseMoraleItem,   Name = "Use Item", },
            };
        }

        public static bool TakeControl(Skirmish game, Soldier active)
        {
            active.TakeControlTest();

            return true;
        }

        public static bool UseMoraleItem(Skirmish game, Soldier active, Item item)
        {
            active.UseItemForMoralePhase(item);

            return true;
        }

        #endregion
    }
}
