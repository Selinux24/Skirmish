using SharpDX;

namespace Engine
{
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
        /// <param name="game">Game</param>
        public Manipulator2D(Game game)
        {
            this.game = game;
        }

        /// <summary>
        /// Updates the internal state
        /// </summary>
        public void Update()
        {
            Update(Vector2.Zero, 1f);
        }
        /// <summary>
        /// Updates the internal state
        /// </summary>
        /// <param name="parentPosition">Parent position</param>
        /// <param name="parentScale">Parent scale</param>
        public void Update(Vector2 parentPosition, float parentScale)
        {
            if (!updateInternals)
            {
                return;
            }

            parentPosition = game.Form.ToScreenSpace(parentPosition);
            var localPosition = game.Form.ToScreenSpace(Position);

            var parentP = new Vector3(parentPosition.X, parentPosition.Y, 0);
            var localP = new Vector3(localPosition.X, localPosition.Y, 0);

            //Local scale (in origin)
            var lsca = Matrix.Scaling(new Vector3(Scale.X, Scale.Y, 0));

            //Local rotation relative to parent position
            var lrot = Matrix.RotationZ(Rotation);
            var ltrn = Matrix.Translation((localP - parentP) * parentScale);

            //Translation to parent position
            var ptrn = Matrix.Translation(parentP);

            this.LocalTransform = lsca * ltrn * lrot * ptrn;

            updateInternals = false;

            Counters.UpdatesPerFrame++;
        }

        /// <summary>
        /// Increments position component d distance along left vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveLeft(GameTime gameTime, float d = 1f)
        {
            this.Position += Vector2.UnitX * -d * this.LinearVelocity * gameTime.ElapsedSeconds;

            updateInternals = true;
        }
        /// <summary>
        /// Increments position component d distance along right vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveRight(GameTime gameTime, float d = 1f)
        {
            this.Position += Vector2.UnitX * d * this.LinearVelocity * gameTime.ElapsedSeconds;

            updateInternals = true;
        }
        /// <summary>
        /// Increments position component d distance along up vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveUp(GameTime gameTime, float d = 1f)
        {
            this.Position += Vector2.UnitY * d * this.LinearVelocity * gameTime.ElapsedSeconds;

            updateInternals = true;
        }
        /// <summary>
        /// Increments position component d distance along down vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveDown(GameTime gameTime, float d = 1f)
        {
            this.Position += Vector2.UnitY * -d * this.LinearVelocity * gameTime.ElapsedSeconds;

            updateInternals = true;
        }

        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="x">X component of position</param>
        /// <param name="y">Y component of position</param>
        public void SetPosition(float x, float y)
        {
            this.Position = new Vector2(x, y);

            updateInternals = true;
        }
        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="position">Position component</param>
        public void SetPosition(Vector2 position)
        {
            this.Position = position;

            updateInternals = true;
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale component</param>
        public void SetScale(float scale)
        {
            this.Scale = new Vector2(scale, scale);

            updateInternals = true;
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="x">X component of scale</param>
        /// <param name="y">Y component of scale</param>
        public void SetScale(float x, float y)
        {
            this.Scale = new Vector2(x, y);

            updateInternals = true;
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale component</param>
        public void SetScale(Vector2 scale)
        {
            this.Scale = scale;

            updateInternals = true;
        }
        /// <summary>
        /// Stes rotation
        /// </summary>
        /// <param name="angle">Rotation angle in radians</param>
        public void SetRotation(float angle)
        {
            this.Rotation = angle;

            updateInternals = true;
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
