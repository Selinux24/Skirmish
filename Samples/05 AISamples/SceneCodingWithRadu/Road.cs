using Engine;
using SharpDX;

namespace AISamples.SceneCodingWithRadu
{
    class Road
    {
        private readonly float laneWidth;
        private readonly float width;
        private readonly float left;
        private readonly float right;
        private readonly float top;
        private readonly float bottom;

        private readonly Segment[] borders;

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

            borders = CalculateBorders();
        }

        private Segment[] CalculateBorders()
        {
            Vector3 topLeft = new(left, 0, top);
            Vector3 topRight = new(right, 0, top);
            Vector3 bottomLeft = new(left, 0, bottom);
            Vector3 bottomRight = new(right, 0, bottom);

            return
            [
                new (){ Point1 = topLeft,     Point2 = topRight },
                new (){ Point1 = topRight,    Point2 = bottomRight },
                new (){ Point1 = bottomRight, Point2 = bottomLeft },
                new (){ Point1 = bottomLeft,  Point2 = topLeft }
            ];
        }
        public Segment[] GetBorders()
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
