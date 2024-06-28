using Engine;
using System.Collections.Generic;
using System.Linq;

namespace TerrainSamples.SceneGrid.Rules
{
    using TerrainSamples.SceneGrid.Rules.Enum;

    public static class ActionsManager
    {
        private const string MovementActionMoveString = "Move";
        private const string MovementActionRunString = "Run";
        private const string MovementActionCrawlString = "Crawl";
        private const string MovementActionAssaultString = "Assault";
        private const string MovementActionCoveringFireString = "Covering Fire";
        private const string MovementActionReloadString = "Reload";
        private const string MovementActionRepairString = "Repair";
        private const string MovementActionInventoryString = "Inventory";
        private const string MovementActionUseItemString = "Use Item";
        private const string MovementActionCommunicationsString = "Communications";
        private const string MovementActionFindCoverString = "Find Cover";
        private const string MovementActionRunAwayString = "Run Away";

        private const string ShootingActionShootString = "Shoot";
        private const string ShootingActionSupressingFireString = "Supressing Fire";
        private const string ShootingActionSupportString = "Support";
        private const string ShootingActionUseItemString = "Use Shooting Item";
        private const string ShootingActionFirstAidString = "First Aid";

        private const string MeleeActionLeaveCombatString = "Leave Combat";
        private const string MeleeActionUseItemString = "Use Melee Item";

        private const string MoraleActionTakeControlString = "Take Control";
        private const string MoraleActionUseItemString = "Use Morale Item";

        public static ActionSpecification[] GetActions(Phase phase, Team team, Soldier soldier, bool onMelee, ActionTypes actionType = ActionTypes.All)
        {
            var teamActions = GetListForTeam(team);

            teamActions = FilterTeamActions(teamActions, phase, actionType);

            IEnumerable<ActionSpecification> soldierActions = [];

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

            List<ActionSpecification> actions = [];

            if (teamActions.Length != 0) actions.AddRange(teamActions);
            if (soldierActions.Any()) actions.AddRange(soldierActions);

            return [.. actions];
        }

        private static ActionSpecification[] FilterTeamActions(IEnumerable<ActionSpecification> actions, Phase phase, ActionTypes actionType)
        {
            if (phase != Phase.End && actionType.HasFlag(ActionTypes.Automatic))
            {
                return actions.Where(a => a.Automatic).ToArray();
            }
            else if (phase != Phase.End && actionType.HasFlag(ActionTypes.Manual))
            {
                return actions.Where(a => !a.Automatic).ToArray();
            }

            return [.. actions];
        }

        private static ActionSpecification[] FilterSoldierActions(IEnumerable<ActionSpecification> actions, Phase phase, Soldier soldier, bool onMelee, ActionTypes actionType)
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

        private static ActionSpecification[] GetListForTeam(Team team)
        {
            Logger.WriteDebug(nameof(ActionsManager), $"{team?.Name}");

            return [];
        }

        #endregion

        #region Movement

        private static ActionSpecification[] GetListForMovement()
        {
            return
            [
                new () { Action = Actions.Move,              Name = MovementActionMoveString               },
                new () { Action = Actions.Run,               Name = MovementActionRunString                },
                new () { Action = Actions.Crawl,             Name = MovementActionCrawlString              },
                new () { Action = Actions.Assault,           Name = MovementActionAssaultString            },
                new () { Action = Actions.CoveringFire,      Name = MovementActionCoveringFireString       },
                new () { Action = Actions.Reload,            Name = MovementActionReloadString             },
                new () { Action = Actions.Repair,            Name = MovementActionRepairString             },
                new () { Action = Actions.Inventory,         Name = MovementActionInventoryString          },
                new () { Action = Actions.UseMovementItem,   Name = MovementActionUseItemString,           ItemAction = true },
                new () { Action = Actions.Communications,    Name = MovementActionCommunicationsString,    LeadersOnly = true },
                new () { Action = Actions.FindCover,         Name = MovementActionFindCoverString,         Automatic = true },
                new () { Action = Actions.RunAway,           Name = MovementActionRunAwayString,           Automatic = true },
            ];
        }

