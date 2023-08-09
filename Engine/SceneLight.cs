using SharpDX;

namespace Engine
{
    /// <summary>
    /// Scene ligth
    /// </summary>
    public abstract class SceneLight : ISceneLight
    {
        /// <inheritdoc/>
        public string Name { get; set; }
        /// <inheritdoc/>
        public bool Enabled { get; set; }
        /// <inheritdoc/>
        public bool CastShadow { get; set; }
        /// <inheritdoc/>
        public bool CastShadowsMarked { get; set; } = false;
        /// <inheritdoc/>
        public Color3 DiffuseColor { get; set; }
        /// <inheritdoc/>
        public Color3 SpecularColor { get; set; }
        /// <inheritdoc/>
        public int ShadowMapIndex { get; set; } = 0;
        /// <inheritdoc/>
        public uint ShadowMapCount { get; set; }
        /// <inheritdoc/>
        public Matrix[] FromLightVP { get; set; }
        /// <inheritdoc/>
        public Vector3 Position { get; set; }
        /// <inheritdoc/>
        public virtual object State { get; set; }
        /// <inheritdoc/>
        public virtual Matrix ParentTransform { get; set; } = Matrix.Identity;

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

        /// <inheritdoc/>
        public abstract bool MarkForShadowCasting(GameEnvironment environment, Vector3 eyePosition);
        /// <inheritdoc/>
        public abstract void ClearShadowParameters();
        /// <inheritdoc/>
        public abstract void SetShadowParameters(Camera camera, int assignedShadowMap);

        /// <inheritdoc/>
        public abstract ISceneLight Clone();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name ?? GetType().ToString()}; Enabled: {Enabled}";
        }
    }
}
