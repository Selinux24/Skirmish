using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Content.FmtCollada
{
    using Engine.Collada;
    using Engine.Collada.Types;
    using Engine.Common;

    /// <summary>
    /// Extensions for collada to sharpDX data parse
    /// </summary>
    static class LoaderColladaExtensions
    {
        /// <summary>
        /// Reads a Vector2 from BasicFloat2
        /// </summary>
        /// <param name="vector">BasicFloat2 vector</param>
        /// <returns>Returns the parsed Vector2 from BasicFloat2</returns>
        public static Vector2 ToVector2(this BasicFloat2 vector)
        {
            if (vector.Values != null && vector.Values.Length == 2)
            {
                return new Vector2(vector.Values[0], vector.Values[1]);
            }
            else
            {
                throw new EngineException("Value cannot be parsed to Vector2.");
            }
        }
        /// <summary>
        /// Reads a Vector3 from BasicFloat3
        /// </summary>
        /// <param name="vector">BasicFloat3 vector</param>
        /// <returns>Returns the parsed Vector3 from BasicFloat3</returns>
        public static Vector3 ToVector3(this BasicFloat3 vector)
        {
            if (vector.Values != null && vector.Values.Length == 3)
            {
                return new Vector3(vector.Values[0], vector.Values[2], vector.Values[1]);
            }
            else
            {
                throw new EngineException("Value cannot be parsed to Vector3.");
            }
        }
        /// <summary>
        /// Reads a Vector4 from BasicFloat4
        /// </summary>
        /// <param name="vector">BasicFloat4 vector</param>
        /// <returns>Returns the parsed Vector4 from BasicFloat4</returns>
        public static Vector4 ToVector4(this BasicFloat4 vector)
        {
            if (vector.Values != null && vector.Values.Length == 4)
            {
                return new Vector4(vector.Values[0], vector.Values[2], vector.Values[1], vector.Values[3]);
            }
            else
            {
                throw new EngineException("Value cannot be parsed to Vector4.");
            }
        }
        /// <summary>
        /// Reads a Color3 from BasicColor
        /// </summary>
        /// <param name="color">BasicColor color</param>
        /// <returns>Returns the parsed Color3 from BasicColor</returns>
        public static Color3 ToColor3(this BasicColor color)
        {
            if (color.Values != null && color.Values.Length == 3)
            {
                return new Color3(color.Values[0], color.Values[1], color.Values[2]);
            }
            else if (color.Values != null && color.Values.Length == 4)
            {
                return new Color3(color.Values[0], color.Values[1], color.Values[2]);
            }
            else
            {
                throw new EngineException("Value cannot be parsed to Color2.");
            }
        }
        /// <summary>
        /// Reads a Color4 from BasicColor
        /// </summary>
        /// <param name="color">BasicColor color</param>
        /// <returns>Returns the parsed Color4 from BasicColor</returns>
        public static Color4 ToColor4(this BasicColor color)
        {
            if (color.Values != null && color.Values.Length == 3)
            {
                return new Color4(color.Values[0], color.Values[1], color.Values[2], 1f);
            }
            else if (color.Values != null && color.Values.Length == 4)
            {
                return new Color4(color.Values[0], color.Values[1], color.Values[2], color.Values[3]);
            }
            else
            {
                throw new EngineException("Value cannot be parsed to Color4.");
            }
        }
        /// <summary>
        /// Reads a Matrix from BasicFloat4x4
        /// </summary>
        /// <param name="matrix">BasicFloat4x4 matrix</param>
        /// <returns>Returns the parsed Matrix from BasicFloat4x4</returns>
        /// <remarks>
        /// From right handed
        /// { rx, ry, rz, 0 }
        /// { ux, uy, uz, 0 }
        /// { lx, ly, lz, 0 }
        /// { px, py, pz, 1 }
        /// To left handed
        /// { rx, rz, ry, 0 }
        /// { lx, lz, ly, 0 }
        /// { ux, uz, uy, 0 }
        /// { px, pz, py, 1 }
        /// </remarks>
        public static Matrix ToMatrix(this BasicFloat4X4 matrix)
        {
            if (matrix.Values != null && matrix.Values.Length == 16)
            {
                Matrix m = new Matrix()
                {
                    M11 = matrix.Values[0],
                    M12 = matrix.Values[2],
                    M13 = matrix.Values[1],
                    M14 = matrix.Values[3],

                    M31 = matrix.Values[4],
                    M32 = matrix.Values[6],
                    M33 = matrix.Values[5],
                    M34 = matrix.Values[7],

                    M21 = matrix.Values[8],
                    M22 = matrix.Values[10],
                    M23 = matrix.Values[9],
                    M24 = matrix.Values[11],

                    M41 = matrix.Values[12],
                    M42 = matrix.Values[14],
                    M43 = matrix.Values[13],
                    M44 = matrix.Values[15],
                };

                return m;
            }
            else
            {
                throw new EngineException("Value cannot be parsed to Matrix 4x4.");
            }
        }

        /// <summary>
        /// Reads a float array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the float array</returns>
        public static float[] ReadFloat(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            int length = source.TechniqueCommon.Accessor.Count;

            return ReadArray(source.FloatArray.Values, length, stride);
        }
        /// <summary>
        /// Reads a string array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the string array</returns>
        public static string[] ReadNames(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            int length = source.TechniqueCommon.Accessor.Count;

            return ReadArray(source.NameArray.Values, length, stride);
        }
        /// <summary>
        /// Reads a string array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the string array</returns>
        public static string[] ReadIDRefs(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            int length = source.TechniqueCommon.Accessor.Count;

            return ReadArray(source.IdRefArray.Values, length, stride);
        }
        /// <summary>
        /// Reads a Vector2 array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Vector2 array</returns>
        public static Vector2[] ReadVector2(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 2)
            {
                Logger.WriteWarning(nameof(LoaderCollada), $"Stride not supported for {stride}: {typeof(Vector2)}");

                return new Vector2[] { };
            }

            int length = source.TechniqueCommon.Accessor.Count;

            int s = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "S");
            int t = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "T");

            List<Vector2> verts = new List<Vector2>();

            for (int i = 0; i < length * stride; i += stride)
            {
                Vector2 v = new Vector2(
                    source.FloatArray[i + s],
                    source.FloatArray[i + t]);

                verts.Add(v);
            }

            return verts.ToArray();
        }
        /// <summary>
        /// Reads a Vector3 array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Vector3 array</returns>
        public static Vector3[] ReadVector3(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 3)
            {
                Logger.WriteWarning(nameof(LoaderCollada), $"Stride not supported for {stride}: {typeof(Vector3)}");

                return new Vector3[] { };
            }

            int x = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "X");
            int y = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "Y");
            int z = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "Z");

            int length = source.TechniqueCommon.Accessor.Count;

            List<Vector3> verts = new List<Vector3>();

            for (int i = 0; i < length * stride; i += stride)
            {
                //To left handed -> Z flipped to Y
                Vector3 v = new Vector3(
                    source.FloatArray[i + x],
                    source.FloatArray[i + z],
                    source.FloatArray[i + y]);

                verts.Add(v);
            }

            return verts.ToArray();
        }
        /// <summary>
        /// Reads a Vector4 array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Vector4 array</returns>
        public static Vector4[] ReadVector4(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 4)
            {
                Logger.WriteWarning(nameof(LoaderCollada), $"Stride not supported for {stride}: {typeof(Vector4)}");

                return new Vector4[] { };
            }

            int x = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "X");
            int y = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "Y");
            int z = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "Z");
            int w = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "W");

            int length = source.TechniqueCommon.Accessor.Count;

            List<Vector4> verts = new List<Vector4>();

            for (int i = 0; i < length * stride; i += stride)
            {
                //To left handed -> Z flipped to Y
                Vector4 v = new Vector4(
                    source.FloatArray[i + x],
                    source.FloatArray[i + z],
                    source.FloatArray[i + y],
                    source.FloatArray[i + w]);

                verts.Add(v);
            }

            return verts.ToArray();
        }
        /// <summary>
        /// Reads a Color3 array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Color3 array</returns>
        public static Color3[] ReadColor3(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 3)
            {
                Logger.WriteWarning(nameof(LoaderCollada), $"Stride not supported for {stride}: {typeof(Color3)}");

                return new Color3[] { };
            }

            int r = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "R");
            int g = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "G");
            int b = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "B");

            int length = source.TechniqueCommon.Accessor.Count;

            List<Color3> colors = new List<Color3>();

            for (int i = 0; i < length * stride; i += stride)
            {
                //To left handed -> Z flipped to Y
                Color3 v = new Color3(
                    source.FloatArray[i + r],
                    source.FloatArray[i + g],
                    source.FloatArray[i + b]);

                colors.Add(v);
            }

            return colors.ToArray();
        }
        /// <summary>
        /// Reads a Color4 array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Color4 array</returns>
        public static Color4[] ReadColor4(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 4)
            {
                Logger.WriteWarning(nameof(LoaderCollada), $"Stride not supported for {stride}: {typeof(Color4)}");

                return new Color4[] { };
            }

            int r = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "R");
            int g = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "G");
            int b = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "B");
            int a = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "A");

            int length = source.TechniqueCommon.Accessor.Count;

            List<Color4> colors = new List<Color4>();

            for (int i = 0; i < length * stride; i += stride)
            {
                //To left handed -> Z flipped to Y
                Color4 v = new Color4(
                    source.FloatArray[i + r],
                    source.FloatArray[i + g],
                    source.FloatArray[i + b],
                    source.FloatArray[i + a]);

                colors.Add(v);
            }

            return colors.ToArray();
        }
        /// <summary>
        /// Reads a Matrix array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Matrix array</returns>
        /// <remarks>
        /// From right handed
        /// { rx, ry, rz, 0 }
        /// { ux, uy, uz, 0 }
        /// { lx, ly, lz, 0 }
        /// { px, py, pz, 1 }
        /// To left handed
        /// { rx, rz, ry, 0 }
        /// { lx, lz, ly, 0 }
        /// { ux, uz, uy, 0 }
        /// { px, pz, py, 1 }
        /// </remarks>
        public static Matrix[] ReadMatrix(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 16)
            {
                Logger.WriteWarning(nameof(LoaderCollada), $"Stride not supported for {stride}: {typeof(Matrix)}");

                return new Matrix[] { };
            }

            int length = source.TechniqueCommon.Accessor.Count;

            List<Matrix> mats = new List<Matrix>();

            for (int i = 0; i < length * stride; i += stride)
            {
                Matrix m = new Matrix()
                {
                    M11 = source.FloatArray[i + 0],
                    M12 = source.FloatArray[i + 8],
                    M13 = source.FloatArray[i + 4],
                    M14 = source.FloatArray[i + 12],

                    M21 = source.FloatArray[i + 2],
                    M22 = source.FloatArray[i + 10],
                    M23 = source.FloatArray[i + 6],
                    M24 = source.FloatArray[i + 14],

                    M31 = source.FloatArray[i + 1],
                    M32 = source.FloatArray[i + 9],
                    M33 = source.FloatArray[i + 5],
                    M34 = source.FloatArray[i + 13],

                    M41 = source.FloatArray[i + 3],
                    M43 = source.FloatArray[i + 7],
                    M42 = source.FloatArray[i + 11],
                    M44 = source.FloatArray[i + 15],
                };

                mats.Add(m);
            }

            return mats.ToArray();
        }
        /// <summary>
        /// Reads an array
        /// </summary>
        /// <typeparam name="T">Array type</typeparam>
        /// <param name="array">Value array</param>
        /// <param name="length">Length</param>
        /// <param name="stride">Stride</param>
        public static T[] ReadArray<T>(IEnumerable<T> array, int length, int stride)
        {
            List<T> n = new List<T>();

            for (int i = 0; i < length * stride; i += stride)
            {
                for (int x = 0; x < stride; x++)
                {
                    T v = array.ElementAt(i + x);

                    n.Add(v);
                }
            }

            return n.ToArray();
        }

        /// <summary>
        /// Reads a transform matrix (SRT) from a Node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the parsed matrix</returns>
        public static Matrix ReadMatrix(this Node node)
        {
            if (node.Matrix != null)
            {
                return ReadMatrixFromTransform(node.Matrix);
            }
            else
            {
                return ReadMatrixLocRotScale(node.Translate, node.Rotate, node.Scale);
            }
        }
        /// <summary>
        /// Reads a transform matrix (SRT) from a matrix in the Node
        /// </summary>
        /// <param name="matrix">Node matrix</param>
        /// <returns>Returns the parsed matrix</returns>
        public static Matrix ReadMatrixFromTransform(BasicFloat4X4[] matrix)
        {
            if (matrix != null)
            {
                BasicFloat4X4 trn = Array.Find(matrix, t => string.Equals(t.SId, "transform"));
                if (trn != null)
                {
                    Matrix m = trn.ToMatrix();

                    return Matrix.Transpose(m);
                }
            }

            return Matrix.Identity;
        }
        /// <summary>
        /// Reads a transform matrix (SRT) from the specified transforms in the Node
        /// </summary>
        /// <param name="translate">Translate</param>
        /// <param name="rotate">Rotate</param>
        /// <param name="scale">Scale</param>
        /// <returns>Returns the parsed matrix</returns>
        public static Matrix ReadMatrixLocRotScale(BasicFloat3[] translate, BasicFloat4[] rotate, BasicFloat3[] scale)
        {
            Matrix finalTranslation = Matrix.Identity;
            Matrix finalRotation = Matrix.Identity;
            Matrix finalScale = Matrix.Identity;

            if (translate != null)
            {
                BasicFloat3 loc = Array.Find(translate, t => string.Equals(t.SId, "location"));
                if (loc != null) finalTranslation *= Matrix.Translation(loc.ToVector3());
            }

            if (rotate != null)
            {
                BasicFloat4 rotX = Array.Find(rotate, t => string.Equals(t.SId, "rotationX"));
                if (rotX != null)
                {
                    Vector4 r = rotX.ToVector4();
                    finalRotation *= Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }

                BasicFloat4 rotY = Array.Find(rotate, t => string.Equals(t.SId, "rotationY"));
                if (rotY != null)
                {
                    Vector4 r = rotY.ToVector4();
                    finalRotation *= Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }

                BasicFloat4 rotZ = Array.Find(rotate, t => string.Equals(t.SId, "rotationZ"));
                if (rotZ != null)
                {
                    Vector4 r = rotZ.ToVector4();
                    finalRotation *= Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }
            }

            if (scale != null)
            {
                BasicFloat3 sca = Array.Find(scale, t => string.Equals(t.SId, "scale"));
                if (sca != null) finalScale *= Matrix.Scaling(sca.ToVector3());
            }

            return finalScale * finalRotation * finalTranslation;
        }

        /// <summary>
        /// Adds vertex input to vertex data
        /// </summary>
        /// <param name="vert">Vertex data instance</param>
        /// <param name="vertexInput">Input</param>
        /// <param name="positions">Positions list</param>
        /// <param name="indices">Index list</param>
        /// <param name="index">Current index</param>
        /// <returns>Returns the updated vertex data in a new instance</returns>
        internal static VertexData UpdateVertexInput(this VertexData vert, Input vertexInput, Vector3[] positions, BasicIntArray indices, int index)
        {
            if (vertexInput != null)
            {
                var vIndex = indices[index + vertexInput.Offset];
                vert.VertexIndex = vIndex;
                vert.Position = positions[vIndex];
            }

            return vert;
        }
        /// <summary>
        /// Adds normal input to vertex data
        /// </summary>
        /// <param name="vert">Vertex data instance</param>
        /// <param name="normalInput">Input</param>
        /// <param name="normals">Normals list</param>
        /// <param name="indices">Index list</param>
        /// <param name="index">Current index</param>
        /// <returns>Returns the updated vertex data in a new instance</returns>
        internal static VertexData UpdateNormalInput(this VertexData vert, Input normalInput, Vector3[] normals, BasicIntArray indices, int index)
        {
            if (normalInput != null)
            {
                var nIndex = indices[index + normalInput.Offset];
                vert.Normal = normals[nIndex];
            }

            return vert;
        }
        /// <summary>
        /// Adds texture coordinates input to vertex data
        /// </summary>
        /// <param name="vert">Vertex data instance</param>
        /// <param name="texCoordInput">Input</param>
        /// <param name="texCoords">Coordinates list</param>
        /// <param name="indices">Index list</param>
        /// <param name="index">Current index</param>
        /// <returns>Returns the updated vertex data in a new instance</returns>
        internal static VertexData UpdateTexCoordInput(this VertexData vert, Input texCoordInput, Vector2[] texCoords, BasicIntArray indices, int index)
        {
            if (texCoordInput != null)
            {
                var tIndex = indices[index + texCoordInput.Offset];
                Vector2 tex = texCoords[tIndex];

                //Invert Vertical coordinate
                tex.Y = -tex.Y;

                vert.Texture = tex;
            }

            return vert;
        }
        /// <summary>
        /// Adds color input to vertex data
        /// </summary>
        /// <param name="vert">Vertex data instance</param>
        /// <param name="colorsInput">Input</param>
        /// <param name="colors">Colors list</param>
        /// <param name="indices">Index list</param>
        /// <param name="index">Current index</param>
        /// <returns>Returns the updated vertex data in a new instance</returns>
        internal static VertexData UpdateColorsInput(this VertexData vert, Input colorsInput, Color3[] colors, BasicIntArray indices, int index)
        {
            if (colorsInput != null)
            {
                int cIndex = indices[index + colorsInput.Offset];
                vert.Color = new Color4(colors[cIndex], 1);
            }

            return vert;
        }
    }
}
