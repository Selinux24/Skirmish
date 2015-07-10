
namespace GameLogic.Rules
{
    public class Weapon
    {
        public string Name { get; set; }
        public int Damage { get; set; }
        public int Penetration { get; set; }

        public Weapon(string name)
        {
            this.Name = name;
        }

        public void Reload()
        {
            //TODO: Reload
        }

        public void Repair()
        {
            //TODO: Repair
        }
    }
}
