
namespace Engine.PathFinding.RecastNavigation
{
    public enum AreaTypes
    {
        /// <summary>
        /// Represents the null area.
        /// When a data element is given this value it is considered to no longer be assigned to a usable area.  (E.g. It is unwalkable.)
        /// </summary>
        RC_NULL_AREA = 0b_0000_0000,
        /// <summary>
        /// The default area id used to indicate a walkable polygon. 
        /// This is also the maximum allowed area id, and the only non-null area id recognized by some steps in the build process. 
        /// </summary>
        RC_WALKABLE_AREA = 0b_0011_1111,
        /// <summary>
        /// Undefined area
        /// </summary>
        RC_UNDEFINED = 0b_1111_1111,
    }
}
