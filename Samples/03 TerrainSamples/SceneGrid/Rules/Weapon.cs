
namespace TerrainSamples.SceneGrid.Rules
{
    public class Weapon(string name)
    {
        public string Name { get; set; } = name;
        public WeaponTypes WeaponType { get; set; }
        public int Range { get; set; }
        public int Damage { get; set; }
        public int Penetration { get; set; }

        public int MaxAttacks { get; set; }
        public int CurrentAttacks { get; set; }

        public int MaxStatePoints { get; set; }
        public int CurrentStatePoints { get; set; }

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
