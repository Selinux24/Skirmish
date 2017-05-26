using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// The MeshData struct contains information about vertex and triangle base and offset values for array indices
    /// </summary>
    struct PolyMeshData
    {
        public int VertexIndex;
        public int VertexCount;
        public int TriangleIndex;
        public int TriangleCount;
    }
}
