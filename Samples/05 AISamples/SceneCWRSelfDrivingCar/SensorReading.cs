using Engine;
using SharpDX;

namespace AISamples.SceneCWRSelfDrivingCar
{
    public record SensorReading(PickingRay Ray, Vector3 Position, float Distance) { }
}
