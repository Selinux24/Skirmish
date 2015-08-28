using SharpDX;

namespace Engine
{
    /// <summary>
    /// Directional light
    /// </summary>
    public class SceneLightDirectional : SceneLight
    {
        /// <summary>
        /// Primary default light source
        /// </summary>
        public static SceneLightDirectional Primary
        {
            get
            {
                return new SceneLightDirectional()
                {
                    Name = "Primary",
                    Ambient = new Color4(0.8f, 0.8f, 0.8f, 1.0f),
                    Diffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f),
                    Specular = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    Direction = Vector3.Normalize(new Vector3(0.57735f, -0.57735f, 0.57735f)),
                    Enabled = true,
                };
            }
        }
        /// <summary>
        /// Secondary default light source
        /// </summary>
        public static SceneLightDirectional Secondary
        {
            get
            {
                return new SceneLightDirectional()
                {
                    Name = "Secondary",
                    Ambient = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    Specular = new Color4(0.25f, 0.25f, 0.25f, 1.0f),
                    Direction = Vector3.Normalize(new Vector3(-0.57735f, -0.57735f, 0.57735f)),
                    Enabled = true,
                };
            }
        }
        /// <summary>
        /// Tertiary default light source
        /// </summary>
        public static SceneLightDirectional Tertiary
        {
            get
            {
                return new SceneLightDirectional()
                {
                    Name = "Tertiary",
                    Ambient = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                    Specular = new Color4(0.0f, 0.0f, 0.0f, 1.0f),
                    Direction = Vector3.Normalize(new Vector3(0.0f, -0.707f, -0.707f)),
                    Enabled = true,
                };
            }
        }

        /// <summary>
        /// Light direction
        /// </summary>
        public Vector3 Direction = Vector3.Zero;
    }
}
