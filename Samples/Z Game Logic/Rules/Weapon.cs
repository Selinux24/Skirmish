
namespace GameLogic.Rules
{
    public class Weapon
    {
        public string Name { get; set; }
        public WeaponTypes WeaponType { get; set; }
        public int Range { get; set; }
        public int Damage { get; set; }
        public int Penetration { get; set; }

        public int MaxAttacks { get; set; }
        public int CurrentAttacks { get; set; }

        public int MaxStatePoints { get; set; }
        public int CurrentStatePoints { get; set; }

        public Weapon(string name)
        {
            this.Name = name;
        }

        public void Reload()
        {
            CurrentAttacks = MaxAttacks;
        }

        public void Repair()
        {
            CurrentStatePoints = MaxStatePoints;
        }
    }
}
