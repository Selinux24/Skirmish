using Engine;
using SharpDX;
using System;

namespace AISamples.SceneCodingWithRadu
{
    class Car
    {
        private float x;
        private float y;
        private readonly OrientedBoundingBox box;

        private float speed = 0;
        private readonly float acceleration = 0.2f;
        private readonly float maxSpeed = 3;
        private readonly float maxReverseSpeed = -1.5f;
        private readonly float friction = 0.05f;

        private float angle = 0;
        private readonly float rotationSpeed = 0.03f;
        private Vector2 direction = Vector2.Zero;

        public CarControls Controls { get; } = new();
        public Sensor Sensor { get; }

        public Car(float x, float y, float width, float height, float depth)
        {
            this.x = x;
            this.y = y;

            box = new(new(width * -0.5f, 0, depth * -0.5f), new(width * 0.5f, height, depth * 0.5f));

            Sensor = new(this, 5, 100);
        }

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

            Sensor.Update();
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

            if (Controls.Left)
            {
                angle -= rot * MathF.Sign(speed);
            }
            if (Controls.Right)
            {
                angle += rot * MathF.Sign(speed);
            }

            direction = new Vector2(MathF.Sin(angle), MathF.Cos(angle));
            x += direction.X * speed;
            y += direction.Y * speed;
        }

        public Vector2 GetPosition()
        {
            return new(x, y);
        }

        public void SetPosition(Vector2 position)
        {
            x = position.X;
            y = position.Y;
        }

        public float GetAngle()
        {
            return angle;
        }

        public Vector2 GetDirection()
        {
            return direction;
        }
    }
}
