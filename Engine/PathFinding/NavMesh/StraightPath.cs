using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Straight path
    /// </summary>
    public class StraightPath
    {
        /// <summary>
        /// Vertex list
        /// </summary>
        private List<StraightPathVertex> verts = new List<StraightPathVertex>();

        /// <summary>
        /// Vertex count
        /// </summary>
        public int Count { get { return verts.Count; } }
        /// <summary>
        /// Gets or set the vertex at index
        /// </summary>
        /// <param name="i">Index</param>
        /// <returns>Returns the vertex at index</returns>
        public StraightPathVertex this[int i]
        {
            get { return verts[i]; }
            set { verts[i] = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public StraightPath()
        {

        }

        /// <summary>
        /// Clear path
        /// </summary>
        public void Clear()
        {
            this.verts.Clear();
        }
        /// <summary>
        /// Append vertex to path
        /// </summary>
        /// <param name="vert">Vertex</param>
        /// <returns></returns>
        public bool AppendVertex(StraightPathVertex vert)
        {
            bool equalToLast = false;
            if (this.Count > 0)
            {
                //can only be done if at least one vertex in path
                Vector3 lastStraightPath = this.verts[Count - 1].Point.Position;
                Vector3 pos = vert.Point.Position;
                equalToLast = Helper.NearEqual(lastStraightPath, pos);
            }

            if (equalToLast)
            {
                //the vertices are equal, update flags and polys
                this.verts[this.Count - 1] = vert;
            }
            else
            {
                //append new vertex
                this.verts.Add(vert);

                if (vert.Flags == StraightPathFlags.End)
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Remove vertex at index
        /// </summary>
        /// <param name="index">Index</param>
        public void RemoveAt(int index)
        {
            this.verts.RemoveAt(index);
        }
        /// <summary>
        /// Remove vertex range
        /// </summary>
        /// <param name="index">Index from</param>
        /// <param name="count">Count from index</param>
        public void RemoveRange(int index, int count)
        {
            this.verts.RemoveRange(index, count);
        }
    }
}
