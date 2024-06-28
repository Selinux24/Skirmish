using Engine.Content;

namespace Engine
{
    /// <summary>
    /// Scene directional light state
    /// </summary>
    public class SceneLightDirectionalState : SceneLightState
    {
        /// <summary>
        /// Initial light direction
        /// </summary>
        public Direction3 InitialDirection { get; set; }
        /// <summary>
        /// Light direction
        /// </summary>
        public Direction3 Direction { get; set; }
        /// <summary>
        /// Base brightness
        /// </summary>
        public float BaseBrightness { get; set; }
        /// <summary>
        /// Light brightness
        /// </summary>
        public float Brightness { get; set; }
        /// <summary>
        /// Shadow map index
        /// </summary>
        public uint ShadowMapCount { get; set; }
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        public Matrix4X4 ToShadowSpace { get; set; }
        /// <summary>
        /// X cascade offset
        /// </summary>
        public Position4 ToCascadeOffsetX { get; set; }
        /// <summary>
        /// Y cascade offset
        /// </summary>
        public Position4 ToCascadeOffsetY { get; set; }
        /// <summary>
        /// Cascasde scale
        /// </summary>
        public Position4 ToCascadeScale { get; set; }
    }
}
