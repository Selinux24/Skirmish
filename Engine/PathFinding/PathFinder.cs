using System;

namespace Engine.PathFinding
{
    using Engine.Common;
    using Engine.PathFinding.AStar;
    using Engine.PathFinding.NavMesh;

    public static class PathFinder
    {
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
                throw new Exception("Bad Graph type");
            }
        }

        public static IGraph Build(PathFinderSettings settings, Triangle[] triangles)
        {
            if (settings is GridGenerationSettings)
            {
                return Grid.Build(triangles, (GridGenerationSettings)settings);
            }
            else if (settings is NavigationMeshGenerationSettings)
            {
                return NavigationMesh.Build(triangles, (NavigationMeshGenerationSettings)settings);
            }
            else
            {
                throw new Exception("Bad Graph type");
            }
        }
    }
}
