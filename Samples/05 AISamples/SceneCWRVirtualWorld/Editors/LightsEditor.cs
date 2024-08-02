using AISamples.Common;
using AISamples.Common.Markings;
using AISamples.Common.Primitives;
using SharpDX;

namespace AISamples.SceneCWRVirtualWorld.Editors
{
    class LightsEditor(World world, float height) : MarkingEditor(nameof(LightsEditor), world, height, true)
    {
        protected override Segment2[] GetTargetSegments()
        {
            return World.GetLaneGuides();
        }
        protected override Marking CreateMarking(Vector2 point, Vector2 direction)
        {
            float width = World.RoadWidth * 0.5f;
            return new Light(point, direction, width * 0.5f, width);
        }
    }
}
