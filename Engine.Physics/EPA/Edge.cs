
namespace Engine.Physics.EPA
{
    using GJKSupportPoint = GJK.SupportPoint;

    /// <summary>
    /// Edge helper
    /// </summary>
    public struct Edge
    {
        /// <summary>
        /// Point A
        /// </summary>
        public GJKSupportPoint A { get; set; }
        /// <summary>
        /// Point B
        /// </summary>
        public GJKSupportPoint B { get; set; }
    }
}
