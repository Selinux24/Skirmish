using Engine;
using SharpDX;

namespace Terrain.AI
{
    /// <summary>
    /// Agent weapon
    /// </summary>
    public class Weapon
    {
        /// <summary>
        /// Last attack time
        /// </summary>
        private float lastAttackTime = 0;

        /// <summary>
        /// Name
        /// </summary>
        public string Name;
        /// <summary>
        /// Fire cadence
        /// </summary>
        public float Cadence;
        /// <summary>
        /// Damage
        /// </summary>
        public float Damage;
        /// <summary>
        /// Range
        /// </summary>
        public float Range;
        /// <summary>
        /// Gets wether the weapon can make an attack
        /// </summary>
        public bool CanShoot
        {
            get
            {
                return this.lastAttackTime > this.Cadence;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="desc">Description</param>
        public Weapon(WeaponDescription desc) : this(desc.Name, desc.Damage, desc.Range, desc.Cadence)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="damage">Damage</param>
        /// <param name="range">Range</param>
        /// <param name="cadence">Cadence</param>
        public Weapon(string name, float damage, float range, float cadence)
        {
            this.Name = name;
            this.Damage = damage;
            this.Range = range;
            this.Cadence = cadence;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            this.lastAttackTime += gameTime.ElapsedSeconds;
        }
        /// <summary>
        /// Performs an attack
        /// </summary>
        /// <param name="brain"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public float Shoot(Brain brain, AIAgent from, AIAgent to)
        {
            if (this.CanShoot)
            {
                if (from.EnemyOnSight(to))
                {
                    var fromPosition = from.Manipulator.Position;
                    var toPosition = to.Manipulator.Position;

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
        /// <summary>
        /// Adds delay to the next attack
        /// </summary>
        /// <param name="delay"></param>
        public void Delay(float delay)
        {
            this.lastAttackTime -= delay;
        }
    }
}
