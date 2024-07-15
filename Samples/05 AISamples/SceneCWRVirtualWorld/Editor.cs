using Engine;
using Engine.BuiltIn.Components.Primitives;
using Engine.Common;
using SharpDX;

namespace AISamples.SceneCWRVirtualWorld
{
    class Editor(PrimitiveListDrawer<Triangle> triangleDrawer, PrimitiveListDrawer<Line3D> lineDrawer)
    {
        private static readonly Color4 lineColor = new(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color4 triColor = new(0.8f, 0.8f, 0.8f, 1f);
        private const float hDelta = 0.1f;

        private readonly PrimitiveListDrawer<Triangle> triangleDrawer = triangleDrawer;
        private readonly PrimitiveListDrawer<Line3D> lineDrawer = lineDrawer;

        public void DrawGraph(Graph graph, float height, float pointStroke, float lineStroke)
        {
            triangleDrawer.Clear();
            lineDrawer.Clear();

            foreach (var segment in graph.GetSegments())
            {
                DrawSegment(segment, height, lineStroke);
            }

            foreach (var point in graph.GetPoints())
            {
                DrawPoint(point, height, pointStroke);
            }
        }

        private void DrawPoint(Vector2 point, float height, float strokeSize)
        {
            var tp = new Vector3(point.X, height + hDelta, point.Y);
            var t = GeometryUtil.CreateCircle(Topology.TriangleList, tp, strokeSize, 32);
            triangleDrawer.AddPrimitives(triColor, Triangle.ComputeTriangleList(t));

            var lp = new Vector3(point.X, height, point.Y);
            var l = GeometryUtil.CreateCircle(Topology.LineList, lp, strokeSize, 32);
            lineDrawer.AddPrimitives(lineColor, Line3D.CreateFromVertices(l));
        }

        private void DrawSegment(Segment2 segment, float height, float strokeSize)
        {
            var t = GeometryUtil.CreateLine2D(Topology.TriangleList, segment.P1, segment.P2, strokeSize, height + hDelta);
            triangleDrawer.AddPrimitives(triColor, Triangle.ComputeTriangleList(t));

            var l = GeometryUtil.CreateLine2D(Topology.LineList, segment.P1, segment.P2, strokeSize, height);
            lineDrawer.AddPrimitives(lineColor, Line3D.CreateFromVertices(l));
        }
    }
}
