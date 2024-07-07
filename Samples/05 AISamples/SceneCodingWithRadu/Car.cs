using Engine;
using SharpDX;
using System;
using System.Linq;

namespace AISamples.SceneCodingWithRadu
{
    class Car
    {
        private float x;
        private float y;
        private readonly OrientedBoundingBox box;
        private OrientedBoundingBox trnBox;
        private readonly Vector3[] points = new Vector3[4];

        private float speed = 0;
        private readonly float acceleration = 0.2f;
        private readonly float maxSpeed;
        private readonly float maxReverseSpeed;
        private readonly float friction = 0.1f;

        private float angle = 0;
        private readonly float rotationSpeed = 0.02f;
        private Vector2 direction = Vector2.Zero;

        public CarControls Controls { get; }
        public bool Forward => !Damaged && Controls.Forward;
        public bool Reverse => !Damaged && Controls.Reverse;
        public bool Left => !Damaged && Controls.Left;
        public bool Right => !Damaged && Controls.Right;

        public CarControlTypes ControlType { get; }
        public Sensor Sensor { get; }
        public NeuralNetwork Brain { get; }
        public bool Damaged { get; private set; } = false;

        public Car(float width, float height, float depth, CarControlTypes controlType, float maxSpeed = 3f, float maxReverseSpeed = 1.5f)
        {
            box = new(new(width * -0.5f, 0, depth * -0.5f), new(width * 0.5f, height, depth * 0.5f));
            ControlType = controlType;
            this.maxSpeed = maxSpeed;
            this.maxReverseSpeed = -MathF.Abs(maxReverseSpeed);

            Controls = new(controlType);

            if (controlType != CarControlTypes.Dummy)
            {
                Sensor = new(this, 5, 50, MathUtil.PiOverTwo);
                Brain = new([Sensor.GetRayCount(), 6, CarControls.InputCount]);
            }
        }

        private void CreatePoints()
        {
            var trn = Matrix.RotationY(angle) * Matrix.Translation(x, 0, y);

            trnBox = box;
            trnBox.Transform(ref trn);
            var corners = trnBox.GetCorners();

            points[0] = new(corners[4].X, 0f, corners[4].Z);
            points[1] = new(corners[5].X, 0f, corners[5].Z);
            points[2] = new(corners[6].X, 0f, corners[6].Z);
            points[3] = new(corners[7].X, 0f, corners[7].Z);
        }
        public Segment[] GetPolygon()
        {
            Segment[] segments = new Segment[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                segments[i] = new Segment(points[i], points[(i + 1) % points.Length]);
            }
            return segments;
        }

        public void Update(IGameTime gameTime, Road road, Car[] traffic)
        {
            float time = gameTime.ElapsedSeconds;

            Move(time);
            CreatePoints();

            if (!Damaged)
            {
                Damaged = AssessDamage(road, traffic);
            }

            if (Sensor == null)
            {
                return;
            }

            Sensor.Update(road, traffic);

            if (Brain == null)
            {
                return;
            }

            var offsets = Sensor.GetReadings().Select(r => r == null ? 0 : 1f - r.Distance).ToArray();
            Brain.FeedForward(offsets);

            Controls.SetControls(Brain.GetOutputs());
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
        private bool AssessDamage(Road road, Car[] traffic)
        {
            var roadBorders = road.GetBorders();

            for (int i = 0; i < roadBorders.Length; i++)
            {
                if (Utils.Segment2DIntersectsPoly2D(roadBorders[i], points))
                {
                    return true;
                }
            }

            for (int i = 0; i < traffic.Length; i++)
            {
                var cBorders = traffic[i].GetPolygon();

                for (int t = 0; t < cBorders.Length; t++)
                {
                    if (Utils.Segment2DIntersectsPoly2D(cBorders[t], points))
                    {
                        traffic[i].Damaged = true;

                        return true;
                    }
                }
            }

            return false;
        }

        public void Reset()
        {
            Damaged = false;
            trnBox = box;
            speed = 0;
            angle = 0;
            direction = Vector2.Zero;

            Controls.Reset();
            Sensor?.Reset();
            Brain?.Reset();
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
        public OrientedBoundingBox GetBox()
        {
            return trnBox;
        }
    }
}
