
namespace Engine
{
    using Engine.Mathematics;

    /// <summary>
    /// Hemispheric light
    /// </summary>
    public class SceneLightHemispheric : SceneLight, ISceneLightHemispheric
    {
        private static readonly Vector3 ambientDown = new(0.0f, 0.0f, 0.0f);
        private static readonly Vector3 ambientUp = new(0.1f, 0.1f, 0.1f);

        /// <inheritdoc/>
        public Vector3 AmbientDown { get; set; } = ambientDown;
        /// <inheritdoc/>
        public Vector3 AmbientUp { get; set; } = ambientUp;

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
        public SceneLightHemispheric(string name, Vector3 ambientDown, Vector3 ambientUp, bool enabled)
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
        public SceneLightHemispheric(string name, Vector3 ambientDown, float brightnessDown, Vector3 ambientUp, float brightnessUp, bool enabled)
            : base(name, false, Vector3.Zero, Vector3.Zero, enabled)
        {
            AmbientDown = ambientDown * brightnessDown;
            AmbientUp = ambientUp * brightnessUp;
        }

        /// <inheritdoc/>
        public override void ClearShadowParameters()
        {
            ShadowMapIndex = -1;
            ShadowMapCount = 0;
            FromLightVP = [];
        }
        /// <inheritdoc/>
        public override void SetShadowParameters(Camera camera, int assignedShadowMap)
        {
            ShadowMapIndex = assignedShadowMap;
            ShadowMapCount = 1;
            FromLightVP = [Matrix4x4.Identity];
        }
        /// <inheritdoc/>
        public override ICullingVolume GetLightVolume()
        {
            return null;
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

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new SceneLightHemisphericState
            {
                Name = Name,
                Enabled = Enabled,
                CastShadow = CastShadow,
                CastShadowsMarked = CastShadowsMarked,
                DiffuseColor = DiffuseColor,
                SpecularColor = SpecularColor,
                ShadowMapIndex = ShadowMapIndex,
                State = State,
                ParentTransform = ParentTransform,

                AmbientDown = AmbientDown,
                AmbientUp = AmbientUp,
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (state is not SceneLightHemisphericState sceneLightsState)
            {
                return;
            }

            Name = sceneLightsState.Name;
            Enabled = sceneLightsState.Enabled;
            CastShadow = sceneLightsState.CastShadow;
            CastShadowsMarked = sceneLightsState.CastShadowsMarked;
            DiffuseColor = sceneLightsState.DiffuseColor;
            SpecularColor = sceneLightsState.SpecularColor;
            ShadowMapIndex = sceneLightsState.ShadowMapIndex;
            State = sceneLightsState.State;
            ParentTransform = sceneLightsState.ParentTransform;

            AmbientDown = sceneLightsState.AmbientDown;
            AmbientUp = sceneLightsState.AmbientUp;
        }
    }
}
