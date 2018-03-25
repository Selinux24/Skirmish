using SharpDX;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation
{
    [Serializable]
    public class MeshData : ISerializable
    {
        public MeshHeader header;
        public List<Vector3> navVerts = new List<Vector3>();
        public List<Poly> navPolys = new List<Poly>();
        public List<PolyDetail> navDMeshes = new List<PolyDetail>();
        public List<Vector3> navDVerts = new List<Vector3>();
        public List<Int4> navDTris = new List<Int4>();
        public List<BVNode> navBvtree = new List<BVNode>();
        public List<OffMeshConnection> offMeshCons = new List<OffMeshConnection>();

        public MeshData()
        {

        }

        protected MeshData(SerializationInfo info, StreamingContext context)
        {
            header = info.GetValue<MeshHeader>("header");

            var navVertsCount = info.GetInt32("navVerts.Count");
            for (int i = 0; i < navVertsCount; i++)
            {
                navVerts.Add(info.GetVector3(string.Format("navVerts.{0}", i)));
            }

            var navPolysCount = info.GetInt32("navPolys.Count");
            for (int i = 0; i < navPolysCount; i++)
            {
                navPolys.Add(info.GetValue<Poly>(string.Format("navPolys.{0}", i)));
            }

            var navDMeshesCount = info.GetInt32("navDMeshes.Count");
            for (int i = 0; i < navDMeshesCount; i++)
            {
                navDMeshes.Add(info.GetValue<PolyDetail>(string.Format("navDMeshes.{0}", i)));
            }

            var navDVertsCount = info.GetInt32("navDVerts.Count");
            for (int i = 0; i < navDVertsCount; i++)
            {
                navDVerts.Add(info.GetVector3(string.Format("navDVerts.{0}", i)));
            }

            var navDTrisCount = info.GetInt32("navDTris.Count");
            for (int i = 0; i < navDTrisCount; i++)
            {
                navDTris.Add(info.GetInt4(string.Format("navDTris.{0}", i)));
            }

            var navBvtreeCount = info.GetInt32("navBvtree.Count");
            for (int i = 0; i < navBvtreeCount; i++)
            {
                navBvtree.Add(info.GetValue<BVNode>(string.Format("navBvtree.{0}", i)));
            }

            var offMeshConsCount = info.GetInt32("offMeshCons.Count");
            for (int i = 0; i < offMeshConsCount; i++)
            {
                offMeshCons.Add(info.GetValue<OffMeshConnection>(string.Format("offMeshCons.{0}", i)));
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("header", header);

            info.AddValue("navVerts.Count", navVerts.Count);
            for (int i = 0; i < navVerts.Count; i++)
            {
                info.AddVector3(string.Format("navVerts.{0}", i), navVerts[i]);
            }

            info.AddValue("navPolys.Count", navPolys.Count);
            for (int i = 0; i < navPolys.Count; i++)
            {
                info.AddValue(string.Format("navPolys.{0}", i), navPolys[i]);
            }

            info.AddValue("navDMeshes.Count", navDMeshes.Count);
            for (int i = 0; i < navDMeshes.Count; i++)
            {
                info.AddValue(string.Format("navDMeshes.{0}", i), navDMeshes[i]);
            }

            info.AddValue("navDVerts.Count", navDVerts.Count);
            for (int i = 0; i < navDVerts.Count; i++)
            {
                info.AddVector3(string.Format("navDVerts.{0}", i), navDVerts[i]);
            }

            info.AddValue("navDTris.Count", navDTris.Count);
            for (int i = 0; i < navDTris.Count; i++)
            {
                info.AddInt4(string.Format("navDTris.{0}", i), navDTris[i]);
            }

            info.AddValue("navBvtree.Count", navBvtree.Count);
            for (int i = 0; i < navBvtree.Count; i++)
            {
                info.AddValue(string.Format("navBvtree.{0}", i), navBvtree[i]);
            }

            info.AddValue("offMeshCons.Count", offMeshCons.Count);
            for (int i = 0; i < offMeshCons.Count; i++)
            {
                info.AddValue(string.Format("offMeshCons.{0}", i), offMeshCons[i]);
            }
        }
    }
}
