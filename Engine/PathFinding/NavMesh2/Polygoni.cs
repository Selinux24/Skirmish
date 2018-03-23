using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Engine.PathFinding.NavMesh2
{
    [Serializable]
    public class Polygoni : ISerializable
    {
        private int[] Vertices = null;

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

        public Polygoni() : this(10)
        {

        }

        public Polygoni(int capacity)
        {
            this.Vertices = Helper.CreateArray(capacity, Constants.NullIdx);
        }

        protected Polygoni(SerializationInfo info, StreamingContext context)
        {
            Vertices = (int[])info.GetValue("Vertices", typeof(int[]));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Vertices", Vertices);
        }

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

        public override string ToString()
        {
            return string.Format("{0}", Vertices?.Join(","));
        }
    }
}
