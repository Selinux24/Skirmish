using Engine;
using SharpDX;

namespace AISamples.SceneCodingWithRadu
{
    public record SensorReading(PickingRay Ray, Vector3 Position, float Distance) { }
}
