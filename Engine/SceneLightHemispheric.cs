using SharpDX;

namespace Engine
{
    /// <summary>
    /// Hemispheric light
    /// </summary>
    public class SceneLightHemispheric : SceneLight, ISceneLightHemispheric
    {
        private static readonly Color3 ambientDown = new Color3(0.8f, 0.8f, 0.8f);
        private static readonly Color3 ambientUp = new Color3(0.2f, 0.2f, 0.2f);

        /// <summary>
        /// Ambient down color
        /// </summary>
        public Color3 AmbientDown { get; set; } = ambientDown;
        /// <summary>
        /// Ambient up color
        /// </summary>
        public Color3 AmbientUp { get; set; } = ambientUp;

        /// <summary>
        /// Constructor
        /// </summary>
        protected SceneLightHemispheric()
            : base()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public SceneLightHemispheric(string name)
            : this(name, ambientDown, ambientUp, true)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="ambientDown">Ambient down color contribution</param>
        /// <param name="ambientUp">Ambient up color contribution</param>
        /// <param name="enabled">Lights is enabled</param>
        public SceneLightHemispheric(string name, Color3 ambientDown, Color3 ambientUp, bool enabled)
            : this(name, ambientDown, 1f, ambientUp, 1f, enabled)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="ambientDown">Ambient down color contribution</param>
        /// <param name="brightnessDown">Brightness down</param>
        /// <param name="ambientUp">Ambient up color contribution</param>
        /// <param name="brightnessUp">Brightness up</param>
        /// <param name="enabled">Lights is enabled</param>
        public SceneLightHemispheric(string name, Color3 ambientDown, float brightnessDown, Color3 ambientUp, float brightnessUp, bool enabled)
            : base(name, false, Color.Transparent, Color.Transparent, enabled)
        {
            AmbientDown = ambientDown * brightnessDown;
            AmbientUp = ambientUp * brightnessUp;
        }

        /// <inheritdoc/>
        public override bool MarkForShadowCasting(Vector3 eyePosition)
        {
            CastShadowsMarked = false;

            return CastShadowsMarked;
        }
        /// <inheritdoc/>
        public override ISceneLight Clone()
        {
            return new SceneLightHemispheric()
            {
                Name = Name,
                Enabled = Enabled,
                CastShadow = CastShadow,
                AmbientDown = AmbientDown,
                AmbientUp = AmbientUp,
                DiffuseColor = DiffuseColor,
                SpecularColor = SpecularColor,
                State = State,
            };
        }
    }
}
