using System;

namespace Engine
{
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
        /// <param name="scene">Scene</param>
        void Update(GameTime gameTime, Scene scene);
        /// <summary>
        /// Draws scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        void Draw(GameTime gameTime, Scene scene);
        /// <summary>
        /// Gets renderer resources
        /// </summary>
        /// <param name="result">Resource type</param>
        /// <returns>Returns renderer specified resource, if renderer produces that resource.</returns>
        EngineShaderResourceView GetResource(SceneRendererResults result);
    }
}
