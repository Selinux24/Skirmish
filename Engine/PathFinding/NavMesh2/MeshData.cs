using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh2
{
    public class MeshData
    {
        public MeshHeader header;
        public List<Vector3> navVerts = new List<Vector3>();
        public List<Poly> navPolys = new List<Poly>();
        public List<PolyDetail> navDMeshes = new List<PolyDetail>();
        public List<Vector3> navDVerts = new List<Vector3>();
        public List<Trianglei> navDTris = new List<Trianglei>();
        public List<BVNode> navBvtree = new List<BVNode>();
        public List<OffMeshConnection> offMeshCons = new List<OffMeshConnection>();
    }
}
