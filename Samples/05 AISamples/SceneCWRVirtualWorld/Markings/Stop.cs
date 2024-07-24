using AISamples.SceneCWRVirtualWorld.Primitives;
using SharpDX;

namespace AISamples.SceneCWRVirtualWorld.Markings
{
    class Stop
    {
        public Vector2 Position { get; }
        public Vector2 Direction { get; }
        public float Width { get; }
        public float Height { get; }

        private readonly Segment2 support;

        public Stop(Vector2 position, Vector2 direction, float width, float height)
        {
            Position = position;
            Direction = direction;
            Width = width;
            Height = height;

            float angle = Utils.Angle(direction.Y, direction.X);
            support = new(
                Utils.Translate(position, angle, height * 0.5f),
                Utils.Translate(position, angle, -height * 0.5f));
        }

        public Segment2 GetSupport()
        {
            return support;
        }
    }
}
