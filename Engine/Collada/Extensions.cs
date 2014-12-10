using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine.Collada
{
    using Engine.Collada.Types;

    public static class Extensions
    {
        public static Color4 ToColor4(this BasicColor value)
        {
            if (value.Values != null && value.Values.Length == 3)
            {
                return new Color4(value.Values[0], value.Values[1], value.Values[2], 1f);
            }
            else if (value.Values != null && value.Values.Length == 4)
            {
                return new Color4(value.Values[0], value.Values[1], value.Values[2], value.Values[3]);
            }
            else
            {
                throw new Exception(string.Format("El valor no es un {0} válido.", value.GetType()));
            }
        }

        public static Vector3 ToVector3(this BasicFloat3 value)
        {
            if (value.Values != null && value.Values.Length == 3)
            {
                return new Vector3(value.Values[0], value.Values[1], value.Values[2]);
            }
            else
            {
                throw new Exception(string.Format("El valor no es un {0} válido.", value.GetType()));
            }
        }

        public static Vector4 ToVector4(this BasicFloat4 value)
        {
            if (value.Values != null && value.Values.Length == 4)
            {
                return new Vector4(value.Values[0], value.Values[1], value.Values[2], value.Values[3]);
            }
            else
            {
                throw new Exception(string.Format("El valor no es un {0} válido.", value.GetType()));
            }
        }

        public static Matrix ToMatrix(this BasicFloat4x4 value)
        {
            if (value.Values != null && value.Values.Length == 16)
            {
                Matrix m = new Matrix()
                {
                    M11 = value.Values[0],
                    M12 = value.Values[1],
                    M13 = value.Values[2],
                    M14 = value.Values[3],

                    M21 = value.Values[4],
                    M22 = value.Values[5],
                    M23 = value.Values[6],
                    M24 = value.Values[7],

                    M31 = value.Values[8],
                    M32 = value.Values[9],
                    M33 = value.Values[10],
                    M34 = value.Values[11],

                    M41 = value.Values[12],
                    M42 = value.Values[13],
                    M43 = value.Values[14],
                    M44 = value.Values[15],
                };

                return m;
            }
            else
            {
                throw new Exception(string.Format("El valor no es un {0} válido.", value.GetType()));
            }
        }

        public static Quaternion ToQuaternion(this BasicFloat4 value)
        {
            if (value.Values != null && value.Values.Length == 4)
            {
                return new Quaternion(value.Values[0], value.Values[1], value.Values[2], value.Values[3]);
            }
            else
            {
                throw new Exception(string.Format("El valor no es un {0} válido.", value.GetType()));
            }
        }

        public static Transforms ReadTransforms(this Node node)
        {
            Matrix finalTranslation = Matrix.Identity;
            Matrix finalRotation = Matrix.Identity;
            Matrix finalScale = Matrix.Identity;

            if (node.Translate != null)
            {
                BasicFloat3 loc = Array.Find(node.Translate, t => string.Equals(t.SId, "location"));
                if (loc != null) finalTranslation *= Matrix.Translation(loc.ToVector3());
            }

            if (node.Rotate != null)
            {
                BasicFloat4 rotX = Array.Find(node.Rotate, t => string.Equals(t.SId, "rotationX"));
                if (rotX != null)
                {
                    Vector4 r = rotX.ToVector4();
                    finalRotation *= Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }

                BasicFloat4 rotY = Array.Find(node.Rotate, t => string.Equals(t.SId, "rotationY"));
                if (rotY != null)
                {
                    Vector4 r = rotY.ToVector4();
                    finalRotation *= Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }

                BasicFloat4 rotZ = Array.Find(node.Rotate, t => string.Equals(t.SId, "rotationZ"));
                if (rotZ != null)
                {
                    Vector4 r = rotZ.ToVector4();
                    finalRotation *= Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }
            }

            if (node.Scale != null)
            {
                BasicFloat3 sca = Array.Find(node.Scale, t => string.Equals(t.SId, "scale"));
                if (sca != null) finalScale *= Matrix.Scaling(sca.ToVector3());
            }

            return new Transforms()
            {
                Translation = finalTranslation,
                Rotation = finalRotation,
                Scale = finalScale,
            };
        }

        public static Matrix ReadMatrix(this Node node)
        {
            Matrix m = Matrix.Identity;

            if (node.Matrix != null)
            {
                BasicFloat4x4 trn = Array.Find(node.Matrix, t => string.Equals(t.SId, "transform"));
                if (trn != null) m = trn.ToMatrix();
            }

            return m;
        }

        public static Matrix[] ReadMatrix(this Source source)
        {
            int length = source.TechniqueCommon.Accessor.Count;
            int stride = source.TechniqueCommon.Accessor.Stride;

            if (stride != 16)
            {
                throw new Exception(string.Format("Stride not supported for {1}: {0}", stride, typeof(Matrix)));
            }

            List<Matrix> mats = new List<Matrix>();

            for (int i = 0; i < length * stride; i += stride)
            {
                Matrix m = new Matrix()
                {
                    M11 = source.FloatArray[i + 0],
                    M12 = source.FloatArray[i + 1],
                    M13 = source.FloatArray[i + 2],
                    M14 = source.FloatArray[i + 3],

                    M21 = source.FloatArray[i + 4],
                    M22 = source.FloatArray[i + 5],
                    M23 = source.FloatArray[i + 6],
                    M24 = source.FloatArray[i + 7],

                    M31 = source.FloatArray[i + 8],
                    M32 = source.FloatArray[i + 9],
                    M33 = source.FloatArray[i + 10],
                    M34 = source.FloatArray[i + 11],

                    M41 = source.FloatArray[i + 12],
                    M42 = source.FloatArray[i + 13],
                    M43 = source.FloatArray[i + 14],
                    M44 = source.FloatArray[i + 15],
                };

                mats.Add(m);
            }

            return mats.ToArray();
        }

        public static Vector3[] ReadVector3(this Source source)
        {
            int length = source.TechniqueCommon.Accessor.Count;
            int stride = source.TechniqueCommon.Accessor.Stride;

            if (stride != 3)
            {
                throw new Exception(string.Format("Stride not supported for {1}: {0}", stride, typeof(Vector3)));
            }

            List<Vector3> verts = new List<Vector3>();

            for (int i = 0; i < length * stride; i += stride)
            {
                Vector3 v = new Vector3(
                    source.FloatArray[i + 0],
                    source.FloatArray[i + 1],
                    source.FloatArray[i + 2]);

                verts.Add(v);
            }

            return verts.ToArray();
        }

        public static Vector2[] ReadVector2(this Source source)
        {
            int length = source.TechniqueCommon.Accessor.Count;
            int stride = source.TechniqueCommon.Accessor.Stride;

            if (stride != 2)
            {
                throw new Exception(string.Format("Stride not supported for {1}: {0}", stride, typeof(Vector2)));
            }

            List<Vector2> verts = new List<Vector2>();

            for (int i = 0; i < length * stride; i += stride)
            {
                Vector2 v = new Vector2(
                    source.FloatArray[i + 0],
                    source.FloatArray[i + 1]);

                verts.Add(v);
            }

            return verts.ToArray();
        }

        public static float[] ReadFloat(this Source source)
        {
            int length = source.TechniqueCommon.Accessor.Count;
            int stride = source.TechniqueCommon.Accessor.Stride;

            if (stride != 1)
            {
                throw new Exception(string.Format("Stride not supported for {1}: {0}", stride, typeof(float)));
            }

            List<float> n = new List<float>();

            for (int i = 0; i < length * stride; i += stride)
            {
                float v = source.FloatArray[i];

                n.Add(v);
            }

            return n.ToArray();
        }

        public static string[] ReadString(this Source source)
        {
            int length = source.TechniqueCommon.Accessor.Count;
            int stride = source.TechniqueCommon.Accessor.Stride;

            if (stride != 1)
            {
                throw new Exception(string.Format("Stride not supported for {1}: {0}", stride, typeof(string)));
            }

            List<string> names = new List<string>();

            for (int i = 0; i < length * stride; i += stride)
            {
                string v = source.NameArray[i];

                names.Add(v);
            }

            return names.ToArray();
        }

        public static Matrix[] ChangeAxis(this Matrix[] m, EnumAxisConversion fixAxis)
        {
            List<Matrix> res = new List<Matrix>();

            if (m != null && m.Length > 0)
            {
                for (int i = 0; i < m.Length; i++)
                {
                    res.Add(m[i].ChangeAxis(fixAxis));
                }
            }

            return res.ToArray();
        }

        public static Matrix ChangeAxis(this Matrix m, EnumAxisConversion fixAxis)
        {
            Matrix data = m;

            if (fixAxis != EnumAxisConversion.None)
            {
                #region Rotation and scale

                // Columns first
                Vector3 tmp = new Vector3(data[0], data[4], data[8]);
                tmp = tmp.ChangeTransformAxis(fixAxis);
                data[0] = tmp[0];
                data[4] = tmp[1];
                data[8] = tmp[2];
                tmp = new Vector3(data[1], data[5], data[9]);
                tmp = tmp.ChangeTransformAxis(fixAxis);
                data[1] = tmp[0];
                data[5] = tmp[1];
                data[9] = tmp[2];
                tmp = new Vector3(data[2], data[6], data[10]);
                tmp = tmp.ChangeTransformAxis(fixAxis);
                data[2] = tmp[0];
                data[6] = tmp[1];
                data[10] = tmp[2];

                // Rows second
                tmp = new Vector3(data[0], data[1], data[2]);
                tmp = tmp.ChangeTransformAxis(fixAxis);
                data[0] = tmp[0];
                data[1] = tmp[1];
                data[2] = tmp[2];
                tmp = new Vector3(data[4], data[5], data[6]);
                tmp = tmp.ChangeTransformAxis(fixAxis);
                data[4] = tmp[0];
                data[5] = tmp[1];
                data[6] = tmp[2];
                tmp = new Vector3(data[8], data[9], data[10]);
                tmp = tmp.ChangeTransformAxis(fixAxis);
                data[8] = tmp[0];
                data[9] = tmp[1];
                data[10] = tmp[2];

                #endregion

                #region Translation

                tmp = new Vector3(data[3], data[7], data[11]);
                tmp = tmp.ChangeTransformAxis(fixAxis);
                data[3] = tmp[0];
                data[7] = tmp[1];
                data[11] = tmp[2];

                #endregion
            }

            return data;
        }

        public static Vector3 ChangeTransformAxis(this Vector3 v, EnumAxisConversion fixAxis)
        {
            return v.ChangeAxis(fixAxis, -1);
        }

        public static Vector3 ChangeScaleAxis(this Vector3 v, EnumAxisConversion fixAxis)
        {
            return v.ChangeAxis(fixAxis, 1);
        }

        private static Vector3 ChangeAxis(this Vector3 v, EnumAxisConversion fixAxis, float sign)
        {
            Vector3 data = v;

            if (fixAxis != EnumAxisConversion.None)
            {
                if (fixAxis == EnumAxisConversion.XtoY)
                {
                    var tmp = data[0];
                    data[0] = sign * data[1];
                    data[1] = tmp;
                }
                else if (fixAxis == EnumAxisConversion.XtoZ)
                {
                    var tmp = data[2];
                    data[2] = data[1];
                    data[1] = data[0];
                    data[0] = tmp;
                }
                else if (fixAxis == EnumAxisConversion.YtoX)
                {
                    var tmp = data[0];
                    data[0] = data[1];
                    data[1] = sign * tmp;
                }
                else if (fixAxis == EnumAxisConversion.YtoZ)
                {
                    var tmp = data[1];
                    data[1] = sign * data[2];
                    data[2] = tmp;
                }
                else if (fixAxis == EnumAxisConversion.ZtoX)
                {
                    var tmp = data[0];
                    data[0] = data[1];
                    data[1] = data[2];
                    data[2] = tmp;
                }
                else if (fixAxis == EnumAxisConversion.ZtoY)
                {
                    var tmp = data[1];
                    data[1] = data[2];
                    data[2] = sign * tmp;
                }
            }

            return data;
        }
    }
}
