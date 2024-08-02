using AISamples.Common.Persistence;
using AISamples.Common.Primitives;
using Engine;
using Engine.BuiltIn.Primitives;
using Engine.Common;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.Common.Items
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

        public IEnumerable<VertexPositionNormalTexture> CreateBuilding(float baseHeight)
        {
            Vector2[] baseVertices = Polygon.GetVertices();
            Vector3[] points = Polygon.Extrude(baseHeight, Height);

            var uvW = Constants.Gray;
            var uvC = Constants.Red;

            // Create the walls
            int vertexCount = baseVertices.Length;
            for (int i = 0; i < vertexCount; i++)
            {
                var p0 = points[i];
                var p1 = points[(i + 1) % vertexCount];
                var p2 = points[i + vertexCount];
                var n0 = Vector3.Normalize(Vector3.Cross(p2 - p0, p1 - p0));

                yield return new() { Position = p0, Normal = n0, Texture = uvW };
                yield return new() { Position = p1, Normal = n0, Texture = uvW };
                yield return new() { Position = p2, Normal = n0, Texture = uvW };

                var p3 = points[(i + 1) % vertexCount];
                var p4 = points[(i + 1) % vertexCount + vertexCount];
                var p5 = points[i + vertexCount];
                var n1 = Vector3.Normalize(Vector3.Cross(p5 - p3, p4 - p3));

                yield return new() { Position = p3, Normal = n1, Texture = uvW };
                yield return new() { Position = p4, Normal = n1, Texture = uvW };
                yield return new() { Position = p5, Normal = n1, Texture = uvW };
            }

            var ceiling = GeometryUtil.CreatePolygon(Topology.TriangleList, points.Skip(vertexCount), false);
            for (int i = 0; i < ceiling.Indices.Count(); i++)
            {
                int index = (int)ceiling.Indices.ElementAt(i);
                var p = ceiling.Vertices.ElementAt(index);

                yield return new() { Position = p, Normal = Vector3.Up, Texture = uvC };
            }
        }
    }
}
