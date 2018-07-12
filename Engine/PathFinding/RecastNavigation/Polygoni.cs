using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Indexed polygon
    /// </summary>
    [Serializable]
    public class Polygoni : ISerializable
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
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected Polygoni(SerializationInfo info, StreamingContext context)
        {
            Vertices = (int[])info.GetValue("Vertices", typeof(int[]));
        }
        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Vertices", Vertices);
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
