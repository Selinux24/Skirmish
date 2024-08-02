using Engine;
using SharpDX;

namespace AISamples.Common
{
    public record SensorReading(PickingRay Ray, Vector3 Position, float Distance) { }
}
