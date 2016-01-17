using Engine;
using Engine.Common;
using SharpDX;
using System.Collections.Generic;

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
                TPPLPoly poly = new TPPLPoly(8);
                poly[0] = new Vector2(+1, +1);
                poly[1] = new Vector2(+0, +1);
                poly[2] = new Vector2(-1, +1);
                poly[3] = new Vector2(-1, +0);
                poly[4] = new Vector2(-1, -1);
                poly[5] = new Vector2(+0, -1);
                poly[6] = new Vector2(+1, -1);
                poly[7] = new Vector2(+0.5f, +0);

                poly.Orientation = OrientationEnum.TPPL_CCW;

                var polys = new List<TPPLPoly>();
                polys.Add(poly);

                List<TPPLPoly> parts;
                TPPLPartition.ConvexPartition_HM(polys, out parts);

                List<TPPLPoly> mergedPolis;
                TPPLPartition.MergeConvex(parts, out mergedPolis);
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
