using Engine;
using SharpDX;

namespace AISamples.SceneCodingWithRadu
{
    class CarFollower(Car car, float distance, float height) : IFollower
    {
        private readonly Car car = car;
        private readonly float distance = distance;
        private readonly float height = height;

        public Vector3 Position { get; private set; }

        public Vector3 Interest { get; private set; }

        public float Velocity { get; set; }

        public void Update(IGameTime gameTime)
        {
            var p = car.GetPosition();
            var d = -car.GetDirection();

            var d3 = new Vector3(d.X, 0f, d.Y) * distance;
            d3.Y = height;

            Interest = new Vector3(p.X, 0f, p.Y);
            Position = Interest + d3;
        }
    }
}
