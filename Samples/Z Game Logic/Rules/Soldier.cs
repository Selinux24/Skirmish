using Engine;

namespace GameLogic.Rules
{
    using GameLogic.Rules.Enum;

    public class Soldier
    {
        public string Name { get; set; }
        public SoldierClasses SoldierClass { get; set; }
        public Team Team { get; set; }

        public readonly int BaseMovingCapacity = 100;
        public readonly int BaseActionPoints = 100;
        public readonly int BaseMelee = 10;
        public readonly int BaseSmallWeapons = 10;
        public readonly int BaseBigWeapons = 10;
        public readonly int BaseStrength = 10;
        public readonly int BaseAgility = 10;
        public readonly int BaseEndurance = 10;
        public readonly int BaseHealth = 100;
        public readonly int BaseInitiative = 1;
        public readonly int BaseHability = 5;

        private bool canMove = true;
        private int turnMovingCapacity = 0;
        private bool canShoot = true;
        private int turnActionPoints = 0;
        private bool onMelee = false;
        private bool canFight = false;
        private int turnMelee = 0;
        private int turnSmallWeapons = 0;
        private int turnBigWeapons = 0;
        private int turnStrength = 0;
        private int turnAgility = 0;
        private int turnEndurance = 0;
        private int wounds = 0;
        private int turnInitiative = 0;
        private int turnHability = 0;

        public int CurrentMovingCapacity
        {
            get
            {
                return this.BaseMovingCapacity - this.GetModifiersMovingCapacity();
            }
        }
        public int CurrentActionPoints
        {
            get
            {
                return this.BaseActionPoints - this.GetModifiersActionPoints();
            }
        }
        public int CurrentMelee
        {
            get
            {
                return this.BaseMelee - this.GetModifiersMelee();
            }
        }
        public int CurrentSmallWeapons
        {
            get
            {
                return this.BaseSmallWeapons - this.GetModifiersSmallWeapons();
            }
        }
        public int CurrentBigWeapons
        {
            get
            {
                return this.BaseBigWeapons - this.GetModifiersBigWeapons();
            }
        }
        public int CurrentStrength
        {
            get
            {
                return this.BaseStrength - this.GetModifiersStrength();
            }
        }
        public int CurrentAgility
        {
            get
            {
                return this.BaseAgility - this.GetModifiersAgility();
            }
        }
        public int CurrentEndurance
        {
            get
            {
                return this.BaseEndurance - this.GetModifiersEndurance();
            }
        }
        public HealthStates CurrentHealth
        {
            get
            {
                int health = this.BaseHealth - this.GetModifiersHealth();

                if (health >= 50) return HealthStates.Healthy;
                else if (health >= 0) return HealthStates.Wounded;
                else return HealthStates.Disabled;
            }
        }
        public int CurrentInitiative
        {
            get
            {
                return this.BaseInitiative - this.GetModifiersInitiative();
            }
        }
        public MoraleStates CurrentMorale { get; private set; }
        public int CurrentFirstAidHability
        {
            get
            {
                return this.BaseHability - this.GetModifiersHability();
            }
        }

        public Weapon CurrentShootingWeapon { get; set; }
        public Weapon CurrentMeleeWeapon { get; set; }
        public Item CurrentItem { get; set; }

        public bool IsLeader
        {
            get
            {
                return this.Team.Leader == this;
            }
        }
        public bool HasItemsForMovingPhase
        {
            get
            {
                if (this.CurrentItem != null)
                {
                    return this.CurrentItem.Class == ItemClasses.Movement;
                }

                return false;
            }
        }
        public bool HasItemsForShootingPhase
        {
            get
            {
                if (this.CurrentItem != null)
                {
                    return this.CurrentItem.Class == ItemClasses.Shooting;
                }

                return false;
            }
        }
        public bool HasItemsForMeleePhase
        {
            get
            {
                if (this.CurrentItem != null)
                {
                    return this.CurrentItem.Class == ItemClasses.Melee;
                }

                return false;
            }
        }
        public bool HasItemsForMoralePhase
        {
            get
            {
                if (this.CurrentItem != null)
                {
                    return this.CurrentItem.Class == ItemClasses.Morale;
                }

                return false;
            }
        }
        public bool IdleForMovement
        {
            get
            {
                return
                    this.CurrentHealth != HealthStates.Disabled &&
                    this.canMove &&
                    !this.onMelee &&
                    this.CurrentMovingCapacity > 0;
            }
        }
        public bool IdleForShooting
        {
            get
            {
                return
                    this.CurrentHealth != HealthStates.Disabled &&
                    this.canShoot &&
                    !this.onMelee &&
                    this.CurrentActionPoints > 0;
            }
        }
        public bool IdleForMelee
        {
            get
            {
                return
                    this.CurrentHealth != HealthStates.Disabled &&
                    this.canFight && this.onMelee;
            }
        }

