using AISamples.Common;
using AISamples.Common.Markings;
using AISamples.Common.Primitives;
using SharpDX;

namespace AISamples.SceneCWRVirtualWorld.Editors
{
    class CrossingsEditor(World world, float height) : MarkingEditor(nameof(CrossingsEditor), world, height)
    {
        protected override Segment2[] GetTargetSegments()
        {
            return World.Graph.GetSegments();
        }
        protected override Marking CreateMarking(Vector2 point, Vector2 direction)
        {
            float width = World.RoadWidth * 0.5f;
            return new Crossing(point, direction, width, width);
        }
    }
}
