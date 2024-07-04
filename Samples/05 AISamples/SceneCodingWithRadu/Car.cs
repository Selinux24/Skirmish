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
        private OrientedBoundingBox trnBox;
        private readonly Vector2[] points = new Vector2[4];

        private float speed = 0;
        private readonly float acceleration = 0.2f;
        private readonly float maxSpeed = 3;
        private readonly float maxReverseSpeed = -1.5f;
        private readonly float friction = 0.05f;

        private float angle = 0;
        private readonly float rotationSpeed = 0.03f;
        private Vector2 direction = Vector2.Zero;

        public CarControls Controls { get; } = new();
        public bool Forward => !Damaged && Controls.Forward;
        public bool Reverse => !Damaged && Controls.Reverse;
        public bool Left => !Damaged && Controls.Left;
        public bool Right => !Damaged && Controls.Right;

        public Sensor Sensor { get; }
        public bool Damaged { get; private set; } = false;

        public Car(float x, float y, float width, float height, float depth)
        {
            this.x = x;
            this.y = y;

            box = new(new(width * -0.5f, 0, depth * -0.5f), new(width * 0.5f, height, depth * 0.5f));

            Sensor = new(this, 5, 50, MathUtil.PiOverTwo);
        }

        public OrientedBoundingBox GetBox()
        {
            var trn = Matrix.RotationY(angle) * Matrix.Translation(x, 0, y);

            trnBox = box;
            trnBox.Transform(ref trn);

            CreatePoints();

            return trnBox;
        }
        private void CreatePoints()
        {
            var corners = trnBox.GetCorners();

            points[0] = new(corners[4].X, corners[4].Z);
            points[1] = new(corners[5].X, corners[5].Z);
            points[2] = new(corners[6].X, corners[6].Z);
            points[3] = new(corners[7].X, corners[7].Z);
        }

        public void Update(IGameTime gameTime, Road road)
        {
            float time = gameTime.ElapsedSeconds;

            Move(time);

            if (!Damaged)
            {
                Damaged = AssessDamage(road);
            }

            Sensor.Update(road);
        }

        private void Move(float time)
        {
            float acc = acceleration * time * 100f;
            float rot = rotationSpeed * time * 100f;

            if (Forward)
            {
                speed += acc;
            }
            else if (Reverse)
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

            if (Left)
            {
                angle -= rot * MathF.Sign(speed);
            }
            if (Right)
            {
                angle += rot * MathF.Sign(speed);
            }

            direction = new Vector2(MathF.Sin(angle), MathF.Cos(angle));
            x += direction.X * speed;
            y += direction.Y * speed;
        }

        private bool AssessDamage(Road road)
        {
            var roadBorders = road.GetBorders();

            for (int i = 0; i < roadBorders.Length; i++)
            {
                if (Utils.Segment2DIntersectsPoly2D(roadBorders[i], points))
                {
                    return true;
                }
            }

            return false;
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
