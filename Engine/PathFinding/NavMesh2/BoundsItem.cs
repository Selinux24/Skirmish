using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    struct BoundsItem
    {
        public int i;
        public Vector2 bmin;
        public Vector2 bmax;

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", i, bmin, bmax);
        }
    }
}
