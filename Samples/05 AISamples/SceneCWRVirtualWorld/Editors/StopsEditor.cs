using AISamples.SceneCWRVirtualWorld.Markings;
using AISamples.SceneCWRVirtualWorld.Primitives;
using Engine;
using Engine.BuiltIn.Components.Primitives;
using Engine.Common;
using SharpDX;
using System.Threading.Tasks;

namespace AISamples.SceneCWRVirtualWorld.Editors
{
    class StopsEditor(World world, float height) : IEditor
    {
        private static readonly Color4 highlightColor = new(1f, 0f, 1f, 0.66f);
        private const float editorPointRadius = 10f;

        private GeometryColorDrawer<Triangle> stopsDrawer = null;

        private readonly World world = world;
        private readonly float height = height;
        private readonly float threshold = editorPointRadius * 3;

        private Scene scene = null;
        private Stop intent = null;
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
            stopsDrawer.Visible = visible;

            if (!visible)
            {
                intent = null;
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

            stopsDrawer = await scene.AddComponentEffect<GeometryColorDrawer<Triangle>, GeometryColorDrawerDescription<Triangle>>(
                nameof(stopsDrawer),
                nameof(stopsDrawer),
                descT,
                Scene.LayerEffects + 1);
        }

        public void UpdateInputEditor(IGameTime gameTime)
        {
            var pRay = scene.GetPickingRay(PickingHullTypes.Geometry);
            if (!scene.PickFirst<Triangle>(pRay, SceneObjectUsages.Ground, out var hit))
            {
                return;
            }

            var p = hit.PickingResult.Position.XZ();

            UpdateHovered(p, threshold);
        }
        private void UpdateHovered(Vector2 point, float threshold)
        {
            intent = null;

            var hovered = Utils.GetNearestSegment(point, world.GetLaneGuides(), threshold);
            if (!hovered.HasValue)
            {
                return;
            }

            var proj = hovered.Value.ProjectPoint(point);
            if (proj.offset >= 0 && proj.offset <= 1)
            {
                float width = world.RoadWidth * 0.5f;
                intent = new(proj.point, hovered.Value.Direction, width * 0.5f, width);
            }
        }

        public void Draw()
        {
            stopsDrawer.Clear();

            if (intent != null)
            {
                DrawSegment(intent.GetSupport(), height, intent.Width, highlightColor);
            }
        }
        private void DrawSegment(Segment2 segment, float height, float lineThickness, Color4 lineColor)
        {
            var t = GeometryUtil.CreateLine2D(Topology.TriangleList, segment.P1, segment.P2, lineThickness, height);
            stopsDrawer.AddPrimitives(lineColor, Triangle.ComputeTriangleList(t));
        }
    }
}
