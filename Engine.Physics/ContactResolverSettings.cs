
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
        /// <remarks>The higher this number, the more the inter-penetration will be visually noticeable.</remarks>
        public float VelocityEpsilon { get; set; } = 0.001f;
        /// <summary>
        /// To avoid instability, penetrations less than this value are considered as no inter penetrations.
        /// </summary>
        /// <remarks>The higher this number, the more the inter penetration will be visually noticeable.</remarks>
        public float PositionEpsilon { get; set; } = 0.001f;

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
        public readonly bool IsValid()
        {
            if (MaxContacts <= 0 || VelocityIterations <= 0 || PositionIterations <= 0 || PositionEpsilon < 0f || PositionEpsilon < 0f)
            {
                return false;
            }

            return true;
        }
    }
}
