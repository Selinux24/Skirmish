﻿using SharpDX;

namespace Engine
{
    /// <summary>
    /// Picking ray
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public struct PickingRay(Vector3 position, Vector3 direction, PickingHullTypes rayPickingParams, float length)
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position { get; set; } = position;
        /// <summary>
        /// Direction
        /// </summary>
        public Vector3 Direction { get; set; } = direction;
        /// <summary>
        /// Ray length
        /// </summary>
        public float RayLength { get; set; } = length;
        /// <summary>
        /// Picking parameters
        /// </summary>
        public PickingHullTypes RayPickingParams { get; set; } = rayPickingParams;
        /// <summary>
        /// Detect facing only primitive normals
        /// </summary>
        public readonly bool FacingOnly
        {
            get
            {
                return RayPickingParams.HasFlag(PickingHullTypes.FacingOnly);
            }
        }
        /// <summary>
        /// Maximum ray distance
        /// </summary>
        public readonly float MaxDistance
        {
            get
            {
                return RayLength <= 0 ? float.MaxValue : RayLength;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Ray ray) : this(ray.Position, ray.Direction, PickingHullTypes.Default, float.MaxValue)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Ray ray, PickingHullTypes rayPickingParams) : this(ray.Position, ray.Direction, rayPickingParams, float.MaxValue)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Ray ray, PickingHullTypes rayPickingParams, float length) : this(ray.Position, ray.Direction, rayPickingParams, length)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Vector3 position, Vector3 direction) : this(position, direction, PickingHullTypes.Default, float.MaxValue)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public PickingRay(Vector3 position, Vector3 direction, PickingHullTypes rayPickingParams) : this(position, direction, rayPickingParams, float.MaxValue)
        {

        }

        /// <summary>
        /// Implicit conversion between Ray and PickingRay
        /// </summary>
        public static implicit operator Ray(PickingRay value)
        {
            return new Ray(value.Position, value.Direction);
        }
        /// <summary>
        /// Implicit conversion between PickingRay and Ray
        /// </summary>
        public static implicit operator PickingRay(Ray value)
        {
            return new PickingRay(value);
        }
    }
}
