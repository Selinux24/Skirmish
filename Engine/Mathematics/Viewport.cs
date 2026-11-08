// Copyright (c) Amer Koleci and contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// Represents a floating-point viewport struct.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [JsonObject(MemberSerialization.OptIn)]
    public struct Viewport : IEquatable<Viewport>
    {
        /// <summary>
        /// Position of the pixel coordinate of the upper-left corner of the viewport.
        /// </summary>
        [JsonProperty]
        public float X;
        /// <summary>
        /// Position of the pixel coordinate of the upper-left corner of the viewport.
        /// </summary>
        [JsonProperty]
        public float Y;
        /// <summary>
        /// Width dimension of the viewport.
        /// </summary>
        [JsonProperty]
        public float Width;
        /// <summary>
        /// Height dimension of the viewport.
        /// </summary>
        [JsonProperty]
        public float Height;

        /// <summary>
        /// Gets or sets the minimum depth of the clip volume.
        /// </summary>
        [JsonProperty]
        public float MinDepth;
        /// <summary>
        /// Gets or sets the maximum depth of the clip volume.
        /// </summary>
        [JsonProperty]
        public float MaxDepth;

        /// <summary>
        /// Gets or sets the bounds of the viewport.
        /// </summary>
        /// <value>The bounds.</value>
        public readonly Rect Bounds => new(X, Y, Width, Height);
        /// <summary>
        /// Gets the aspect ratio used by the viewport.
        /// </summary>
        /// <value>The aspect ratio.</value>
        public readonly float AspectRatio
        {
            get
            {
                if (!Helper.IsZero(Height))
                {
                    return Width / Height;
                }

                return 0.0f;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="width">The width of the viewport in pixels.</param>
        /// <param name="height">The height of the viewport in pixels.</param>
        public Viewport(float width, float height)
        {
            X = 0.0f;
            Y = 0.0f;
            Width = width;
            Height = height;
            MinDepth = 0.0f;
            MaxDepth = 1.0f;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="width">The width of the viewport in pixels.</param>
        /// <param name="height">The height of the viewport in pixels.</param>
        public Viewport(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = 0.0f;
            MaxDepth = 1.0f;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the viewport in pixels.</param>
        /// <param name="width">The width of the viewport in pixels.</param>
        /// <param name="height">The height of the viewport in pixels.</param>
        /// <param name="minDepth">The minimum depth of the clip volume.</param>
        /// <param name="maxDepth">The maximum depth of the clip volume.</param>
        public Viewport(float x, float y, float width, float height, float minDepth, float maxDepth)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinDepth = minDepth;
            MaxDepth = maxDepth;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="bounds">A <see cref="Rect"/> that defines the location and size of the viewport in a render target.</param>
        public Viewport(in Rect bounds)
        {
            X = bounds.X;
            Y = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;
            MinDepth = 0.0f;
            MaxDepth = 1.0f;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="bounds">A <see cref="RectI"/> that defines the location and size of the viewport in a render target.</param>
        public Viewport(in RectI bounds)
        {
            X = bounds.X;
            Y = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;
            MinDepth = 0.0f;
            MaxDepth = 1.0f;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="bounds">A <see cref="Vector4"/> that defines the location and size of the viewport in a render target.</param>
        public Viewport(in Vector4 bounds)
        {
            X = bounds.X;
            Y = bounds.Y;
            Width = bounds.Z;
            Height = bounds.W;
            MinDepth = 0.0f;
            MaxDepth = 1.0f;
        }

        /// <summary>
        /// Projects a 3D vector from object space into screen space.
        /// </summary>
        /// <param name="source">The vector to project.</param>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="view">The view matrix.</param>
        /// <param name="world">The world matrix.</param>
        public readonly Vector3 Project(in Vector3 source, in Matrix4x4 projection, in Matrix4x4 view, in Matrix4x4 world)
        {
            Matrix4x4 worldViewProjection = Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection);
            return Project(source, worldViewProjection);
        }
        /// <summary>
        /// Projects a 3D vector from object space into screen space.
        /// </summary>
        /// <param name="source">The vector to project.</param>
        /// <param name="worldViewProjection">The World-View-Projection matrix.</param>
        /// <returns>The unprojected vector. </returns>
        public readonly Vector3 Project(Vector3 source, Matrix4x4 worldViewProjection)
        {
            Vector3 vector = Vector3.TransformCoordinate(source, worldViewProjection);
            float a = (source.X * worldViewProjection.M14) + (source.Y * worldViewProjection.M24) + (source.Z * worldViewProjection.M34) + worldViewProjection.M44;

            if (!Helper.IsOne(a))
            {
                vector.X /= a;
                vector.Y /= a;
                vector.Z /= a;
            }

            vector.X = ((vector.X + 1.0f) * 0.5f * Width) + X;
            vector.Y = ((-vector.Y + 1.0f) * 0.5f * Height) + Y;
            vector.Z = (vector.Z * (MaxDepth - MinDepth)) + MinDepth;
            return vector;
        }

        /// <summary>
        /// Converts a screen space point into a corresponding point in world space.
        /// </summary>
        /// <param name="source">The vector to project.</param>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="view">The view matrix.</param>
        /// <param name="world">The world matrix.</param>
        /// <returns>The unprojected vector.</returns>
        public readonly Vector3 Unproject(Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
        {
            Matrix4x4 worldViewProjection = Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection);
            return Unproject(source, worldViewProjection);
        }
        /// <summary>
        /// Converts a screen space point into a corresponding point in world space.
        /// </summary>
        /// <param name="source">The vector to project.</param>
        /// <param name="worldViewProjection">The World-View-Projection matrix.</param>
        /// <returns>The unprojected vector.</returns>
        public readonly Vector3 Unproject(Vector3 source, Matrix4x4 worldViewProjection)
        {
            Matrix4x4 matrix = Matrix4x4.Invert(worldViewProjection);

            source.X = (((source.X - X) / Width) * 2.0f) - 1.0f;
            source.Y = -(((source.Y - Y) / Height * 2.0f) - 1.0f);
            source.Z = (source.Z - MinDepth) / (MaxDepth - MinDepth);

            float a = (source.X * matrix.M14) + (source.Y * matrix.M24) + (source.Z * matrix.M34) + matrix.M44;
            source = Vector3.TransformCoordinate(source, matrix);

            if (!Helper.IsOne(a))
            {
                source /= a;
            }

            return source;
        }

        /// <summary>
        /// Converts a screen space point into a corresponding point in 2D screen space.
        /// </summary>
        /// <param name="source">The vector to project.</param>
        /// <param name="worldViewProjection">The World-View-Projection matrix.</param>
        /// <param name="isInsideScreen">Returns true if the resulting point is inside the screen</param>
        /// <returns>The unprojected vector.</returns>
        public readonly Vector2 UnprojectToScreen(Vector3 source, Matrix4x4 worldViewProjection, out bool isInsideScreen)
        {
            isInsideScreen = true;

            // Go to projection space
            var projected = Vector3.Transform(source, worldViewProjection);

            // Clip
            // -Wp < Xp <= Wp 
            // -Wp < Yp <= Wp 
            //   0 < Zp <= Wp 
            if (projected.X < -projected.W)
            {
                projected.X = -projected.W;
                isInsideScreen = false;
            }
            if (projected.X > projected.W)
            {
                projected.X = projected.W;
                isInsideScreen = false;
            }
            if (projected.Y < -projected.W)
            {
                projected.Y = -projected.W;
                isInsideScreen = false;
            }
            if (projected.Y > projected.W)
            {
                projected.Y = projected.W;
                isInsideScreen = false;
            }
            if (projected.Z < 0)
            {
                projected.Z = 0;
                isInsideScreen = false;
            }
            if (projected.Z > projected.W)
            {
                projected.Z = projected.W;
                isInsideScreen = false;
            }

            // Divide by w, to move from homogeneous coordinates to 3D coordinates again 
            projected /= projected.W;

            // Perform the viewport scaling, to get the appropiate coordinates inside the viewport 
            projected.X = ((projected.X + 1f) * 0.5f * Width) + X;
            projected.Y = ((1f - projected.Y) * 0.5f * Height) + Y;
            projected.Z = (projected.Z * (MaxDepth - MinDepth)) + MinDepth;

            return projected.XY;
        }


        public static RectI ComputeDisplayArea(ViewportScaling scaling, int backBufferWidth, int backBufferHeight, int outputWidth, int outputHeight)
        {
            switch (scaling)
            {
                case ViewportScaling.Stretch:
                    // Output fills the entire window area
                    return new RectI(0, 0, outputWidth, outputHeight);

                case ViewportScaling.AspectRatioStretch:
                // Output fills the window area but respects the original aspect ratio, using pillar boxing or letter boxing as required
                // Note: This scaling option is not supported for legacy Win32 windows swap chains
                {
                    Debug.Assert(backBufferHeight > 0);
                    float aspectRatio = (float)backBufferWidth / backBufferHeight;

                    // Horizontal fill
                    float scaledWidth = outputWidth;
                    float scaledHeight = outputWidth / aspectRatio;
                    if (scaledHeight >= outputHeight)
                    {
                        // Do vertical fill
                        scaledWidth = outputHeight * aspectRatio;
                        scaledHeight = outputHeight;
                    }

                    float offsetX = (outputWidth - scaledWidth) * 0.5f;
                    float offsetY = (outputHeight - scaledHeight) * 0.5f;

                    // Clip to display window
                    return new RectI(
                        (int)MathF.Max(0, offsetX),
                        (int)MathF.Max(0, offsetY),
                        (int)MathF.Min(outputWidth, scaledWidth),
                        (int)MathF.Min(outputHeight, scaledHeight)
                        );
                }

                case ViewportScaling.None:
                default:
                    // Output is displayed in the upper left corner of the window area
                    return new RectI(0, 0, Math.Min(backBufferWidth, outputWidth), Math.Min(backBufferHeight, outputHeight));
            }
        }

        public static RectI ComputeTitleSafeArea(int backBufferWidth, int backBufferHeight)
        {
            float safew = (backBufferWidth + 19.0f) / 20.0f;
            float safeh = (backBufferHeight + 19.0f) / 20.0f;

            return new()
            {
                Left = (int)safew,
                Top = (int)safeh,
                Right = (int)(backBufferWidth - safew + 0.5f),
                Bottom = (int)(backBufferHeight - safeh + 0.5f)
            };
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object obj) => obj is Viewport value && Equals(value);
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Viewport other)
        {
            return
                Helper.NearEqual(X, other.X) &&
                Helper.NearEqual(Y, other.Y) &&
                Helper.NearEqual(Width, other.Width) &&
                Helper.NearEqual(Height, other.Height) &&
                Helper.NearEqual(MinDepth, other.MinDepth) &&
                Helper.NearEqual(MaxDepth, other.MaxDepth);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Viewport left, Viewport right) => left.Equals(right);
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Viewport left, Viewport right) => !left.Equals(right);

        /// <inheritdoc/>
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height, MinDepth, MaxDepth);

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(MinDepth)}: {MinDepth}, {nameof(MaxDepth)}: {MaxDepth}";
        }
    }
}
