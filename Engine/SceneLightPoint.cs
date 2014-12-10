using SharpDX;

namespace Engine
{
    /// <summary>
    /// Punto de luz de distancia finita
    /// </summary>
    public class SceneLightPoint
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
        /// Posición de la luz
        /// </summary>
        public Vector3 Position = Vector3.Zero;
        /// <summary>
        /// Radio de luminosidad
        /// </summary>
        public float Range = 0.0f;
        /// <summary>
        /// Atributos
        /// </summary>
        public Vector3 Attributes = Vector3.Zero;
        /// <summary>
        /// Obtiene o establece si la luz está encendida
        /// </summary>
        public bool Enabled = false;
    }
}
