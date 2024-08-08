using AISamples.Common.Primitives;
using Engine;
using SharpDX;
using System;
using System.Linq;

namespace AISamples.Common.Agents
{
    class Car
    {
        private float x;
        private float y;
        private readonly OrientedBoundingBox box;
        private OrientedBoundingBox trnBox;
        private readonly float radius;
        private readonly Vector3[] points = new Vector3[4];

        private float speed = 0;
        private readonly float acceleration = 0.2f;
        private readonly float maxSpeed;
        private readonly float maxReverseSpeed;
        private readonly float friction = 0.1f;

        private float angle = 0;
        private readonly float rotationSpeed = 0.02f;
        private Vector2 direction = Vector2.Zero;

        private float scale = 1f;

        private int stuckedFrames = 0;
        private const int maxStuckedFrames = 600;

        public AgentControls Controls { get; }
        public bool Forward => !Damaged && Controls.Forward;
        public bool Reverse => !Damaged && Controls.Reverse;
        public bool Left => !Damaged && Controls.Left;
        public bool Right => !Damaged && Controls.Right;

        public AgentControlTypes ControlType { get; }
        public Sensor Sensor { get; }
        public NeuralNetwork Brain { get; }
        public bool Damaged { get; private set; } = false;
        public bool Stucked { get; private set; } = false;
        public float FittnessValue { get; set; } = 0;

        public Car(float width, float height, float depth, AgentControlTypes controlType, float maxSpeed = 3f, float maxReverseSpeed = 1.5f)
        {
            box = new(new(width * -0.5f, 0, depth * -0.5f), new(width * 0.5f, height, depth * 0.5f));
            ControlType = controlType;
            this.maxSpeed = maxSpeed;
            this.maxReverseSpeed = -MathF.Abs(maxReverseSpeed);
            radius = MathF.Max(maxSpeed, maxReverseSpeed) + MathF.Max(width, depth);

            Controls = new(controlType);

            if (controlType != AgentControlTypes.Dummy)
            {
                Sensor = new(this, 5, 50, MathUtil.PiOverTwo);
                Brain = new([Sensor.GetRayCount(), 6, AgentControls.InputCount]);
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
        public Segment2[] GetPolygon()
        {
            Segment2[] segments = new Segment2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                segments[i] = new Segment2(points[i].XZ(), points[(i + 1) % points.Length].XZ());
            }
            return segments;
        }

        public void Update(IGameTime gameTime, Segment2[] roadBorders, Car[] traffic, bool damageTraffic)
        {
            float time = gameTime.ElapsedSeconds;

            Move(time);
            CreatePoints();

            if (!Damaged)
            {
                var damageBorders = Array.FindAll(roadBorders, b => b.DistanceToPoint(new(x, y)) <= radius);
                Damaged = AssessDamage(damageBorders, traffic, damageTraffic);
            }

            if (Sensor == null)
            {
                return;
            }

            var rayLength = Sensor.GetRayLength();
            var closeBorders = Array.FindAll(roadBorders, b => b.DistanceToPoint(new(x, y)) <= rayLength);
            Sensor.Update(closeBorders, traffic);

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

            direction = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * speed;
            x += direction.X;
            y += direction.Y;
            FittnessValue += direction.LengthSquared();

            if (Forward == Reverse)
            {
                stuckedFrames++;
            }
            else
            {
                stuckedFrames = 0;
            }

            Stucked = stuckedFrames >= maxStuckedFrames;
        }
        private bool AssessDamage(Segment2[] roadBorders, Car[] traffic, bool damageTraffic)
        {
            var points2d = points.Select(p => p.XZ()).ToArray();

            for (int i = 0; i < roadBorders.Length; i++)
            {
                if (Utils.SegmentIntersectsPoly(roadBorders[i], points2d))
                {
                    return true;
                }
            }

            for (int i = 0; i < traffic.Length; i++)
            {
                var cBorders = traffic[i].GetPolygon();

                for (int t = 0; t < cBorders.Length; t++)
                {
                    if (Utils.SegmentIntersectsPoly(cBorders[t], points2d))
                    {
                        if (damageTraffic) traffic[i].Damaged = true;

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
        public void SetDirection(Vector2 direction)
        {
            angle = -Utils.Angle(direction.Y, direction.X) + MathUtil.PiOverTwo;
        }
        public void SetScale(float scale)
        {
            this.scale = scale;
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
        public Matrix GetTransform(float height = 0)
        {
            var trn = Matrix.RotationY(angle) * Matrix.Translation(x, height, y);

            return Matrix.Scaling(scale) * trn;
        }
    }
}
