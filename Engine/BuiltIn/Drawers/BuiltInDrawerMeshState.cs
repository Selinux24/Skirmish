﻿using SharpDX;

namespace Engine.BuiltIn.Drawers
{
    /// <summary>
    /// Drawer mesh state
    /// </summary>
    public struct BuiltInDrawerMeshState : IDrawerMeshState
    {
        /// <summary>
        /// Default state
        /// </summary>
        public static BuiltInDrawerMeshState Default()
        {
            return new BuiltInDrawerMeshState
            {
                Local = Matrix.Identity,
                AnimationOffset1 = 0,
                AnimationOffset2 = 0,
                AnimationInterpolationAmount = 0f,
            };
        }
        /// <summary>
        /// Default state with local transform
        /// </summary>
        public static BuiltInDrawerMeshState SetLocal(Matrix local)
        {
            return new BuiltInDrawerMeshState
            {
                Local = local,
                AnimationOffset1 = 0,
                AnimationOffset2 = 0,
                AnimationInterpolationAmount = 0f,
            };
        }

        /// <inheritdoc/>
        public Matrix Local { get; set; }
        /// <summary>
        /// First offset in the animation palette
        /// </summary>
        public uint AnimationOffset1 { get; set; }
        /// <summary>
        /// Second offset in the animation palette
        /// </summary>
        public uint AnimationOffset2 { get; set; }
        /// <summary>
        /// Interpolation amount between the offsets
        /// </summary>
        public float AnimationInterpolationAmount { get; set; }
    }
}