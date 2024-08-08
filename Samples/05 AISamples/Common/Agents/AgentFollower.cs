using Engine;
using SharpDX;
using System;

namespace AISamples.Common.Agents
{
    class AgentFollower(float distance, float height) : IFollower
    {
        private readonly float distance = distance;
        private readonly float height = height;
        private Car lastCar = null;

        public Func<Car> Car { get; set; }
        public Vector3 Position { get; private set; }
        public Vector3 Interest { get; private set; }
        public float Velocity { get; set; }

        public void Update(IGameTime gameTime)
        {
            var car = Car?.Invoke();
            if (car == null)
            {
                //Clears the last car
                lastCar = car;

                return;
            }

            var p = car.GetPosition();
            var d = -car.GetDirection();

            var d3 = new Vector3(d.X, 0f, d.Y) * distance;
            d3.Y = height;

            Interest = new Vector3(p.X, 0f, p.Y);

            if (lastCar != car)
            {
                lastCar = car;

                //Move camera to the new car
                Position = Interest + d3;
            }
            else
            {
                //Interpolates the camera position
                Position = Vector3.Lerp(Position, Interest + d3, 0.1f);
            }
        }
    }
}
