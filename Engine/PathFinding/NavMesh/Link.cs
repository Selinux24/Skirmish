using SharpDX;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A link is formed between two polygons in a TiledNavMesh
    /// </summary>
    class Link
    {
        /// <summary>
        /// Entity links to external entity.
        /// </summary>
        public const int External = unchecked((int)0x80000000);
        /// <summary>
        /// Doesn't link to anything.
        /// </summary>
        public const int Null = unchecked((int)0xffffffff);

        public static bool IsExternal(int link)
        {
            return (link & Link.External) != 0;
        }

        /// <summary>
        /// Gets or sets the neighbor reference (the one it's linked to)
        /// </summary>
        public PolyId Reference { get; set; }
        /// <summary>
        /// Gets or sets the index of polygon edge
        /// </summary>
        public int Edge { get; set; }
        /// <summary>
        /// Gets or sets the polygon side
        /// </summary>
        public BoundarySide Side { get; set; }
        /// <summary>
        /// Gets or sets the minimum Vector3 of the bounding box
        /// </summary>
        public int BMin { get; set; }
        /// <summary>
        /// Gets or sets the maximum Vector3 of the bounding box
        /// </summary>
        public int BMax { get; set; }


        public bool CheckBoundaries(Vector3 startPosition, Vector3 endPosition, Vector3 left, Vector3 right, float tmax)
        {
            if (this.Side == BoundarySide.PlusX || this.Side == BoundarySide.MinusX)
            {
                //calculate link size
                float s = 1.0f / 255.0f;
                float lmin = left.Z + (right.Z - left.Z) * (this.BMin * s);
                float lmax = left.Z + (right.Z - left.Z) * (this.BMax * s);
                if (lmin > lmax)
                {
                    //swap
                    float temp = lmin;
                    lmin = lmax;
                    lmax = temp;
                }

                //find z intersection
                float z = startPosition.Z + (endPosition.Z - startPosition.Z) * tmax;
                if (z >= lmin && z <= lmax)
                {
                    return true;
                }
            }
            else if (this.Side == BoundarySide.PlusZ || this.Side == BoundarySide.MinusZ)
            {
                //calculate link size
                float s = 1.0f / 255.0f;
                float lmin = left.X + (right.X - left.X) * (this.BMin * s);
                float lmax = left.X + (right.X - left.X) * (this.BMax * s);
                if (lmin > lmax)
                {
                    //swap
                    float temp = lmin;
                    lmin = lmax;
                    lmax = temp;
                }

                //find x intersection
                float x = startPosition.X + (endPosition.X - startPosition.X) * tmax;
                if (x >= lmin && x <= lmax)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
