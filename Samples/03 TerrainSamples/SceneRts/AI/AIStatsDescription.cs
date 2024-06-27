
namespace TerrainSamples.SceneRts.AI
{
    /// <summary>
    /// AI agent statistics description
    /// </summary>
    public class AIStatsDescription
    {
        /// <summary>
        /// Primary weapon
        /// </summary>
        public WeaponDescription PrimaryWeapon { get; set; }
        /// <summary>
        /// Secondary weapon
        /// </summary>
        public WeaponDescription SecondaryWeapon { get; set; }
        /// <summary>
        /// Life
        /// </summary>
        public float Life { get; set; }
        /// <summary>
        /// Distance of sight
        /// </summary>
        public float SightDistance { get; set; }
        /// <summary>
        /// Angle of sight in degrees
        /// </summary>
        public float SightAngle { get; set; }
    }
}
