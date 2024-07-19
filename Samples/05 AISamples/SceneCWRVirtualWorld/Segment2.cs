using SharpDX;
using System;
using System.Collections.Generic;

namespace AISamples.SceneCWRVirtualWorld
{
    struct Segment2(Vector2 p1, Vector2 p2) : IEquatable<Segment2>
    {
        public Vector2 P1 { get; set; } = p1;
        public Vector2 P2 { get; set; } = p2;
        public readonly float Length => Vector2.Distance(P1, P2);
        public readonly Vector2 Direction => Vector2.Normalize(P2 - P1);

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
                (P1 == other.P1 && P2 == other.P2) ||
                (P1 == other.P2 && P2 == other.P1);
        }
        public override readonly bool Equals(object obj)
        {
            return obj is Segment2 segment && Equals(segment);
        }
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(P1, P2);
        }

        public static Segment2[] Divide(Segment2 segment, float dashSize, float gapSize)
        {
            var length = segment.Length;
            var direction = segment.Direction;
            var dashCount = (int)(length / (dashSize + gapSize));
            var dashLength = dashSize * direction;
            var gapLength = gapSize * direction;
            var dashStart = segment.P1;
            var dashEnd = dashStart + dashLength;

            var dashes = new List<Segment2>();
            for (int i = 0; i < dashCount; i++)
            {
                dashes.Add(new(dashStart, dashEnd));
                dashStart = dashEnd + gapLength;
                dashEnd = dashStart + dashLength;
            }
            dashes.Add(new(dashStart, segment.P2));

            return [.. dashes];
        }
    }
}