        public static bool Move(Soldier active, int wastedPoints)
        {
            if (active.IdleForMovement)
            {
                active.Move(MovementModes.Walk, wastedPoints);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Run(Soldier active, int wastedPoints)
        {
            if (active.IdleForMovement)
            {
                active.Move(MovementModes.Run, wastedPoints);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Crawl(Soldier active, int wastedPoints)
        {
            if (active.IdleForMovement)
            {
                active.Move(MovementModes.Crawl, wastedPoints);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Assault(Soldier active, Soldier passive, int wastedPoints)
        {
            // This test must be repeated many times
            if (passive.CurrentHealth != HealthStates.Disabled)
            {
                active.Assault(wastedPoints);
                passive.Assault(0);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CoveringFire(Soldier active, Weapon weapon, Area area, int wastedPoints)
        {
            if (wastedPoints <= 0)
            {
                return false;
            }

            active.SetState(SoldierStates.CoveringFire, weapon, area);

            return true;
        }

        public static bool Reload(Soldier active, Weapon weapon, int wastedPoints)
        {
            active.ReloadTest(weapon, wastedPoints);

            return true;
        }

        public static bool Repair(Soldier active, Weapon weapon, int wastedPoints)
        {
            active.RepairTest(weapon, wastedPoints);

            return true;
        }

        public static bool Inventory(Soldier active, int wastedPoints)
        {
            active.Inventory(wastedPoints);

            return true;
        }

        public static bool UseMovementItem(Soldier active, Item item, int wastedPoints)
        {
            active.UseItemForMovementPhase(item, wastedPoints);

            return true;
        }

        public static bool Communications(Soldier active)
        {
            active.CommunicationsTest();

            return true;
        }

        public static bool FindCover(Soldier active)
        {
            active.Move(MovementModes.FindCover, 0);

            return true;
        }

        public static bool RunAway(Soldier active)
        {
            active.Move(MovementModes.RunAway, 0);

            return true;
        }

        #endregion

        #region Shooting

        private static ActionSpecification[] GetListForShooting()
        {
            return
            [
                new () { Action = Actions.Shoot,             Name = ShootingActionShootString,            },
                new () { Action = Actions.SupressingFire,    Name = ShootingActionSupressingFireString,   },
                new () { Action = Actions.Support,           Name = ShootingActionSupportString,          NeedsCommunicator = true, },
                new () { Action = Actions.UseShootingItem,   Name = ShootingActionUseItemString,          ItemAction = true },
                new () { Action = Actions.FirstAid,          Name = ShootingActionFirstAidString,         Classes = SoldierClasses.Medic },
            ];
        }

        public static bool Shoot(Soldier active, Weapon weapon, float distance, Soldier passive, int wastedPoints)
        {
            if (active.ShootingTest(weapon, distance, wastedPoints))
            {
                passive.HitTest(weapon);
            }

            return true;
        }

        public static bool SupressingFire(Soldier active, Weapon weapon, Area area, int wastedPoints)
        {
            if (wastedPoints <= 0)
            {
                return false;
            }

            active.SetState(SoldierStates.SupressingFire, weapon, area);

            return true;
        }

        public static bool Support(Soldier active)
        {
            active.SupportTest();

            return true;
        }

        public static bool UseShootingItem(Soldier active, Item item, int wastedPoints)
        {
            active.UseItemForShootingPhase(item, wastedPoints);

            return true;
        }

        public static bool FirstAid(Soldier active, Soldier passive, int wastedPoints)
        {
            if (active.FirstAidTest(wastedPoints))
            {
                passive.HealingTest(active.CurrentFirstAidHability);
            }

            return true;
        }

        #endregion

        #region Melee

        private static ActionSpecification[] GetListForMelee()
        {
            return
            [
                new () { Action = Actions.LeaveCombat,      Name = MeleeActionLeaveCombatString,  },
                new () { Action = Actions.UseMeleeItem,     Name = MeleeActionUseItemString,      ItemAction = true },
            ];
        }

        public static bool Leave(Soldier active)
        {
            return active.LeaveMeleeTest();
        }

        public static bool UseMeleeItem(Soldier active, Item item)
        {
            active.UseItemForMeleePhase(item);

            return true;
        }

        #endregion

        #region Morale

        private static ActionSpecification[] GetListForMorale()
        {
            return
            [
                new () { Action = Actions.TakeControl,     Name = MoraleActionTakeControlString,  Automatic = true },
                new () { Action = Actions.UseMoraleItem,   Name = MoraleActionUseItemString,      },
            ];
        }

        public static bool TakeControl(Soldier active)
        {
            active.TakeControlTest();

            return true;
        }

        public static bool UseMoraleItem(Soldier active, Item item)
        {
            active.UseItemForMoralePhase(item);

            return true;
        }

        #endregion
    }
}
