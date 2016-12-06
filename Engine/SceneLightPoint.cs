using SharpDX;

namespace Engine
{
    /// <summary>
    /// Point light
    /// </summary>
    public class SceneLightPoint : SceneLight
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
        /// Updates internal state
        /// </summary>
        private void Update()
        {
            this.BoundingSphere = new BoundingSphere(this.position, this.radius);
            this.Transform = Matrix.Scaling(this.radius) * Matrix.Translation(this.position);
        }
    }
}
