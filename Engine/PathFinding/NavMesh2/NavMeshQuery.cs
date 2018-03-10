
namespace Engine.PathFinding.NavMesh2
{
    public class NavMeshQuery
    {
        const int NodeParentBits = 24;
        const int NodeStateBits = 2;

        private NavigationMesh2 m_nav = null;
        private NodePool m_nodePool = null;
        private NodePool m_tinyNodePool = null;
        private NodeQueue m_openList = null;

        public void Init(NavigationMesh2 nav, int maxNodes)
        {
            if (maxNodes > int.MaxValue || maxNodes > (1 << NodeParentBits) - 1)
            {
                throw new EngineException("DT_INVALID_PARAM");
            }

            m_nav = nav;

            if (m_nodePool == null || m_nodePool.GetMaxNodes() < maxNodes)
            {
                if (m_nodePool != null)
                {
                    Helper.Dispose(m_nodePool);
                    m_nodePool = null;
                }

                m_nodePool = new NodePool(maxNodes, Helper.NextPowerOfTwo(maxNodes / 4));
            }
            else
            {
                m_nodePool.Clear();
            }

            if (m_tinyNodePool == null)
            {
                m_tinyNodePool = new NodePool(64, 32);
            }
            else
            {
                m_tinyNodePool.Clear();
            }

            if (m_openList == null || m_openList.GetCapacity() < maxNodes)
            {
                if (m_openList != null)
                {
                    Helper.Dispose(m_openList);
                    m_openList = null;
                }

                m_openList = new NodeQueue(maxNodes);
            }
            else
            {
                m_openList.Clear();
            }
        }
    }
}
