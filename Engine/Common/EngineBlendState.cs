using SharpDX;
using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Engine blend state
    /// </summary>
    public class EngineBlendState : IDisposable
    {
        /// <summary>
        /// Internal blend state
        /// </summary>
        private BlendState1 state = null;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Blend factor
        /// </summary>
        public Color4? BlendFactor { get; set; }
        /// <summary>
        /// Sample mask
        /// </summary>
        public int SampleMask { get; set; }

        /// <summary>
        /// Creates a default blend state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Returns the default blend state</returns>
        public static EngineBlendState Default(Graphics graphics)
        {
            BlendStateDescription1 desc = new BlendStateDescription1
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };

            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = BlendOption.One;
            desc.RenderTarget[0].DestinationBlend = BlendOption.Zero;

            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

            return graphics.CreateBlendState(nameof(Default), desc, Color.Transparent, -1);
        }
        /// <summary>
        /// Creates an alpha enabled blend state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Creates the alpha enabled blend state</returns>
        public static EngineBlendState AlphaBlend(Graphics graphics)
        {
            BlendStateDescription1 desc = new BlendStateDescription1
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };

            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;

            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

            return graphics.CreateBlendState(nameof(AlphaBlend), desc, Color.Transparent, -1);
        }
        /// <summary>
        /// Creates an alpha enabled conservative blend state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Creates the alpha enabled blend state</returns>
        public static EngineBlendState AlphaConservativeBlend(Graphics graphics)
        {
            BlendStateDescription1 desc = new BlendStateDescription1
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };

            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;

            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.InverseSourceAlpha;

            return graphics.CreateBlendState(nameof(AlphaConservativeBlend), desc, Color.Transparent, -1);
        }
        /// <summary>
        /// Creates a transparent enabled blend state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <remarks>It's equal to AlphaBlend, but with AlphaToCoverageEnable enabled</remarks>
        /// <returns>Creates the transparent enabled blend state</returns>
        public static EngineBlendState Transparent(Graphics graphics)
        {
            BlendStateDescription1 desc = new BlendStateDescription1
            {
                AlphaToCoverageEnable = true,
                IndependentBlendEnable = false
            };

            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;

            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

            return graphics.CreateBlendState(nameof(Transparent), desc, Color.Transparent, -1);
        }
        /// <summary>
        /// Creates a transparent enabled conservative blend state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <remarks>It's equal to AlphaBlend, but with AlphaToCoverageEnable enabled</remarks>
        /// <returns>Creates the transparent enabled blend state</returns>
        public static EngineBlendState TransparentConservative(Graphics graphics)
        {
            BlendStateDescription1 desc = new BlendStateDescription1
            {
                AlphaToCoverageEnable = true,
                IndependentBlendEnable = false
            };

            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;

            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.InverseSourceAlpha;

            return graphics.CreateBlendState(nameof(TransparentConservative), desc, Color.Transparent, -1);
        }
        /// <summary>
        /// Creates an additive blend state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Creates the additive enabled blend state</returns>
        public static EngineBlendState Additive(Graphics graphics)
        {
            BlendStateDescription1 desc = new BlendStateDescription1
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };

            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationBlend = BlendOption.One;

            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

            return graphics.CreateBlendState(nameof(Additive), desc, Color.Transparent, -1);
        }
        /// <summary>
        /// Creates a deferred composer blend state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="rtCount">Render target count</param>
        /// <returns>Creates the deferred composer blend state</returns>
        public static EngineBlendState DeferredComposer(Graphics graphics, int rtCount)
        {
            BlendStateDescription1 desc = new BlendStateDescription1
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = true
            };

            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = BlendOption.One;
            desc.RenderTarget[0].DestinationBlend = BlendOption.Zero;
            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

            for (int i = 1; i < rtCount; i++)
            {
                desc.RenderTarget[i].IsBlendEnabled = true;
                desc.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[i].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[i].SourceBlend = BlendOption.One;
                desc.RenderTarget[i].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[i].DestinationAlphaBlend = BlendOption.Zero;
            }

            return graphics.CreateBlendState(nameof(DeferredComposer), desc, Color.Transparent, -1);
        }
        /// <summary>
        /// Creates a deferred composer transparent enabled blend state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="rtCount">Render target count</param>
        /// <returns>Creates the deferred composer transparent enabled blend state</returns>
        public static EngineBlendState DeferredComposerTransparent(Graphics graphics, int rtCount)
        {
            BlendStateDescription1 desc = new BlendStateDescription1
            {
                AlphaToCoverageEnable = true,
                IndependentBlendEnable = true
            };

            //Transparent blending only in first buffer
            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

            for (int i = 1; i < rtCount; i++)
            {
                desc.RenderTarget[i].IsBlendEnabled = true;
                desc.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[i].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[i].SourceBlend = BlendOption.One;
                desc.RenderTarget[i].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[i].DestinationAlphaBlend = BlendOption.Zero;
            }

            return graphics.CreateBlendState(nameof(DeferredComposerTransparent), desc, Color.Transparent, -1);
        }
        /// <summary>
        /// Creates a deferred composer alpha enabled blend state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="rtCount">Render target count</param>
        /// <returns>Creates the deferred composer alpha enabled blend state</returns>
        public static EngineBlendState DeferredComposerAlpha(Graphics graphics, int rtCount)
        {
            BlendStateDescription1 desc = new BlendStateDescription1
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = true
            };

            //Additive blending only in first buffer
            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

            for (int i = 1; i < rtCount; i++)
            {
                desc.RenderTarget[i].IsBlendEnabled = true;
                desc.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[i].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[i].SourceBlend = BlendOption.One;
                desc.RenderTarget[i].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[i].DestinationAlphaBlend = BlendOption.Zero;
            }

            return graphics.CreateBlendState(nameof(DeferredComposerAlpha), desc, Color.Transparent, -1);
        }
        /// <summary>
        /// Creates a deferred composer additive enabled blend state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="rtCount">Render target count</param>
        /// <returns>Creates the deferred composer additive enabled blend state</returns>
        public static EngineBlendState DeferredComposerAdditive(Graphics graphics, int rtCount)
        {
            BlendStateDescription1 desc = new BlendStateDescription1
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = true
            };

            //Additive blending only in first buffer
            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationBlend = BlendOption.One;

            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

            for (int i = 1; i < rtCount; i++)
            {
                desc.RenderTarget[i].IsBlendEnabled = true;
                desc.RenderTarget[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[i].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[i].SourceBlend = BlendOption.One;
                desc.RenderTarget[i].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[i].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[i].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[i].DestinationAlphaBlend = BlendOption.Zero;
            }

            return graphics.CreateBlendState(nameof(DeferredComposerAdditive), desc, Color.Transparent, -1);
        }
        /// <summary>
        /// Creates a deferred lighting blend state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Creates the deferred lighting blend state</returns>
        public static EngineBlendState DeferredLighting(Graphics graphics)
        {
            BlendStateDescription1 desc = new BlendStateDescription1
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };

            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceBlend = BlendOption.One;
            desc.RenderTarget[0].DestinationBlend = BlendOption.One;
            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;

            return graphics.CreateBlendState(nameof(DeferredLighting), desc, Color.Transparent, -1);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="blendState">Blend state</param>
        /// <param name="blendFactor">Blend factor</param>
        /// <param name="sampleMask">Sample mask</param>
        internal EngineBlendState(string name, BlendState1 blendState, Color4? blendFactor, int sampleMask)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A blend state name must be specified.");
            state = blendState ?? throw new ArgumentNullException(nameof(blendState), "A blend state must be specified.");
            BlendFactor = blendFactor;
            SampleMask = sampleMask;

            state.DebugName = name;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineBlendState()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                state?.Dispose();
                state = null;
            }
        }

        /// <summary>
        /// Gets the internal blend state
        /// </summary>
        /// <returns>Returns the internal blend state</returns>
        internal BlendState1 GetBlendState()
        {
            return state;
        }
    }
}
