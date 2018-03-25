
namespace Engine.PathFinding.RecastNavigation
{
    public enum TileCacheAreas
    {
        /// <summary>
        /// Represents the null area.
        /// When a data element is given this value it is considered to no longer be 
        /// assigned to a usable area.  (E.g. It is unwalkable.)
        /// </summary>
        RC_NULL_AREA = 0,
        /// <summary>
        /// The default area id used to indicate a walkable polygon. 
        /// This is also the maximum allowed area id, and the only non-null area id 
        /// recognized by some steps in the build process. 
        /// </summary>
        RC_WALKABLE_AREA = 63,
    }
}
