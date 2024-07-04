using SharpDX;

namespace AISamples.SceneCodingWithRadu
{
    class Road
    {
        private readonly float laneWidth;
        private readonly float width;
        private readonly float left;
        private readonly float right;

        const float infinity = 250;
        private readonly float top = infinity;
        private readonly float bottom = -infinity;

        private readonly RoadBorder[] borders = [];

        public int LaneCount { get; }

        public Road(float x, float laneWidth, int laneCount)
        {
            this.laneWidth = laneWidth;
            width = laneWidth * laneCount;
            left = x - (laneWidth * laneCount * 0.5f);
            right = x + (laneWidth * laneCount * 0.5f);

            LaneCount = laneCount;

            borders = CalculateBorders();
        }

        private RoadBorder[] CalculateBorders()
        {
            Vector2 topLeft = new(left, top);
            Vector2 topRight = new(right, top);
            Vector2 bottomLeft = new(left, bottom);
            Vector2 bottomRight = new(right, bottom);

            return
            [
                new RoadBorder { A = topLeft, B = topRight },
                new RoadBorder { A = topRight, B = bottomRight },
                new RoadBorder { A = bottomRight, B = bottomLeft },
                new RoadBorder { A = bottomLeft, B = topLeft }
            ];
        }
        public RoadBorder[] GetBorders()
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

    class RoadBorder
    {
        public Vector2 A { get; set; }
        public Vector2 B { get; set; }
    }
}
