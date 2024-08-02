using AISamples.Common.Primitives;
using SharpDX;

namespace AISamples.Common
{
    record SensorReading(Segment2 Ray, Vector3 Position, float Distance) { }
}
