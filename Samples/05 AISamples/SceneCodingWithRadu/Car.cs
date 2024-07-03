using Engine;
using SharpDX;
using System;

namespace AISamples.SceneCodingWithRadu
{
    class Car(float x, float y, float width, float height, float depth)
    {
        private float x = x;
        private float y = y;

        private float speed = 0;
        private readonly float acceleration = 0.2f;
        private readonly float maxSpeed = 3;
        private readonly float maxReverseSpeed = -1.5f;
        private readonly float friction = 0.05f;

        private float angle = 0;
        private readonly float rotationSpeed = 0.03f;

        public CarControls Controls { get; } = new();

        private readonly OrientedBoundingBox box = new(new(width * -0.5f, 0, depth * -0.5f), new(width * 0.5f, height, depth * 0.5f));

        public OrientedBoundingBox GetBox()
        {
            var trn = Matrix.RotationY(angle) * Matrix.Translation(x, 0, y);

            var trnBox = box;
            trnBox.Transform(ref trn);
            return trnBox;
        }

        public void Update(IGameTime gameTime)
        {
            float time = gameTime.ElapsedSeconds;

            Move(time);
        }

        private void Move(float time)
        {
            float acc = acceleration * time * 100f;
            float rot = rotationSpeed * time * 100f;

            if (Controls.Forward)
            {
                speed += acc;
            }
            else if (Controls.Reverse)
            {
                speed -= acc;
            }

            if (speed > 0f)
            {
                speed = MathF.Min(maxSpeed, speed);
                speed -= friction;
            }
            if (speed < 0f)
            {
                speed = MathF.Max(maxReverseSpeed, speed);
                speed += friction;
            }
            if (MathF.Abs(speed) < friction)
            {
                speed = 0;
            }

            int flip = speed != 0 ? MathF.Sign(speed) : 0;
            if (Controls.Left)
            {
                angle -= rot * flip;
            }
            if (Controls.Right)
            {
                angle += rot * flip;
            }

            x += MathF.Sin(angle) * speed;
            y += MathF.Cos(angle) * speed;
        }
    }
}
