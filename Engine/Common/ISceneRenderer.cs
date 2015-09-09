using SharpDX.Direct3D11;
using System;

namespace Engine.Common
{
    /// <summary>
    /// Scene renderer interface
    /// </summary>
    public interface ISceneRenderer : IScreenFitted, IDisposable
    {
        /// <summary>
        /// Updates renderer parameters
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
        ShaderResourceView GetResource(SceneRendererResultEnum result);
    }
}
