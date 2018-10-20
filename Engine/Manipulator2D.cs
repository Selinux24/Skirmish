using SharpDX;

namespace Engine
{
    /// <summary>
    /// 2D manipulator
    /// </summary>
    public class Manipulator2D
    {
        /// <summary>
        /// Position component
        /// </summary>
        private Vector2 position = Vector2.Zero;

        /// <summary>
        /// Gets Position component
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(this.position.X, -this.position.Y);
            }
        }
        /// <summary>
        /// Gets Scale component
        /// </summary>
        public Vector2 Scale { get; private set; } = Vector2.One;
        /// <summary>
        /// Gets final transform of controller
        /// </summary>
        public Matrix LocalTransform { get; private set; } = Matrix.Identity;
        /// <summary>
        /// Linear velocity modifier
        /// </summary>
        public float LinearVelocity { get; set; } = 1f;

        /// <summary>
        /// Contructor
        /// </summary>
        public Manipulator2D()
        {

        }
        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="relativeCenter">Relative window center</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void Update(GameTime gameTime, Point relativeCenter, float width, float height)
        {
            this.LocalTransform =
                Matrix.Scaling(this.Scale.X * width, this.Scale.Y * height, 1f) *
                Matrix.Translation(-relativeCenter.X, +relativeCenter.Y, 0f) *
                Matrix.Translation(this.position.X, this.position.Y, 0f);

            Counters.UpdatesPerFrame++;
        }

        /// <summary>
        /// Increments position component d distance along left vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveLeft(GameTime gameTime, float d = 1f)
        {
            this.position += Vector2.UnitX * -d * this.LinearVelocity * gameTime.ElapsedSeconds;
        }
        /// <summary>
        /// Increments position component d distance along right vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveRight(GameTime gameTime, float d = 1f)
        {
            this.position += Vector2.UnitX * d * this.LinearVelocity * gameTime.ElapsedSeconds;
        }
        /// <summary>
        /// Increments position component d distance along up vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveUp(GameTime gameTime, float d = 1f)
        {
            this.position += Vector2.UnitY * d * this.LinearVelocity * gameTime.ElapsedSeconds;
        }
        /// <summary>
        /// Increments position component d distance along down vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveDown(GameTime gameTime, float d = 1f)
        {
            this.position += Vector2.UnitY * -d * this.LinearVelocity * gameTime.ElapsedSeconds;
        }

        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="x">X component of position</param>
        /// <param name="y">Y component of position</param>
        public void SetPosition(float x, float y)
        {
            this.position = new Vector2(x, -y);
        }
        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="position">Position component</param>
        public void SetPosition(Vector2 position)
        {
            this.position = new Vector2(position.X, -position.Y);
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale component</param>
        public void SetScale(float scale)
        {
            this.Scale = new Vector2(scale, scale);
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="x">X component of scale</param>
        /// <param name="y">Y component of scale</param>
        public void SetScale(float x, float y)
        {
            this.Scale = new Vector2(x, y);
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale component</param>
        public void SetScale(Vector2 scale)
        {
            this.Scale = scale;
        }

        /// <summary>
        /// Gets manipulator text representation
        /// </summary>
        /// <returns>Returns manipulator text description</returns>
        public override string ToString()
        {
            return string.Format("{0}", this.LocalTransform.GetDescription());
        }
    }
}
