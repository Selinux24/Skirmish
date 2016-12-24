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
                return new SceneLightDirectional(
                    "Key light", 
                    true,
                    new Color4(1, 0.9607844f, 0.8078432f, 1f),
                    new Color4(1, 0.9607844f, 0.8078432f, 1f),
                    true,
                    new Vector3(-0.5265408f, -0.5735765f, -0.6275069f));
            }
        }
        /// <summary>
        /// Secondary default light source
        /// </summary>
        public static SceneLightDirectional Secondary
        {
            get
            {
                return new SceneLightDirectional(
                    "Fill light", 
                    false,
                    new Color4(0.9647059f, 0.7607844f, 0.4078432f, 1f),
                    new Color4(0, 0, 0, 0),
                    true,
                    new Vector3(0.7198464f, 0.3420201f, 0.6040227f));
            }
        }
        /// <summary>
        /// Tertiary default light source
        /// </summary>
        public static SceneLightDirectional Tertiary
        {
            get
            {
                return new SceneLightDirectional(
                    "Back light", 
                    false,
                    new Color4(0.3231373f, 0.3607844f, 0.3937255f, 1f),
                    new Color4(0.3231373f, 0.3607844f, 0.3937255f, 1f),
                    true,
                    new Vector3(0.4545195f, -0.7660444f, 0.4545195f));
            }
        }

        /// <summary>
        /// Light direction
        /// </summary>
        public Vector3 Direction = Vector3.Zero;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="castShadow">Light casts shadow</param>
        /// <param name="diffuse">Diffuse color contribution</param>
        /// <param name="specular">Specular color contribution</param>
        /// <param name="enabled">Lights is enabled</param>
        /// <param name="direction">Direction</param>
        public SceneLightDirectional(string name, bool castShadow, Color4 diffuse, Color4 specular, bool enabled, Vector3 direction)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            this.Direction = direction;
        }

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
