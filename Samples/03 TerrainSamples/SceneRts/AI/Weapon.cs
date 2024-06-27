using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.AI
{
    /// <summary>
    /// Agent weapon
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="name">Name</param>
    /// <param name="damage">Damage</param>
    /// <param name="range">Range</param>
    /// <param name="cadence">Cadence</param>
    public class Weapon(string name, float damage, float range, float cadence)
    {
        /// <summary>
        /// Last attack time
        /// </summary>
        private float lastAttackTime = 0;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = name;
        /// <summary>
        /// Fire cadence
        /// </summary>
        public float Cadence { get; set; } = cadence;
        /// <summary>
        /// Damage
        /// </summary>
        public float Damage { get; set; } = damage;
        /// <summary>
        /// Range
        /// </summary>
        public float Range { get; set; } = range;
        /// <summary>
        /// Gets whether the weapon can make an attack
        /// </summary>
        public bool CanShoot
        {
            get
            {
                return lastAttackTime > Cadence;
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
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(IGameTime gameTime)
        {
            lastAttackTime += gameTime.ElapsedSeconds;
        }
        /// <summary>
        /// Performs an attack
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public float Shoot(AIAgent from, AIAgent to)
        {
            if (CanShoot && from.EnemyOnSight(to))
            {
                var fromPosition = from.Manipulator.Position;
                var toPosition = to.Manipulator.Position;

                var distance = Vector3.Distance(toPosition, fromPosition);
                if (distance <= Range)
                {
                    lastAttackTime = 0;
                    var dmg = Helper.RandomGenerator.NextFloat(0, Damage);
                    if (Helper.RandomGenerator.NextFloat(0, 1) > 0.9f) { dmg *= 2f; } //Critic

                    return dmg;
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
            lastAttackTime -= delay;
        }
    }
}
