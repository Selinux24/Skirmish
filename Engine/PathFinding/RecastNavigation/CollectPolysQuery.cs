using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    public class CollectPolysQuery : IPolyQuery
    {
        public int[] m_polys;
        public int m_maxPolys;
        public int m_numCollected;
        public bool m_overflow;

        public CollectPolysQuery(int[] polys, int maxPolys)
        {
            m_polys = polys;
            m_maxPolys = maxPolys;
            m_numCollected = 0;
            m_overflow = false;
        }

        public int NumCollected()
        {
            return m_numCollected;
        }
        public bool Overflowed()
        {
            return m_overflow;
        }

        public void Process(MeshTile tile, Poly[] polys, int[] refs, int count)
        {
            int numLeft = m_maxPolys - m_numCollected;
            int toCopy = count;
            if (toCopy > numLeft)
            {
                m_overflow = true;
                toCopy = numLeft;
            }

            Array.Copy(m_polys, m_numCollected, refs, 0, toCopy);

            m_numCollected += toCopy;
        }
    }
}
