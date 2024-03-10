using Engine;
using System.Collections.Generic;

namespace TerrainSamples.SceneGrid.Rules
{
    using TerrainSamples.SceneGrid.Rules.Enum;

    public class Melee
    {
        private readonly List<Soldier> soldiers = [];

        public string[] Factions
        {
            get
            {
                var factions = new List<string>();

                soldiers.ForEach(a =>
                {
                    if (!factions.Contains(a.Team.Faction))
                    {
                        factions.Add(a.Team.Faction);
                    }
                });

                return [.. factions];
            }
        }
        public bool Done
        {
            get
            {
                return Factions.Length <= 1;
            }
        }

        public Melee()
        {

        }

        public bool ContainsSoldier(Soldier soldier)
        {
            if (soldiers.Contains(soldier))
            {
                return true;
            }

            return false;
        }
        public void AddFighter(Soldier soldier)
        {
            if (!soldiers.Contains(soldier))
            {
                soldiers.Add(soldier);
            }
        }
        public void RemoveFighter(Soldier soldier)
        {
            soldiers.Remove(soldier);
        }

        public void Resolve()
        {
            for (int i = 1; i <= 10; i++)
            {
                var iSoldiers = soldiers.FindAll(s => s.CurrentInitiative == i);
                if (iSoldiers.Count > 0)
                {
                    foreach (Soldier s in iSoldiers)
                    {
                        var enemy = GetRandomEnemy(s);
                        if (enemy != null && s.FightingTest())
                        {
                            enemy.HitTest(s.CurrentMeleeWeapon);
                        }
                    }
                }
            }

            soldiers.ForEach((s) =>
            {
                if (s.CurrentHealth == HealthStates.Disabled) s.MeleeDisolved();
            });

            soldiers.RemoveAll(s => s.CurrentHealth == HealthStates.Disabled);
        }
        public void Disolve()
        {
            foreach (Soldier s in soldiers)
            {
                s.MeleeDisolved();
            }
        }

        private Soldier GetRandomEnemy(Soldier soldier)
        {
            var enemyList = soldiers.FindAll(s => s.Team.Faction != soldier.Team.Faction);
            if (enemyList.Count > 0)
            {
                return enemyList[Helper.RandomGenerator.Next(0, enemyList.Count - 1)];
            }

            return null;
        }
    }
}
