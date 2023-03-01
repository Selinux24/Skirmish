using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Contact acummulator data
    /// </summary>
    class ContactAcummulatorData
    {
        /// <summary>
        /// Contact edge
        /// </summary>
        public Segment Edge { get; set; }
        /// <summary>
        /// Contact point
        /// </summary>
        public Vector3 Point { get; set; }
        /// <summary>
        /// Penetration
        /// </summary>
        public float Penetration { get; set; }
        /// <summary>
        /// Contact normal
        /// </summary>
        public Vector3 Normal { get; set; }
        /// <summary>
        /// Contact direction
        /// </summary>
        public int Direction { get; set; }
    }
}
