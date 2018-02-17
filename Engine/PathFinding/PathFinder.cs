﻿
namespace Engine.PathFinding
{
    using Engine.Common;
    using Engine.PathFinding.AStar;
    using Engine.PathFinding.NavMesh;

    /// <summary>
    /// Path finder generation class
    /// </summary>
    public static class PathFinder
    {
        /// <summary>
        /// Builds a path finder from vertex and index data
        /// </summary>
        /// <param name="settings">Generation settings</param>
        /// <param name="vertices">Vertex list</param>
        /// <param name="indices">Index list</param>
        /// <returns>Returns the generated graph for path finding over a triangle mesh</returns>
        public static IGraph Build(PathFinderSettings settings, VertexData[] vertices, uint[] indices)
        {
            if (settings is GridGenerationSettings)
            {
                return Grid.Build(vertices, indices, (GridGenerationSettings)settings);
            }
            else if (settings is NavigationMeshGenerationSettings)
            {
                return NavigationMesh.Build(vertices, indices, (NavigationMeshGenerationSettings)settings);
            }
            else
            {
                throw new EngineException("Bad Graph type");
            }
        }
        /// <summary>
        /// Builds a path finder from triangle data
        /// </summary>
        /// <param name="settings">Generation settings</param>
        /// <param name="triangles">Triangle list</param>
        /// <returns>Returns the generated graph for path finding over a triangle mesh</returns>
        public static IGraph Build(PathFinderSettings settings, Triangle[] triangles)
        {
            //DEBUG WIP NavMesh2
            triangles = NavMesh2.InputGeometry.DebugTris();
            //triangles = new[]
            //{
            //    new Triangle(-100, 0, -100, -100, 0, 100, 100, 0, 100),
            //    new Triangle(-100, 0, -100, 100, 0, 100, 100, 0, -100),
            //};

            if (settings is GridGenerationSettings)
            {
                return Grid.Build(triangles, (GridGenerationSettings)settings);
            }
            else if (settings is NavigationMeshGenerationSettings)
            {
                return NavigationMesh.Build(triangles, (NavigationMeshGenerationSettings)settings);
            }
            else if (settings is NavMesh2.BuildSettings)
            {
                var n = NavMesh2.NavMesh.Build(triangles, (NavMesh2.BuildSettings)settings);

                return null;
            }
            else
            {
                throw new EngineException("Bad Graph type");
            }
        }
    }
}
