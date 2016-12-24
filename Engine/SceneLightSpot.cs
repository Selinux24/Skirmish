using SharpDX;

namespace Engine
{
    /// <summary>
    /// Spot light
    /// </summary>
    public class SceneLightSpot : SceneLight
    {
        /// <summary>
        /// Position
        /// </summary>
        private Vector3 position = Vector3.Zero;
        /// <summary>
        /// Radius
        /// </summary>
        private float radius = 1f;
        /// <summary>
        /// Cone angle
        /// </summary>
        private float angle = 0f;

        /// <summary>
        /// Ligth position
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return this.position;
            }
            set
            {
                if (this.position != value)
                {
                    this.position = value;

                    this.Update();
                }
            }
        }
        /// <summary>
        /// Ligth direction
        /// </summary>
        public Vector3 Direction = Vector3.Zero;
        /// <summary>
        /// Cone angle in degrees
        /// </summary>
        public float Angle
        {
            get
            {
                return this.angle;
            }
            set
            {
                this.angle = value;
            }
        }
        /// <summary>
        /// Cone angle in radians
        /// </summary>
        public float AngleRadians
        {
            get
            {
                return MathUtil.DegreesToRadians(this.angle);
            }
            set
            {
                this.angle = MathUtil.RadiansToDegrees(value);
            }
        }
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius
        {
            get
            {
                return this.radius;
            }
            set
            {
                if (this.radius != value)
                {
                    this.radius = value;

                    this.Update();
                }
            }
        }
        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity = 1f;
        /// <summary>
        /// Gets the bounding sphere of the active light
        /// </summary>
        public BoundingSphere BoundingSphere { get; private set; }
        /// <summary>
        /// Transform matrix
        /// </summary>
        public Matrix Transform { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="castShadow">Light casts shadow</param>
        /// <param name="diffuse">Diffuse color contribution</param>
        /// <param name="specular">Specular color contribution</param>
        /// <param name="enabled">Lights is enabled</param>
        /// <param name="position">Position</param>
        /// <param name="direction">Direction</param>
        /// <param name="angle">Angle in degrees</param>
        /// <param name="radius">Radius</param>
        /// <param name="intensity">Intensity</param>
        public SceneLightSpot(
            string name, bool castShadow, Color4 diffuse, Color4 specular, bool enabled,
            Vector3 position, Vector3 direction, float angle, float radius, float intensity)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            this.position = position;
            this.Direction = direction;
            this.angle = angle;
            this.radius = radius;
            this.Intensity = intensity;

            this.Update();
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        private void Update()
        {
            this.BoundingSphere = new BoundingSphere(this.position, this.radius);
        }
    }
}
