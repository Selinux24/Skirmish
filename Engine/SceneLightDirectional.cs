using SharpDX;

namespace Engine
{
    /// <summary>
    /// Directional light
    /// </summary>
    public class SceneLightDirectional : SceneLight, ISceneLightDirectional
    {
        /// <summary>
        /// Primary default light source
        /// </summary>
        public static SceneLightDirectional KeyLight
        {
            get
            {
                return new SceneLightDirectional(
                    "Key light",
                    true,
                    new Color4(1, 0.9607844f, 0.8078432f, 1f),
                    new Color4(1, 0.9607844f, 0.8078432f, 1f) * 0.5f,
                    true,
                    new Vector3(-0.5265408f, -0.5735765f, -0.6275069f),
                    1f);
            }
        }
        /// <summary>
        /// Secondary default light source
        /// </summary>
        public static SceneLightDirectional FillLight
        {
            get
            {
                return new SceneLightDirectional(
                    "Fill light",
                    false,
                    new Color4(0.9647059f, 0.7607844f, 0.4078432f, 1f),
                    new Color4(0, 0, 0, 0),
                    true,
                    new Vector3(0.7198464f, 0.3420201f, 0.6040227f),
                    1f);
            }
        }
        /// <summary>
        /// Tertiary default light source
        /// </summary>
        public static SceneLightDirectional BackLight
        {
            get
            {
                return new SceneLightDirectional(
                    "Back light",
                    false,
                    new Color4(0.3231373f, 0.3607844f, 0.3937255f, 1f),
                    new Color4(0.3231373f, 0.3607844f, 0.3937255f, 1f) * 0.25f,
                    true,
                    new Vector3(0.4545195f, -0.7660444f, 0.4545195f),
                    1f);
            }
        }

        /// <summary>
        /// Initial light direction
        /// </summary>
        private Vector3 initialDirection = Vector3.ForwardLH;

        /// <summary>
        /// Light direction
        /// </summary>
        public Vector3 Direction { get; set; }
        /// <summary>
        /// Base brightness
        /// </summary>
        public float BaseBrightness { get; set; } = 1f;
        /// <summary>
        /// Light brightness
        /// </summary>
        public float Brightness { get; set; } = 1f;
        /// <summary>
        /// Parent local transform matrix
        /// </summary>
        public override Matrix ParentTransform
        {
            get
            {
                return base.ParentTransform;
            }
            set
            {
                base.ParentTransform = value;

                UpdateLocalTransform();
            }
        }
        /// <summary>
        /// Shadow map count
        /// </summary>
        public uint ShadowMapCount { get; set; } = 0;
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        public Matrix ToShadowSpace { get; set; }
        /// <summary>
        /// X cascade offset
        /// </summary>
        public Vector4 ToCascadeOffsetX { get; set; }
        /// <summary>
        /// Y cascade offset
        /// </summary>
        public Vector4 ToCascadeOffsetY { get; set; }
        /// <summary>
        /// Cascasde scale
        /// </summary>
        public Vector4 ToCascadeScale { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected SceneLightDirectional()
            : base()
        {
            UpdateLocalTransform();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="castShadow">Light casts shadow</param>
        /// <param name="diffuse">Diffuse color contribution</param>
        /// <param name="specular">Specular color contribution</param>
        /// <param name="enabled">Lights is enabled</param>
        /// <param name="direction">Direction</param>
        /// <param name="brigthness">Brightness</param>
        public SceneLightDirectional(string name, bool castShadow, Color4 diffuse, Color4 specular, bool enabled, Vector3 direction, float brigthness)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            initialDirection = direction;
            BaseBrightness = Brightness = brigthness;

            UpdateLocalTransform();
        }

        /// <summary>
        /// Updates local transform
        /// </summary>
        private void UpdateLocalTransform()
        {
            Direction = Vector3.TransformNormal(initialDirection, ParentTransform);
        }

        /// <inheritdoc/>
        public override void ClearShadowParameters()
        {
            base.ClearShadowParameters();

            ShadowMapCount = 0;
            ToShadowSpace = Matrix.Identity;
            ToCascadeOffsetX = Vector4.Zero;
            ToCascadeOffsetY = Vector4.Zero;
            ToCascadeScale = Vector4.Zero;
        }
        /// <inheritdoc/>
        public override bool MarkForShadowCasting(Vector3 eyePosition)
        {
            CastShadowsMarked = CastShadow;

            return CastShadowsMarked;
        }
        /// <inheritdoc/>
        public override ISceneLight Clone()
        {
            return new SceneLightDirectional()
            {
                Name = Name,
                Enabled = Enabled,
                CastShadow = CastShadow,
                DiffuseColor = DiffuseColor,
                SpecularColor = SpecularColor,
                State = State,

                Direction = Direction,
                Brightness = Brightness,

                initialDirection = initialDirection,

                ParentTransform = ParentTransform,
            };
        }

        /// <summary>
        /// Gets light position at specified distance
        /// </summary>
        /// <param name="distance">Distance</param>
        /// <returns>Returns light position at specified distance</returns>
        public Vector3 GetPosition(float distance)
        {
            return distance * -2f * Direction;
        }
    }
}
