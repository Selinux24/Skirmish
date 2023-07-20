using Engine;
using System.Collections.Generic;

namespace GameLogic.Rules
{
    using GameLogic.Rules.Enum;

    public class Melee
    {
        private readonly List<Soldier> soldiers = new();

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

                return factions.ToArray();
            }
        }
        public bool Done
        {
            get
            {
                return this.Factions.Length <= 1;
            }
        }

        public Melee()
        {

        }

        public bool ContainsSoldier(Soldier soldier)
        {
            if (this.soldiers.Contains(soldier))
            {
                return true;
            }

            return false;
        }
        public void AddFighter(Soldier soldier)
        {
            if (!this.soldiers.Contains(soldier))
            {
                this.soldiers.Add(soldier);
            }
        }
        public void RemoveFighter(Soldier soldier)
        {
            if (this.soldiers.Contains(soldier))
            {
                this.soldiers.Remove(soldier);
            }
        }

        public void Resolve()
        {
            for (int i = 1; i <= 10; i++)
            {
                Soldier[] iSoldiers = this.soldiers.FindAll(s => s.CurrentInitiative == i).ToArray();
                if (iSoldiers.Length > 0)
                {
                    foreach (Soldier s in iSoldiers)
                    {
                        var enemy = this.GetRandomEnemy(s);
                        if (enemy != null && s.FightingTest())
                        {
                            enemy.HitTest(s.CurrentMeleeWeapon);
                        }
                    }
                }
            }

            this.soldiers.ForEach((s) =>
            {
                if (s.CurrentHealth == HealthStates.Disabled) s.MeleeDisolved();
            });

            this.soldiers.RemoveAll(s => s.CurrentHealth == HealthStates.Disabled);
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
            Soldier[] enemyList = this.soldiers.FindAll(s => s.Team.Faction != soldier.Team.Faction).ToArray();
            if (enemyList.Length > 0)
            {
                return enemyList[Helper.RandomGenerator.Next(0, enemyList.Length - 1)];
            }

            return null;
        }
    }
}
