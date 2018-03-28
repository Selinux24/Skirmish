using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine.Content
{
    /// <summary>
    /// Loader for .obj files
    /// </summary>
    public class LoaderOBJ : ILoader, IDisposable
    {
        public static CultureInfo Locale = CultureInfo.InvariantCulture;

        public LoaderOBJ()
        {

        }

        public void Dispose()
        {

        }

        public ModelContent[] Load(string contentFolder, ModelContentDescription content)
        {
            Matrix transform = Matrix.Identity;

            if (content.Scale != 1f)
            {
                transform = Matrix.Scaling(content.Scale);
            }

            return Load(contentFolder, content.ModelFileName, transform);
        }

        private ModelContent[] Load(string contentFolder, string fileName, Matrix transform)
        {
            MemoryStream[] modelList = ContentManager.FindContent(contentFolder, fileName);
            if (modelList != null && modelList.Length > 0)
            {
                ModelContent[] res = new ModelContent[modelList.Length];

                for (int i = 0; i < modelList.Length; i++)
                {
                    LoadObj(modelList[i], transform, out VertexData[] vertices, out uint[] indices);

                    res[i] = ModelContent.GenerateTriangleList(vertices, indices);
                }

                return res;
            }
            else
            {
                throw new EngineException(string.Format("Model not found: {0}", fileName));
            }
        }

        public void Save(Triangle[] triangles, string fileName)
        {
            List<Vector3> points = new List<Vector3>();
            List<Int3> indices = new List<Int3>();

            // Extract data from triangles
            for (int i = 0; i < triangles.Length; i++)
            {
                var p1Index = points.IndexOf(triangles[i].Point1);
                var p2Index = points.IndexOf(triangles[i].Point2);
                var p3Index = points.IndexOf(triangles[i].Point3);

                if (p1Index < 0)
                {
                    p1Index = points.Count;
                    points.Add(triangles[i].Point1);
                }
                if (p2Index < 0)
                {
                    p2Index = points.Count;
                    points.Add(triangles[i].Point2);
                }
                if (p3Index < 0)
                {
                    p3Index = points.Count;
                    points.Add(triangles[i].Point3);
                }

                indices.Add(new Int3(p1Index, p2Index, p3Index));
            }

            // Write the file
            using (StreamWriter wr = new StreamWriter(fileName, false, Encoding.Default))
            {
                foreach (var point in points)
                {
                    wr.WriteLine(string.Format(Locale, "v {0:0.000000000} {1:0.000000000} {2:0.000000000}", point.X, point.Y, point.Z));
                }

                foreach (var triIndex in indices)
                {
                    wr.WriteLine(string.Format(Locale, "f {0} {1} {2}", triIndex.X + 1, triIndex.Y + 1, triIndex.Z + 1));
                }
            }
        }

        private static void LoadObj(Stream stream, Matrix transform, out VertexData[] vertices, out uint[] indices)
        {
            List<VertexData> vertList = new List<VertexData>();
            List<uint> indexList = new List<uint>();

            bool doTransform = !transform.IsIdentity;

            using (StreamReader rd = new StreamReader(stream))
            {
                List<Vector3> points = new List<Vector3>();

                while (!rd.EndOfStream)
                {
                    string strLine = rd.ReadLine();
                    if (!string.IsNullOrWhiteSpace(strLine))
                    {
                        if (strLine.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                        {
                            var numbers = strLine.Split(" ".ToArray(), StringSplitOptions.None);

                            var v = new Vector3(
                                float.Parse(numbers[1], NumberStyles.Float, Locale),
                                float.Parse(numbers[2], NumberStyles.Float, Locale),
                                float.Parse(numbers[3], NumberStyles.Float, Locale));

                            if (doTransform)
                            {
                                v = Vector3.TransformCoordinate(v, transform);
                            }

                            points.Add(v);
                        }

                        if (strLine.StartsWith("f", StringComparison.OrdinalIgnoreCase))
                        {
                            var numbers = strLine.Split(" ".ToArray(), StringSplitOptions.None);

                            indexList.Add(uint.Parse(numbers[1], NumberStyles.Integer, Locale) - 1);
                            indexList.Add(uint.Parse(numbers[2], NumberStyles.Integer, Locale) - 1);
                            indexList.Add(uint.Parse(numbers[3], NumberStyles.Integer, Locale) - 1);
                        }
                    }
                }

                Vector3[] nrm = new Vector3[points.Count];

                for (int i = 0; i < indexList.Count; i += 3)
                {
                    var t = new Triangle(
                        points[(int)indexList[i + 0]],
                        points[(int)indexList[i + 1]],
                        points[(int)indexList[i + 2]]);

                    nrm[(int)indexList[i + 0]] = t.Normal;
                    nrm[(int)indexList[i + 1]] = t.Normal;
                    nrm[(int)indexList[i + 2]] = t.Normal;
                }

                for (int i = 0; i < points.Count; i++)
                {
                    vertList.Add(new VertexData
                    {
                        Position = points[i],
                        Normal = nrm[i],
                    });
                }
            }

            indices = indexList.ToArray();
            vertices = vertList.ToArray();
        }
    }
}
