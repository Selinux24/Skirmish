using System;
using System.Globalization;

namespace Engine.Mathematics
{
    /// <summary>
    /// Camera parameters
    /// </summary>
    public struct FrustumCameraParams : IFormattable
    {
        const string CamParamsStr = "Near:{0} Far:{1} Aspect:{2} FoV: {3}";

        /// <summary>
        /// Position of the camera.
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Looking at direction of the camera.
        /// </summary>
        public Vector3 LookAtDir;
        /// <summary>
        /// Up direction.
        /// </summary>
        public Vector3 UpDir;
        /// <summary>
        /// Field of view.
        /// </summary>
        public float FOV;
        /// <summary>
        /// Z near distance.
        /// </summary>
        public float ZNear;
        /// <summary>
        /// Z far distance.
        /// </summary>
        public float ZFar;
        /// <summary>
        /// Aspect ratio.
        /// </summary>
        public float AspectRatio;

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                CamParamsStr,
                ZNear, ZFar, AspectRatio, FOV);
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                CamParamsStr,
                ZNear.ToString(format, formatProvider),
                ZFar.ToString(format, formatProvider),
                AspectRatio.ToString(format, formatProvider),
                FOV.ToString(format, formatProvider));
        }
    }
}
