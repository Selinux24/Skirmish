using System;

namespace Engine.PathFinding
{
    using Engine.PathFinding.AStar;
    using Engine.PathFinding.RecastNavigation;

    /// <summary>
    /// Path finder generation class
    /// </summary>
    public static class PathFinder
    {
        /// <summary>
        /// Builds a path finder from triangle data
        /// </summary>
        /// <param name="sourceFunction">Geometry source function</param>
        /// <param name="settings">Generation settings</param>
        /// <returns>Returns the generated graph for path finding over a triangle mesh</returns>
        public static IGraph Build(Func<Triangle[]> sourceFunction, PathFinderSettings settings)
        {
            IGraph res = null;

            if (settings is GridGenerationSettings)
            {
                res = new Grid();
            }
            else if (settings is BuildSettings)
            {
                res = new Graph();
            }
            else
            {
                throw new EngineException("Bad Graph type");
            }

            res.Build(sourceFunction, settings);

            return res;
        }
    }
}
