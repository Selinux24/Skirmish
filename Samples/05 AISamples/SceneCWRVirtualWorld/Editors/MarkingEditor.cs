using AISamples.Common;
using AISamples.Common.Markings;
using AISamples.Common.Primitives;
using Engine;
using Engine.BuiltIn.Components.Primitives;
using Engine.BuiltIn.Primitives;
using Engine.Content;
using SharpDX;
using System.Threading.Tasks;

namespace AISamples.SceneCWRVirtualWorld.Editors
{
    abstract class MarkingEditor(string name, World world, float height, bool is3d = false) : IEditor
    {
        private const float editorPointRadius = 10f;

        private readonly string name = name;
        private readonly bool is3d = is3d;
        private readonly float threshold = editorPointRadius * 3;
        private GeometryDrawer<VertexPositionTexture> markingIntentDrawer = null;
        private Scene scene = null;
        private bool visible = true;

        protected World World { get; private set; } = world;
        protected float Height { get; private set; } = height;
        protected Marking Intent { get; set; } = null;

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
            markingIntentDrawer.Visible = visible;

            if (!visible)
            {
                Intent = null;
            }
        }

        public async Task Initialize(Scene scene)
        {
            this.scene = scene;

            (string, string)[] images =
            [
                ("diffuse", Constants.MarkingsTexture)
            ];

            var material = MaterialBlinnPhongContent.Default;
            material.DiffuseTexture = "diffuse";
            material.IsTransparent = true;

            var descS = new GeometryDrawerDescription<VertexPositionTexture>()
            {
                Count = 20000,
                DepthEnabled = is3d,
                BlendMode = BlendModes.Alpha,
                Topology = Topology.TriangleList,
                Images = images,
                Material = material,
                TintColor = new(1f, 1f, 1f, 0.75f),
                UseAnisotropic = true,
            };

            markingIntentDrawer = await scene.AddComponentEffect<GeometryDrawer<VertexPositionTexture>, GeometryDrawerDescription<VertexPositionTexture>>(
                name,
                nameof(markingIntentDrawer),
                descS,
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
            bool rightClicked = scene.Game.Input.MouseButtonJustPressed(MouseButtons.Right);
            var p = hit.PickingResult.Position.XZ();

            if (leftClicked)
            {
                Add();
                return;
            }

            if (rightClicked)
            {
                Remove(p);
                return;
            }

            UpdateHovered(p, threshold);
        }
        private void UpdateHovered(Vector2 point, float threshold)
        {
            Intent = null;

            var hovered = Utils.GetNearestSegment(point, GetTargetSegments(), threshold);
            if (!hovered.HasValue)
            {
                return;
            }

            var proj = hovered.Value.ProjectPoint(point);
            if (proj.offset >= 0 && proj.offset <= 1)
            {
                Intent = CreateMarking(proj.point, hovered.Value.Direction);
            }
        }
        protected abstract Segment2[] GetTargetSegments();
        protected abstract Marking CreateMarking(Vector2 point, Vector2 direction);
        private void Add()
        {
            if (Intent == null)
            {
                return;
            }

            World.AddMarking(Intent);
            Intent = null;
        }
        private void Remove(Vector2 point)
        {
            var marking = World.GetMarkingAtPoint(point);
            if (marking != null)
            {
                World.RemoveMarking(marking);
            }
        }

        public void Draw()
        {
            markingIntentDrawer.Clear();

            if (Intent != null)
            {
                markingIntentDrawer.AddPrimitives(Intent.Draw(Height));
            }
        }
    }
}
