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
        /// Spot exponent used in the spotlight calculation to control the cone
        /// </summary>
        public float Angle = 0.0f;
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
        public BoundingFrustum BoundingFrustum { get; private set; }
        /// <summary>
        /// Transform matrix
        /// </summary>
        public Matrix Transform { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="direction">Direction</param>
        /// <param name="angle">Angle</param>
        /// <param name="radius">Radius</param>
        public SceneLightSpot(Vector3 position, Vector3 direction, float angle, float radius)
        {
            this.position = position;
            this.Direction = direction;
            this.Angle = angle;
            this.radius = radius;

            this.Update();
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        private void Update()
        {
            //TODO: apply direction to spot light transform
            this.Transform = Matrix.Scaling(this.radius) * Matrix.Translation(this.position);

            Vector3 direction = Vector3.Normalize(this.Transform.Down);
            Vector3 up = Vector3.Normalize(direction == Vector3.Down ? this.Transform.Left : this.Transform.Up);

            this.BoundingFrustum = BoundingFrustum.FromCamera(
                this.position,
                direction,
                up,
                MathUtil.DegreesToRadians(this.Angle),
                0.1f, 100f, 1f);
        }
    }
}
