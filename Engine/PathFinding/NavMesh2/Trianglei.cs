
namespace Engine.PathFinding.NavMesh2
{
    public class Trianglei
    {
        private int[] vertices = new int[] { 0, 0, 0, 0x00 };

        public int X { get { return this[0]; } set { this[0] = value; } }
        public int Y { get { return this[1]; } set { this[1] = value; } }
        public int Z { get { return this[2]; } set { this[2] = value; } }
        public int R { get { return this[3]; } set { this[3] = value; } }

        public int this[int index]
        {
            get
            {
                return this.vertices[index];
            }
            set
            {
                this.vertices[index] = value;
            }
        }

        public Trianglei()
        {

        }

        public Trianglei(int x, int y, int z, int r)
        {
            vertices = new[] { x, y, z, r };
        }

        public override string ToString()
        {
            return string.Format("X: {0}; Y: {1}; Z: {2}; Region: {3};", X, Y, Z, R);
        }
    }
}
