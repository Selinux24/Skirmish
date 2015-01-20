using SharpDX;

namespace Engine
{
    /// <summary>
    /// 2D manipulator
    /// </summary>
    public class Manipulator2D
    {
        /// <summary>
        /// Final transform for the controller
        /// </summary>
        private Matrix localTransform = Matrix.Identity;
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
        /// Gets final transform of controller
        /// </summary>
        public Matrix LocalTransform
        {
            get
            {
                return this.localTransform;
            }
        }
        /// <summary>
        /// Linear velocity modifier
        /// </summary>
        public float LinearVelocity = 1f;

        /// <summary>
        /// Contructor
        /// </summary>
        public Manipulator2D()
        {
            this.position = Vector2.Zero;
        }
        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="relativeCenter">Relative window center</param>
        /// <param name="width">Object width</param>
        /// <param name="height">Object height</param>
        public void Update(GameTime gameTime, Point relativeCenter, float width, float height)
        {
            this.localTransform =
                Matrix.Scaling(width, height, 1f) *
                Matrix.Translation(-relativeCenter.X, +relativeCenter.Y, 0f) *
                Matrix.Translation(this.position.X, this.position.Y, 0f);
        }

        /// <summary>
        /// Increments position component d length along d vector
        /// </summary>
        /// <param name="d">Distance</param>
        private void Move(Vector2 d)
        {
            this.position += d;
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
    }
}
