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
        struct VSGlobal : IBufferData
        {
            public static VSGlobal Build(uint materialPaletteWidth, uint animationPaletteWidth)
            {
                return new VSGlobal
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
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(VSGlobal));
            }
        }
        /// <summary>
        /// Per-frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 128)]
        struct VSPerFrame : IBufferData
        {
            public static VSPerFrame Build(Matrix localTransform, DrawContext context)
            {
                return new VSPerFrame
                {
                    World = Matrix.Transpose(localTransform),
                    WorldViewProjection = Matrix.Transpose(localTransform * context.ViewProjection),
                };
            }
            public static VSPerFrame Build(Matrix localTransform, DrawContextShadows context)
            {
                return new VSPerFrame
                {
                    World = Matrix.Transpose(localTransform),
                    WorldViewProjection = Matrix.Transpose(localTransform * context.ViewProjection),
                };
            }

            /// <summary>
            /// World matrix
            /// </summary>
            [FieldOffset(0)]
            public Matrix World;
            /// <summary>
            /// World view projection matrix
            /// </summary>
            [FieldOffset(64)]
            public Matrix WorldViewProjection;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(VSPerFrame));
            }
        }
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        struct PSPerFrameNoLit : IBufferData
        {
            public static PSPerFrameNoLit Build(DrawContext context)
            {
                return new PSPerFrameNoLit
                {
                    EyePositionWorld = context.EyePosition,

                    FogColor = context.Lights?.FogColor ?? Color.Transparent,

                    FogStart = context.Lights?.FogStart ?? 0,
                    FogRange = context.Lights?.FogRange ?? 0,
                };
            }

            /// <summary>
            /// Eye position world
            /// </summary>
            [FieldOffset(0)]
            public Vector3 EyePositionWorld;

            /// <summary>
            /// Fog color
            /// </summary>
            [FieldOffset(16)]
            public Color4 FogColor;

            /// <summary>
            /// Fog start distance
            /// </summary>
            [FieldOffset(32)]
            public float FogStart;
            /// <summary>
            /// Fog range distance
            /// </summary>
            [FieldOffset(36)]
            public float FogRange;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PSPerFrameNoLit));
            }
        }
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 4176)]
        struct PSPerFrameLit : IBufferData
        {
            public static PSPerFrameLit Build(DrawContext context)
            {
                var hemiLight = BufferLightHemispheric.Build(context.Lights?.GetVisibleHemisphericLight());
                var dirLights = BufferLightDirectional.Build(context.Lights?.GetVisibleDirectionalLights(), out int dirLength);
                var pointLights = BufferLightPoint.Build(context.Lights?.GetVisiblePointLights(), out int pointLength);
                var spotLights = BufferLightSpot.Build(context.Lights?.GetVisibleSpotLights(), out int spotLength);

                return new PSPerFrameLit
                {
                    EyePositionWorld = context.EyePosition,

                    FogColor = context.Lights?.FogColor ?? Color.Transparent,

                    FogStart = context.Lights?.FogStart ?? 0,
                    FogRange = context.Lights?.FogRange ?? 0,

                    LevelOfDetail = context.LevelOfDetail,

                    DirLightsCount = (uint)dirLength,
                    PointLightsCount = (uint)pointLength,
                    SpotLightsCount = (uint)spotLength,
                    ShadowIntensity = context.Lights?.ShadowIntensity ?? 0f,

                    HemiLight = hemiLight,
                    DirLights = dirLights,
                    PointLights = pointLights,
                    SpotLights = spotLights,
                };
            }

            /// <summary>
            /// Eye position world
            /// </summary>
            [FieldOffset(0)]
            public Vector3 EyePositionWorld;

            /// <summary>
            /// Fog color
            /// </summary>
            [FieldOffset(16)]
            public Color4 FogColor;

            /// <summary>
            /// Fog start distance
            /// </summary>
            [FieldOffset(32)]
            public float FogStart;
            /// <summary>
            /// Fog range distance
            /// </summary>
            [FieldOffset(36)]
            public float FogRange;

            /// <summary>
            /// Level of detail values
            /// </summary>
            [FieldOffset(48)]
            public Vector3 LevelOfDetail;

            /// <summary>
            /// Directional lights count
            /// </summary>
            [FieldOffset(64)]
            public uint DirLightsCount;
            /// <summary>
            /// Point lights count
            /// </summary>
            [FieldOffset(68)]
            public uint PointLightsCount;
            /// <summary>
            /// Spot lights count
            /// </summary>
            [FieldOffset(72)]
            public uint SpotLightsCount;
            /// <summary>
            /// Shadow intensity
            /// </summary>
            [FieldOffset(76)]
            public float ShadowIntensity;

            /// <summary>
            /// Hemispheric light
            /// </summary>
            [FieldOffset(80)]
            public BufferLightHemispheric HemiLight;
            /// <summary>
            /// Directional lights
            /// </summary>
            [FieldOffset(112), MarshalAs(UnmanagedType.ByValArray, SizeConst = BufferLightDirectional.MAX)]
            public BufferLightDirectional[] DirLights;
            /// <summary>
            /// Point lights
            /// </summary>
            [FieldOffset(592), MarshalAs(UnmanagedType.ByValArray, SizeConst = BufferLightPoint.MAX)]
            public BufferLightPoint[] PointLights;
            /// <summary>
            /// Spot lights
            /// </summary>
            [FieldOffset(1872), MarshalAs(UnmanagedType.ByValArray, SizeConst = BufferLightSpot.MAX)]
            public BufferLightSpot[] SpotLights;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PSPerFrameLit));
            }
        }
        /// <summary>
        /// Hemispheric light buffer
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct BufferLightHemispheric : IBufferData
        {
            /// <summary>
            /// Maximum light count
            /// </summary>
            public const int MAX = 1;

            /// <summary>
            /// Default hemispheric light
            /// </summary>
            public static BufferLightHemispheric Default
            {
                get
                {
                    return new BufferLightHemispheric()
                    {
                        AmbientDown = Color3.White,
                        AmbientUp = Color3.White,
                    };
                }
            }
            /// <summary>
            /// Builds a hemispheric light buffer
            /// </summary>
            /// <param name="light">Light</param>
            /// <returns>Returns the new buffer</returns>
            public static BufferLightHemispheric Build(ISceneLightHemispheric light)
            {
                if (light != null)
                {
                    return new BufferLightHemispheric(light);
                }

                return Default;
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

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="light">Light</param>
            public BufferLightHemispheric(ISceneLightHemispheric light)
            {
                AmbientDown = light.AmbientDown;
                AmbientUp = light.AmbientUp;
            }

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(BufferLightHemispheric));
            }
        }
        /// <summary>
        /// Directional light buffer
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 160)]
        struct BufferLightDirectional : IBufferData
        {
            /// <summary>
            /// Maximum light count
            /// </summary>
            public const int MAX = 3;

            /// <summary>
            /// Default buffer collection
            /// </summary>
            public static BufferLightDirectional[] Default
            {
                get
                {
                    return new BufferLightDirectional[MAX];
                }
            }
            /// <summary>
            /// Builds a light buffer collection
            /// </summary>
            /// <param name="lights">Light list</param>
            /// <param name="lightCount">Returns the assigned light count</param>
            /// <returns>Returns a light buffer collection</returns>
            public static BufferLightDirectional[] Build(IEnumerable<ISceneLightDirectional> lights, out int lightCount)
            {
                if (!lights.Any())
                {
                    lightCount = 0;

                    return Default;
                }

                var bDirLights = Default;

                var dir = lights.ToArray();
                for (int i = 0; i < Math.Min(dir.Length, MAX); i++)
                {
                    bDirLights[i] = new BufferLightDirectional(dir[i]);
                }

                lightCount = Math.Min(dir.Length, MAX);

                return bDirLights;
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

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="light">Light</param>
            public BufferLightDirectional(ISceneLightDirectional light)
            {
                DiffuseColor = light.DiffuseColor * light.Brightness;
                SpecularColor = light.SpecularColor * light.Brightness;
                DirToLight = -light.Direction;
                CastShadow = light.CastShadowsMarked ? 1 : 0;
                ToCascadeOffsetX = light.ToCascadeOffsetX;
                ToCascadeOffsetY = light.ToCascadeOffsetY;
                ToCascadeScale = light.ToCascadeScale;
                ToShadowSpace = light.ToShadowSpace;
            }

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(BufferLightDirectional));
            }
        }
        /// <summary>
        /// Spot light buffer
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 144)]
        struct BufferLightSpot : IBufferData
        {
            /// <summary>
            /// Maximum light count
            /// </summary>
            public const int MAX = 16;

            /// <summary>
            /// Default buffer collection
            /// </summary>
            public static BufferLightSpot[] Default
            {
                get
                {
                    return new BufferLightSpot[MAX];
                }
            }
            /// <summary>
            /// Builds a light buffer collection
            /// </summary>
            /// <param name="lights">Light list</param>
            /// <param name="lightCount">Returns the assigned light count</param>
            /// <returns>Returns a light buffer collection</returns>
            public static BufferLightSpot[] Build(IEnumerable<ISceneLightSpot> lights, out int lightCount)
            {
                if (!lights.Any())
                {
                    lightCount = 0;

                    return Default;
                }

                var bSpotLights = Default;

                var spot = lights.ToArray();
                for (int i = 0; i < Math.Min(spot.Length, MAX); i++)
                {
                    bSpotLights[i] = new BufferLightSpot(spot[i]);
                }

                lightCount = Math.Min(spot.Length, MAX);

                return bSpotLights;
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

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="light">Light</param>
            public BufferLightSpot(ISceneLightSpot light)
            {
                Position = light.Position;
                Direction = light.Direction;
                DiffuseColor = light.DiffuseColor;
                SpecularColor = light.SpecularColor;
                Intensity = light.Intensity;
                Intensity = light.Intensity;
                Angle = light.FallOffAngleRadians;
                Radius = light.Radius;
                CastShadow = light.CastShadowsMarked ? 1 : 0;
                MapIndex = light.ShadowMapIndex;
                MapCount = light.ShadowMapCount;

                FromLightVP = Matrix.Identity;
                if (light.FromLightVP?.Length > 0)
                {
                    FromLightVP = light.FromLightVP[0];
                }
            }

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(BufferLightSpot));
            }
        }
        /// <summary>
        /// Point light buffer
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        struct BufferLightPoint : IBufferData
        {
            /// <summary>
            /// Maximum light count
            /// </summary>
            public const int MAX = 16;

            /// <summary>
            /// Default buffer collection
            /// </summary>
            public static BufferLightPoint[] Default
            {
                get
                {
                    return new BufferLightPoint[MAX];
                }
            }
            /// <summary>
            /// Builds a light buffer collection
            /// </summary>
            /// <param name="lights">Light list</param>
            /// <param name="lightCount">Returns the assigned light count</param>
            /// <returns>Returns a light buffer collection</returns>
            public static BufferLightPoint[] Build(IEnumerable<ISceneLightPoint> lights, out int lightCount)
            {
                if (!lights.Any())
                {
                    lightCount = 0;

                    return Default;
                }

                var bPointLights = Default;

                var point = lights.ToArray();
                for (int i = 0; i < Math.Min(point.Length, MAX); i++)
                {
                    bPointLights[i] = new BufferLightPoint(point[i]);
                }

                lightCount = Math.Min(point.Length, MAX);

                return bPointLights;
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

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="light">Light</param>
            public BufferLightPoint(ISceneLightPoint light)
            {
                Position = light.Position;
                DiffuseColor = light.DiffuseColor;
                SpecularColor = light.SpecularColor;
                Intensity = light.Intensity;
                Radius = light.Radius;
                CastShadow = light.CastShadowsMarked ? 1 : 0;
                MapIndex = light.ShadowMapIndex;

                var perspectiveMatrix = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, Radius + 0.1f);
                PerspectiveValues = new Vector2(perspectiveMatrix[2, 2], perspectiveMatrix[3, 2]);
            }

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(BufferLightPoint));
            }
        }
    }
}
