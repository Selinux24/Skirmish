
namespace Engine.Physics
{
    /// <summary>
    /// Contact resolver settings
    /// </summary>
    public struct ContactResolverSettings
    {
        /// <summary>
        /// Default settings
        /// </summary>
        public static ContactResolverSettings Default
        {
            get
            {
                return new ContactResolverSettings();
            }
        }

        /// <summary>
        /// Maximum contacts
        /// </summary>
        public int MaxContacts { get; set; } = 256;
        /// <summary>
        /// Maximum number of velocity iterations
        /// </summary>
        public int VelocityIterations { get; set; } = 2048;
        /// <summary>
        /// Maximum number of position iterations
        /// </summary>
        public int PositionIterations { get; set; } = 2048;
        /// <summary>
        /// To avoid instability, speeds less than this value are considered 0
        /// </summary>
        /// <remarks>The higher this number, the more the interpenetration will be visually noticeable.</remarks>
        public float VelocityEpsilon { get; set; } = 0.01f;
        /// <summary>
        /// To avoid instability, penetrations less than this value are considered as no interpenetrations.
        /// </summary>
        /// <remarks>The higher this number, the more the interpenetration will be visually noticeable.</remarks>
        public float PositionEpsilon { get; set; } = 0.01f;
        /// <summary> 
        /// Friction factor to add in all collisions
        /// </summary>
        public float Friction { get; set; } = 0.9f;
        /// <summary> 
        /// Restitution factor to add on all collisions
        /// </summary>
        public float Restitution { get; set; } = 0.2f;
        /// <summary>
        /// Tolerance
        /// </summary>
        public float Tolerance { get; set; } = 0.1f;

        /// <summary>
        /// Constructor
        /// </summary>
        public ContactResolverSettings()
        {

        }

        /// <summary>
        /// Gets whether the contact resolver is valid or not
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (MaxContacts <= 0 || VelocityIterations <= 0 || PositionIterations <= 0 || PositionEpsilon < 0f || PositionEpsilon < 0f)
            {
                return false;
            }

            return true;
        }
    }
}
