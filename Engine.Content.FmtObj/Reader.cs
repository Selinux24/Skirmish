using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Engine.Content.FmtObj
{
    static class Reader
    {
        /// <summary>
        /// Loads an obj file from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="folder">Content folder</param>
        /// <param name="transform">Transform to apply to all vertices</param>
        /// <param name="content">Result content</param>
        /// <param name="materials">Result materials</param>
        public static void LoadObj(Stream stream, string folder, Matrix transform, out IEnumerable<SubMeshContent> content, out IEnumerable<Material> materials)
        {
            List<SubMeshContent> models = new List<SubMeshContent>();

            var lines = ReadStreamLines(stream).ToList();

            //Read all materials libs file names
            List<string> matLibFiles = new List<string>();
            lines.ForEach(v => matLibFiles.AddRange(ReadMaterialFileName(folder, v, "mtllib")));

            //Read all materials
            List<Material> matLibs = new List<Material>();
            matLibs.AddRange(ReadMaterialsFromFile(matLibFiles));

            //Read all points
            List<Vector3> points = new List<Vector3>();
            lines.ForEach(v => points.AddRange(ReadVector3(v, "v")));

            //Read all texture uvs
            List<Vector2> uvs = new List<Vector2>();
            lines.ForEach(v => uvs.AddRange(ReadVector2(v, "vt")));

            //Read all normals
            List<Vector3> normals = new List<Vector3>();
            lines.ForEach(v => normals.AddRange(ReadVector3(v, "vn")));

            if (!transform.IsIdentity)
            {
                points = TransformCoordinate(points, transform);
                normals = TransformNormal(normals, transform);
            }

            var useMatIndices = lines.FindAllIndexOf("usemtl");
            if (useMatIndices.Any())
            {
                for (int i = 0; i < useMatIndices.Length; i++)
                {
                    int size = i == useMatIndices.Length - 1 ?
                        lines.Count - useMatIndices[i] :
                        useMatIndices[i + 1] - useMatIndices[i];

                    // Take the mesh definition lines
                    var submeshLines = lines.Skip(useMatIndices[i]).Take(size);

                    var mesh = CreateMesh(submeshLines, points, uvs, normals);
                    if (mesh != null)
                    {
                        models.Add(mesh);
                    }
                }
            }
            else
            {
                // One model
                var mesh = CreateMesh(lines, points, uvs, normals);
                if (mesh != null)
                {
                    models.Add(mesh);
                }
            }

            materials = matLibs.ToArray();
            content = models.ToArray();
        }
        private static IEnumerable<string> ReadStreamLines(Stream stream)
        {
            List<string> lines = new List<string>();

            using (StreamReader rd = new StreamReader(stream))
            {
                while (!rd.EndOfStream)
                {
                    string strLine = rd.ReadLine();
                    if (!string.IsNullOrWhiteSpace(strLine) && !strLine.StartsWith("#"))
                    {
                        lines.Add(strLine);
                    }
                }
            }

            return lines;
        }
        private static int[] FindAllIndexOf(this IEnumerable<string> values, string val)
        {
            return values.Select((b, i) => b.StartsWith(val, StringComparison.OrdinalIgnoreCase) ? i : -1).Where(i => i != -1).ToArray();
        }
        private static SubMeshContent CreateMesh(IEnumerable<string> submeshLines, IEnumerable<Vector3> points, IEnumerable<Vector2> uvs, IEnumerable<Vector3> normals)
        {
            // Get material name (if any)
            string material = ReadUseMaterial(submeshLines.First());

            // Read model faces
            List<Face[]> faces = new List<Face[]>();
            submeshLines.ToList().ForEach(v => faces.AddRange(ReadFaces(v)));

            if (faces.Any())
            {
                return CreateModel(material, points, uvs, normals, faces, 0);
            }

            return null;
        }

        private static List<Vector3> TransformCoordinate(IEnumerable<Vector3> list, Matrix transform)
        {
            var tmp = list.ToList();

            List<Vector3> res = new List<Vector3>();
            tmp.ForEach(p => res.Add(Vector3.TransformCoordinate(p, transform)));

            return res;
        }
        private static List<Vector3> TransformNormal(IEnumerable<Vector3> list, Matrix transform)
        {
            var tmp = list.ToList();

            List<Vector3> res = new List<Vector3>();
            tmp.ForEach(p => res.Add(Vector3.TransformNormal(p, transform)));

            return res;
        }

        private static string ReadString(string line, int index)
        {
            return line?.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(index);
        }
        private static float ReadFloat(string line, int index, float defaultValue = 0)
        {
            var tValue = line?.Split(" ".ToCharArray()).ElementAtOrDefault(index);
            if (float.TryParse(tValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float res))
            {
                return res;
            }

            return defaultValue;
        }
        private static Color3 ReadColor3(string line, int index)
        {
            float r = ReadFloat(line, index + 0);
            float g = ReadFloat(line, index + 1);
            float b = ReadFloat(line, index + 2);

            return new Color3(r, g, b);
        }
        private static IEnumerable<Vector2> ReadVector2(string strLine, string dataType)
        {
            if (strLine.StartsWith(dataType + " ", StringComparison.OrdinalIgnoreCase))
            {
                var numbers = strLine.Split(" ".ToArray(), StringSplitOptions.None);

                var v = new Vector2(
                    float.Parse(numbers[1], NumberStyles.Float, LoaderObj.Locale),
                    float.Parse(numbers[2], NumberStyles.Float, LoaderObj.Locale));

                v.Y = 1 - v.Y;

                return new[] { v };
            }

            return new Vector2[] { };
        }
        private static IEnumerable<Vector3> ReadVector3(string strLine, string dataType)
        {
            if (strLine.StartsWith(dataType + " ", StringComparison.OrdinalIgnoreCase))
            {
                var numbers = strLine.Split(" ".ToArray(), StringSplitOptions.None);

                var v = new Vector3(
                    float.Parse(numbers[1], NumberStyles.Float, LoaderObj.Locale),
                    float.Parse(numbers[2], NumberStyles.Float, LoaderObj.Locale),
                    float.Parse(numbers[3], NumberStyles.Float, LoaderObj.Locale));

                //To Left handed
                Vector3 vr = v;
                vr.Z = -vr.Z;

                return new[] { vr };
            }

            return new Vector3[] { };
        }
        private static IEnumerable<Face[]> ReadFaces(string strLine)
        {
            if (strLine.StartsWith("f ", StringComparison.OrdinalIgnoreCase))
            {
                var numbers = strLine.Split(" ".ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                numbers.RemoveAt(0);

                var face = numbers
                    .Select(n =>
                    {
                        List<uint> res = new List<uint>();

                        var nums = n.Split("/".ToArray(), StringSplitOptions.None);
                        foreach (var index in nums)
                        {
                            uint value = string.IsNullOrWhiteSpace(index) ?
                                0 :
                                uint.Parse(index, NumberStyles.Integer, LoaderObj.Locale);

                            res.Add(value);
                        }

                        return new Face(res.ToArray());
                    })
                    .ToArray();

                return new[] { face };
            }

            return new Face[][] { };
        }

        private static string ReadUseMaterial(string strLine)
        {
            if (strLine.StartsWith("usemtl ", StringComparison.OrdinalIgnoreCase))
            {
                return strLine?.Split(" ".ToArray(), StringSplitOptions.None)?.ElementAtOrDefault(1);
            }

            return null;
        }
        private static IEnumerable<string> ReadMaterialFileName(string folder, string strLine, string dataType)
        {
            if (strLine.StartsWith(dataType + " ", StringComparison.OrdinalIgnoreCase))
            {
                string materialFile = strLine.Split(" ".ToArray(), StringSplitOptions.None).ElementAtOrDefault(1);
                if (string.IsNullOrWhiteSpace(materialFile))
                {
                    return new string[] { };
                }

                string path = Path.Combine(folder, materialFile);
                if (File.Exists(path))
                {
                    return new[] { path };
                }
            }

            return new string[] { };
        }
        private static IEnumerable<Material> ReadMaterialsFromFile(IEnumerable<string> fileNames)
        {
            List<Material> materials = new List<Material>();

            foreach (var fileName in fileNames.Distinct())
            {
                string[] matLines = File.ReadAllLines(fileName);

                var matIndices = matLines.FindAllIndexOf("newmtl");
                for (int i = 0; i < matIndices.Length; i++)
                {
                    int size = i == matIndices.Length - 1 ?
                        matLines.Length - matIndices[i] :
                        matIndices[i + 1] - matIndices[i];

                    var mat = ReadMaterial(matLines.Skip(matIndices[i]).Take(size));
                    materials.Add(mat);
                }
            }

            return materials;
        }
        private static Material ReadMaterial(IEnumerable<string> lines)
        {
            return new Material()
            {
                Name = ReadString(lines.FirstOrDefault(l => l.StartsWith("newmtl ", StringComparison.OrdinalIgnoreCase)), 1),
                ShaderProfile = ReadString(lines.FirstOrDefault(l => l.StartsWith("illum ", StringComparison.OrdinalIgnoreCase)), 1),

                Ns = ReadFloat(lines.FirstOrDefault(l => l.StartsWith("ns ", StringComparison.OrdinalIgnoreCase)), 1),
                MapNs = ReadString(lines.FirstOrDefault(l => l.StartsWith("map_ns ", StringComparison.OrdinalIgnoreCase)), 1),

                Ka = ReadColor3(lines.FirstOrDefault(l => l.StartsWith("ka ", StringComparison.OrdinalIgnoreCase)), 1),
                MapKa = ReadString(lines.FirstOrDefault(l => l.StartsWith("map_ka ", StringComparison.OrdinalIgnoreCase)), 1),

                Kd = ReadColor3(lines.FirstOrDefault(l => l.StartsWith("kd ", StringComparison.OrdinalIgnoreCase)), 1),
                MapKd = ReadString(lines.FirstOrDefault(l => l.StartsWith("map_kd ", StringComparison.OrdinalIgnoreCase)), 1),

                Ks = ReadColor3(lines.FirstOrDefault(l => l.StartsWith("ks ", StringComparison.OrdinalIgnoreCase)), 1),
                MapKs = ReadString(lines.FirstOrDefault(l => l.StartsWith("map_ks ", StringComparison.OrdinalIgnoreCase)), 1),

                Ke = ReadColor3(lines.FirstOrDefault(l => l.StartsWith("ke ", StringComparison.OrdinalIgnoreCase)), 1),
                Ni = ReadFloat(lines.FirstOrDefault(l => l.StartsWith("ni ", StringComparison.OrdinalIgnoreCase)), 1),

                D = ReadFloat(lines.FirstOrDefault(l => l.StartsWith("d ", StringComparison.OrdinalIgnoreCase)), 1),
                MapD = ReadString(lines.FirstOrDefault(l => l.StartsWith("map_d ", StringComparison.OrdinalIgnoreCase)), 1),

                MapBump = ReadString(lines.FirstOrDefault(l => l.StartsWith("map_bump ", StringComparison.OrdinalIgnoreCase)), 1),
            };
        }

        private static IEnumerable<VertexData> CreateVertexData(IEnumerable<Vector3> points, IEnumerable<Vector2> uvs, IEnumerable<Vector3> normals, IEnumerable<Face[]> faces, int offset)
        {
            List<VertexData> vertexList = new List<VertexData>();

            int faceIndex = 0;
            foreach (var face in faces)
            {
                int vertexIndex = 0;
                foreach (var faceVertex in face)
                {
                    int vIndex = faceVertex.GetPositionIndex(offset);
                    int? uvIndex = faceVertex.GetUVIndex(offset);
                    int? nmIndex = faceVertex.GetNormalIndex(offset);

                    VertexData v = new VertexData
                    {
                        Position = points.ElementAt(vIndex),
                        Texture = uvIndex >= 0 ? (Vector2?)uvs.ElementAt(uvIndex.Value) : null,
                        Normal = nmIndex >= 0 ? (Vector3?)normals.ElementAt(nmIndex.Value) : null,
                        FaceIndex = faceIndex,
                        VertexIndex = vertexIndex++
                    };

                    vertexList.Add(v);
                }

                faceIndex++;
            }

            return vertexList.ToArray();
        }
        private static SubMeshContent CreateModel(string material, IEnumerable<Vector3> points, IEnumerable<Vector2> uvs, IEnumerable<Vector3> normals, IEnumerable<Face[]> faces, int offset)
        {
            var vertexList = CreateVertexData(points, uvs, normals, faces, offset);

            List<uint> mIndices = new List<uint>();

            //Generate indices
            for (int f = 0; f < faces.Count(); f++)
            {
                int nv = faces.ElementAt(f).Length;
                for (int i = 2; i < nv; i++)
                {
                    int iDelta = offset + (f * 3);

                    int a = i - 2 + iDelta;
                    int b = i - 1 + iDelta;
                    int c = i + iDelta;

                    //Read From CW to CCW
                    mIndices.Add((uint)a);
                    mIndices.Add((uint)c);
                    mIndices.Add((uint)b);
                }
            }

            if (!normals.Any())
            {
                vertexList = ComputeNormals(vertexList, mIndices);
            }

            SubMeshContent content = new SubMeshContent(Topology.TriangleList, material ?? ContentData.NoMaterial, uvs.Any(), false);

            content.SetVertices(vertexList);
            content.SetIndices(mIndices);

            return content;
        }
        private static IEnumerable<VertexData> ComputeNormals(IEnumerable<VertexData> vertexList, IEnumerable<uint> mIndices)
        {
            VertexData[] data = vertexList.ToArray();

            for (int i = 0; i < mIndices.Count(); i += 3)
            {
                int i1 = (int)mIndices.ElementAt(i + 0);
                int i2 = (int)mIndices.ElementAt(i + 1);
                int i3 = (int)mIndices.ElementAt(i + 2);

                VertexData p1 = data[i1];
                VertexData p2 = data[i2];
                VertexData p3 = data[i3];

                Triangle t = new Triangle(p1.Position.Value, p2.Position.Value, p3.Position.Value);

                p1.Normal = t.Normal;
                p2.Normal = t.Normal;
                p3.Normal = t.Normal;

                data[i1] = p1;
                data[i2] = p2;
                data[i3] = p3;
            }

            return data;
        }
    }
}
