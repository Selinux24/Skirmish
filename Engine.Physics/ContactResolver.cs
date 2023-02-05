
namespace Engine.Physics
{
    /// <summary>
    /// Contact resolver
    /// </summary>
    public struct ContactResolver
    {
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
        /// Constructor
        /// </summary>
        public ContactResolver()
        {

        }

        /// <summary>
        /// Gets whether the contact resolver is valid or not
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (VelocityIterations <= 0 || PositionIterations <= 0 || PositionEpsilon < 0f || PositionEpsilon < 0f)
            {
                return false;
            }

            return true;
        }
    }
}
