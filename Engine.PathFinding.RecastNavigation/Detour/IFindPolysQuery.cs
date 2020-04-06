using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Find polygon query interface
    /// </summary>
    public interface IFindPolysQuery
    {
        /// <summary>
        /// Performs the query against a segment
        /// </summary>
        /// <param name="va">Segment point A</param>
        /// <param name="vb">Segment point B</param>
        /// <returns>Returns true if the query passes</returns>
        bool Contains(Vector3 va, Vector3 vb);
    }
}
