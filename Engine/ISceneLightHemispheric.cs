using SharpDX;

namespace Engine
{
    /// <summary>
    /// Hemispheric light interface
    /// </summary>
    public interface ISceneLightHemispheric : ISceneLight, IHasGameState
    {
        /// <summary>
        /// Ambient down color
        /// </summary>
        Color3 AmbientDown { get; set; }
        /// <summary>
        /// Ambient up color
        /// </summary>
        Color3 AmbientUp { get; set; }
    }
}