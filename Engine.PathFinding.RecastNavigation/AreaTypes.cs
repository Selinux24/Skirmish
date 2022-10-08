﻿
namespace Engine.PathFinding.RecastNavigation
{
    public enum AreaTypes
    {
        /// <summary>
        /// Represents the null area.
        /// When a data element is given this value it is considered to no longer be 
        /// assigned to a usable area.  (E.g. It is unwalkable.)
        /// </summary>
        RC_NULL_AREA = GraphAreaTypes.Unwalkable,
        /// <summary>
        /// The default area id used to indicate a walkable polygon. 
        /// This is also the maximum allowed area id, and the only non-null area id 
        /// recognized by some steps in the build process. 
        /// </summary>
        RC_WALKABLE_AREA = GraphAreaTypes.Walkable,
    }
}
