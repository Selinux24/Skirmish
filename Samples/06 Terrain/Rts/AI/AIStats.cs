using Engine;
using SharpDX;

namespace Terrain.Rts.AI
{
    /// <summary>
    /// AI agent statistics
    /// </summary>
    public class AIStats
    {
        /// <summary>
        /// Initial life
        /// </summary>
        private readonly float initialLife;

        /// <summary>
        /// Primary weapon
        /// </summary>
        public Weapon PrimaryWeapon { get; set; }
        /// <summary>
        /// Secondary weapon
        /// </summary>
        public Weapon SecondaryWeapon { get; set; }
        /// <summary>
        /// Current weapon
        /// </summary>
        public Weapon CurrentWeapon { get; set; }
        /// <summary>
        /// Current life
        /// </summary>
        public float Life { get; set; }
        /// <summary>
        /// Distance of sight
        /// </summary>
        public float SightDistance { get; set; }
        /// <summary>
        /// Angle of sight in radians
        /// </summary>
        public float SightAngle { get; set; }
        /// <summary>
        /// Current damage
        /// </summary>
        public float Damage
        {
            get
            {
                return 1f - (Life / initialLife);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">Description</param>
        public AIStats(AIStatsDescription description)
        {
            PrimaryWeapon = new Weapon(description.PrimaryWeapon);
            SecondaryWeapon = new Weapon(description.SecondaryWeapon);
            initialLife = description.Life;
            Life = description.Life;
            SightDistance = description.SightDistance;
            SightAngle = MathUtil.DegreesToRadians(description.SightAngle);

            CurrentWeapon = PrimaryWeapon;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            PrimaryWeapon?.Update(gameTime);
            SecondaryWeapon?.Update(gameTime);
        }
    }
}
