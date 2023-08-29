using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;

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
        /// Shader list
        /// </summary>
        private static readonly List<IEngineShader> shaders = new();
        /// <summary>
        /// Constant buffer list
        /// </summary>
        private static readonly List<IEngineConstantBuffer> constantBuffers = new();
        /// <summary>
        /// Sampler state list
        /// </summary>
        private static readonly List<EngineSamplerState> samplerStates = new();
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
            samplerStates.ForEach(cb => cb?.Dispose());
            samplerStates.Clear();

            constantBuffers.ForEach(cb => cb?.Dispose());
            constantBuffers.Clear();

            shaders.ForEach(sh => sh?.Dispose());
            shaders.Clear();

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

            vertexShaders.Clear();
            hullShaders.Clear();
            domainShaders.Clear();
            geometryShaders.Clear();
            pixelShaders.Clear();
            computeShaders.Clear();
        }

        /// <summary>
        /// Compiles a shader or retrieves it from the shader cache
        /// </summary>
        /// <typeparam name="T">Type of shader</typeparam>
        /// <param name="entryPoint">Entry point function name</param>
        /// <param name="byteCode">Shader byte code</param>
        /// <param name="profile">Shader profile</param>
        public static EngineVertexShader CompileVertexShader<T>(string entryPoint, byte[] byteCode, string profile = null) where T : IBuiltInShader<EngineVertexShader>
        {
            string name = typeof(T).FullName;

            EngineVertexShader sh = shaders.OfType<EngineVertexShader>().FirstOrDefault(s => s.Name == name);
            if (sh != null)
            {
                return sh;
            }

            sh = graphics.CompileVertexShader(name, entryPoint, byteCode, profile ?? HelperShaders.VSProfile);
            shaders.Add(sh);
            return sh;
        }
        /// <summary>
        /// Compiles a shader or retrieves it from the shader cache
        /// </summary>
        /// <typeparam name="T">Type of shader</typeparam>
        /// <param name="entryPoint">Entry point function name</param>
        /// <param name="byteCode">Shader byte code</param>
        /// <param name="profile">Shader profile</param>
        public static EngineHullShader CompileHullShader<T>(string entryPoint, byte[] byteCode, string profile = null) where T : IBuiltInShader<EngineHullShader>
        {
            string name = typeof(T).FullName;

            EngineHullShader sh = shaders.OfType<EngineHullShader>().FirstOrDefault(s => s.Name == name);
            if (sh != null)
            {
                return sh;
            }

            sh = graphics.CompileHullShader(name, entryPoint, byteCode, profile ?? HelperShaders.HSProfile);
            shaders.Add(sh);
            return sh;
        }
        /// <summary>
        /// Compiles a shader or retrieves it from the shader cache
        /// </summary>
        /// <typeparam name="T">Type of shader</typeparam>
        /// <param name="entryPoint">Entry point function name</param>
        /// <param name="byteCode">Shader byte code</param>
        /// <param name="profile">Shader profile</param>
        public static EngineDomainShader CompileDomainShader<T>(string entryPoint, byte[] byteCode, string profile = null) where T : IBuiltInShader<EngineDomainShader>
        {
            string name = typeof(T).FullName;

            EngineDomainShader sh = shaders.OfType<EngineDomainShader>().FirstOrDefault(s => s.Name == name);
            if (sh != null)
            {
                return sh;
            }

            sh = graphics.CompileDomainShader(name, entryPoint, byteCode, profile ?? HelperShaders.DSProfile);
            shaders.Add(sh);
            return sh;
        }
        /// <summary>
        /// Compiles a shader or retrieves it from the shader cache
        /// </summary>
        /// <typeparam name="T">Type of shader</typeparam>
        /// <param name="entryPoint">Entry point function name</param>
        /// <param name="byteCode">Shader byte code</param>
        /// <param name="profile">Shader profile</param>
        public static EngineGeometryShader CompileGeometryShader<T>(string entryPoint, byte[] byteCode, string profile = null) where T : IBuiltInShader<EngineGeometryShader>
        {
            string name = typeof(T).FullName;

            EngineGeometryShader sh = shaders.OfType<EngineGeometryShader>().FirstOrDefault(s => s.Name == name);
            if (sh != null)
            {
                return sh;
            }

            sh = graphics.CompileGeometryShader(name, entryPoint, byteCode, profile ?? HelperShaders.GSProfile);
            shaders.Add(sh);
            return sh;
        }
        /// <summary>
        /// Compiles a shader or retrieves it from the shader cache
        /// </summary>
        /// <typeparam name="T">Type of shader</typeparam>
        /// <param name="entryPoint">Entry point function name</param>
        /// <param name="byteCode">Shader byte code</param>
        /// <param name="so">Stream out configuration</param>
        /// <param name="profile">Shader profile</param>
        public static EngineGeometryShader CompileGeometryShaderWithStreamOut<T>(string entryPoint, byte[] byteCode, EngineStreamOutputElement[] so, string profile = null)
        {
            string name = typeof(T).FullName;

            EngineGeometryShader sh = shaders.OfType<EngineGeometryShader>().FirstOrDefault(s => s.Name == name);
            if (sh != null)
            {
                return sh;
            }

            sh = graphics.CompileGeometryShaderWithStreamOut(name, entryPoint, byteCode, profile ?? HelperShaders.GSProfile, so);
            shaders.Add(sh);
            return sh;
        }
        /// <summary>
        /// Compiles a shader or retrieves it from the shader cache
        /// </summary>
        /// <typeparam name="T">Type of shader</typeparam>
        /// <param name="entryPoint">Entry point function name</param>
        /// <param name="byteCode">Shader byte code</param>
        /// <param name="profile">Shader profile</param>
        public static EnginePixelShader CompilePixelShader<T>(string entryPoint, byte[] byteCode, string profile = null) where T : IBuiltInShader<EnginePixelShader>
        {
            string name = typeof(T).FullName;

            EnginePixelShader sh = shaders.OfType<EnginePixelShader>().FirstOrDefault(s => s.Name == name);
            if (sh != null)
            {
                return sh;
            }

            sh = graphics.CompilePixelShader(name, entryPoint, byteCode, profile ?? HelperShaders.PSProfile);
            shaders.Add(sh);
            return sh;
        }
        /// <summary>
        /// Compiles a shader or retrieves it from the shader cache
        /// </summary>
        /// <typeparam name="T">Type of shader</typeparam>
        /// <param name="entryPoint">Entry point function name</param>
        /// <param name="byteCode">Shader byte code</param>
        /// <param name="profile">Shader profile</param>
        public static EngineComputeShader CompileComputeShader<T>(string entryPoint, byte[] byteCode, string profile = null) where T : IBuiltInShader<EngineComputeShader>
        {
            string name = typeof(T).FullName;

            EngineComputeShader sh = shaders.OfType<EngineComputeShader>().FirstOrDefault(s => s.Name == name);
            if (sh != null)
            {
                return sh;
            }

            sh = graphics.CompileComputeShader(name, entryPoint, byteCode, profile ?? HelperShaders.DSProfile);
            shaders.Add(sh);
            return sh;
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
                return (T)Activator.CreateInstance(typeof(T));
            }

            T vs = vertexShaders.OfType<T>().FirstOrDefault();
            if (vs != null)
            {
                return vs;
            }

            vs = (T)Activator.CreateInstance(typeof(T));
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
                return (T)Activator.CreateInstance(typeof(T));
            }

            T hs = hullShaders.OfType<T>().FirstOrDefault();
            if (hs != null)
            {
                return hs;
            }

            hs = (T)Activator.CreateInstance(typeof(T));
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
                return (T)Activator.CreateInstance(typeof(T));
            }

            T ds = domainShaders.OfType<T>().FirstOrDefault();
            if (ds != null)
            {
                return ds;
            }

            ds = (T)Activator.CreateInstance(typeof(T));
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
                return (T)Activator.CreateInstance(typeof(T));
            }

            T gs = geometryShaders.OfType<T>().FirstOrDefault();
            if (gs != null)
            {
                return gs;
            }

            gs = (T)Activator.CreateInstance(typeof(T));
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
                return (T)Activator.CreateInstance(typeof(T));
            }

            T ps = pixelShaders.OfType<T>().FirstOrDefault();
            if (ps != null)
            {
                return ps;
            }

            ps = (T)Activator.CreateInstance(typeof(T));
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
                return (T)Activator.CreateInstance(typeof(T));
            }

            T cs = computeShaders.OfType<T>().FirstOrDefault();
            if (cs != null)
            {
                return cs;
            }

            cs = (T)Activator.CreateInstance(typeof(T));
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
            samplerPoint ??= EngineSamplerState.Point(graphics, nameof(BuiltInShaders));

            return samplerPoint;
        }
        /// <summary>
        /// Sampler linear
        /// </summary>
        public static EngineSamplerState GetSamplerLinear()
        {
            samplerLinear ??= EngineSamplerState.Linear(graphics, nameof(BuiltInShaders));

            return samplerLinear;
        }
        /// <summary>
        /// Sampler anisotropic
        /// </summary>
        public static EngineSamplerState GetSamplerAnisotropic()
        {
            samplerAnisotropic ??= EngineSamplerState.Anisotropic(graphics, nameof(BuiltInShaders), 4);

            return samplerAnisotropic;
        }
        /// <summary>
        /// Sampler comparison less equal
        /// </summary>
        public static EngineSamplerState GetSamplerComparisonLessEqualBorder()
        {
            samplerComparisonLessEqualBorder ??= EngineSamplerState.ComparisonLessEqualBorder(graphics, nameof(BuiltInShaders));

            return samplerComparisonLessEqualBorder;
        }
        /// <summary>
        /// Sampler comparison less equal
        /// </summary>
        public static EngineSamplerState GetSamplerComparisonLessEqualClamp()
        {
            samplerComparisonLessEqualClamp ??= EngineSamplerState.ComparisonLessEqualClamp(graphics, nameof(BuiltInShaders));

            return samplerComparisonLessEqualClamp;
        }
        /// <summary>
        /// Gets a custom sampler
        /// </summary>
        /// <param name="name">Sampler name</param>
        /// <param name="samplerDesc">Sampler description</param>
        public static EngineSamplerState GetSamplerCustom(string name, EngineSamplerStateDescription samplerDesc)
        {
            var s = EngineSamplerState.Create(graphics, name, samplerDesc);
            samplerStates.Add(s);
            return s;
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
            dc.UpdateConstantBuffer(cbGlobal, Global.Build(materialPaletteWidth, animationPaletteWidth));

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
            var cbHemispheric = GetConstantBuffer<PSHemispheric>();
            var cbDirectionals = GetConstantBuffer<PSDirectional>();
            var cbSpots = GetConstantBuffer<PSSpots>();
            var cbPoints = GetConstantBuffer<PSPoints>();

            dc.UpdateConstantBuffer(cbPerFrame, PerFrame.Build(context));
            dc.UpdateConstantBuffer(cbHemispheric, PSHemispheric.Build(context));
            dc.UpdateConstantBuffer(cbDirectionals, PSDirectional.Build(context));
            dc.UpdateConstantBuffer(cbSpots, PSSpots.Build(context));
            dc.UpdateConstantBuffer(cbPoints, PSPoints.Build(context));

            rvShadowMapDir = context.ShadowMapDirectional?.DepthMapTexture;
            rvShadowMapSpot = context.ShadowMapSpot?.DepthMapTexture;
            rvShadowMapPoint = context.ShadowMapPoint?.DepthMapTexture;
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
