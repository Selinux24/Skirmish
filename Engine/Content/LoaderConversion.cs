using SharpDX;

namespace Engine.Content
{
    /// <summary>
    /// Geometry conversions
    /// </summary>
    public struct LoaderConversion
    {
        /// <summary>
        /// Geometry orientation conversions
        /// </summary>
        public enum GeometryOrientationConversions
        {
            /// <summary>
            /// No conversion
            /// </summary>
            None,
            /// <summary>
            /// X to Y
            /// </summary>
            XtoY,
            /// <summary>
            /// X to Z
            /// </summary>
            XtoZ,
            /// <summary>
            /// Y to X
            /// </summary>
            YtoX,
            /// <summary>
            /// Y to Z
            /// </summary>
            YtoZ,
            /// <summary>
            /// Z to X
            /// </summary>
            ZtoX,
            /// <summary>
            /// Z to Y
            /// </summary>
            ZtoY,
        }

        /// <summary>
        /// Conversion selection
        /// </summary>
        /// <param name="transpose">Do transposition</param>
        /// <param name="orientationFrom">Current up axis</param>
        /// <param name="orientationTo">Desired up axis</param>
        /// <returns>Returns the conversion to perform</returns>
        public static Matrix Compute(GeometryOrientations orientationFrom, GeometryOrientations orientationTo)
        {
            GeometryOrientationConversions axisConversion = GeometryOrientationConversions.None;

            if (orientationFrom == GeometryOrientations.XUp && orientationTo == GeometryOrientations.YUp)
            {
                axisConversion = GeometryOrientationConversions.XtoY;
            }
            else if (orientationFrom == GeometryOrientations.XUp && orientationTo == GeometryOrientations.ZUp)
            {
                axisConversion = GeometryOrientationConversions.XtoZ;
            }
            else if (orientationFrom == GeometryOrientations.YUp && orientationTo == GeometryOrientations.XUp)
            {
                axisConversion = GeometryOrientationConversions.YtoX;
            }
            else if (orientationFrom == GeometryOrientations.YUp && orientationTo == GeometryOrientations.ZUp)
            {
                axisConversion = GeometryOrientationConversions.YtoZ;
            }
            else if (orientationFrom == GeometryOrientations.ZUp && orientationTo == GeometryOrientations.XUp)
            {
                axisConversion = GeometryOrientationConversions.ZtoX;
            }
            else if (orientationFrom == GeometryOrientations.ZUp && orientationTo == GeometryOrientations.YUp)
            {
                axisConversion = GeometryOrientationConversions.ZtoY;
            }

            Matrix data;

            switch (axisConversion)
            {
                case GeometryOrientationConversions.XtoY:
                    data = Matrix.RotationZ(MathUtil.PiOverTwo);
                    break;
                case GeometryOrientationConversions.YtoX:
                    data = Matrix.RotationZ(-MathUtil.PiOverTwo);
                    break;
                case GeometryOrientationConversions.XtoZ:
                    data = Matrix.RotationY(MathUtil.PiOverTwo);
                    break;
                case GeometryOrientationConversions.ZtoX:
                    data = Matrix.RotationY(-MathUtil.PiOverTwo);
                    break;
                case GeometryOrientationConversions.YtoZ:
                    data = Matrix.RotationX(MathUtil.PiOverTwo);
                    break;
                case GeometryOrientationConversions.ZtoY:
                    data = Matrix.RotationX(-MathUtil.PiOverTwo);
                    break;
                default:
                    data = Matrix.Identity;
                    break;
            }

            return data;
        }
    }
}
