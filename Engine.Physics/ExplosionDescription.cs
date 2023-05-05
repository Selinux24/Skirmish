
namespace Engine.Physics
{
    /// <summary>
    /// Explosion description
    /// </summary>
    public struct ExplosionDescription
    {
        /// <summary>
        /// Radius in which the objects are not attracted in the implosion, because they are trapped in the epicenter
        /// </summary>
        public float ImplosionMinRadius { get; set; }
        /// <summary>
        /// Radius at which objects are attracted in the first phase of the explosion.
        /// </summary>
        public float ImplosionMaxRadius { get; set; }
        /// <summary>
        /// Implosion Phase Duration
        /// </summary>
        public float ImplosionDuration { get; set; }
        /// <summary>
        /// The maximum force that is applied in the implosion
        /// </summary>
        public float ImplosionForce { get; set; }

        /// <summary>
        /// Speed at which the shock wave travels
        /// </summary>
        /// <remarks>
        /// The width of the shock wave must be greater than or equal to the maximum distance it can reach. Width >= Speed * Duration
        /// </remarks>
        public float ShockwaveSpeed { get; set; }
        /// <summary>
        /// Shock wave width
        /// </summary>
        /// <remarks>
        /// The faster the wave, the wider the wave should be
        /// </remarks>
        public float ShockwaveThickness { get; set; }
        /// <summary>
        /// Attenuation when the shock wave overtakes an object
        /// </summary>
        public float ShockwaveOvertakenAttenuation { get; set; }
        /// <summary>
        /// Force applied at the center of the blast wave on a stationary object.
        /// </summary>
        /// <remarks>
        /// Objects in front of or behind the wave gain proportionally less force.
        /// </remarks>
        public float PeakConcussionForce { get; set; }
        /// <summary>
        /// The duration of the shock wave phase
        /// </summary>
        /// <remarks>
        /// The closer to the end, the less powerful the wave.
        /// </remarks>
        public float ConcussionDuration { get; set; }

        /// <summary>
        /// Creates an explosion at the specified position
        /// </summary>
        /// <returns>Gets the explosion force generator</returns>
        public static ExplosionDescription CreateExplosion()
        {
            return new ExplosionDescription
            {
                ImplosionMinRadius = 1f,
                ImplosionMaxRadius = 3f,
                ImplosionDuration = 0.2f,
                ImplosionForce = 1000f,

                ShockwaveSpeed = 50f,
                ShockwaveThickness = 2f,
                ShockwaveOvertakenAttenuation = 0.5f,
                PeakConcussionForce = 100000f,
                ConcussionDuration = 0.5f
            };
        }
        /// <summary>
        /// Creates a big explosion at the specified position
        /// </summary>
        /// <returns>Gets the explosion force generator</returns>
        public static ExplosionDescription CreateBigExplosion()
        {
            return new ExplosionDescription
            {
                ImplosionMinRadius = 5f,
                ImplosionMaxRadius = 300f,
                ImplosionDuration = 2f,
                ImplosionForce = 10000f,

                ShockwaveSpeed = 50f,
                ShockwaveThickness = 5f,
                ShockwaveOvertakenAttenuation = 0.5f,
                PeakConcussionForce = 100000f,
                ConcussionDuration = 1f
            };
        }
    }
}