        public Soldier(string name, SoldierClasses soldierClass, Team team)
        {
            this.Name = name;
            this.SoldierClass = soldierClass;
            this.Team = team;

            this.CurrentShootingWeapon = new Weapon("Gun") { Damage = 80, Penetration = 20, };
            this.CurrentMeleeWeapon = new Weapon("Sword") { Damage = 50, Penetration = 10, };
        }

        private int GetModifiersMovingCapacity()
        {
            return this.turnMovingCapacity;
        }
        private int GetModifiersActionPoints()
        {
            return this.turnActionPoints;
        }
        private int GetModifiersMelee()
        {
            return this.turnMelee;
        }
        private int GetModifiersSmallWeapons()
        {
            return turnSmallWeapons;
        }
        private int GetModifiersBigWeapons()
        {
            return turnBigWeapons;
        }
        private int GetModifiersStrength()
        {
            return this.turnStrength;
        }
        private int GetModifiersAgility()
        {
            return this.turnAgility;
        }
        private int GetModifiersEndurance()
        {
            return this.turnEndurance;
        }
        private int GetModifiersHealth()
        {
            return this.wounds;
        }
        private int GetModifiersInitiative()
        {
            return this.turnInitiative;
        }
        private int GetModifiersHability()
        {
            return this.turnHability;
        }

        public bool IdleForPhase(Phase phase)
        {
            if (phase == Phase.Movement) return this.IdleForMovement && this.canMove;
            else if (phase == Phase.Shooting) return this.IdleForShooting && this.canShoot;
            else if (phase == Phase.Melee) return this.canFight;
            else if (phase == Phase.Morale) return true;
            else return false;
        }
        public void NextTurn()
        {
            if (this.CurrentHealth == HealthStates.Disabled || this.CurrentMorale == MoraleStates.Demoralized)
            {
                this.canMove = false;
                this.canShoot = false;
                this.canFight = false;

                this.turnMovingCapacity = this.BaseMovingCapacity;
                this.turnActionPoints = this.BaseMovingCapacity;

                this.turnMelee = 0;
                this.turnSmallWeapons = 0;
                this.turnBigWeapons = 0;

                this.turnStrength = 0;
                this.turnAgility = 0;
                this.turnEndurance = 0;
                this.turnInitiative = 0;
                this.turnHability = 0;
            }
            else if (this.CurrentMorale == MoraleStates.Cowed)
            {
                this.canMove = !this.onMelee;
                this.canShoot = false;
                this.canFight = this.onMelee;

                this.turnMovingCapacity = this.onMelee ? this.BaseMovingCapacity : 0;
                this.turnActionPoints = this.onMelee ? this.BaseMovingCapacity : 0;

                this.turnMelee = this.BaseMelee / 2;
                this.turnSmallWeapons = this.BaseSmallWeapons / 2;
                this.turnBigWeapons = this.BaseBigWeapons / 2;

                this.turnStrength = 0;
                this.turnAgility = 0;
                this.turnEndurance = 0;
                this.turnInitiative = 0;
                this.turnHability = 0;
            }
            else
            {
                this.canMove = !this.onMelee;
                this.canShoot = !this.onMelee;
                this.canFight = this.onMelee;

                this.turnMovingCapacity = this.onMelee ? this.BaseMovingCapacity : 0;
                this.turnActionPoints = this.onMelee ? this.BaseMovingCapacity : 0;

                this.turnMelee = this.BaseMelee;
                this.turnSmallWeapons = this.BaseSmallWeapons;
                this.turnBigWeapons = this.BaseBigWeapons;

                this.turnStrength = 0;
                this.turnAgility = 0;
                this.turnEndurance = 0;
                this.turnInitiative = 0;
                this.turnHability = 0;
            }
        }

