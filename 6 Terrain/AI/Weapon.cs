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

        public float Shoot(Brain brain, AIAgent from, AIAgent to)
        {
            if (this.CanShoot)
            {
                if (from.OnSight(to))
                {
                    var fromPosition = from.Model.Manipulator.Position;
                    var toPosition = to.Model.Manipulator.Position;

                    var distance = Vector3.Distance(toPosition, fromPosition);
                    if (distance <= this.Range)
                    {
                        //TODO: Ray picking

                        this.lastAttackTime = 0;
                        var damage = Helper.RandomGenerator.NextFloat(0, this.Damage);
                        if (Helper.RandomGenerator.NextFloat(0, 1) > 0.9f) { damage *= 2f; } //Critic

                        return damage;
                    }
                }
            }

            return 0;
        }

        public void Delay(float delay)
        {
            this.lastAttackTime -= delay;
        }
    }
}
