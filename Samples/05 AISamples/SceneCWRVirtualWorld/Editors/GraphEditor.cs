using AISamples.Common;
using AISamples.Common.Primitives;
using Engine;
using Engine.BuiltIn.Components.Primitives;
using Engine.Common;
using SharpDX;
using System.Threading.Tasks;

namespace AISamples.SceneCWRVirtualWorld.Editors
{
    class GraphEditor(World world, float height) : IEditor
    {
        private static readonly Color4 graphColor = new(0f, 0f, 0f, 0.5f);
        private static readonly Color4 highlightColor = new(1f, 1f, 0f, 0.33f);
        private const float editorPointRadius = 10f;
        private const float hLayer = 0.5f;
        private const float hDelta = 0.1f;

        private GeometryColorDrawer<Triangle> graphDrawer = null;

        private readonly World world = world;
        private readonly float height = height;
        private readonly float pointRadius = editorPointRadius;
        private readonly float lineThickness = editorPointRadius * 0.1f;
        private readonly float threshold = editorPointRadius * 3;

        private Scene scene = null;
        private Vector2? selected = null;
        private Vector2? hovered = null;
        private bool drawHovered = false;
        private bool dragging = false;
        private int draggingPointIndex = -1;
        private float dragTime = 0;
        private Vector2 mouse = Vector2.Zero;
        private bool visible = true;

        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible != value) visible = value;

                SetVisible();
            }
        }
        private void SetVisible()
        {
            graphDrawer.Visible = visible;

            if (!visible)
            {
                selected = null;
                hovered = null;
                dragging = false;
            }
        }

        public async Task Initialize(Scene scene)
        {
            this.scene = scene;

            var descT = new GeometryColorDrawerDescription<Triangle>()
            {
                Count = 20000,
                DepthEnabled = false,
                BlendMode = BlendModes.Alpha,
            };

            graphDrawer = await scene.AddComponentEffect<GeometryColorDrawer<Triangle>, GeometryColorDrawerDescription<Triangle>>(
                nameof(graphDrawer),
                nameof(graphDrawer),
                descT,
                Scene.LayerEffects + 3);
        }

        public void UpdateInputEditor(IGameTime gameTime)
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
                AddPoint(p);
                return;
            }

            if (rightClicked)
            {
                RemovePoint();
                return;
            }

            if (leftReleased)
            {
                StopDragging();
                return;
            }

            UpdateHovered(p, threshold, gameTime.TotalSeconds - dragTime);
        }
        private void AddPoint(Vector2 point)
        {
            if (dragging)
            {
                return;
            }

            if (hovered.HasValue)
            {
                AddPointAndSegment(hovered.Value);

                draggingPointIndex = world.Graph.GetPointIndex(hovered.Value);
                dragging = true;

                return;
            }

            world.Graph.AddPoint(point);
            AddPointAndSegment(point);
            hovered = point;
        }
        private void AddPointAndSegment(Vector2 point)
        {
            if (selected.HasValue)
            {
                world.Graph.TryAddSegment(new(selected.Value, point));
            }
            selected = point;
        }
        private void RemovePoint()
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

            world.Graph.RemovePoint(hovered.Value);

            hovered = null;
        }
        private void UpdateHovered(Vector2 point, float threshold, float dragTime)
        {
            mouse = point;

            if (dragging && dragTime > 0.2f)
            {
                world.Graph.UpdatePoint(draggingPointIndex, point);
                hovered = point;
                selected = point;

                drawHovered = true;

                return;
            }

            hovered = Utils.GetNearestPoint(point, world.Graph.GetPoints(), threshold);
            if (hovered.HasValue && selected != hovered)
            {
                drawHovered = true;
            }
        }
        private void StopDragging()
        {
            draggingPointIndex = -1;
            dragging = false;
        }

        public void Draw()
        {
            graphDrawer.Clear();

            foreach (var point in world.Graph.GetPoints())
            {
                DrawPoint(point, height + hLayer, pointRadius, graphColor);
            }

            foreach (var segment in world.Graph.GetSegments())
            {
                DrawSegment(segment, height + hLayer, lineThickness, graphColor);
            }

            if (drawHovered && hovered.HasValue)
            {
                DrawPoint(hovered.Value, height + hLayer + 1, pointRadius * 0.4f, highlightColor);
            }
            drawHovered = false;

            if (selected.HasValue)
            {
                DrawPoint(selected.Value, height + hLayer + 1, pointRadius * 0.6f, highlightColor);

                DrawSegment(new Segment2(selected.Value, hovered ?? mouse), height + hLayer, lineThickness, graphColor);
            }
        }
        private void DrawPoint(Vector2 point, float height, float strokeSize, Color4 tColor)
        {
            var tp = new Vector3(point.X, height + hDelta, point.Y);
            var t = GeometryUtil.CreateCircle(Topology.TriangleList, tp, strokeSize, 32);
            graphDrawer.AddPrimitives(tColor, Triangle.ComputeTriangleList(t));
        }
        private void DrawSegment(Segment2 segment, float height, float lineThickness, Color4 lineColor)
        {
            var t = GeometryUtil.CreateLine2D(Topology.TriangleList, segment.P1, segment.P2, lineThickness, height + hDelta);
            graphDrawer.AddPrimitives(lineColor, Triangle.ComputeTriangleList(t));
        }
    }
}
