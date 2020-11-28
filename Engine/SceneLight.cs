using SharpDX;

namespace Engine
{
    /// <summary>
    /// Scene ligth
    /// </summary>
    public abstract class SceneLight : ISceneLight
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
        /// Gets or sets whether the light casts shadow
        /// </summary>
        public bool CastShadow { get; set; }
        /// <summary>
        /// Gets or sets whether the light is marked for shadow cast the next call
        /// </summary>
        public bool CastShadowsMarked { get; set; } = false;
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color3 DiffuseColor { get; set; }
        /// <summary>
        /// Specular color
        /// </summary>
        public Color3 SpecularColor { get; set; }
        /// <summary>
        /// Free use variable
        /// </summary>
        public virtual object State { get; set; }
        /// <summary>
        /// Parent local transform matrix
        /// </summary>
        /// <summary>
        /// Parent local transform matrix
        /// </summary>
        public virtual Matrix ParentTransform { get; set; } = Matrix.Identity;
        /// <summary>
        /// First shadow map index
        /// </summary>
        public int ShadowMapIndex { get; set; } = 0;

        /// <summary>
        /// Evaluates whether a light must be processed as a shadow casting light or not
        /// </summary>
        /// <param name="environment">Game environment</param>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="castShadow">Cast shadows</param>
        /// <param name="position">Light position</param>
        /// <param name="radius">Light radius</param>
        /// <returns>Returns true if the light cast shadow</returns>
        public static bool EvaluateLight(GameEnvironment environment, Vector3 eyePosition, bool castShadow, Vector3 position, float radius)
        {
            if (!castShadow)
            {
                // Discard no shadow casting lights
                return false;
            }

            float dist = Vector3.Distance(position, eyePosition);
            if (dist >= environment.LODDistanceMedium)
            {
                // Discard too far lights
                return false;
            }

            float thr = radius / (dist <= 0 ? 1 : dist);
            if (thr < environment.ShadowRadiusDistanceThreshold)
            {
                // Discard too small lights based on radius and distance
                return false;
            }

            return true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected SceneLight()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="castShadow">Light casts shadow</param>
        /// <param name="diffuse">Diffuse color contribution</param>
        /// <param name="specular">Specular color contribution</param>
        /// <param name="enabled">Lights is enabled</param>
        protected SceneLight(string name, bool castShadow, Color3 diffuse, Color3 specular, bool enabled)
        {
            Name = name;
            Enabled = enabled;
            CastShadow = castShadow;
            DiffuseColor = diffuse;
            SpecularColor = specular;
        }

        /// <summary>
        /// Clears all light shadow parameters
        /// </summary>
        public virtual void ClearShadowParameters()
        {
            ShadowMapIndex = -1;
        }
        /// <summary>
        /// Test the light shadow casting based on the viewer position
        /// </summary>
        /// <param name="environment">Game environment</param>
        /// <param name="eyePosition">Viewer eye position</param>
        /// <returns>Returns true if the light can cast shadows</returns>
        /// <remarks>This method updates the light internal cast shadow flag</remarks>
        public abstract bool MarkForShadowCasting(GameEnvironment environment, Vector3 eyePosition);

        /// <summary>
        /// Clones current light
        /// </summary>
        /// <returns>Returns a new instante with same data</returns>
        public abstract ISceneLight Clone();

        /// <summary>
        /// Gets the text representation of the light
        /// </summary>
        /// <returns>Returns the text representation of the light</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return $"{Name}; Enabled: {Enabled}";
            }
            else
            {
                return $"{GetType()}; Enabled: {Enabled}";
            }
        }
    }
}
