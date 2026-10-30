
namespace Engine
{
    using Engine.Mathematics;

    /// <summary>
    /// Hemispheric light interface
    /// </summary>
    public interface ISceneLightHemispheric : ISceneLight, IHasGameState
    {
        /// <summary>
        /// Ambient down color
        /// </summary>
        Vector3 AmbientDown { get; set; }
        /// <summary>
        /// Ambient up color
        /// </summary>
        Vector3 AmbientUp { get; set; }
    }
}