using SharpDX;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Mesh data
    /// </summary>
    [Serializable]
    public class MeshData : ISerializable
    {
        /// <summary>
        /// Mesh header
        /// </summary>
        public MeshHeader Header { get; set; }
        /// <summary>
        /// Navigation vertices
        /// </summary>
        public List<Vector3> NavVerts { get; set; } = new List<Vector3>();
        /// <summary>
        /// Navigation polygons
        /// </summary>
        public List<Poly> NavPolys { get; set; } = new List<Poly>();
        /// <summary>
        /// Navigation detail meshes
        /// </summary>
        public List<PolyDetail> NavDMeshes { get; set; } = new List<PolyDetail>();
        /// <summary>
        /// Navigation detail vertices
        /// </summary>
        public List<Vector3> NavDVerts { get; set; } = new List<Vector3>();
        /// <summary>
        /// Navigation detail triangles
        /// </summary>
        public List<Int4> NavDTris { get; set; } = new List<Int4>();
        /// <summary>
        /// Navigation BVTree
        /// </summary>
        public List<BVNode> NavBvtree { get; set; } = new List<BVNode>();
        /// <summary>
        /// Off-mesh connections
        /// </summary>
        public List<OffMeshConnection> OffMeshCons { get; set; } = new List<OffMeshConnection>();

        /// <summary>
        /// Constructor
        /// </summary>
        public MeshData()
        {

        }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization information</param>
        /// <param name="context">Serializatio context</param>
        protected MeshData(SerializationInfo info, StreamingContext context)
        {
            Header = info.GetValue<MeshHeader>("header");

            var navVertsCount = info.GetInt32("navVerts.Count");
            for (int i = 0; i < navVertsCount; i++)
            {
                NavVerts.Add(info.GetVector3(string.Format("navVerts.{0}", i)));
            }

            var navPolysCount = info.GetInt32("navPolys.Count");
            for (int i = 0; i < navPolysCount; i++)
            {
                NavPolys.Add(info.GetValue<Poly>(string.Format("navPolys.{0}", i)));
            }

            var navDMeshesCount = info.GetInt32("navDMeshes.Count");
            for (int i = 0; i < navDMeshesCount; i++)
            {
                NavDMeshes.Add(info.GetValue<PolyDetail>(string.Format("navDMeshes.{0}", i)));
            }

            var navDVertsCount = info.GetInt32("navDVerts.Count");
            for (int i = 0; i < navDVertsCount; i++)
            {
                NavDVerts.Add(info.GetVector3(string.Format("navDVerts.{0}", i)));
            }

            var navDTrisCount = info.GetInt32("navDTris.Count");
            for (int i = 0; i < navDTrisCount; i++)
            {
                NavDTris.Add(info.GetInt4(string.Format("navDTris.{0}", i)));
            }

            var navBvtreeCount = info.GetInt32("navBvtree.Count");
            for (int i = 0; i < navBvtreeCount; i++)
            {
                NavBvtree.Add(info.GetValue<BVNode>(string.Format("navBvtree.{0}", i)));
            }

            var offMeshConsCount = info.GetInt32("offMeshCons.Count");
            for (int i = 0; i < offMeshConsCount; i++)
            {
                OffMeshCons.Add(info.GetValue<OffMeshConnection>(string.Format("offMeshCons.{0}", i)));
            }
        }
        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("header", Header);

            info.AddValue("navVerts.Count", NavVerts.Count);
            for (int i = 0; i < NavVerts.Count; i++)
            {
                info.AddVector3(string.Format("navVerts.{0}", i), NavVerts[i]);
            }

            info.AddValue("navPolys.Count", NavPolys.Count);
            for (int i = 0; i < NavPolys.Count; i++)
            {
                info.AddValue(string.Format("navPolys.{0}", i), NavPolys[i]);
            }

            info.AddValue("navDMeshes.Count", NavDMeshes.Count);
            for (int i = 0; i < NavDMeshes.Count; i++)
            {
                info.AddValue(string.Format("navDMeshes.{0}", i), NavDMeshes[i]);
            }

            info.AddValue("navDVerts.Count", NavDVerts.Count);
            for (int i = 0; i < NavDVerts.Count; i++)
            {
                info.AddVector3(string.Format("navDVerts.{0}", i), NavDVerts[i]);
            }

            info.AddValue("navDTris.Count", NavDTris.Count);
            for (int i = 0; i < NavDTris.Count; i++)
            {
                info.AddInt4(string.Format("navDTris.{0}", i), NavDTris[i]);
            }

            info.AddValue("navBvtree.Count", NavBvtree.Count);
            for (int i = 0; i < NavBvtree.Count; i++)
            {
                info.AddValue(string.Format("navBvtree.{0}", i), NavBvtree[i]);
            }

            info.AddValue("offMeshCons.Count", OffMeshCons.Count);
            for (int i = 0; i < OffMeshCons.Count; i++)
            {
                info.AddValue(string.Format("offMeshCons.{0}", i), OffMeshCons[i]);
            }
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("header: {0};", Header);
        }
    }
}
