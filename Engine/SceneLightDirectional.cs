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
                    LightColor = Color.White,
                    AmbientIntensity = 0.1f,
                    DiffuseIntensity = 0.5f,
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
                    LightColor = Color.Yellow,
                    AmbientIntensity = 0.02f,
                    DiffuseIntensity = 0.05f,
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
                    LightColor = Color.LightBlue,
                    AmbientIntensity = 0.01f,
                    DiffuseIntensity = 0.01f,
                    Direction = Vector3.Normalize(new Vector3(-0.57735f, -0.57735f, -0.57735f)),
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
