using Engine;
using SharpDX;

namespace TerrainTest.AI
{
    public class Weapon
    {
        private float lastAttackTime = 0;

        public string Name;
        public float Cadence;
        public float Damage;
        public float Range;

        public bool CanShoot
        {
            get
            {
                return this.lastAttackTime > this.Cadence;
            }
        }

        public Weapon(WeaponDescription desc) : this(desc.Name, desc.Damage, desc.Range, desc.Cadence)
        {

        }
        public Weapon(string name, float damage, float range, float cadence)
        {
            this.Name = name;
            this.Damage = damage;
            this.Range = range;
            this.Cadence = cadence;
        }

        public void Update(GameTime gameTime)
        {
            this.lastAttackTime += gameTime.ElapsedSeconds;
        }

        public float Shoot(Brain brain)
        {
            if (this.CanShoot)
            {
                this.lastAttackTime = 0;
                var d = brain.RandomGenerator.NextFloat(0, this.Damage);
                if (brain.RandomGenerator.NextFloat(0, 1) > 0.9f) { d *= 2f; } //Critic

                return d;
            }

            return 0;
        }

        public void Delay(float delay)
        {
            this.lastAttackTime -= delay;
        }
    }
}
