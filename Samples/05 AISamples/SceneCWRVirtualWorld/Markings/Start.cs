using Engine;
using Engine.BuiltIn.Primitives;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.SceneCWRVirtualWorld.Markings
{
    class Start(Vector2 position, Vector2 direction, float width, float height) : Marking(position, direction, width, height)
    {
        protected override VertexPositionTexture[] CreateMarking(IEnumerable<Triangle> tris)
        {
            Vector3[] allPoints = tris.SelectMany(t => new Vector3[] { t.Point3, t.Point2, t.Point1 }).ToArray();

            Vector3[] points = [.. allPoints.Distinct()];
            Vector2[] uvs = [new(0.7f, 1), new(0.7f, 0.5f), new(0.5f, 0.5f), new(0.5f, 1)];

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
