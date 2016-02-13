using Engine;
using Engine.Common;
using SharpDX;

namespace HeightmapTest
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("8 Heightmap", false, 800, 450))
#else
            using (Game cl = new Game("8 Heightmap"))
#endif
            {
                {
                    Polygon poly = new Polygon(8);
                    poly[0] = new Vector2(+1, +1);
                    poly[1] = new Vector2(+0, +1);
                    poly[2] = new Vector2(-1, +1);
                    poly[3] = new Vector2(-1, +0);
                    poly[4] = new Vector2(-1, -1);
                    poly[5] = new Vector2(+0, -1);
                    poly[6] = new Vector2(+1, -1);
                    poly[7] = new Vector2(+0.5f, +0);

                    poly.Orientation = GeometricOrientation.CounterClockwise;

                    Polygon[] parts;
                    if (NavMesh.ConvexPartition(new[] { poly }, out parts))
                    {
                        Polygon[] mergedPolis;
                        NavMesh.MergeConvex(parts, out mergedPolis);

                        Line2[] edges = mergedPolis[0].GetEdges();
                    }
                }

                {
                    Triangle[] tris = new Triangle[8];
                    tris[0] = new Triangle(new Vector3(-1, 0, 1), new Vector3(-1, 0, 0), new Vector3(0, 0, 1));
                    tris[1] = new Triangle(new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector3(0, 0, 0));
                    tris[2] = new Triangle(new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(1, 0, 1));
                    tris[3] = new Triangle(new Vector3(1, 0, 1), new Vector3(0, 0, 0), new Vector3(0.5f, 0, 0));
                    tris[4] = new Triangle(new Vector3(-1, 0, 0), new Vector3(-1, 0, -1), new Vector3(0, 0, 0));
                    tris[5] = new Triangle(new Vector3(0, 0, 0), new Vector3(-1, 0, -1), new Vector3(0, 0, -1));
                    tris[6] = new Triangle(new Vector3(0, 0, 0), new Vector3(0, 0, -1), new Vector3(0.5f, 0, 0));
                    tris[7] = new Triangle(new Vector3(0.5f, 0, 0), new Vector3(0, 0, -1), new Vector3(1, 0, -1));

                    NavMesh nm = NavMesh.Build(tris, 0);
                }

                {
                    Triangle[] tris = new Triangle[6];
                    tris[0] = new Triangle(new Vector3(-1, 0, 1), new Vector3(-1, 0, 0), new Vector3(0, 0, 1));
                    tris[1] = new Triangle(new Vector3(0, 0, 1), new Vector3(-1, 0, 0), new Vector3(0, 0, 0));
                    tris[2] = new Triangle(new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(1, 0, 1));
                    tris[3] = new Triangle(new Vector3(1, 0, 1), new Vector3(0, 0, 0), new Vector3(0.5f, 0, 0));
                    tris[4] = new Triangle(new Vector3(0, 0, 0), new Vector3(0, 0, -1), new Vector3(0.5f, 0, 0));
                    tris[5] = new Triangle(new Vector3(0.5f, 0, 0), new Vector3(0, 0, -1), new Vector3(1, 0, -1));

                    NavMesh nm = NavMesh.Build(tris, 0);
                }
#if DEBUG
                cl.VisibleMouse = false;
                cl.LockMouse = false;
#else
                cl.VisibleMouse = false;
                cl.LockMouse = true;
#endif

                cl.AddScene(new TestScene3D(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
