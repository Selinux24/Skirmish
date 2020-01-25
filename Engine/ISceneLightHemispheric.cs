using SharpDX;

namespace Engine
{
    /// <summary>
    /// Hemispheric light interface
    /// </summary>
    public interface ISceneLightHemispheric : ISceneLight
    {
        /// <summary>
        /// Ambient down color
        /// </summary>
        Color4 AmbientDown { get; set; }
        /// <summary>
        /// Ambient up color
        /// </summary>
        Color4 AmbientUp { get; set; }
    }
}