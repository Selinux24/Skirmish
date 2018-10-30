using System;

namespace Engine.PathFinding.RecastNavigation
{
    public class CollectPolysQuery : IPolyQuery
    {
        public int[] Polys { get; protected set; }
        public int MaxPolys { get; protected set; }
        public int NumCollected { get; protected set; }
        public bool Overflow { get; protected set; }

        public CollectPolysQuery(int[] polys, int maxPolys)
        {
            Polys = polys;
            MaxPolys = maxPolys;
            NumCollected = 0;
            Overflow = false;
        }

        public void Process(MeshTile tile, Poly[] polys, int[] refs, int count)
        {
            int numLeft = MaxPolys - NumCollected;
            int toCopy = count;
            if (toCopy > numLeft)
            {
                Overflow = true;
                toCopy = numLeft;
            }

            Array.Copy(Polys, NumCollected, refs, 0, toCopy);

            NumCollected += toCopy;
        }
    }
}
