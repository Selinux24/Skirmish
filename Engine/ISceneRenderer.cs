﻿using System;

namespace Engine
{
    using Engine.BuiltIn.PostProcess;
    using Engine.Common;

    /// <summary>
    /// Scene renderer interface
    /// </summary>
    public interface ISceneRenderer : IScreenFitted, IDisposable
    {
        /// <summary>
        /// Updates scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void Update(GameTime gameTime);
        /// <summary>
        /// Draws scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void Draw(GameTime gameTime);

        /// <summary>
        /// Sets the post-processing effect
        /// </summary>
        /// <param name="renderPass">Render pass</param>
        /// <param name="state">State</param>
        void SetPostProcessingEffect(RenderPass renderPass, BuiltInPostProcessState state);
        /// <summary>
        /// Clears the post-processing effect
        /// </summary>
        void ClearPostProcessingEffects();

        /// <summary>
        /// Updates the scene renderer globals
        /// </summary>
        void UpdateGlobals();

        /// <summary>
        /// Gets renderer resources
        /// </summary>
        /// <param name="result">Resource type</param>
        /// <returns>Returns renderer specified resource, if renderer produces that resource.</returns>
        EngineShaderResourceView GetResource(SceneRendererResults result);
    }
}
