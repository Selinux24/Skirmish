using SharpDX;

namespace Engine.Helpers
{
    public static class Extensions
    {
        public static Vector3 ToVector3(this Vector4 vector4)
        {
            return new Vector3(vector4.X, vector4.Y, vector4.Z);
        }

        public static Vector4 ToPosition(this Vector3 vector3)
        {
            return new Vector4(vector3.X, vector3.Y, vector3.Z, 1.0f);
        }

        public static Vector4 ToVector(this Vector3 vector3)
        {
            return new Vector4(vector3.X, vector3.Y, vector3.Z, 0.0f);
        }

        public static string Description(this Matrix matrix)
        {
            if (matrix.IsIdentity)
            {
                return "Identity";
            }
            else
            {
                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                matrix.Decompose(out scale, out rotation, out translation);

                if (MathUtil.IsZero(translation.X)) translation.X = 0f;
                if (MathUtil.IsZero(translation.Y)) translation.Y = 0f;
                if (MathUtil.IsZero(translation.Z)) translation.Z = 0f;

                if (MathUtil.IsZero(rotation.X)) rotation.X = 0f;
                if (MathUtil.IsZero(rotation.Y)) rotation.Y = 0f;
                if (MathUtil.IsZero(rotation.Z)) rotation.Z = 0f;
                if (MathUtil.IsZero(rotation.W)) rotation.W = 0f;
                if (MathUtil.IsOne(rotation.X)) rotation.X = 1f;
                if (MathUtil.IsOne(rotation.Y)) rotation.Y = 1f;
                if (MathUtil.IsOne(rotation.Z)) rotation.Z = 1f;
                if (MathUtil.IsOne(rotation.W)) rotation.W = 1f;

                if (MathUtil.IsZero(scale.X)) scale.X = 0f;
                if (MathUtil.IsZero(scale.Y)) scale.Y = 0f;
                if (MathUtil.IsZero(scale.Z)) scale.Z = 0f;
                if (MathUtil.IsOne(scale.X)) scale.X = 1f;
                if (MathUtil.IsOne(scale.Y)) scale.Y = 1f;
                if (MathUtil.IsOne(scale.Z)) scale.Z = 1f;

                string desc = "";

                if (translation != Vector3.Zero)
                {
                    desc += string.Format("Translation: {0}; ", translation);
                }

                if (rotation.Angle != 0f)
                {
                    desc += string.Format("Axis: {0}; Angle: {1}; ", rotation.Axis, rotation.Angle);
                }

                if (scale != Vector3.One)
                {
                    desc += string.Format("Scale: {0}; ", scale);
                }

                return desc.Trim();
            }
        }
    }
}
