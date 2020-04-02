using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
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

        public void Process(MeshTile tile, IEnumerable<int> refs)
        {
            if (!refs?.Any() == true)
            {
                return;
            }

            int numLeft = MaxPolys - NumCollected;
            int toCopy = refs.Count();
            if (toCopy > numLeft)
            {
                Overflow = true;
                toCopy = numLeft;
            }

            Array.Copy(refs.ToArray(), 0, Polys, NumCollected, toCopy);

            NumCollected += toCopy;
        }
    }
}
