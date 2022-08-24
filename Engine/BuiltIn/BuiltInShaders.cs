using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn
{
    using Engine.BuiltInEffects;
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
        private static readonly List<IEngineConstantBuffer> constantBuffers = new List<IEngineConstantBuffer>();
        /// <summary>
        /// Vertex shader list
        /// </summary>
        private static readonly List<IBuiltInVertexShader> vertexShaders = new List<IBuiltInVertexShader>();
        /// <summary>
        /// Geometry shader list
        /// </summary>
        private static readonly List<IBuiltInGeometryShader> geometryShaders = new List<IBuiltInGeometryShader>();
        /// <summary>
        /// Pixel shader list
        /// </summary>
        private static readonly List<IBuiltInPixelShader> pixelShaders = new List<IBuiltInPixelShader>();
        /// <summary>
        /// Drawer list
        /// </summary>
        private static readonly List<IBuiltInDrawer> drawers = new List<IBuiltInDrawer>();

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
        private static EngineShaderResourceView vsGlobalMaterialPalette;
        /// <summary>
        /// Animation palette resource view
        /// </summary>
        private static EngineShaderResourceView vsGlobalAnimationPalette;
        /// <summary>
        /// Directional shadow map resource view
        /// </summary>
        private static EngineShaderResourceView psPerFrameLitShadowMapDir;
        /// <summary>
        /// Spot shadow map resource view
        /// </summary>
        private static EngineShaderResourceView psPerFrameLitShadowMapSpot;
        /// <summary>
        /// Point shadow map resource view
        /// </summary>
        private static EngineShaderResourceView psPerFrameLitShadowMapPoint;

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
            geometryShaders.ForEach(gs => gs?.Dispose());
            geometryShaders.Clear();
            pixelShaders.ForEach(ps => ps?.Dispose());
            pixelShaders.Clear();
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
        public static EngineConstantBuffer<T> GetConstantBuffer<T>() where T : struct, IBufferData
        {
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
        public static T GetVertexShader<T>() where T : IBuiltInVertexShader
        {
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
        /// Gets or creates a built-in geometry shader
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        public static T GetGeometryShader<T>() where T : IBuiltInGeometryShader
        {
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
        public static T GetPixelShader<T>() where T : IBuiltInPixelShader
        {
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
        /// Gets or creates a built-in drawer
        /// </summary>
        /// <typeparam name="T">Drawer type</typeparam>
        public static T GetDrawer<T>() where T : IBuiltInDrawer
        {
            T dr = drawers.OfType<T>().FirstOrDefault();
            if (dr != null)
            {
                return dr;
            }

            dr = (T)Activator.CreateInstance(typeof(T), graphics);
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
        /// <param name="materialPalette">Material palette resource view</param>
        /// <param name="materialPaletteWidth">Material palette width</param>
        /// <param name="animationPalette">Animation palette resource view</param>
        /// <param name="animationPaletteWidth">Animation palette width</param>
        public static void UpdateGlobals(EngineShaderResourceView materialPalette, uint materialPaletteWidth, EngineShaderResourceView animationPalette, uint animationPaletteWidth)
        {
            vsGlobalMaterialPalette = materialPalette;
            vsGlobalAnimationPalette = animationPalette;

            var vsGlobal = GetConstantBuffer<VSGlobal>();
            vsGlobal?.WriteData(VSGlobal.Build(materialPaletteWidth, animationPaletteWidth));
        }
        /// <summary>
        /// Gets the built-in global vertex shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetVSGlobal()
        {
            return GetConstantBuffer<VSGlobal>();
        }
        /// <summary>
        /// Gets the built-in global vertex shader material palette texture
        /// </summary>
        public static EngineShaderResourceView GetMaterialPalette()
        {
            return vsGlobalMaterialPalette;
        }
        /// <summary>
        /// Gets the built-in global vertex shader animation palette texture
        /// </summary>
        public static EngineShaderResourceView GetAnimationPalette()
        {
            return vsGlobalAnimationPalette;
        }

        /// <summary>
        /// Gets the built-in per frame vertex shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetVSPerFrame()
        {
            return GetConstantBuffer<VSPerFrame>();
        }

        /// <summary>
        /// Updates per-frame data
        /// </summary>
        /// <param name="context">Draw context</param>
        public static void UpdatePerFrame(DrawContext context)
        {
            if (context == null)
            {
                return;
            }

            var psPerFrame = GetConstantBuffer<PSPerFrame>();
            psPerFrame?.WriteData(PSPerFrame.Build(context));

            var psHemispheric = GetConstantBuffer<PSHemispheric>();
            psHemispheric?.WriteData(PSHemispheric.Build(context));
            var psDirectionals = GetConstantBuffer<PSDirectional>();
            psDirectionals?.WriteData(PSDirectional.Build(context));
            var psSpots = GetConstantBuffer<PSSpots>();
            psSpots?.WriteData(PSSpots.Build(context));
            var psPoints = GetConstantBuffer<PSPoints>();
            psPoints?.WriteData(PSPoints.Build(context));

            psPerFrameLitShadowMapDir = context.ShadowMapDirectional?.Texture;
            psPerFrameLitShadowMapSpot = context.ShadowMapSpot?.Texture;
            psPerFrameLitShadowMapPoint = context.ShadowMapPoint?.Texture;
        }
        /// <summary>
        /// Updates per-object data
        /// </summary>
        /// <param name="localTransform">Local transform</param>
        /// <param name="viewProjection">View projection matrix</param>
        public static void UpdatePerObject(Matrix localTransform, Matrix viewProjection)
        {
            var vsPerFrame = GetConstantBuffer<VSPerFrame>();
            vsPerFrame?.WriteData(VSPerFrame.Build(localTransform, viewProjection));
        }

        /// <summary>
        /// Gets the built-in per frame pixel shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetPSPerFrame()
        {
            return GetConstantBuffer<PSPerFrame>();
        }
        /// <summary>
        /// Gets the built-in hemispheric light pixel shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetPSHemispheric()
        {
            return GetConstantBuffer<PSHemispheric>();
        }
        /// <summary>
        /// Gets the built-in directional lights pixel shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetPSDirectionals()
        {
            return GetConstantBuffer<PSDirectional>();
        }
        /// <summary>
        /// Gets the built-in spot lights pixel shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetPSSpots()
        {
            return GetConstantBuffer<PSSpots>();
        }
        /// <summary>
        /// Gets the built-in point lights pixel shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetPSPoints()
        {
            return GetConstantBuffer<PSPoints>();
        }
        /// <summary>
        /// Gets the built-in global pixel shader directional shadow map
        /// </summary>
        public static EngineShaderResourceView GetPSPerFrameLitShadowMapDir()
        {
            return psPerFrameLitShadowMapDir;
        }
        /// <summary>
        /// Gets the built-in global pixel shader sopt shadow map
        /// </summary>
        public static EngineShaderResourceView GetPSPerFrameLitShadowMapSpot()
        {
            return psPerFrameLitShadowMapSpot;
        }
        /// <summary>
        /// Gets the built-in global pixel shader point shadow map
        /// </summary>
        public static EngineShaderResourceView GetPSPerFrameLitShadowMapPoint()
        {
            return psPerFrameLitShadowMapPoint;
        }
    }
}
