﻿using AISamples.SceneCWRVirtualWorld.Primitives;
using Engine;
using Engine.BuiltIn.Primitives;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AISamples.SceneCWRVirtualWorld.Markings
{
    class Stop(Vector2 position, Vector2 direction, float width, float height) : Marking(position, direction, width, height)
    {
        public Segment2 GetBorder()
        {
            return GetPolygon().GetSegments()[2];
        }

        protected override VertexPositionTexture[] CreateMarking(IEnumerable<Triangle> tris)
        {
            Vector3[] allPoints = tris.SelectMany(t => new Vector3[] { t.Point3, t.Point2, t.Point1 }).ToArray();
            allPoints = Utils.ScaleFromCenter(allPoints, 0.66f);

            Vector3[] points = [.. allPoints.Distinct()];
            Vector2[] uvs = [new(0.5f, 1), new(0.5f, 0.5f), new(0, 0.5f), new(0, 1)];

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
