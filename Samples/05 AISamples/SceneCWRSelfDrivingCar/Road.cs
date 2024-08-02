using AISamples.Common.Primitives;
using SharpDX;
using System;

namespace AISamples.SceneCWRSelfDrivingCar
{
    class Road
    {
        private readonly float laneWidth;
        private readonly float width;
        private readonly float left;
        private readonly float right;
        private readonly float top;
        private readonly float bottom;

        private readonly Segment2[] borders;

        public int LaneCount { get; }

        public Road(float x, float laneWidth, int laneCount, float infinity = 250)
        {
            this.laneWidth = laneWidth;
            width = laneWidth * laneCount;
            left = x - (laneWidth * laneCount * 0.5f);
            right = x + (laneWidth * laneCount * 0.5f);
            top = infinity;
            bottom = -infinity;

            LaneCount = laneCount;

            borders = CalculateBorders(0);
        }

        private Segment2[] CalculateBorders(float y)
        {
            Vector2 topLeft = new(left, top + y);
            Vector2 topRight = new(right, top + y);
            Vector2 bottomLeft = new(left, bottom + y);
            Vector2 bottomRight = new(right, bottom + y);

            return
            [
                new (){ P1 = topLeft,     P2 = topRight },
                new (){ P1 = topRight,    P2 = bottomRight },
                new (){ P1 = bottomRight, P2 = bottomLeft },
                new (){ P1 = bottomLeft,  P2 = topLeft }
            ];
        }

        public void Update(float depth)
        {
            var newBorders = CalculateBorders(depth);
            Array.Copy(newBorders, borders, borders.Length);
        }

        public Segment2[] GetBorders()
        {
            return [.. borders];
        }
        public RectangleF[] GetLanes()
        {
            RectangleF[] lanes = new RectangleF[LaneCount];
            for (int i = 0; i < LaneCount; i++)
            {
                float x = MathUtil.Lerp(left, right, i / (float)LaneCount);

                lanes[i] = new RectangleF(x, top, width / LaneCount, bottom - top);
            }
            return lanes;
        }
        public Vector2 GetLaneCenter(int laneIndex)
        {
            laneIndex = MathUtil.Clamp(laneIndex, 0, LaneCount - 1);

            return new Vector2(left + laneWidth * 0.5f + laneIndex * laneWidth, 0f);
        }
    }
}
