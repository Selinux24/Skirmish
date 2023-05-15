
namespace Engine.Physics
{
    /// <summary>
    /// Contact pair
    /// </summary>
    public struct ContactPair
    {
        /// <summary>
        /// First physics object
        /// </summary>
        public IPhysicsObject Obj1 { get; set; }
        /// <summary>
        /// Second physics object
        /// </summary>
        public IPhysicsObject Obj2 { get; set; }
    }
}