        public void Move(int points)
        {
            this.ConsumeMovingCapacity(points);

            //Normal move
            this.canMove = false;
            this.canShoot = true;
            this.canFight = false;
        }
        public void Run(int points)
        {
            this.ConsumeMovingCapacity(points);

            //Running
            this.canMove = false;
            this.canShoot = false;
            this.canFight = false;
        }
        public void Crawl(int points)
        {
            this.ConsumeMovingCapacity(points);

            //Crawling move
            this.canMove = false;
            this.canShoot = true;
            this.canFight = false;
        }
        public void Assault(int points)
        {
            this.ConsumeMovingCapacity(points);

            this.onMelee = true;

            this.canMove = false;
            this.canShoot = false;
            this.canFight = true;
        }
        public void ReloadTest(Weapon weapon, int points)
        {
            this.ConsumeMovingCapacity(points);

            this.canMove = false;
            this.canShoot = true;
            this.canFight = false;

            weapon.Reload();
        }
        public void RepairTest(Weapon weapon, int points)
        {
            this.ConsumeMovingCapacity(points);

            this.canMove = false;
            this.canShoot = true;
            this.canFight = false;

            weapon.Repair();
        }
        public void Inventory(int points)
        {
            this.ConsumeMovingCapacity(points);

            this.canMove = false;
            this.canShoot = true;
            this.canFight = false;
        }
        public void CommunicationsTest()
        {
            // Stationary when doing communications
            this.turnMovingCapacity = 0;

            this.canMove = false;
            this.canShoot = false;
            this.canFight = false;
        }
        public void FindCover()
        {
            // Automatic movement to nearest cover
            this.turnMovingCapacity = 0;

            this.canMove = false;
            this.canShoot = false;
            this.canFight = false;
        }
        public void RunAway()
        {
            // Automatic movement to nearest scenery exit
            this.turnMovingCapacity = 0;

            this.canMove = false;
            this.canShoot = false;
            this.canFight = false;
        }

        public bool ShootingTest(Weapon weapon, float distance, int points)
        {
            this.ConsumeActionPoints(points);

            return (Helper.RandomGenerator.Next(0, 6) >= 4);
        }
        public void SupportTest()
        {
            this.turnActionPoints = 0;
        }
        public bool FirstAidTest(int points)
        {
            this.ConsumeActionPoints(points);

            return (Helper.RandomGenerator.Next(0, 6) > 5);
        }

        public bool FightingTest()
        {
            return (Helper.RandomGenerator.Next(0, 6) >= 4);
        }
        public bool LeaveMeleeTest()
        {
            if (Helper.RandomGenerator.Next(0, 6) > 4)
            {
                this.canFight = false;
                this.onMelee = false;

                return true;
            }

            return false;
        }

        public void TakeControlTest()
        {
            if (Helper.RandomGenerator.Next(0, 6) > 2)
            {
                this.canMove = true;
                this.canShoot = true;
                this.canFight = false;
            }
        }

        public void SetState(SoldierStates soldierStates, Weapon weapon, Area area)
        {
            if (soldierStates != SoldierStates.None)
            {
                this.turnMovingCapacity = 0;
                this.turnActionPoints = 0;
            }
        }

        public void UseItemForMovementPhase(Item item, int points)
        {
            item.Use();

            this.ConsumeMovingCapacity(points);
        }
        public void UseItemForShootingPhase(Item item, int points)
        {
            item.Use();

            this.ConsumeActionPoints(points);
        }
        public void UseItemForMeleePhase(Item item)
        {
            item.Use();
        }
        public void UseItemForMoralePhase(Item item)
        {
            item.Use();
        }

        public void HitTest(Weapon weapon)
        {
            this.wounds += (Helper.RandomGenerator.Next(0, weapon.Damage) + weapon.Penetration);
        }
        public void HealingTest(int hability)
        {
            this.wounds -= (Helper.RandomGenerator.Next(-25, 25) + hability);
        }

        private void ConsumeMovingCapacity(int points)
        {
            this.turnMovingCapacity += points;

            if (this.turnMovingCapacity > this.BaseMovingCapacity) this.turnMovingCapacity = this.BaseMovingCapacity;
        }
        private void ConsumeActionPoints(int points)
        {
            this.turnActionPoints += points;

            if (this.turnActionPoints > this.BaseActionPoints) this.turnActionPoints = this.BaseActionPoints;
        }

        internal void MeleeDisolved()
        {
            this.onMelee = false;
        }

        public void AnimateHurt(Weapon weapon)
        {
            // TODO: hurt animation
        }
        public void AnimateKill(Weapon weapon)
        {
            // TODO: kill animation
        }

        public override string ToString()
        {
            string melee = this.onMelee ? "Can't fight" : "Not on melee";

            return string.Format(
                "{0} [{1}][{2}] -> {3}, {4}, {5}",
                this.Name,
                this.CurrentHealth,
                this.CurrentMorale,
                this.IdleForMovement ? "Can move" : "Can't move",
                this.IdleForShooting ? "Can shoot" : "Can't shoot",
                this.IdleForMelee ? "Can fight" : melee);
        }
    }
}
