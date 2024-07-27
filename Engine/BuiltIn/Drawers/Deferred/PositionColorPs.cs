﻿using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Deferred
{
    /// <summary>
    /// Position color pixel shader
    /// </summary>
    public class PositionColorPs : IShader<EnginePixelShader>
    {
        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionColorPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<PositionColorPs>("main", DeferredRenderingResources.PositionColor_ps);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            //No resources
        }
    }
}