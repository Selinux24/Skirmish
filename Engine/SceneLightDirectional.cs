using SharpDX;

namespace Engine
{
    /// <summary>
    /// Luz direccional de distancia infinita
    /// </summary>
    public class SceneLightDirectional
    {
        /// <summary>
        /// Componente de brillo ambiental
        /// </summary>
        public Color4 Ambient = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        /// <summary>
        /// Componente de absorción
        /// </summary>
        public Color4 Diffuse = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        /// <summary>
        /// Componente de refracción
        /// </summary>
        public Color4 Specular = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        /// <summary>
        /// Vector dirección
        /// </summary>
        public Vector3 Direction = Vector3.Zero;
        /// <summary>
        /// Obtiene o establece si la luz está encendida
        /// </summary>
        public bool Enabled = false;
    }
}
