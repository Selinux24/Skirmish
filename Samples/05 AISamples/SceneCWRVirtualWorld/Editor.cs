using Engine;
using Engine.BuiltIn.Components.Primitives;
using Engine.Common;
using SharpDX;
using System.Threading.Tasks;

namespace AISamples.SceneCWRVirtualWorld
{
    class Editor()
    {
        private static readonly Color4 triColor = new(0f, 0f, 0f, 1f);
        private static readonly Color4 highlightColor = Color.Yellow;
        private const float hDelta = 0.1f;

        private PrimitiveListDrawer<Triangle> triangleDrawer = null;

        private Vector2? selected = null;
        private Vector2? hovered = null;
        private bool drawHovered = false;
        public bool dragging = false;
        public int draggingPointIndex = -1;
        private float dragTime = 0;
        private Vector2 mouse = Vector2.Zero;

        public async Task InitializeEditorDrawer(Scene scene)
        {
            var descT = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = 20000,
                DepthEnabled = false,
                BlendMode = BlendModes.Alpha,
            };
            triangleDrawer = await scene.AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>(
                "editorTriangleDrawer",
                "editorTriangleDrawer",
                descT);
        }
        public void UpdateInputEditor(Scene scene, Graph graph, IGameTime gameTime, float threshold)
        {
            var pRay = scene.GetPickingRay(PickingHullTypes.Geometry);
            if (!scene.PickFirst<Triangle>(pRay, SceneObjectUsages.Ground, out var hit))
            {
                return;
            }

            bool leftClicked = scene.Game.Input.MouseButtonJustPressed(MouseButtons.Left);
            bool leftReleased = scene.Game.Input.MouseButtonJustReleased(MouseButtons.Left);
            bool rightClicked = scene.Game.Input.MouseButtonJustPressed(MouseButtons.Right);
            var p = hit.PickingResult.Position.XZ();

            if (leftClicked)
            {
                dragTime = gameTime.TotalSeconds;
                AddPoint(graph, p);
                return;
            }

            if (rightClicked)
            {
                RemovePoint(graph);
                return;
            }

            if (leftReleased)
            {
                StopDragging();
                return;
            }

            UpdateHovered(graph, p, threshold, gameTime.TotalSeconds - dragTime);
        }

        public void DrawGraph(Graph graph, float height, float pointStroke, float lineStroke)
        {
            triangleDrawer.Clear();

            foreach (var point in graph.GetPoints())
            {
                DrawPoint(point, height, pointStroke, triColor);
            }

            foreach (var segment in graph.GetSegments())
            {
                DrawSegment(segment, height, lineStroke, triColor);
            }

            if (drawHovered && hovered.HasValue)
            {
                DrawPoint(hovered.Value, height + 1, pointStroke * 0.4f, highlightColor);
            }
            drawHovered = false;

            if (selected.HasValue)
            {
                DrawPoint(selected.Value, height + 1, pointStroke * 0.6f, highlightColor);

                DrawSegment(new Segment2(selected.Value, hovered ?? mouse), height, lineStroke, highlightColor);
            }
        }
        private void DrawPoint(Vector2 point, float height, float strokeSize, Color4 tColor)
        {
            var tp = new Vector3(point.X, height + hDelta, point.Y);
            var t = GeometryUtil.CreateCircle(Topology.TriangleList, tp, strokeSize, 32);
            triangleDrawer.AddPrimitives(tColor, Triangle.ComputeTriangleList(t));
        }
        private void DrawSegment(Segment2 segment, float height, float strokeSize, Color4 tColor)
        {
            var t = GeometryUtil.CreateLine2D(Topology.TriangleList, segment.P1, segment.P2, strokeSize, height + hDelta);
            triangleDrawer.AddPrimitives(tColor, Triangle.ComputeTriangleList(t));
        }

        public void AddPoint(Graph graph, Vector2 point)
        {
            if (dragging)
            {
                return;
            }

            if (hovered.HasValue)
            {
                AddPointAndSegment(graph, hovered.Value);

                draggingPointIndex = graph.GetPointIndex(hovered.Value);
                dragging = true;

                return;
            }

            graph.AddPoint(point);
            AddPointAndSegment(graph, point);
            hovered = point;
        }
        private void AddPointAndSegment(Graph graph, Vector2 point)
        {
            if (selected.HasValue)
            {
                graph.TryAddSegment(new(selected.Value, point));
            }
            selected = point;
        }
        public void RemovePoint(Graph graph)
        {
            if (selected.HasValue)
            {
                selected = null;
                return;
            }

            if (!hovered.HasValue)
            {
                selected = null;
                return;
            }

            graph.RemovePoint(hovered.Value);

            if (selected == hovered)
            {
                selected = null;
            }

            hovered = null;
        }
        public void UpdateHovered(Graph graph, Vector2 point, float threshold, float dragTime)
        {
            mouse = point;

            if (dragging && dragTime > 0.2f)
            {
                graph.UpdatePoint(draggingPointIndex, point);
                hovered = point;
                selected = point;

                drawHovered = true;

                return;
            }

            hovered = graph.GetNearestPoint(point, threshold);
            if (hovered.HasValue && selected != hovered)
            {
                drawHovered = true;
            }
        }
        public void StopDragging()
        {
            draggingPointIndex = -1;
            dragging = false;
        }
    }
}
