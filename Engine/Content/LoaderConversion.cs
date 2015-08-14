using SharpDX;

namespace Engine.Content
{
    /// <summary>
    /// Geometry conversions
    /// </summary>
    public struct LoaderConversion
    {
        /// <summary>
        /// Coordinate system conversions
        /// </summary>
        public enum CoordinateSystemConversions
        {
            /// <summary>
            /// No conversion
            /// </summary>
            None,
            /// <summary>
            /// Left handed to right handed
            /// </summary>
            LHtoRH,
            /// <summary>
            /// Right handed to left handed
            /// </summary>
            RHtoLH,
        }
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
        /// <param name="orientationFrom">Current up axis</param>
        /// <param name="orientationTo">Desired up axis</param>
        /// <param name="coordinateFrom">Current coordinate system</param>
        /// <param name="coordinateTo">Desired coordinate system</param>
        /// <returns>Returns the conversion to perform</returns>
        public static LoaderConversion Compute(
            Matrix transform,
            CoordinateSystems coordinateFrom, CoordinateSystems coordinateTo, 
            GeometryOrientations orientationFrom, GeometryOrientations orientationTo)
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

            CoordinateSystemConversions coordianteConversion = CoordinateSystemConversions.None;

            if (coordinateFrom == CoordinateSystems.LeftHanded && coordinateTo == CoordinateSystems.RightHanded)
            {
                coordianteConversion = CoordinateSystemConversions.LHtoRH;
            }
            else if (coordinateFrom == CoordinateSystems.RightHanded && coordinateTo == CoordinateSystems.LeftHanded)
            {
                coordianteConversion = CoordinateSystemConversions.RHtoLH;
            }

            return new LoaderConversion()
            {
                Transform = transform,
                GeometryOrientationConversion = axisConversion,
                CoordinateSystemConversion = coordianteConversion,
            };
        }

        /// <summary>
        /// Transform
        /// </summary>
        public Matrix Transform;
        /// <summary>
        /// Coordinate system conversion
        /// </summary>
        public CoordinateSystemConversions CoordinateSystemConversion;
        /// <summary>
        /// Orientation conversion
        /// </summary>
        public GeometryOrientationConversions GeometryOrientationConversion;

        /// <summary>
        /// Apply transform matrix to position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns transformed position</returns>
        public Vector3 ApplyCoordinateTransform(Vector3 position)
        {
            return this.Transform.IsIdentity ? position : Vector3.TransformCoordinate(position, this.Transform);
        }
        /// <summary>
        /// Apply transform matrix to normal
        /// </summary>
        /// <param name="normal">Normal</param>
        /// <returns>Returns transformed normal</returns>
        public Vector3 ApplyNormalTransform(Vector3 normal)
        {
            return this.Transform.IsIdentity ? normal : Vector3.TransformNormal(normal, this.Transform);
        }
        /// <summary>
        /// Change coordinate system
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns vector</returns>
        public Vector3 ChangeCoordinateSystem(Vector3 vector)
        {
            if (this.CoordinateSystemConversion != CoordinateSystemConversions.None)
            {
                return new Vector3(-vector.X, vector.Y, vector.Z);
            }
            else
            {
                return vector;
            }
        }
        /// <summary>
        /// Change geometry orientation matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <returns>Returns changed matrix</returns>
        public Matrix ChangeGeometryOrientation(Matrix matrix)
        {
            Matrix data = matrix;

            if (this.GeometryOrientationConversion != GeometryOrientationConversions.None)
            {
                #region Rotation and scale

                // Columns first
                Vector3 tmp = new Vector3(data[0], data[4], data[8]);
                tmp = this.ChangeGeometryOrientation(tmp);
                data[0] = tmp[0];
                data[4] = tmp[1];
                data[8] = tmp[2];
                tmp = new Vector3(data[1], data[5], data[9]);
                tmp = this.ChangeGeometryOrientation(tmp);
                data[1] = tmp[0];
                data[5] = tmp[1];
                data[9] = tmp[2];
                tmp = new Vector3(data[2], data[6], data[10]);
                tmp = this.ChangeGeometryOrientation(tmp);
                data[2] = tmp[0];
                data[6] = tmp[1];
                data[10] = tmp[2];

                // Rows second
                tmp = new Vector3(data[0], data[1], data[2]);
                tmp = this.ChangeGeometryOrientation(tmp);
                data[0] = tmp[0];
                data[1] = tmp[1];
                data[2] = tmp[2];
                tmp = new Vector3(data[4], data[5], data[6]);
                tmp = this.ChangeGeometryOrientation(tmp);
                data[4] = tmp[0];
                data[5] = tmp[1];
                data[6] = tmp[2];
                tmp = new Vector3(data[8], data[9], data[10]);
                tmp = this.ChangeGeometryOrientation(tmp);
                data[8] = tmp[0];
                data[9] = tmp[1];
                data[10] = tmp[2];

                #endregion

                #region Translation

                tmp = new Vector3(data[3], data[7], data[11]);
                tmp = this.ChangeGeometryOrientation(tmp);
                data[3] = tmp[0];
                data[7] = tmp[1];
                data[11] = tmp[2];

                #endregion
            }

            return data;
        }
        /// <summary>
        /// Change geometry orientation vector
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns changed vector</returns>
        public Vector3 ChangeGeometryOrientation(Vector3 vector)
        {
            return ChangeAxis(vector, this.GeometryOrientationConversion, -1);
        }
        /// <summary>
        /// Change scale orientation vector
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns change scale</returns>
        public Vector3 ChangeScaleOrientationAxis(Vector3 vector)
        {
            return ChangeAxis(vector, this.GeometryOrientationConversion, 1);
        }
        /// <summary>
        /// Change axis to vector
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <param name="orientation">Orientation</param>
        /// <param name="sign">Sign</param>
        /// <returns>Return changed vector</returns>
        private static Vector3 ChangeAxis(Vector3 vector, GeometryOrientationConversions orientation, float sign)
        {
            Vector3 data = vector;

            if (orientation != GeometryOrientationConversions.None)
            {
                if (orientation == GeometryOrientationConversions.XtoY)
                {
                    var tmp = data[0];
                    data[0] = sign * data[1];
                    data[1] = tmp;
                }
                else if (orientation == GeometryOrientationConversions.XtoZ)
                {
                    var tmp = data[2];
                    data[2] = data[1];
                    data[1] = data[0];
                    data[0] = tmp;
                }
                else if (orientation == GeometryOrientationConversions.YtoX)
                {
                    var tmp = data[0];
                    data[0] = data[1];
                    data[1] = sign * tmp;
                }
                else if (orientation == GeometryOrientationConversions.YtoZ)
                {
                    var tmp = data[1];
                    data[1] = sign * data[2];
                    data[2] = tmp;
                }
                else if (orientation == GeometryOrientationConversions.ZtoX)
                {
                    var tmp = data[0];
                    data[0] = data[1];
                    data[1] = data[2];
                    data[2] = tmp;
                }
                else if (orientation == GeometryOrientationConversions.ZtoY)
                {
                    var tmp = data[1];
                    data[0] = data[0] * sign;
                    data[1] = data[2];
                    data[2] = tmp;
                }
            }

            return data;
        }
    }
}
