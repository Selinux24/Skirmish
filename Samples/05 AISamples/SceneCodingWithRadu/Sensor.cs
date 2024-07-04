using Engine;
using SharpDX;
using System;

namespace AISamples.SceneCodingWithRadu
{
    class Sensor(Car car, int rayCount, float rayLength, float raySpread = MathUtil.PiOverFour)
    {
        private readonly Car car = car;
        private readonly int rayCount = rayCount;
        private readonly float rayLength = rayLength;
        private readonly float raySpread = raySpread;

        private readonly PickingRay[] rays = new PickingRay[rayCount];

        public void Update()
        {
            for (int i = 0; i < rayCount; i++)
            {
                // Calculate the angle of the ray
                float angle = MathUtil.Lerp(-raySpread * 0.5f, raySpread * 0.5f, i / ((float)rayCount - 1));
                angle -= car.GetAngle();

                var start = car.GetPosition();
                var end = new Vector2(
                    start.X - MathF.Sin(angle) * rayLength,
                    start.Y + MathF.Cos(angle) * rayLength);
                var direction = Vector2.Normalize(end - start);

                rays[i] = new PickingRay(new Vector3(start.X, 0f, start.Y), new Vector3(direction.X, 0f, direction.Y), PickingHullTypes.Default, rayLength);
            }
        }

        public PickingRay[] GetRays()
        {
            return [.. rays];
        }
    }
}
