using System;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Indexed polygon
    /// </summary>
    [Serializable]
    public class Polygoni
    {
        /// <summary>
        /// Vertex indices
        /// </summary>
        private int[] Vertices = null;
        /// <summary>
        /// Gets the polygon vertex index by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the polygon vertex index by index</returns>
        public int this[int index]
        {
            get
            {
                return this.Vertices[index];
            }
            set
            {
                this.Vertices[index] = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Polygoni() : this(10)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Polygon capacity</param>
        public Polygoni(int capacity)
        {
            this.Vertices = Helper.CreateArray(capacity, Recast.RC_MESH_NULL_IDX);
        }

        /// <summary>
        /// Copy the current polygon to another instance
        /// </summary>
        /// <returns>Returns the new instance</returns>
        public Polygoni Copy()
        {
            int[] vertices = new int[Vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Vertices[i];
            }

            return new Polygoni(Vertices.Length)
            {
                Vertices = vertices,
            };
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("{0}", Vertices?.Join(","));
        }
    }
}
