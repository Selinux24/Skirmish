using SharpDX;

namespace Engine
{
    /// <summary>
    /// Transform manipulator
    /// </summary>
    public class Manipulator
    {
        /// <summary>
        /// Final transform for the controller
        /// </summary>
        private Matrix localTransform = Matrix.Identity;
        /// <summary>
        /// Rotation component
        /// </summary>
        private Quaternion rotation = Quaternion.Identity;
        /// <summary>
        /// Scaling component
        /// </summary>
        private Vector3 scaling = Vector3.Zero;
        /// <summary>
        /// Position component
        /// </summary>
        private Vector3 position = Vector3.Zero;

        /// <summary>
        /// Gets Position component
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return this.position;
            }
        }
        /// <summary>
        /// Gets Forward vector
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                return this.localTransform.Forward;
            }
        }
        /// <summary>
        /// Gets Backward vector
        /// </summary>
        public Vector3 Backward
        {
            get
            {
                return this.localTransform.Backward;
            }
        }
        /// <summary>
        /// Gets Left vector
        /// </summary>
        public Vector3 Left
        {
            get
            {
                return this.localTransform.Left;
            }
        }
        /// <summary>
        /// Gets Right vector
        /// </summary>
        public Vector3 Right
        {
            get
            {
                return this.localTransform.Right;
            }
        }
        /// <summary>
        /// Gets Up vector
        /// </summary>
        public Vector3 Up
        {
            get
            {
                return this.localTransform.Up;
            }
        }
        /// <summary>
        /// Gets Down vector
        /// </summary>
        public Vector3 Down
        {
            get
            {
                return this.localTransform.Down;
            }
        }
        /// <summary>
        /// Gets Scaling component
        /// </summary>
        public Vector3 Scaling
        {
            get
            {
                return this.scaling;
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
        /// Gets or sets the controller thats this instance is following
        /// </summary>
        public Manipulator Following { get; set; }
        /// <summary>
        /// Gers or set the relative vector position to the followed controller
        /// </summary>
        public Matrix FollowingRelative { get; set; }

        /// <summary>
        /// Contructor
        /// </summary>
        public Manipulator()
        {
            this.position = Vector3.Zero;
            this.rotation = Quaternion.Identity;
            this.scaling = new Vector3(1);

            this.Following = null;
            this.FollowingRelative = Matrix.Identity;
        }
        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            if (this.Following != null)
            {
                this.localTransform =
                    Matrix.Scaling(this.scaling) *
                    Matrix.RotationQuaternion(this.Following.rotation) *
                    Matrix.Translation(this.Following.position) *
                    this.FollowingRelative;

                this.localTransform.Decompose(out this.scaling, out this.rotation, out this.position);
            }
            else
            {
                this.localTransform =
                    Matrix.Scaling(this.scaling) *
                    Matrix.RotationQuaternion(this.rotation) *
                    Matrix.Translation(this.position);
            }
        }

        /// <summary>
        /// Increments position component d length along d vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void Move(Vector3 d)
        {
            this.position += d;
        }
        /// <summary>
        /// Increments position component d distance along forward vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveForward(float d)
        {
            this.position += this.Forward * -d;
        }
        /// <summary>
        /// Increments position component d distance along backward vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveBackward(float d)
        {
            this.position += this.Backward * -d;
        }
        /// <summary>
        /// Increments position component d distance along left vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveLeft(float d)
        {
            this.position += this.Left * d;
        }
        /// <summary>
        /// Increments position component d distance along right vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveRight(float d)
        {
            this.position += this.Right * d;
        }
        /// <summary>
        /// Increments position component d distance along up vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveUp(float d)
        {
            this.position += this.Up * d;
        }
        /// <summary>
        /// Increments position component d distance along down vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveDown(float d)
        {
            this.position += this.Down * d;
        }
        /// <summary>
        /// Increments rotation component by axis
        /// </summary>
        /// <param name="yaw">Yaw (Y) amount (radians)</param>
        /// <param name="pitch">Pitch (X) amount (radians)</param>
        /// <param name="roll">Roll (Z) amount (radians)</param>
        public void Rotate(float yaw, float pitch, float roll)
        {
            this.rotation *= Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
        }
        /// <summary>
        /// Increments rotation yaw (Y) to the left
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void YawLeft(float a)
        {
            this.Rotate(-a, 0, 0);
        }
        /// <summary>
        /// Increments rotation yaw (Y) to the right
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void YawRight(float a)
        {
            this.Rotate(a, 0, 0);
        }
        /// <summary>
        /// Increments rotation pitch (X) up
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void PitchUp(float a)
        {
            this.Rotate(0, -a, 0);
        }
        /// <summary>
        /// Increments rotation pitch (X) down
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void PitchDown(float a)
        {
            this.Rotate(0, a, 0);
        }
        /// <summary>
        /// Increments rotation roll (Z) left
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void RollLeft(float a)
        {
            this.Rotate(0, 0, a);
        }
        /// <summary>
        /// Increments rotation roll (Z) right
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void RollRight(float a)
        {
            this.Rotate(0, 0, -a);
        }
        /// <summary>
        /// Clamped scale increment
        /// </summary>
        /// <param name="scale">Scale amount (percent 0 to x)</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        public void Scale(float scale, Vector3? minSize = null, Vector3? maxSize = null)
        {
            this.Scale(new Vector3(scale), minSize, maxSize);
        }
        /// <summary>
        /// Clamped scale increment
        /// </summary>
        /// <param name="scaleX">X axis scale amount (percent 0 to x)</param>
        /// <param name="scaleY">Y axis scale amount (percent 0 to x)</param>
        /// <param name="scaleZ">Z axis scale amount (percent 0 to x)</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        public void Scale(float scaleX, float scaleY, float scaleZ, Vector3? minSize = null, Vector3? maxSize = null)
        {
            this.Scale(new Vector3(scaleX, scaleY, scaleZ), minSize, maxSize);
        }
        /// <summary>
        /// Clamped scale increment
        /// </summary>
        /// <param name="scale">Scaling component</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        public void Scale(Vector3 scale, Vector3? minSize = null, Vector3? maxSize = null)
        {
            Vector3 newScaling = this.scaling + scale;

            if (maxSize.HasValue)
            {
                if (newScaling.LengthSquared() > maxSize.Value.LengthSquared()) newScaling = maxSize.Value;
            }

            if (minSize.HasValue)
            {
                if (newScaling.LengthSquared() < minSize.Value.LengthSquared()) newScaling = minSize.Value;
            }

            this.SetScale(newScaling);
        }

        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="x">X component of position</param>
        /// <param name="y">Y component of position</param>
        /// <param name="z">Z component of position</param>
        public void SetPosition(float x, float y, float z)
        {
            this.SetPosition(new Vector3(x, y, z));
        }
        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="position">Position component</param>
        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }
        /// <summary>
        /// Sets rotation
        /// </summary>
        /// <param name="yaw">Yaw (Y)</param>
        /// <param name="pitch">Pitch (X)</param>
        /// <param name="roll">Roll (Z)</param>
        public void SetRotation(float yaw, float pitch, float roll)
        {
            this.SetRotation(Quaternion.RotationYawPitchRoll(yaw, pitch, roll));
        }
        /// <summary>
        /// Sets rotation
        /// </summary>
        /// <param name="rotation">Rotation component</param>
        public void SetRotation(Quaternion rotation)
        {
            this.rotation = rotation;
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale amount (0 to x)</param>
        public void SetScale(float scale)
        {
            this.SetScale(new Vector3(scale));
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scaleX">Scale along X axis</param>
        /// <param name="scaleY">Scale along Y axis</param>
        /// <param name="scaleZ">Scale along Z axis</param>
        public void SetScale(float scaleX, float scaleY, float scaleZ)
        {
            this.SetScale(new Vector3(scaleX, scaleY, scaleZ));
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale vector</param>
        public void SetScale(Vector3 scale)
        {
            this.scaling = scale;
        }
    }
}
