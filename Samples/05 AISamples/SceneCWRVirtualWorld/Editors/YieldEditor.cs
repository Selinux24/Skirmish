using AISamples.Common;
using AISamples.Common.Markings;
using AISamples.Common.Primitives;
using SharpDX;

namespace AISamples.SceneCWRVirtualWorld.Editors
{
    class YieldEditor(World world, float height) : MarkingEditor(nameof(YieldEditor), world, height)
    {
        protected override Segment2[] GetTargetSegments()
        {
            return World.GetLaneGuides();
        }
        protected override Marking CreateMarking(Vector2 point, Vector2 direction)
        {
            float width = World.RoadWidth * 0.5f;
            return new Yield(point, direction, width * 0.5f, width);
        }
    }
}
