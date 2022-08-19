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
            pixelShaders.ForEach(ps => ps?.Dispose());
            pixelShaders.Clear();
            drawers.Clear();

            samplerPoint?.Dispose();
            samplerPoint = null;
            samplerLinear?.Dispose();
            samplerLinear = null;
            samplerAnisotropic?.Dispose();
            samplerAnisotropic = null;
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
            return (T)Activator.CreateInstance(typeof(T), graphics);
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

            samplerPoint = EngineSamplerState.Point(graphics);

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

            samplerLinear = EngineSamplerState.Linear(graphics);

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

            samplerAnisotropic = EngineSamplerState.Anisotropic(graphics, 4);

            return samplerAnisotropic;
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
        /// Updates per-frame data
        /// </summary>
        /// <param name="localTransform">Local transform</param>
        /// <param name="context">Draw context</param>
        public static void UpdatePerFrame(Matrix localTransform, DrawContextShadows context)
        {
            if (context == null)
            {
                return;
            }

            var vsPerFrame = GetConstantBuffer<VSPerFrame>();
            vsPerFrame?.WriteData(VSPerFrame.Build(localTransform, context));
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
        /// <param name="localTransform">Local transform</param>
        /// <param name="context">Draw context</param>
        public static void UpdatePerFrame(Matrix localTransform, DrawContext context)
        {
            if (context == null)
            {
                return;
            }

            var vsPerFrame = GetConstantBuffer<VSPerFrame>();
            vsPerFrame?.WriteData(VSPerFrame.Build(localTransform, context));

            var psPerFrameNoLit = GetConstantBuffer<PSPerFrameNoLit>();
            psPerFrameNoLit?.WriteData(PSPerFrameNoLit.Build(context));

            var psPerFrameLit = GetConstantBuffer<PSPerFrameLit>();
            psPerFrameLit?.WriteData(PSPerFrameLit.Build(context));
            psPerFrameLitShadowMapDir = context.ShadowMapDirectional?.Texture;
            psPerFrameLitShadowMapSpot = context.ShadowMapSpot?.Texture;
            psPerFrameLitShadowMapPoint = context.ShadowMapPoint?.Texture;
        }
        /// <summary>
        /// Gets the built-in per frame pixel shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetPSPerFrameNoLit()
        {
            return GetConstantBuffer<PSPerFrameNoLit>();
        }
        /// <summary>
        /// Gets the built-in per frame with lights pixel shader constant buffer
        /// </summary>
        public static IEngineConstantBuffer GetPSPerFrameLit()
        {
            return GetConstantBuffer<PSPerFrameLit>();
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
