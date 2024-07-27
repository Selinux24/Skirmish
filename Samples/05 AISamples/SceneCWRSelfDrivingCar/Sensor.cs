﻿using Engine;
using SharpDX;
using System;
using System.Linq;

namespace AISamples.SceneCWRSelfDrivingCar
{
    class Sensor(Car car, int rayCount, float rayLength, float raySpread = MathUtil.PiOverFour)
    {
        private readonly Car car = car;
        private readonly int rayCount = rayCount;
        private readonly float rayLength = rayLength;
        private readonly float raySpread = raySpread;

        private readonly PickingRay[] rays = new PickingRay[rayCount];
        private readonly SensorReading[] readings = new SensorReading[rayCount];

        public void Update(Road road, Car[] traffic)
        {
            var roadBorders = road.GetBorders();
            var carBorders = traffic.SelectMany(c => c.GetPolygon()).ToArray();
            Segment[] segments = [.. roadBorders, .. carBorders];

            CastRays();

            for (int i = 0; i < rayCount; i++)
            {
                readings[i] = GetReading(rays[i], segments);
            }
        }
        private void CastRays()
        {
            for (int i = 0; i < rayCount; i++)
            {
                // Calculate the angle of the ray
                float amount = rayCount == 1 ? 0.5f : i / ((float)rayCount - 1);
                float angle = MathUtil.Lerp(-raySpread * 0.5f, raySpread * 0.5f, amount);
                angle -= car.GetAngle();

                var start = car.GetPosition();
                var end = new Vector2(
                    start.X - MathF.Sin(angle) * rayLength,
                    start.Y + MathF.Cos(angle) * rayLength);
                var direction = Vector2.Normalize(end - start);

                rays[i] = new PickingRay(new Vector3(start.X, 0f, start.Y), new Vector3(direction.X, 0f, direction.Y), PickingHullTypes.Default, rayLength);
            }
        }
        private static SensorReading GetReading(PickingRay ray, Segment[] segments)
        {
            bool found = false;
            float minDistance = float.MaxValue;
            Vector3 minPosition = Vector3.Zero;
            for (int b = 0; b < segments.Length; b++)
            {
                bool touch = Utils.Segment2DIntersectsSegment2D(ray.Segment, segments[b], out Vector3 p, out float d);
                if (!touch)
                {
                    continue;
                }

                found = true;

                if (d < minDistance)
                {
                    minPosition = p;
                    minDistance = d;
                }
            }

            if (!found)
            {
                return null;
            }

            return new(ray, minPosition, minDistance);
        }

        public void Reset()
        {
            for (int i = 0; i < readings.Length; i++)
            {
                readings[i] = null;
            }
        }

        public int GetRayCount()
        {
            return rayCount;
        }
        public PickingRay[] GetRays()
        {
            return [.. rays];
        }
        public SensorReading[] GetReadings()
        {
            return [.. readings];
        }
    }
}