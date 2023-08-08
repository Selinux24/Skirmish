using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;

    internal static partial class BuiltInShaders
    {
        /// <summary>
        /// Global data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        struct Global : IBufferData
        {
            /// <summary>
            /// Builds the main vertex shader global buffer
            /// </summary>
            /// <param name="materialPaletteWidth">Global material palette width</param>
            /// <param name="animationPaletteWidth">Global animation palette width</param>
            public static Global Build(uint materialPaletteWidth, uint animationPaletteWidth)
            {
                return new Global
                {
                    MaterialPaletteWidth = materialPaletteWidth,
                    AnimationPaletteWidth = animationPaletteWidth
                };
            }

            /// <summary>
            /// Material palette width
            /// </summary>
            [FieldOffset(0)]
            public uint MaterialPaletteWidth;
            /// <summary>
            /// Animation palette width
            /// </summary>
            [FieldOffset(4)]
            public uint AnimationPaletteWidth;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(Global));
            }
        }



        /// <summary>
        /// Per-frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 208)]
        struct PerFrame : IBufferData
        {
            /// <summary>
            /// Builds the main vertex shader Per-Frame buffer
            /// </summary>
            /// <param name="context">Draw context</param>
            public static PerFrame Build(DrawContext context)
            {
                return new PerFrame
                {
                    ViewProjection = Matrix.Transpose(context.Camera.ViewProjection),
                    OrthoViewProjection = Matrix.Transpose(context.Form.GetOrthoProjectionMatrix()),
                    EyePosition = context.Camera.Position,
                    ScreenResolution = context.Form.RenderRectangle.BottomRight,
                    TotalTime = context.GameTime.TotalSeconds,
                    ElapsedTime = context.GameTime.ElapsedSeconds,
                    LevelOfDetail = context.LevelOfDetail,
                    ShadowIntensity = context.Lights?.ShadowIntensity ?? 0,
                    FogColor = context.Lights?.FogColor ?? Color.Transparent,
                    FogStart = context.Lights?.FogStart ?? 0,
                    FogRange = context.Lights?.FogRange ?? 0,
                };
            }

            /// <summary>
            /// View projection matrix
            /// </summary>
            [FieldOffset(0)]
            public Matrix ViewProjection;

            /// <summary>
            /// Ortho view projection matrix
            /// </summary>
            [FieldOffset(64)]
            public Matrix OrthoViewProjection;

            /// <summary>
            /// Eye position
            /// </summary>
            [FieldOffset(128)]
            public Vector3 EyePosition;

            /// <summary>
            /// Screen resolution
            /// </summary>
            [FieldOffset(144)]
            public Vector2 ScreenResolution;
            /// <summary>
            /// Total time
            /// </summary>
            [FieldOffset(152)]
            public float TotalTime;
            /// <summary>
            /// Elapsed time
            /// </summary>
            [FieldOffset(156)]
            public float ElapsedTime;

            /// <summary>
            /// Level of detail values
            /// </summary>
            [FieldOffset(160)]
            public Vector3 LevelOfDetail;
            /// <summary>
            /// Shadow intensity
            /// </summary>
            [FieldOffset(172)]
            public float ShadowIntensity;

            /// <summary>
            /// Fog color
            /// </summary>
            [FieldOffset(176)]
            public Color4 FogColor;

            /// <summary>
            /// Fog start distance
            /// </summary>
            [FieldOffset(192)]
            public float FogStart;
            /// <summary>
            /// Fog range distance
            /// </summary>
            [FieldOffset(196)]
            public float FogRange;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(PerFrame));
            }
        }



        /// <summary>
        /// Hemispheric light buffer
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct BufferLightHemispheric : IBufferData
        {
            /// <summary>
            /// Builds a hemispheric light buffer
            /// </summary>
            /// <param name="light">Light</param>
            /// <returns>Returns the new buffer</returns>
            public static BufferLightHemispheric Build(ISceneLightHemispheric light)
            {
                return new BufferLightHemispheric
                {
                    AmbientDown = light?.AmbientDown ?? Color3.Black,
                    AmbientUp = light?.AmbientUp ?? Color3.Black,
                };
            }

            /// <summary>
            /// Ambient Up
            /// </summary>
            [FieldOffset(0)]
            public Color3 AmbientDown;
            /// <summary>
            /// Ambient Down
            /// </summary>
            [FieldOffset(16)]
            public Color3 AmbientUp;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(BufferLightHemispheric));
            }
        }
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct PSHemispheric : IBufferData
        {
            /// <summary>
            /// Builds the main pixel shader Per-Frame buffer with lighting
            /// </summary>
            /// <param name="context">Draw context</param>
            public static PSHemispheric Build(DrawContext context)
            {
                var hemiLight = BufferLightHemispheric.Build(context.Lights?.GetVisibleHemisphericLight());

                return new PSHemispheric
                {
                    HemiLight = hemiLight,
                };
            }

            /// <summary>
            /// Hemispheric light
            /// </summary>
            [FieldOffset(0)]
            public BufferLightHemispheric HemiLight;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(PSHemispheric));
            }
        }



        /// <summary>
        /// Directional light buffer
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 160)]
        public struct BufferLightDirectional : IBufferData
        {
            /// <summary>
            /// Builds a light buffer collection
            /// </summary>
            /// <param name="lights">Light list</param>
            /// <param name="maxLights">Maximum lights</param>
            /// <param name="lightCount">Returns the assigned light count</param>
            /// <returns>Returns a light buffer collection</returns>
            public static IEnumerable<BufferLightDirectional> Build(IEnumerable<ISceneLightDirectional> lights, int maxLights, out int lightCount)
            {
                if (lights?.Any() != true)
                {
                    lightCount = 0;

                    return new BufferLightDirectional[maxLights];
                }

                var bDirLights = new BufferLightDirectional[maxLights];

                for (int i = 0; i < Math.Min(lights.Count(), maxLights); i++)
                {
                    bDirLights[i] = Build(lights.ElementAt(i));
                }

                lightCount = Math.Min(lights.Count(), maxLights);

                return bDirLights;
            }
            /// <summary>
            /// Builds a light buffer
            /// </summary>
            /// <param name="light">Light</param>
            /// <returns>Returns a light buffer</returns>
            public static BufferLightDirectional Build(ISceneLightDirectional light)
            {
                if (light == null)
                {
                    return new BufferLightDirectional();
                }

                var brightness = light?.Brightness ?? 0;

                return new BufferLightDirectional
                {
                    DiffuseColor = (light?.DiffuseColor ?? Color3.Black) * brightness,
                    SpecularColor = (light?.SpecularColor ?? Color3.Black) * brightness,
                    DirToLight = -light?.Direction ?? Vector3.Zero,
                    CastShadow = light?.CastShadowsMarked ?? false ? 1 : 0,
                    ToCascadeOffsetX = light?.ToCascadeOffsetX ?? Vector4.Zero,
                    ToCascadeOffsetY = light?.ToCascadeOffsetY ?? Vector4.Zero,
                    ToCascadeScale = light?.ToCascadeScale ?? Vector4.Zero,
                    ToShadowSpace = Matrix.Transpose(light?.ToShadowSpace ?? Matrix.Zero),
                };
            }

            /// <summary>
            /// Diffuse color
            /// </summary>
            [FieldOffset(0)]
            public Color3 DiffuseColor;

            /// <summary>
            /// Specular color
            /// </summary>
            [FieldOffset(16)]
            public Color3 SpecularColor;

            /// <summary>
            /// Light direction vector
            /// </summary>
            [FieldOffset(32)]
            public Vector3 DirToLight;
            /// <summary>
            /// The light casts shadow
            /// </summary>
            [FieldOffset(44)]
            public float CastShadow;

            /// <summary>
            /// X cascade offsets
            /// </summary>
            [FieldOffset(48)]
            public Vector4 ToCascadeOffsetX;

            /// <summary>
            /// Y cascade offsets
            /// </summary>
            [FieldOffset(64)]
            public Vector4 ToCascadeOffsetY;

            /// <summary>
            /// Cascade scales
            /// </summary>
            [FieldOffset(80)]
            public Vector4 ToCascadeScale;

            /// <summary>
            /// From light view * projection matrix array
            /// </summary>
            [FieldOffset(96)]
            public Matrix ToShadowSpace;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(BufferLightDirectional));
            }
        }
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16 + (160 * MaxDirectional))]
        struct PSDirectional : IBufferData
        {
            /// <summary>
            /// Maximum directional lights
            /// </summary>
            public const int MaxDirectional = 3;

            /// <summary>
            /// Builds the main pixel shader Per-Frame buffer with lighting
            /// </summary>
            /// <param name="context">Draw context</param>
            public static PSDirectional Build(DrawContext context)
            {
                var dirLights = BufferLightDirectional.Build(context.Lights?.GetVisibleDirectionalLights(), MaxDirectional, out int dirLength);

                return new PSDirectional
                {
                    DirLightsCount = (uint)dirLength,
                    DirLights = dirLights.ToArray(),
                };
            }

            /// <summary>
            /// Directional lights count
            /// </summary>
            [FieldOffset(0)]
            public uint DirLightsCount;
            /// <summary>
            /// Directional lights
            /// </summary>
            [FieldOffset(16), MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDirectional)]
            public BufferLightDirectional[] DirLights;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(PSDirectional));
            }
        }



        /// <summary>
        /// Spot light buffer
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 144)]
        public struct BufferLightSpot : IBufferData
        {
            /// <summary>
            /// Builds a light buffer collection
            /// </summary>
            /// <param name="lights">Light list</param>
            /// <param name="maxLights">Maximum lights</param>
            /// <param name="lightCount">Returns the assigned light count</param>
            /// <returns>Returns a light buffer collection</returns>
            public static IEnumerable<BufferLightSpot> Build(IEnumerable<ISceneLightSpot> lights, int maxLights, out int lightCount)
            {
                if (lights?.Any() != true)
                {
                    lightCount = 0;

                    return new BufferLightSpot[maxLights];
                }

                var bSpotLights = new BufferLightSpot[maxLights];

                var spots = lights.ToArray();
                for (int i = 0; i < Math.Min(spots.Length, maxLights); i++)
                {
                    bSpotLights[i] = Build(spots[i]);
                }

                lightCount = Math.Min(spots.Length, maxLights);

                return bSpotLights;
            }
            /// <summary>
            /// Builds a light buffer
            /// </summary>
            /// <param name="light">Light</param>
            /// <returns>Returns a light buffer</returns>
            public static BufferLightSpot Build(ISceneLightSpot light)
            {
                if (light == null)
                {
                    return new BufferLightSpot();
                }

                return new BufferLightSpot
                {
                    Position = light?.Position ?? Vector3.Zero,
                    Direction = light?.Direction ?? Vector3.Zero,
                    DiffuseColor = light?.DiffuseColor ?? Color3.Black,
                    SpecularColor = light?.SpecularColor ?? Color3.Black,
                    Intensity = light?.Intensity ?? 0,
                    Angle = light?.FallOffAngleRadians ?? 0,
                    Radius = light?.Radius ?? 0,
                    CastShadow = light?.CastShadowsMarked ?? false ? 1 : 0,
                    MapIndex = light?.ShadowMapIndex ?? -1,
                    MapCount = light?.ShadowMapCount ?? 0,
                    FromLightVP = light?.FromLightVP?.Any() == true ? Matrix.Transpose(light.FromLightVP[0]) : Matrix.Zero,
                };
            }

            /// <summary>
            /// Diffuse color
            /// </summary>
            [FieldOffset(0)]
            public Color3 DiffuseColor;

            /// <summary>
            /// Specular color
            /// </summary>
            [FieldOffset(16)]
            public Color3 SpecularColor;

            /// <summary>
            /// Light position
            /// </summary>
            [FieldOffset(32)]
            public Vector3 Position;
            /// <summary>
            /// Spot radius
            /// </summary>
            [FieldOffset(44)]
            public float Angle;

            /// <summary>
            /// Light direction vector
            /// </summary>
            [FieldOffset(48)]
            public Vector3 Direction;
            /// <summary>
            /// Intensity
            /// </summary>
            [FieldOffset(60)]
            public float Intensity;

            /// <summary>
            /// Light radius
            /// </summary>
            [FieldOffset(64)]
            public float Radius;
            /// <summary>
            /// The light casts shadow
            /// </summary>
            [FieldOffset(68)]
            public float CastShadow;
            /// <summary>
            /// Shadow map index
            /// </summary>
            [FieldOffset(72)]
            public int MapIndex;
            /// <summary>
            /// Sub-shadow map count
            /// </summary>
            [FieldOffset(76)]
            public uint MapCount;

            /// <summary>
            /// From light view * projection matrix array
            /// </summary>
            [FieldOffset(80)]
            public Matrix FromLightVP;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(BufferLightSpot));
            }
        }
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16 + (144 * MaxSpots))]
        struct PSSpots : IBufferData
        {
            /// <summary>
            /// Maximum spot lights
            /// </summary>
            public const int MaxSpots = 16;

            /// <summary>
            /// Builds the main pixel shader Per-Frame buffer with lighting
            /// </summary>
            /// <param name="context">Draw context</param>
            public static PSSpots Build(DrawContext context)
            {
                var spotLights = BufferLightSpot.Build(context.Lights?.GetVisibleSpotLights(), MaxSpots, out int spotLength);

                return new PSSpots
                {
                    SpotLightsCount = (uint)spotLength,
                    SpotLights = spotLights.ToArray(),
                };
            }

            /// <summary>
            /// Spot lights count
            /// </summary>
            [FieldOffset(0)]
            public uint SpotLightsCount;
            /// <summary>
            /// Spot lights
            /// </summary>
            [FieldOffset(16), MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSpots)]
            public BufferLightSpot[] SpotLights;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(PSSpots));
            }
        }



        /// <summary>
        /// Point light buffer
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct BufferLightPoint : IBufferData
        {
            /// <summary>
            /// Builds a light buffer collection
            /// </summary>
            /// <param name="lights">Light list</param>
            /// <param name="maxLights">Maximum lights</param>
            /// <param name="lightCount">Returns the assigned light count</param>
            /// <returns>Returns a light buffer collection</returns>
            public static IEnumerable<BufferLightPoint> Build(IEnumerable<ISceneLightPoint> lights, int maxLights, out int lightCount)
            {
                if (lights?.Any() != true)
                {
                    lightCount = 0;

                    return new BufferLightPoint[maxLights];
                }

                var bPointLights = new BufferLightPoint[maxLights];

                var points = lights.ToArray();
                for (int i = 0; i < Math.Min(points.Length, maxLights); i++)
                {
                    bPointLights[i] = Build(points[i]);
                }

                lightCount = Math.Min(points.Length, maxLights);

                return bPointLights;
            }
            /// <summary>
            /// Builds a light buffer
            /// </summary>
            /// <param name="light">Light</param>
            /// <returns>Returns a light buffer</returns>
            public static BufferLightPoint Build(ISceneLightPoint light)
            {
                if (light == null)
                {
                    return new BufferLightPoint();
                }

                float radius = light?.Radius ?? 0;
                var perspectiveMatrix = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, radius + 0.1f);

                return new BufferLightPoint
                {
                    Position = light?.Position ?? Vector3.Zero,
                    DiffuseColor = light?.DiffuseColor ?? Color3.Black,
                    SpecularColor = light?.SpecularColor ?? Color3.Black,
                    Intensity = light?.Intensity ?? 0,
                    Radius = light?.Radius ?? 0,
                    CastShadow = light?.CastShadowsMarked ?? false ? 1 : 0,
                    MapIndex = light?.ShadowMapIndex ?? -1,
                    PerspectiveValues = new Vector2(perspectiveMatrix[2, 2], perspectiveMatrix[3, 2]),
                };
            }

            /// <summary>
            /// Diffuse color
            /// </summary>
            [FieldOffset(0)]
            public Color3 DiffuseColor;

            /// <summary>
            /// Specular color
            /// </summary>
            [FieldOffset(16)]
            public Color3 SpecularColor;

            /// <summary>
            /// Light position
            /// </summary>
            [FieldOffset(32)]
            public Vector3 Position;
            /// <summary>
            /// Intensity
            /// </summary>
            [FieldOffset(44)]
            public float Intensity;

            /// <summary>
            /// Light radius
            /// </summary>
            [FieldOffset(48)]
            public float Radius;
            /// <summary>
            /// The light casts shadow
            /// </summary>
            [FieldOffset(52)]
            public float CastShadow;
            /// <summary>
            /// Perspective values
            /// </summary>
            [FieldOffset(56)]
            public Vector2 PerspectiveValues;

            /// <summary>
            /// Shadow map index
            /// </summary>
            [FieldOffset(64)]
            public int MapIndex;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(BufferLightPoint));
            }
        }
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16 + (80 * MaxPoints))]
        struct PSPoints : IBufferData
        {
            /// <summary>
            /// Maximum point lights
            /// </summary>
            public const int MaxPoints = 16;

            /// <summary>
            /// Builds the main pixel shader Per-Frame buffer with lighting
            /// </summary>
            /// <param name="context">Draw context</param>
            public static PSPoints Build(DrawContext context)
            {
                var pointLights = BufferLightPoint.Build(context.Lights?.GetVisiblePointLights(), MaxPoints, out int pointLength);

                return new PSPoints
                {
                    PointLightsCount = (uint)pointLength,
                    PointLights = pointLights.ToArray(),
                };
            }

            /// <summary>
            /// Point lights count
            /// </summary>
            [FieldOffset(0)]
            public uint PointLightsCount;
            /// <summary>
            /// Point lights
            /// </summary>
            [FieldOffset(16), MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPoints)]
            public BufferLightPoint[] PointLights;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(PSPoints));
            }
        }
    }
}
