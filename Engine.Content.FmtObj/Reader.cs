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
        /// <param name="transform">Transform to apply to all vertices</param>
        /// <param name="content">Result content</param>
        public static void LoadObj(Stream stream, Matrix transform, out IEnumerable<SubMeshContent> content)
        {
            List<SubMeshContent> models = new List<SubMeshContent>();

            bool doTransform = !transform.IsIdentity;

            using (StreamReader rd = new StreamReader(stream))
            {
                List<Vector3> points = new List<Vector3>();
                List<Vector2> uvs = new List<Vector2>();
                List<Vector3> normals = new List<Vector3>();
                List<Face[]> indices = new List<Face[]>();

                bool currIsPosition = false;
                bool prevIsPosition = false;

                int offset = 0;

                while (!rd.EndOfStream)
                {
                    string strLine = rd.ReadLine();
                    if (!string.IsNullOrWhiteSpace(strLine))
                    {
                        prevIsPosition = currIsPosition;

                        var ps = ReadVector3(strLine, "v", doTransform, transform);
                        currIsPosition = ps.Any();

                        bool modelBegin = !prevIsPosition && currIsPosition;
                        if (modelBegin && points.Any())
                        {
                            models.Add(CreateModel(points, uvs, normals, indices, offset));

                            offset += points.Count;

                            points.Clear();
                            uvs.Clear();
                            normals.Clear();
                            indices.Clear();
                        }

                        points.AddRange(ps);
                        uvs.AddRange(ReadVector2(strLine, "vt"));
                        normals.AddRange(ReadVector3(strLine, "vn", doTransform, transform));
                        indices.AddRange(ReadIndices(strLine));
                    }
                }

                if (indices.Any())
                {
                    models.Add(CreateModel(points, uvs, normals, indices, offset));
                }
            }

            content = models.ToArray();
        }

        private static IEnumerable<Vector2> ReadVector2(string strLine, string dataType)
        {
            if (strLine.StartsWith(dataType + " ", StringComparison.OrdinalIgnoreCase))
            {
                var numbers = strLine.Split(" ".ToArray(), StringSplitOptions.None);

                var v = new Vector2(
                    float.Parse(numbers[1], NumberStyles.Float, LoaderObj.Locale),
                    float.Parse(numbers[2], NumberStyles.Float, LoaderObj.Locale));

                return new[] { v };
            }

            return new Vector2[] { };
        }

        private static IEnumerable<Vector3> ReadVector3(string strLine, string dataType, bool doTransform, Matrix transform)
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

                if (doTransform)
                {
                    vr = Vector3.TransformNormal(vr, transform);
                }

                return new[] { vr };
            }

            return new Vector3[] { };
        }

        private static IEnumerable<Face[]> ReadIndices(string strLine)
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

        private static IEnumerable<VertexData> CreateVertexData(List<Vector3> points, List<Vector2> uvs, List<Vector3> normals, List<Face[]> faces, int offset)
        {
            List<VertexData> vertexList = new List<VertexData>();

            foreach (var vertex in points)
            {
                vertexList.Add(new VertexData
                {
                    Position = vertex,
                });
            }

            int faceIndex = 0;
            foreach (var face in faces)
            {
                int vertexIndex = 0;
                foreach (var faceVertex in face)
                {
                    int vIndex = faceVertex.GetPositionIndex(offset);
                    int? uvIndex = faceVertex.GetUVIndex(offset);
                    int? nmIndex = faceVertex.GetNormalIndex(offset);

                    VertexData v = vertexList[vIndex];

                    v.Texture = uvIndex >= 0 ? (Vector2?)uvs[uvIndex.Value] : null;
                    v.Normal = nmIndex >= 0 ? (Vector3?)normals[nmIndex.Value] : null;
                    v.FaceIndex = faceIndex;
                    v.VertexIndex = vertexIndex++;

                    vertexList[vIndex] = v;
                }

                faceIndex++;
            }

            return vertexList.ToArray();
        }

        private static SubMeshContent CreateModel(List<Vector3> points, List<Vector2> uvs, List<Vector3> normals, List<Face[]> faces, int offset)
        {
            var vertexList = CreateVertexData(points, uvs, normals, faces, offset);

            List<uint> mIndices = new List<uint>();

            //Generate indices
            foreach (var face in faces)
            {
                int nv = face.Length;
                for (int i = 2; i < nv; i++)
                {
                    int a = face[0].GetPositionIndex(offset);
                    int b = face[i - 1].GetPositionIndex(offset);
                    int c = face[i].GetPositionIndex(offset);

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

            SubMeshContent content = new SubMeshContent(Topology.TriangleList, ModelContent.NoMaterial, false, false);

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
