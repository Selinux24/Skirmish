using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Path point
    /// </summary>
    public struct PathPoint
    {
        /// <summary>
        /// Reference
        /// </summary>
        public int Ref { get; set; }
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Pos { get; set; }

        /// <summary>
        /// Gets whether the point is valid or not
        /// </summary>
        /// <param name="nm">Navigation mesh</param>
        public readonly bool IsValid(NavMesh nm)
        {
            return !Pos.IsInfinity() && nm.IsValidPolyRef(Ref);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{Ref} => {Pos}";
        }
    }
}
