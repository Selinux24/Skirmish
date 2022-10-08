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
                return BaseMovingCapacity - GetModifiersMovingCapacity();
            }
        }
        public int CurrentActionPoints
        {
            get
            {
                return BaseActionPoints - GetModifiersActionPoints();
            }
        }
        public int CurrentMelee
        {
            get
            {
                return BaseMelee - GetModifiersMelee();
            }
        }
        public int CurrentSmallWeapons
        {
            get
            {
                return BaseSmallWeapons - GetModifiersSmallWeapons();
            }
        }
        public int CurrentBigWeapons
        {
            get
            {
                return BaseBigWeapons - GetModifiersBigWeapons();
            }
        }
        public int CurrentStrength
        {
            get
            {
                return BaseStrength - GetModifiersStrength();
            }
        }
        public int CurrentAgility
        {
            get
            {
                return BaseAgility - GetModifiersAgility();
            }
        }
        public int CurrentEndurance
        {
            get
            {
                return BaseEndurance - GetModifiersEndurance();
            }
        }
        public HealthStates CurrentHealth
        {
            get
            {
                int health = BaseHealth - GetModifiersHealth();

                if (health >= 50) return HealthStates.Healthy;
                else if (health >= 0) return HealthStates.Wounded;
                else return HealthStates.Disabled;
            }
        }
        public int CurrentInitiative
        {
            get
            {
                return BaseInitiative - GetModifiersInitiative();
            }
        }
        public MoraleStates CurrentMorale { get; private set; }
        public int CurrentFirstAidHability
        {
            get
            {
                return BaseHability - GetModifiersHability();
            }
        }

        public Weapon CurrentShootingWeapon { get; set; }
        public Weapon CurrentMeleeWeapon { get; set; }
        public Item CurrentItem { get; set; }

        public Area CurrentArea { get; set; }

        public bool IsLeader
        {
            get
            {
                return Team.Leader == this;
            }
        }
        public bool HasItemsForMovingPhase
        {
            get
            {
                if (CurrentItem != null)
                {
                    return CurrentItem.Class == ItemClasses.Movement;
                }

                return false;
            }
        }
        public bool HasItemsForShootingPhase
        {
            get
            {
                if (CurrentItem != null)
                {
                    return CurrentItem.Class == ItemClasses.Shooting;
                }

                return false;
            }
        }
        public bool HasItemsForMeleePhase
        {
            get
            {
                if (CurrentItem != null)
                {
                    return CurrentItem.Class == ItemClasses.Melee;
                }

                return false;
            }
        }
        public bool HasItemsForMoralePhase
        {
            get
            {
                if (CurrentItem != null)
                {
                    return CurrentItem.Class == ItemClasses.Morale;
                }

                return false;
            }
        }
        public bool IdleForMovement
        {
            get
            {
                return
                    CurrentHealth != HealthStates.Disabled &&
                    canMove &&
                    !onMelee &&
                    CurrentMovingCapacity > 0;
            }
        }
        public bool IdleForShooting
        {
            get
            {
                return
                    CurrentHealth != HealthStates.Disabled &&
                    canShoot &&
                    !onMelee &&
                    CurrentActionPoints > 0;
            }
        }
        public bool IdleForMelee
        {
            get
            {
                return
                    CurrentHealth != HealthStates.Disabled &&
                    canFight && onMelee;
            }
        }

        public Soldier(string name, SoldierClasses soldierClass, Team team)
        {
            Name = name;
            SoldierClass = soldierClass;
            Team = team;

            CurrentShootingWeapon = new Weapon("Gun") { Damage = 80, Penetration = 20, };
            CurrentMeleeWeapon = new Weapon("Sword") { Damage = 50, Penetration = 10, };
        }

        private int GetModifiersMovingCapacity()
        {
            return turnMovingCapacity;
        }
        private int GetModifiersActionPoints()
        {
            return turnActionPoints;
        }
        private int GetModifiersMelee()
        {
            return turnMelee;
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
            return turnStrength;
        }
        private int GetModifiersAgility()
        {
            return turnAgility;
        }
        private int GetModifiersEndurance()
        {
            return turnEndurance;
        }
        private int GetModifiersHealth()
        {
            return wounds;
        }
        private int GetModifiersInitiative()
        {
            return turnInitiative;
        }
        private int GetModifiersHability()
        {
            return turnHability;
        }

        public bool IdleForPhase(Phase phase)
        {
            if (phase == Phase.Movement) return IdleForMovement && canMove;
            else if (phase == Phase.Shooting) return IdleForShooting && canShoot;
            else if (phase == Phase.Melee) return canFight;
            else if (phase == Phase.Morale) return true;
            else return false;
        }
        public void NextTurn()
        {
            if (CurrentHealth == HealthStates.Disabled || CurrentMorale == MoraleStates.Demoralized)
            {
                canMove = false;
                canShoot = false;
                canFight = false;

                turnMovingCapacity = BaseMovingCapacity;
                turnActionPoints = BaseMovingCapacity;

                turnMelee = 0;
                turnSmallWeapons = 0;
                turnBigWeapons = 0;

                turnStrength = 0;
                turnAgility = 0;
                turnEndurance = 0;
                turnInitiative = 0;
                turnHability = 0;
            }
            else if (CurrentMorale == MoraleStates.Cowed)
            {
                canMove = !onMelee;
                canShoot = false;
                canFight = onMelee;

                turnMovingCapacity = onMelee ? BaseMovingCapacity : 0;
                turnActionPoints = onMelee ? BaseMovingCapacity : 0;

                turnMelee = BaseMelee / 2;
                turnSmallWeapons = BaseSmallWeapons / 2;
                turnBigWeapons = BaseBigWeapons / 2;

                turnStrength = 0;
                turnAgility = 0;
                turnEndurance = 0;
                turnInitiative = 0;
                turnHability = 0;
            }
            else
            {
                canMove = !onMelee;
                canShoot = !onMelee;
                canFight = onMelee;

                turnMovingCapacity = onMelee ? BaseMovingCapacity : 0;
                turnActionPoints = onMelee ? BaseMovingCapacity : 0;

                turnMelee = BaseMelee;
                turnSmallWeapons = BaseSmallWeapons;
                turnBigWeapons = BaseBigWeapons;

                turnStrength = 0;
                turnAgility = 0;
                turnEndurance = 0;
                turnInitiative = 0;
                turnHability = 0;
            }
        }

        public void Move(MovementModes mode, int points)
        {
            ConsumeMovingCapacity(points);

            switch (mode)
            {
                case MovementModes.Walk:
                case MovementModes.Crawl:
                    canMove = false;
                    canShoot = true;
                    canFight = false;
                    break;
                case MovementModes.Run:
                    //Running
                    canMove = false;
                    canShoot = false;
                    canFight = false;
                    break;
                case MovementModes.FindCover:
                case MovementModes.RunAway:
                    // Automatic movement to nearest cover
                    turnMovingCapacity = 0;

                    canMove = false;
                    canShoot = false;
                    canFight = false;
                    break;
                default:
                    break;
            }
        }
        public void Assault(int points)
        {
            ConsumeMovingCapacity(points);

            onMelee = true;

            canMove = false;
            canShoot = false;
            canFight = true;
        }
        public void ReloadTest(Weapon weapon, int points)
        {
            ConsumeMovingCapacity(points);

            canMove = false;
            canShoot = true;
            canFight = false;

            weapon.Reload();
        }
        public void RepairTest(Weapon weapon, int points)
        {
            ConsumeMovingCapacity(points);

            canMove = false;
            canShoot = true;
            canFight = false;

            weapon.Repair();
        }
        public void Inventory(int points)
        {
            ConsumeMovingCapacity(points);

            canMove = false;
            canShoot = true;
            canFight = false;
        }
        public void CommunicationsTest()
        {
            // Stationary when doing communications
            turnMovingCapacity = 0;

            canMove = false;
            canShoot = false;
            canFight = false;
        }

        public bool ShootingTest(Weapon weapon, float distanceToTarget, int points)
        {
            ConsumeActionPoints(points);

            if (weapon == null)
            {
                return false;
            }

            if (distanceToTarget > weapon.Range)
            {
                return false;
            }

            return (Helper.RandomGenerator.Next(0, 6) >= CurrentSmallWeapons);
        }
        public void SupportTest()
        {
            turnActionPoints = 0;
        }
        public bool FirstAidTest(int points)
        {
            ConsumeActionPoints(points);

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
                canFight = false;
                onMelee = false;

                return true;
            }

            return false;
        }

        public void TakeControlTest()
        {
            if (Helper.RandomGenerator.Next(0, 6) > 2)
            {
                canMove = true;
                canShoot = true;
                canFight = false;
            }
        }

        public void SetState(SoldierStates soldierStates, Weapon weapon, Area area)
        {
            if (soldierStates != SoldierStates.None)
            {
                turnMovingCapacity = 0;
                turnActionPoints = 0;
            }

            switch (weapon?.WeaponType)
            {
                case WeaponTypes.Ranged:
                    CurrentShootingWeapon = weapon;
                    break;
                case WeaponTypes.Melee:
                    CurrentMeleeWeapon = weapon;
                    break;
                default:
                    break;
            }

            CurrentArea = area;
        }

        public void UseItemForMovementPhase(Item item, int points)
        {
            item.Use();

            ConsumeMovingCapacity(points);
        }
        public void UseItemForShootingPhase(Item item, int points)
        {
            item.Use();

            ConsumeActionPoints(points);
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
            wounds += (Helper.RandomGenerator.Next(0, weapon.Damage) + weapon.Penetration);
        }
        public void HealingTest(int hability)
        {
            wounds -= (Helper.RandomGenerator.Next(-25, 25) + hability);
        }

        private void ConsumeMovingCapacity(int points)
        {
            turnMovingCapacity += points;

            if (turnMovingCapacity > BaseMovingCapacity) turnMovingCapacity = BaseMovingCapacity;
        }
        private void ConsumeActionPoints(int points)
        {
            turnActionPoints += points;

            if (turnActionPoints > BaseActionPoints) turnActionPoints = BaseActionPoints;
        }

        internal void MeleeDisolved()
        {
            onMelee = false;
        }

        public void AnimateHurt(Weapon weapon)
        {
            if (weapon == null)
            {
                return;
            }

            switch (weapon.WeaponType)
            {
                case WeaponTypes.Ranged:
                    // Ranged impact animation and sound
                    AnimateLib("RangedImpact");
                    break;
                case WeaponTypes.Melee:
                    // Melee impact animation and sound
                    AnimateLib("MeleeImpact");
                    break;
                default:
                    break;
            }
        }
        public void AnimateKill(Weapon weapon)
        {
            if (weapon == null)
            {
                return;
            }

            switch (weapon.WeaponType)
            {
                case WeaponTypes.Ranged:
                    // Ranged kill animation and sound
                    AnimateLib("MeleeKill");
                    break;
                case WeaponTypes.Melee:
                    // Melee kill animation and sound
                    AnimateLib("MeleeKill");
                    break;
                default:
                    break;
            }
        }
        private void AnimateLib(string animation)
        {
            Logger.WriteDebug(this, animation);
        }

        public override string ToString()
        {
            string melee = onMelee ? "Can't fight" : "Not on melee";

            return string.Format(
                "{0} [{1}][{2}] -> {3}, {4}, {5}",
                Name,
                CurrentHealth,
                CurrentMorale,
                IdleForMovement ? "Can move" : "Can't move",
                IdleForShooting ? "Can shoot" : "Can't shoot",
                IdleForMelee ? "Can fight" : melee);
        }
    }
}
