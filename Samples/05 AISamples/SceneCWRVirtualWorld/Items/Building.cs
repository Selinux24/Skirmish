using AISamples.Common;
using AISamples.SceneCWRVirtualWorld.Content;
using AISamples.SceneCWRVirtualWorld.Primitives;
using Engine;
using Engine.BuiltIn.Primitives;
using Engine.Common;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.SceneCWRVirtualWorld.Items
{
    class Building(Polygon polygon, float height)
    {
        public Polygon Polygon { get; set; } = polygon;
        public float Height { get; set; } = height;

        public static BuildingFile FromBuilding(Building building)
        {
            return new()
            {
                Polygon = Polygon.FromPolygon(building.Polygon),
                Height = building.Height,
            };
        }
        public static Building FromBuildingFile(BuildingFile building)
        {
            var polygon = Polygon.FromPolygonFile(building.Polygon);
            var height = building.Height;

            return new(polygon, height);
        }

        public IEnumerable<VertexPositionTexture> CreateBuilding(float baseHeight)
        {
            Vector2[] baseVertices = Polygon.GetVertices();
            Vector3[] points = Polygon.Extrude(baseHeight, Height);

            var uvW = Constants.Gray;
            var uvC = Constants.Red;

            // Create the walls
            int vertexCount = baseVertices.Length;
            for (int i = 0; i < vertexCount; i++)
            {
                yield return new VertexPositionTexture() { Position = points[i], Texture = uvW };
                yield return new VertexPositionTexture() { Position = points[(i + 1) % vertexCount], Texture = uvW };
                yield return new VertexPositionTexture() { Position = points[i + vertexCount], Texture = uvW };

                yield return new VertexPositionTexture() { Position = points[(i + 1) % vertexCount], Texture = uvW };
                yield return new VertexPositionTexture() { Position = points[(i + 1) % vertexCount + vertexCount], Texture = uvW };
                yield return new VertexPositionTexture() { Position = points[i + vertexCount], Texture = uvW };
            }

            var ceiling = GeometryUtil.CreatePolygon(Topology.TriangleList, points.Skip(vertexCount), false);
            for (int i = 0; i < ceiling.Indices.Count(); i++)
            {
                int index = (int)ceiling.Indices.ElementAt(i);
                var p = ceiling.Vertices.ElementAt(index);

                yield return new VertexPositionTexture() { Position = p, Texture = uvC };
            }
        }
    }
}
