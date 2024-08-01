using AISamples.SceneCWRVirtualWorld.Content;
using SharpDX;
using System;

namespace AISamples.SceneCWRVirtualWorld.Primitives
{
    struct Segment2(Vector2 p1, Vector2 p2) : IEquatable<Segment2>
    {
        public Vector2 P1 { get; set; } = p1;
        public Vector2 P2 { get; set; } = p2;
        public readonly float Length => Vector2.Distance(P1, P2);
        public readonly Vector2 Direction => Vector2.Normalize(P2 - P1);

        public static Segment2File FromSegment(Segment2 segment)
        {
            return new()
            {
                P1 = Vector2File.FromVector2(segment.P1),
                P2 = Vector2File.FromVector2(segment.P2),
            };
        }
        public static Segment2 FromSegmentFile(Segment2File segment)
        {
            return new(Vector2File.FromVector2File(segment.P1), Vector2File.FromVector2File(segment.P2));
        }

        public readonly float DistanceToPoint(Vector2 point)
        {
            var proj = ProjectPoint(point);
            if (proj.offset > 0 && proj.offset < 1)
            {
                return Vector2.Distance(point, proj.point);
            }
            float distToP1 = Vector2.Distance(point, P1);
            float distToP2 = Vector2.Distance(point, P2);
            return MathF.Min(distToP1, distToP2);
        }
        public readonly (Vector2 point, float offset) ProjectPoint(Vector2 point)
        {
            var a = Vector2.Subtract(point, P1);
            var b = Vector2.Subtract(P2, P1);
            var normB = Vector2.Normalize(b);
            float scaler = Vector2.Dot(a, normB);
            return (P1 + scaler * normB, scaler / b.Length());
        }

        public static bool operator ==(Segment2 left, Segment2 right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Segment2 left, Segment2 right)
        {
            return !left.Equals(right);
        }

        public readonly bool Equals(Segment2 other)
        {
            return
                P1 == other.P1 && P2 == other.P2 ||
                P1 == other.P2 && P2 == other.P1;
        }
        public override readonly bool Equals(object obj)
        {
            return obj is Segment2 segment && Equals(segment);
        }
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(P1, P2);
        }
    }
}
