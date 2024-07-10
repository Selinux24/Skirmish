using Engine;
using SharpDX;

namespace AISamples.SceneCWRSelfDrivingCar
{
    class CarFollower(float distance, float height) : IFollower
    {
        private readonly float distance = distance;
        private readonly float height = height;

        public Car Car { get; set; }
        public Vector3 Position { get; private set; }
        public Vector3 Interest { get; private set; }
        public float Velocity { get; set; }

        public void Update(IGameTime gameTime)
        {
            if (Car == null)
            {
                return;
            }

            var p = Car.GetPosition();
            var d = -Car.GetDirection();

            var d3 = new Vector3(d.X, 0f, d.Y) * distance;
            d3.Y = height;

            Interest = new Vector3(p.X, 0f, p.Y);
            Position = Vector3.Lerp(Position, Interest + d3, 0.1f);
        }
    }
}
