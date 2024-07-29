using AISamples.SceneCWRVirtualWorld.Primitives;
using Engine;
using Engine.BuiltIn.Primitives;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.SceneCWRVirtualWorld.Markings
{
    class Crossing(Vector2 position, Vector2 direction, float width, float height) : Marking(position, direction, width, height)
    {
        public Segment2[] GetBorders()
        {
            var polySegments = GetPolygon().GetSegments();

            return [polySegments[0], polySegments[2]];
        }

        protected override VertexPositionTexture[] CreateMarking(IEnumerable<Triangle> tris)
        {
            Vector3[] allPoints = tris.SelectMany(t => new Vector3[] { t.Point3, t.Point2, t.Point1 }).ToArray();
            allPoints = Utils.ScaleFromCenter(allPoints, 1.066f);

            Vector3[] points = [.. allPoints.Distinct()];
            Vector2[] uvs = [new(1, 0.5f), new(1, 0), new(0, 0), new(0, 0.5f)];

            VertexPositionTexture[] vertices = new VertexPositionTexture[allPoints.Length];
            for (int i = 0; i < allPoints.Length; i++)
            {
                var p = allPoints[i];
                var uv = uvs[Array.IndexOf(points, p)];

                vertices[i] = new VertexPositionTexture() { Position = p, Texture = uv };
            }
            return vertices;
        }
    }
}
