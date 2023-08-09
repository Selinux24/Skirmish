using SharpDX;

namespace Engine
{
    /// <summary>
    /// Directional light
    /// </summary>
    public interface ISceneLightDirectional : ISceneLight, IHasGameState
    {
        /// <summary>
        /// Light direction
        /// </summary>
        Vector3 Direction { get; set; }
        /// <summary>
        /// Base brightness
        /// </summary>
        float BaseBrightness { get; set; }
        /// <summary>
        /// Light brightness
        /// </summary>
        float Brightness { get; set; }
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        Matrix ToShadowSpace { get; set; }
        /// <summary>
        /// X cascade offset
        /// </summary>
        Vector4 ToCascadeOffsetX { get; set; }
        /// <summary>
        /// Y cascade offset
        /// </summary>
        Vector4 ToCascadeOffsetY { get; set; }
        /// <summary>
        /// Cascade scale
        /// </summary>
        Vector4 ToCascadeScale { get; set; }

        /// <summary>
        /// Updates internal state 
        /// </summary>
        /// <param name="size">Map size</param>
        /// <param name="cascades">Cascade distances</param>
        void UpdateEnvironment(int size, float[] cascades);
    }
}
