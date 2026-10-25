using AISamples.Common.Primitives;
using System.Numerics;

namespace AISamples.Common
{
    record SensorReading(Segment2 Ray, Vector3 Position, float Distance) { }
}
