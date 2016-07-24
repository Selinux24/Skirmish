using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.PathFinding
{
    /// <summary>
    /// Flags for choosing how the path is built.
    /// </summary>
    [Flags]
    public enum PathBuildFlags
    {
        /// <summary>
        /// Build normally.
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Adds a vertex to the path at each polygon edge crossing, but only when the areas of the two polygons are
        /// different
        /// </summary>
        AreaCrossingVertices = 0x01,
        /// <summary>
        /// Adds a vertex to the path at each polygon edge crossing.
        /// </summary>
        AllCrossingVertices = 0x02
    }
}
