using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Built-in shaders resource helper
    /// </summary>
    internal static partial class BuiltInShaders
    {
        /// <summary>
        /// Graphics
        /// </summary>
        private static Graphics graphics = null;
        /// <summary>
        /// Constant buffer list
        /// </summary>
        private static readonly List<IEngineConstantBuffer> constantBuffers = new();
        /// <summary>
        /// Vertex shader list
        /// </summary>
        private static readonly List<IBuiltInShader<EngineVertexShader>> vertexShaders = new();
        /// <summary>
        /// Hull shader list
        /// </summary>
        private static readonly List<IBuiltInShader<EngineHullShader>> hullShaders = new();
        /// <summary>
        /// Domain shader list
        /// </summary>
        private static readonly List<IBuiltInShader<EngineDomainShader>> domainShaders = new();
        /// <summary>
        /// Geometry shader list
        /// </summary>
        private static readonly List<IBuiltInShader<EngineGeometryShader>> geometryShaders = new();
        /// <summary>
        /// Pixel shader list
        /// </summary>
        private static readonly List<IBuiltInShader<EnginePixelShader>> pixelShaders = new();
        /// <summary>
        /// Compute shader list
        /// </summary>
        private static readonly List<IBuiltInShader<EngineComputeShader>> computeShaders = new();
        /// <summary>
        /// Drawer list
        /// </summary>
        private static readonly List<IBuiltInDrawer> drawers = new();

        /// <summary>
        /// Sampler point
        /// </summary>
        private static EngineSamplerState samplerPoint = null;
        /// <summary>
        /// Sampler linear
        /// </summary>
        private static EngineSamplerState samplerLinear = null;
        /// <summary>
        /// Sampler anisotropic
        /// </summary>
        private static EngineSamplerState samplerAnisotropic = null;
        /// <summary>
        /// Sampler comparison less equal
        /// </summary>
        private static EngineSamplerState samplerComparisonLessEqualBorder = null;
        /// <summary>
        /// Sampler comparison less equal
        /// </summary>
        private static EngineSamplerState samplerComparisonLessEqualClamp = null;

        /// <summary>
        /// Material palette resource view
        /// </summary>
        private static EngineShaderResourceView rvMaterialPalette;
        /// <summary>
        /// Animation palette resource view
        /// </summary>
        private static EngineShaderResourceView rvAnimationPalette;
        /// <summary>
        /// Directional shadow map resource view
        /// </summary>
        private static EngineShaderResourceView rvShadowMapDir;
        /// <summary>
        /// Spot shadow map resource view
        /// </summary>
        private static EngineShaderResourceView rvShadowMapSpot;
        /// <summary>
        /// Point shadow map resource view
        /// </summary>
        private static EngineShaderResourceView rvShadowMapPoint;

        /// <summary>
        /// Initializes pool
        /// </summary>
        /// <param name="graphics">Device</param>
        public static void Initialize(Graphics graphics)
        {
            BuiltInShaders.graphics = graphics;
        }
        /// <summary>
        /// Dispose of used resources
        /// </summary>
        public static void DisposeResources()
        {
            constantBuffers.ForEach(cb => cb?.Dispose());
            constantBuffers.Clear();

            vertexShaders.ForEach(vs => vs?.Dispose());
            vertexShaders.Clear();
            hullShaders.ForEach(vs => vs?.Dispose());
            hullShaders.Clear();
            domainShaders.ForEach(vs => vs?.Dispose());
            domainShaders.Clear();
            geometryShaders.ForEach(gs => gs?.Dispose());
            geometryShaders.Clear();
            pixelShaders.ForEach(ps => ps?.Dispose());
            pixelShaders.Clear();
            computeShaders.ForEach(ps => ps?.Dispose());
            computeShaders.Clear();

            drawers.OfType<IDisposable>().ToList().ForEach(dr => dr?.Dispose());
            drawers.Clear();

            samplerPoint?.Dispose();
            samplerPoint = null;
            samplerLinear?.Dispose();
            samplerLinear = null;
            samplerAnisotropic?.Dispose();
            samplerAnisotropic = null;
            samplerComparisonLessEqualBorder?.Dispose();
            samplerComparisonLessEqualBorder = null;
            samplerComparisonLessEqualClamp?.Dispose();
            samplerComparisonLessEqualClamp = null;
        }

        /// <summary>
        /// Gets or creates a constant buffer for the specified data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="singleton">Use one instance</param>
        public static EngineConstantBuffer<T> GetConstantBuffer<T>(bool singleton = true) where T : struct, IBufferData
        {
            if (!singleton)
            {
                return new EngineConstantBuffer<T>(graphics, nameof(BuiltInShaders) + "." + typeof(T).Name);
            }

            EngineConstantBuffer<T> cb = constantBuffers.OfType<EngineConstantBuffer<T>>().FirstOrDefault();
            if (cb != null)
            {
                return cb;
            }

            cb = new EngineConstantBuffer<T>(graphics, nameof(BuiltInShaders) + "." + typeof(T).Name);
            constantBuffers.Add(cb);

            return cb;
        }
        /// <summary>
        /// Gets or creates a built-in vertex shader
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="singleton">Use one instance</param>
        public static T GetVertexShader<T>(bool singleton = true) where T : IBuiltInShader<EngineVertexShader>
        {
            if (!singleton)
            {
                return (T)Activator.CreateInstance(typeof(T), graphics);
            }

            T vs = vertexShaders.OfType<T>().FirstOrDefault();
            if (vs != null)
            {
                return vs;
            }

            vs = (T)Activator.CreateInstance(typeof(T), graphics);
            vertexShaders.Add(vs);

            return vs;
        }
        /// <summary>
        /// Gets or creates a built-in hull shader
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="singleton">Use one instance</param>
        public static T GetHullShader<T>(bool singleton = true) where T : IBuiltInShader<EngineHullShader>
        {
            if (!singleton)
            {
                return (T)Activator.CreateInstance(typeof(T), graphics);
            }

            T hs = hullShaders.OfType<T>().FirstOrDefault();
            if (hs != null)
            {
                return hs;
            }

            hs = (T)Activator.CreateInstance(typeof(T), graphics);
            hullShaders.Add(hs);

            return hs;
        }
        /// <summary>
        /// Gets or creates a built-in domain shader
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="singleton">Use one instance</param>
        public static T GetDomainShader<T>(bool singleton = true) where T : IBuiltInShader<EngineDomainShader>
        {
            if (!singleton)
            {
                return (T)Activator.CreateInstance(typeof(T), graphics);
            }

            T ds = domainShaders.OfType<T>().FirstOrDefault();
            if (ds != null)
            {
                return ds;
            }

            ds = (T)Activator.CreateInstance(typeof(T), graphics);
            domainShaders.Add(ds);

            return ds;
        }
        /// <summary>
        /// Gets or creates a built-in geometry shader
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="singleton">Use one instance</param>
        public static T GetGeometryShader<T>(bool singleton = true) where T : IBuiltInShader<EngineGeometryShader>
        {
            if (!singleton)
            {
                return (T)Activator.CreateInstance(typeof(T), graphics);
            }

            T gs = geometryShaders.OfType<T>().FirstOrDefault();
            if (gs != null)
            {
                return gs;
            }

            gs = (T)Activator.CreateInstance(typeof(T), graphics);
            geometryShaders.Add(gs);

            return gs;
        }
        /// <summary>
        /// Gets or creates a built-in pixel shader
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="singleton">Use one instance</param>
        public static T GetPixelShader<T>(bool singleton = true) where T : IBuiltInShader<EnginePixelShader>
        {
            if (!singleton)
            {
                return (T)Activator.CreateInstance(typeof(T), graphics);
            }

            T ps = pixelShaders.OfType<T>().FirstOrDefault();
            if (ps != null)
            {
                return ps;
            }

            ps = (T)Activator.CreateInstance(typeof(T), graphics);
            pixelShaders.Add(ps);

            return ps;
        }
        /// <summary>
        /// Gets or creates a built-in compute shader
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="singleton">Use one instance</param>
        public static T GetComputeShader<T>(bool singleton = true) where T : IBuiltInShader<EngineComputeShader>
        {
            if (!singleton)
            {
                return (T)Activator.CreateInstance(typeof(T), graphics);
            }

            T cs = computeShaders.OfType<T>().FirstOrDefault();
            if (cs != null)
            {
                return cs;
            }

            cs = (T)Activator.CreateInstance(typeof(T), graphics);
            computeShaders.Add(cs);

            return cs;
        }
        /// <summary>
        /// Gets or creates a built-in drawer
        /// </summary>
        /// <typeparam name="T">Drawer type</typeparam>
        /// <param name="singleton">Use one instance</param>
        public static T GetDrawer<T>(bool singleton = true) where T : IBuiltInDrawer
        {
            if (!singleton)
            {
                return (T)Activator.CreateInstance(typeof(T));
            }

            T dr = drawers.OfType<T>().FirstOrDefault();
            if (dr != null)
            {
                return dr;
            }

            dr = (T)Activator.CreateInstance(typeof(T));
            drawers.Add(dr);

            return dr;
        }

        /// <summary>
        /// Sampler point
        /// </summary>
        public static EngineSamplerState GetSamplerPoint()
        {
            if (samplerPoint != null)
            {
                return samplerPoint;
            }

            samplerPoint = EngineSamplerState.Point(graphics, nameof(BuiltInShaders));

            return samplerPoint;
        }
        /// <summary>
        /// Sampler linear
        /// </summary>
        public static EngineSamplerState GetSamplerLinear()
        {
            if (samplerLinear != null)
            {
                return samplerLinear;
            }

            samplerLinear = EngineSamplerState.Linear(graphics, nameof(BuiltInShaders));

            return samplerLinear;
        }
        /// <summary>
        /// Sampler anisotropic
        /// </summary>
        public static EngineSamplerState GetSamplerAnisotropic()
        {
            if (samplerAnisotropic != null)
            {
                return samplerAnisotropic;
            }

            samplerAnisotropic = EngineSamplerState.Anisotropic(graphics, nameof(BuiltInShaders), 4);

            return samplerAnisotropic;
        }
        /// <summary>
        /// Sampler comparison less equal
        /// </summary>
        public static EngineSamplerState GetSamplerComparisonLessEqualBorder()
        {
            if (samplerComparisonLessEqualBorder != null)
            {
                return samplerComparisonLessEqualBorder;
            }

            samplerComparisonLessEqualBorder = EngineSamplerState.ComparisonLessEqualBorder(graphics, nameof(BuiltInShaders));

            return samplerComparisonLessEqualBorder;
        }
        /// <summary>
        /// Sampler comparison less equal
        /// </summary>
        public static EngineSamplerState GetSamplerComparisonLessEqualClamp()
        {
            if (samplerComparisonLessEqualClamp != null)
            {
                return samplerComparisonLessEqualClamp;
            }

            samplerComparisonLessEqualClamp = EngineSamplerState.ComparisonLessEqualClamp(graphics, nameof(BuiltInShaders));

            return samplerComparisonLessEqualClamp;
        }

        /// <summary>
        /// Updates global data
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="materialPalette">Material palette resource view</param>
        /// <param name="materialPaletteWidth">Material palette width</param>
        /// <param name="animationPalette">Animation palette resource view</param>
        /// <param name="animationPaletteWidth">Animation palette width</param>
        public static void UpdateGlobals(IEngineDeviceContext dc, EngineShaderResourceView materialPalette, uint materialPaletteWidth, EngineShaderResourceView animationPalette, uint animationPaletteWidth)
        {
            var cbGlobal = GetConstantBuffer<Global>();
            cbGlobal?.WriteData(Global.Build(materialPaletteWidth, animationPaletteWidth));
            dc.UpdateConstantBuffer(cbGlobal);

            rvMaterialPalette = materialPalette;
            rvAnimationPalette = animationPalette;
        }
        /// <summary>
        /// Gets the built-in global shaders constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetGlobalConstantBuffer()
        {
            return GetConstantBuffer<Global>();
        }
        /// <summary>
        /// Gets the built-in global vertex shader material palette texture
        /// </summary>
        public static EngineShaderResourceView GetMaterialPaletteResourceView()
        {
            return rvMaterialPalette;
        }
        /// <summary>
        /// Gets the built-in global vertex shader animation palette texture
        /// </summary>
        public static EngineShaderResourceView GetAnimationPaletteResourceView()
        {
            return rvAnimationPalette;
        }

        /// <summary>
        /// Updates per-frame data
        /// </summary>
        /// <param name="context">Draw context</param>
        public static void UpdatePerFrame(DrawContext context)
        {
            var dc = context.DeviceContext;

            var cbPerFrame = GetConstantBuffer<PerFrame>();
            cbPerFrame?.WriteData(PerFrame.Build(context));
            var cbHemispheric = GetConstantBuffer<PSHemispheric>();
            cbHemispheric?.WriteData(PSHemispheric.Build(context));
            var cbDirectionals = GetConstantBuffer<PSDirectional>();
            cbDirectionals?.WriteData(PSDirectional.Build(context));
            var cbSpots = GetConstantBuffer<PSSpots>();
            cbSpots?.WriteData(PSSpots.Build(context));
            var cbPoints = GetConstantBuffer<PSPoints>();
            cbPoints?.WriteData(PSPoints.Build(context));

            dc.UpdateConstantBuffer(cbPerFrame);
            dc.UpdateConstantBuffer(cbHemispheric);
            dc.UpdateConstantBuffer(cbDirectionals);
            dc.UpdateConstantBuffer(cbSpots);
            dc.UpdateConstantBuffer(cbPoints);

            rvShadowMapDir = context.ShadowMapDirectional?.Texture;
            rvShadowMapSpot = context.ShadowMapSpot?.Texture;
            rvShadowMapPoint = context.ShadowMapPoint?.Texture;
        }
        /// <summary>
        /// Gets the built-in per frame constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetPerFrameConstantBuffer()
        {
            return GetConstantBuffer<PerFrame>();
        }

        /// <summary>
        /// Gets the built-in hemispheric light pixel shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetHemisphericConstantBuffer()
        {
            return GetConstantBuffer<PSHemispheric>();
        }
        /// <summary>
        /// Gets the built-in directional lights pixel shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetDirectionalsConstantBuffer()
        {
            return GetConstantBuffer<PSDirectional>();
        }
        /// <summary>
        /// Gets the built-in spot lights pixel shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetSpotsConstantBuffer()
        {
            return GetConstantBuffer<PSSpots>();
        }
        /// <summary>
        /// Gets the built-in point lights pixel shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetPointsConstantBuffer()
        {
            return GetConstantBuffer<PSPoints>();
        }
        /// <summary>
        /// Gets the built-in global pixel shader directional shadow map
        /// </summary>
        public static EngineShaderResourceView GetShadowMapDirResourceView()
        {
            return rvShadowMapDir;
        }
        /// <summary>
        /// Gets the built-in global pixel shader sopt shadow map
        /// </summary>
        public static EngineShaderResourceView GetShadowMapSpotResourceView()
        {
            return rvShadowMapSpot;
        }
        /// <summary>
        /// Gets the built-in global pixel shader point shadow map
        /// </summary>
        public static EngineShaderResourceView GetShadowMapPointResourceView()
        {
            return rvShadowMapPoint;
        }
    }
}
