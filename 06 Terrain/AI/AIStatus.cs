using Engine;
using SharpDX;

namespace Terrain.AI
{
    /// <summary>
    /// AI status
    /// </summary>
    public class AIStatus
    {
        /// <summary>
        /// Initial life
        /// </summary>
        private readonly float initialLife;

        /// <summary>
        /// Primary weapon
        /// </summary>
        public Weapon PrimaryWeapon;
        /// <summary>
        /// Secondary weapon
        /// </summary>
        public Weapon SecondaryWeapon;
        /// <summary>
        /// Current weapon
        /// </summary>
        public Weapon CurrentWeapon;
        /// <summary>
        /// Current life
        /// </summary>
        public float Life;
        /// <summary>
        /// Distance of sight
        /// </summary>
        public float SightDistance;
        /// <summary>
        /// Angle of sight in radians
        /// </summary>
        public float SightAngle;
        /// <summary>
        /// Current damage
        /// </summary>
        public float Damage
        {
            get
            {
                return 1f - (this.Life / this.initialLife);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">Description</param>
        public AIStatus(AIStatusDescription description)
        {
            this.PrimaryWeapon = new Weapon(description.PrimaryWeapon);
            this.SecondaryWeapon = new Weapon(description.SecondaryWeapon);
            this.initialLife = description.Life;
            this.Life = description.Life;
            this.SightDistance = description.SightDistance;
            this.SightAngle = MathUtil.DegreesToRadians(description.SightAngle);

            this.CurrentWeapon = this.PrimaryWeapon;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            this.PrimaryWeapon?.Update(gameTime);
            this.SecondaryWeapon?.Update(gameTime);
        }
    }
}
