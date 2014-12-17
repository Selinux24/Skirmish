using System.Collections.Generic;
using System;

namespace GameLogic.Rules
{
    public class Melee
    {
        private List<Soldier> soldiers = new List<Soldier>();

        public string[] Factions
        {
            get
            {
                List<string> factions = new List<string>();

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
                Soldier[] iSoldiers = this.soldiers.FindAll(s => s.Initiative == i).ToArray();
                if (iSoldiers.Length > 0)
                {
                    foreach (Soldier s in iSoldiers)
                    {
                        Soldier enemy = this.GetRandomEnemy(s);

                        enemy.FightTest(s);
                    }
                }
            }

            this.soldiers.RemoveAll(s => s.Health == HealthStates.Disabled);
        }

        private Soldier GetRandomEnemy(Soldier soldier)
        {
            Soldier[] enemyList = this.soldiers.FindAll(s => s.Team.Faction != soldier.Team.Faction).ToArray();

            return enemyList[0];
        }
    }
}
