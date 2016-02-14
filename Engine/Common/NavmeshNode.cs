using SharpDX;
using System;

namespace Engine.Common
{
    using Engine.PathFinding;

    public class NavmeshNode : GraphNode
    {
        public Polygon Poly;

        public NavmeshNode(Polygon poly)
        {
            this.Poly = poly;
        }

        public override bool Contains(Vector3 point, out float distance)
        {
            distance = 0;

            if (Polygon.PointInPoly(this.Poly, point))
            {
                return true;
            }

            return false;
        }

        public override Vector3[] GetPoints()
        {
            return this.Poly.Points;
        }
    }
}
