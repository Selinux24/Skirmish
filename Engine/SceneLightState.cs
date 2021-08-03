
namespace Engine
{
    /// <summary>
    /// Scene light base state
    /// </summary>
    public abstract class SceneLightState : IGameState
    {
        /// <summary>
        /// Light name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Enables or disables the light
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Casts shadows
        /// </summary>
        public bool CastShadow { get; set; }
        /// <summary>
        /// The light is marked for shadow cast the next call
        /// </summary>
        public bool CastShadowsMarked { get; set; }
        /// <summary>
        /// Diffuse color
        /// </summary>
        public ColorRgba DiffuseColor { get; set; }
        /// <summary>
        /// Specular color
        /// </summary>
        public ColorRgba SpecularColor { get; set; }
        /// <summary>
        /// Shadow map index
        /// </summary>
        public int ShadowMapIndex { get; set; }
        /// <summary>
        /// Free use variable
        /// </summary>
        public object State { get; set; }
        /// <summary>
        /// Parent local transform matrix
        /// </summary>
        /// <summary>
        /// Parent local transform matrix
        /// </summary>
        public Matrix4x4 ParentTransform { get; set; }
    }
}
