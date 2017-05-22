using Engine;
using SharpDX;

namespace TerrainTest.AI
{
    /// <summary>
    /// AI status
    /// </summary>
    public class AIStatus
    {
        /// <summary>
        /// Initial life
        /// </summary>
        private float initialLife;

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

        public float Damage
        {
            get
            {
                return 1f - (this.Life / this.initialLife);
            }
        }

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

        public void Update(GameTime gameTime)
        {
            this.PrimaryWeapon?.Update(gameTime);
            this.SecondaryWeapon?.Update(gameTime);
        }
    }
}
