using SharpDX;

namespace Engine.PathFinding.NavMesh
{
    class NavigationMeshQueryFilter
    {
        private float[] areaCost;

        public NavigationMeshQueryFilter()
        {
            areaCost = new float[Area.MaxValues];
            for (int i = 0; i < areaCost.Length; i++)
                areaCost[i] = 1;
        }

        public virtual bool PassFilter(PolyId polyId, MeshTile tile, Poly poly)
        {
            return true;
        }

        public virtual float GetCost(Vector3 a, Vector3 b,
            PolyId prevRef, MeshTile prevTile, Poly prevPoly,
            PolyId curRef, MeshTile curTile, Poly curPoly,
            PolyId nextRef, MeshTile nextTile, Poly nextPoly)
        {
            return (a - b).Length() * areaCost[(int)curPoly.Area.Id];
        }

        public float GetAreaCost(Area area)
        {
            return areaCost[area.Id];
        }

        public void SetAreaCost(Area area, float value)
        {
            areaCost[area.Id] = value;
        }
    }
}
