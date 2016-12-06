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
                    DiffuseColor = Color.White,
                    SpecularColor = Color.CornflowerBlue,
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
                    DiffuseColor = Color.CornflowerBlue,
                    SpecularColor = Color.CornflowerBlue,
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
                    DiffuseColor = Color.Yellow,
                    SpecularColor = Color.Yellow,
                    Direction = Vector3.Normalize(new Vector3(-0.57735f, -0.57735f, -0.57735f)),
                    Enabled = true,
                };
            }
        }

        /// <summary>
        /// Light direction
        /// </summary>
        public Vector3 Direction = Vector3.Zero;

        /// <summary>
        /// Gets light position at specified distance
        /// </summary>
        /// <param name="distance">Distance</param>
        /// <returns>Returns light position at specified distance</returns>
        public Vector3 GetPosition(float distance)
        {
            return distance * -2f * this.Direction;
        }
    }
}
