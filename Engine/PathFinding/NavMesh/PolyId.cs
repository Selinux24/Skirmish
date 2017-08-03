using System;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct PolyId : IEquatable<PolyId>
    {
        /// <summary>
        /// A null ID that isn't associated with any polygon or tile.
        /// </summary>
        public static readonly PolyId Null = new PolyId(0);

        public static bool operator ==(PolyId left, PolyId right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(PolyId left, PolyId right)
        {
            return !(left == right);
        }

        private int bits;

        public int Id
        {
            get
            {
                return bits;
            }
        }

        public PolyId(int raw)
        {
            bits = raw;
        }

        public bool Equals(PolyId other)
        {
            return bits == other.bits;
        }
        public override bool Equals(object obj)
        {
            var polyObj = obj as PolyId?;
            if (polyObj.HasValue)
            {
                return this.Equals(polyObj.Value);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            //TODO actual hash code
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return bits.ToString();
        }
    }
}
