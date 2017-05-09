using Engine;
using SharpDX;

namespace TerrainTest.AI
{
    class Weapon
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

        public float Shoot()
        {
            this.lastAttackTime = 0;
            var d = AgentManager.rnd.NextFloat(0, this.Damage);
            if (AgentManager.rnd.NextFloat(0, 1) > 0.9f) { d *= 2f; } //Critic

            return d;
        }

        public void Delay(float delay)
        {
            this.lastAttackTime -= delay;
        }
    }
}
