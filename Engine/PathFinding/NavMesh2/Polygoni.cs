
using System;

namespace Engine.PathFinding.NavMesh2
{
    public class Polygoni
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

        public Polygoni(int capacity)
        {
            this.Vertices = Helper.CreateArray(capacity, Constants.NullIdx);
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
