using SharpDX;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.Content.FmtObj
{
    static class Writer
    {
        public static void WriteObj(StreamWriter wr, IEnumerable<Triangle> triangles)
        {
            var points = new List<Vector3>();
            triangles.ToList().ForEach(t => points.AddRange([t.Point1, t.Point2, t.Point3]));

            var normals = new List<Vector3>();
            triangles.ToList().ForEach(t => normals.AddRange([t.Normal, t.Normal, t.Normal]));

            int index = 0;
            var indices = new List<Int3>();
            triangles.ToList().ForEach(t => indices.Add(new Int3(index++, index++, index++)));

            // Write the file
            WriteData(wr, "v", points.Cast<Vector3?>());
            bool useNm = WriteData(wr, "vn", normals.Cast<Vector3?>());

            foreach (var triIndex in indices)
            {
                string i1 = FormatFace((uint)triIndex.X + 1, false, useNm);
                string i2 = FormatFace((uint)triIndex.Y + 1, false, useNm);
                string i3 = FormatFace((uint)triIndex.Z + 1, false, useNm);

                //Store CCW
                wr.WriteLine(string.Format(LoaderObj.Locale, "f {0} {2} {1}", i1, i2, i3));
            }
        }

        public static void WriteObj(StreamWriter wr, SubMeshContent s)
        {
            var vertices = s.Vertices;
            WriteData(wr, "v", vertices.Select(v => v.Position));
            bool useUv = WriteData(wr, "vt", vertices.Select(v => v.Texture));
            bool useNorm = WriteData(wr, "vn", vertices.Select(v => v.Normal));

            var indices = s.Indices;
            WriteData(wr, "f", indices, useUv, useNorm);
        }

        private static bool WriteData(StreamWriter wr, string dataType, IEnumerable<Vector2?> vectors)
        {
            bool hasData = true;

            foreach (var v in vectors)
            {
                if (!v.HasValue)
                {
                    continue;
                }

                wr.WriteLine(string.Format(LoaderObj.Locale, "{0} {1:0.000000000} {2:0.000000000}", dataType, v.Value.X, v.Value.Y));

                hasData = true;
            }

            return hasData;
        }

        private static bool WriteData(StreamWriter wr, string dataType, IEnumerable<Vector3?> vectors)
        {
            bool hasData = true;

            foreach (var v in vectors)
            {
                if (!v.HasValue)
                {
                    continue;
                }

                //To Right handed
                Vector3 vr = v.Value;
                vr.Z = -vr.Z;

                wr.WriteLine(string.Format(LoaderObj.Locale, "{0} {1:0.000000000} {2:0.000000000} {3:0.000000000}", dataType, vr.X, vr.Y, vr.Z));

                hasData = true;
            }

            return hasData;
        }

        private static void WriteData(StreamWriter wr, string dataType, IEnumerable<uint> indices, bool useUV, bool useNm)
        {
            for (int i = 0; i < indices.Count(); i += 3)
            {
                string i1 = FormatFace(indices.ElementAt(i + 0) + 1, useUV, useNm);
                string i2 = FormatFace(indices.ElementAt(i + 1) + 1, useUV, useNm);
                string i3 = FormatFace(indices.ElementAt(i + 2) + 1, useUV, useNm);

                wr.WriteLine(string.Format(LoaderObj.Locale, "{0} {1} {2} {3}", dataType, i1, i2, i3));
            }
        }

        private static string FormatFace(uint index, bool useUV, bool useNm)
        {
            if (useUV)
            {
                if (useNm)
                {
                    return $"{index}/{index}/{index}";
                }
                else
                {
                    return $"{index}/{index}";
                }
            }
            else
            {
                if (useNm)
                {
                    return $"{index}//{index}";
                }
                else
                {
                    return $"{index}";
                }
            }
        }
    }
}
