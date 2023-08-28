using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// 2D manipulator
    /// </summary>
    public class Manipulator2D
    {
        /// <summary>
        /// Game instance
        /// </summary>
        private readonly Game game;
        /// <summary>
        /// Update internals flag
        /// </summary>
        private bool updateInternals = true;

        /// <summary>
        /// Gets the size
        /// </summary>
        public Vector2 Size { get; private set; } = Vector2.One;
        /// <summary>
        /// Gets Position component
        /// </summary>
        public Vector2 Position { get; private set; } = Vector2.Zero;
        /// <summary>
        /// Gets Scale component
        /// </summary>
        public Vector2 Scale { get; private set; } = Vector2.One;
        /// <summary>
        /// Gets Rotation angle
        /// </summary>
        public float Rotation { get; private set; } = 0f;
        /// <summary>
        /// Gets the size transform
        /// </summary>
        /// <remarks>Scaling matrix based on the size</remarks>
        public Matrix SizeTransform { get; private set; } = Matrix.Identity;
        /// <summary>
        /// Gets the sprite transform
        /// </summary>
        /// <remarks>Transform matrix without the size transform</remarks>
        public Matrix SpriteTransform { get; private set; } = Matrix.Identity;
        /// <summary>
        /// Gets final transform of controller
        /// </summary>
        /// <remarks>Size transforms * sprite transform</remarks>
        public Matrix LocalTransform { get; private set; } = Matrix.Identity;
        /// <summary>
        /// Linear velocity modifier
        /// </summary>
        public float LinearVelocity { get; set; } = 1f;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public Manipulator2D(Game game)
        {
            this.game = game;
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        /// <param name="parentPosition">Parent position</param>
        public void Update2D(Vector2? parentPosition)
        {
            if (!updateInternals)
            {
                return;
            }

            var localPosition = Position + (Size * 0.5f);
            var localScrPosition = game.Form.ToScreenSpace(localPosition);

            Vector2 pivotPoint = Vector2.Zero;
            if (parentPosition.HasValue)
            {
                var parentScrPosition = game.Form.ToScreenSpace(parentPosition.Value);
                pivotPoint = parentScrPosition - localScrPosition;
            }

            SizeTransform = Matrix.Scaling(new Vector3(Size, 0));
            SpriteTransform = Matrix.Transformation2D(pivotPoint, 0, Scale, pivotPoint, Rotation, localScrPosition);
            LocalTransform = SizeTransform * SpriteTransform;

            updateInternals = false;

            FrameCounters.PickCounters.TransformUpdatesPerFrame++;
        }

        /// <summary>
        /// Increments position component d distance along left vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveLeft(GameTime gameTime, float d = 1f)
        {
            Position += Vector2.UnitX * -d * LinearVelocity * gameTime.ElapsedSeconds;

            updateInternals = true;
        }
        /// <summary>
        /// Increments position component d distance along right vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveRight(GameTime gameTime, float d = 1f)
        {
            Position += Vector2.UnitX * d * LinearVelocity * gameTime.ElapsedSeconds;

            updateInternals = true;
        }
        /// <summary>
        /// Increments position component d distance along up vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveUp(GameTime gameTime, float d = 1f)
        {
            Position += Vector2.UnitY * d * LinearVelocity * gameTime.ElapsedSeconds;

            updateInternals = true;
        }
        /// <summary>
        /// Increments position component d distance along down vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveDown(GameTime gameTime, float d = 1f)
        {
            Position += Vector2.UnitY * -d * LinearVelocity * gameTime.ElapsedSeconds;

            updateInternals = true;
        }

        /// <summary>
        /// Sets the size
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void SetSize(float width, float height)
        {
            Size = new Vector2(width, height);

            updateInternals = true;
        }
        /// <summary>
        /// Sets the size
        /// </summary>
        /// <param name="size">Size</param>
        public void SetSize(Vector2 size)
        {
            Size = size;

            updateInternals = true;
        }
        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="x">X component of position</param>
        /// <param name="y">Y component of position</param>
        public void SetPosition(float x, float y)
        {
            Position = new Vector2(x, y);

            updateInternals = true;
        }
        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="position">Position component</param>
        public void SetPosition(Vector2 position)
        {
            Position = position;

            updateInternals = true;
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale component</param>
        public void SetScale(float scale)
        {
            Scale = new Vector2(scale, scale);

            updateInternals = true;
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="x">X component of scale</param>
        /// <param name="y">Y component of scale</param>
        public void SetScale(float x, float y)
        {
            Scale = new Vector2(x, y);

            updateInternals = true;
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale component</param>
        public void SetScale(Vector2 scale)
        {
            Scale = scale;

            updateInternals = true;
        }
        /// <summary>
        /// Sets rotation
        /// </summary>
        /// <param name="angle">Rotation angle in radians</param>
        public void SetRotation(float angle)
        {
            Rotation = angle;

            updateInternals = true;
        }

        /// <summary>
        /// Gets manipulator text representation
        /// </summary>
        /// <returns>Returns manipulator text description</returns>
        public override string ToString()
        {
            return $"Size: {SizeTransform.GetDescription()}; Sprite: {SpriteTransform.GetDescription()}";
        }
    }
}
