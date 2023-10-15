using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// 3D manipulator
    /// </summary>
    public class Manipulator3D : IManipulator3D, IHasGameState
    {
        /// <summary>
        /// State updated event
        /// </summary>
        public event EventHandler Updated;

        /// <summary>
        /// Transform update needed flag
        /// </summary>
        protected bool transformUpdateNeeded = true;
        /// <summary>
        /// Final transform for the controller
        /// </summary>
        protected Matrix localTransform = Matrix.Identity;
        /// <summary>
        /// Rotation component
        /// </summary>
        protected Quaternion rotation = Quaternion.Identity;
        /// <summary>
        /// Scaling component
        /// </summary>
        protected Vector3 scaling = Vector3.One;
        /// <summary>
        /// Position component
        /// </summary>
        protected Vector3 position = Vector3.Zero;

        /// <inheritdoc/>
        public Vector3 Forward { get; private set; }
        /// <inheritdoc/>
        public Vector3 Backward { get; private set; }
        /// <inheritdoc/>
        public Vector3 Left { get; private set; }
        /// <inheritdoc/>
        public Vector3 Right { get; private set; }
        /// <inheritdoc/>
        public Vector3 Up { get; private set; }
        /// <inheritdoc/>
        public Vector3 Down { get; private set; }
        /// <inheritdoc/>
        public Vector3 Position
        {
            get
            {
                return position;
            }
        }
        /// <inheritdoc/>
        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
        }
        /// <inheritdoc/>
        public Vector3 Scaling
        {
            get
            {
                return scaling;
            }
        }
        /// <inheritdoc/>
        public Vector3 Velocity { get; private set; }
        /// <inheritdoc/>
        public Matrix LocalTransform
        {
            get
            {
                if (transformUpdateNeeded)
                {
                    UpdateLocalTransform();
                }

                return localTransform;
            }
        }
        /// <inheritdoc/>
        public Matrix GlobalTransform
        {
            get
            {
                if (transformUpdateNeeded)
                {
                    UpdateLocalTransform();
                }

                if (Parent != null)
                {
                    return localTransform * Parent.GlobalTransform;
                }

                return localTransform;
            }
        }

        /// <summary>
        /// Parent manipulator
        /// </summary>
        public Manipulator3D Parent { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Manipulator3D()
        {
            UpdateLocalTransform();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Parent manipulator</param>
        public Manipulator3D(Manipulator3D parent) : this()
        {
            Parent = parent;
        }

        /// <inheritdoc/>
        public virtual void Update(GameTime gameTime)
        {
            UpdateLocalTransform();
        }
        /// <inheritdoc/>
        public virtual void UpdateInternals(bool force)
        {
            if (force) transformUpdateNeeded = true;

            UpdateLocalTransform();
        }
        /// <summary>
        /// Update internal state
        /// </summary>
        private void UpdateLocalTransform()
        {
            Vector3 prePos = localTransform.TranslationVector;
            Velocity = position - prePos;

            if (!transformUpdateNeeded)
            {
                return;
            }

            Matrix sca = Matrix.Scaling(scaling);
            Matrix rot = Matrix.RotationQuaternion(rotation);
            Matrix tra = Matrix.Translation(position);

            localTransform = sca * rot * tra;

            Forward = rot.Forward;
            Backward = rot.Backward;
            Left = rot.Left;
            Right = rot.Right;
            Up = rot.Up;
            Down = rot.Down;

            transformUpdateNeeded = false;

            Updated?.Invoke(this, new EventArgs());

            FrameCounters.PickCounters.TransformUpdatesPerFrame++;
        }

        /// <inheritdoc/>
        public void Move(Vector3 delta)
        {
            if (delta == Vector3.Zero)
            {
                return;
            }

            var newPosition = position + delta;
            if (newPosition == Vector3.Zero)
            {
                return;
            }

            SetPosition(newPosition);
        }
        /// <inheritdoc/>
        public void Move(GameTime gameTime, Vector3 v, float delta = 1f)
        {
            Move(v * delta * gameTime.ElapsedSeconds);
        }
        /// <inheritdoc/>
        public void MoveForward(GameTime gameTime, float delta = 1f)
        {
            Move(gameTime, Forward, delta);
        }
        /// <inheritdoc/>
        public void MoveBackward(GameTime gameTime, float delta = 1f)
        {
            Move(gameTime, Backward, delta);
        }
        /// <inheritdoc/>
        public void MoveLeft(GameTime gameTime, float delta = 1f)
        {
            Move(gameTime, Left, -delta);
        }
        /// <inheritdoc/>
        public void MoveRight(GameTime gameTime, float delta = 1f)
        {
            Move(gameTime, Right, -delta);
        }
        /// <inheritdoc/>
        public void MoveUp(GameTime gameTime, float delta = 1f)
        {
            Move(gameTime, Up, delta);
        }
        /// <inheritdoc/>
        public void MoveDown(GameTime gameTime, float delta = 1f)
        {
            Move(gameTime, Down, delta);
        }

        /// <inheritdoc/>
        public void Rotate(float deltaYaw, float deltaPitch, float deltaRoll)
        {
            if (deltaYaw == 0f && deltaPitch == 0f && deltaRoll == 0f)
            {
                return;
            }

            var newRotation = rotation * Quaternion.RotationYawPitchRoll(deltaYaw, deltaPitch, deltaRoll);
            if (newRotation.IsIdentity)
            {
                return;
            }

            SetRotation(newRotation);
        }
        /// <inheritdoc/>
        public void YawLeft(GameTime gameTime, float delta = Helper.Radian)
        {
            Rotate(-delta * gameTime.ElapsedSeconds, 0, 0);
        }
        /// <inheritdoc/>
        public void YawRight(GameTime gameTime, float delta = Helper.Radian)
        {
            Rotate(delta * gameTime.ElapsedSeconds, 0, 0);
        }
        /// <inheritdoc/>
        public void PitchUp(GameTime gameTime, float delta = Helper.Radian)
        {
            Rotate(0, delta * gameTime.ElapsedSeconds, 0);
        }
        /// <inheritdoc/>
        public void PitchDown(GameTime gameTime, float delta = Helper.Radian)
        {
            Rotate(0, -delta * gameTime.ElapsedSeconds, 0);
        }
        /// <inheritdoc/>
        public void RollLeft(GameTime gameTime, float delta = Helper.Radian)
        {
            Rotate(0, 0, -delta * gameTime.ElapsedSeconds);
        }
        /// <inheritdoc/>
        public void RollRight(GameTime gameTime, float delta = Helper.Radian)
        {
            Rotate(0, 0, delta * gameTime.ElapsedSeconds);
        }

        /// <inheritdoc/>
        public void Scale(Vector3 delta, Vector3? minSize = null, Vector3? maxSize = null)
        {
            if (delta == Vector3.Zero)
            {
                return;
            }

            var newScale = scaling + delta;

            if (maxSize.HasValue && newScale.LengthSquared() > maxSize.Value.LengthSquared())
            {
                newScale = maxSize.Value;
            }

            if (minSize.HasValue && newScale.LengthSquared() < minSize.Value.LengthSquared())
            {
                newScale = minSize.Value;
            }

            if (newScale == Vector3.Zero)
            {
                return;
            }

            SetScale(newScale);
        }
        /// <inheritdoc/>
        public void Scale(GameTime gameTime, float delta, Vector3? minSize = null, Vector3? maxSize = null)
        {
            Scale(gameTime, new Vector3(delta), minSize, maxSize);
        }
        /// <inheritdoc/>
        public void Scale(GameTime gameTime, float deltaScaleX, float deltaScaleY, float deltaScaleZ, Vector3? minSize = null, Vector3? maxSize = null)
        {
            Scale(gameTime, new Vector3(deltaScaleX, deltaScaleY, deltaScaleZ), minSize, maxSize);
        }
        /// <inheritdoc/>
        public void Scale(GameTime gameTime, Vector3 delta, Vector3? minSize = null, Vector3? maxSize = null)
        {
            Scale(delta * gameTime.ElapsedSeconds, minSize, maxSize);
        }

        /// <inheritdoc/>
        public void SetPosition(float x, float y, float z, bool updateState = false)
        {
            SetPosition(new Vector3(x, y, z), updateState);
        }
        /// <inheritdoc/>
        public void SetPosition(Vector3 position, bool updateState = false)
        {
            if (this.position == position)
            {
                return;
            }

            this.position = position;

            transformUpdateNeeded = true;

            if (updateState) UpdateLocalTransform();
        }

        /// <inheritdoc/>
        public void SetRotation(float yaw, float pitch, float roll, bool updateState = false)
        {
            SetRotation(Quaternion.RotationYawPitchRoll(yaw, pitch, roll), updateState);
        }
        /// <inheritdoc/>
        public void SetRotation(Quaternion rotation, bool updateState = false)
        {
            if (this.rotation == rotation)
            {
                return;
            }

            this.rotation = rotation;

            transformUpdateNeeded = true;

            if (updateState) UpdateLocalTransform();
        }

        /// <inheritdoc/>
        public void SetScale(float scale, bool updateState = false)
        {
            SetScale(new Vector3(scale), updateState);
        }
        /// <inheritdoc/>
        public void SetScale(float scaleX, float scaleY, float scaleZ, bool updateState = false)
        {
            SetScale(new Vector3(scaleX, scaleY, scaleZ), updateState);
        }
        /// <inheritdoc/>
        public void SetScale(Vector3 scale, bool updateState = false)
        {
            if (scaling == scale)
            {
                return;
            }

            scaling = scale;

            transformUpdateNeeded = true;

            if (updateState) UpdateLocalTransform();
        }

        /// <inheritdoc/>
        public void SetTransform(Vector3 position, Quaternion rotation, float scale, bool updateState = false)
        {
            SetPosition(position);
            SetRotation(rotation);
            SetScale(scale, updateState);
        }
        /// <inheritdoc/>
        public void SetTransform(Vector3 position, Quaternion rotation, Vector3 scale, bool updateState = false)
        {
            SetPosition(position);
            SetRotation(rotation);
            SetScale(scale, updateState);
        }
        /// <inheritdoc/>
        public void SetTransform(Matrix transform)
        {
            if (!transform.Decompose(out var newScaling, out var newRotation, out var newPosition))
            {
                return;
            }

            scaling = newScaling;
            rotation = newRotation;
            position = newPosition;

            transformUpdateNeeded = true;

            UpdateLocalTransform();
        }

        /// <inheritdoc/>
        public void LookAt(Vector3 target, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false)
        {
            LookAt(target, Vector3.Up, axis, interpolationAmount, updateState);
        }
        /// <inheritdoc/>
        public void LookAt(Vector3 target, Vector3 up, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false)
        {
            if (Parent != null)
            {
                //Set parameters to local space
                var parentTransform = Matrix.Invert(Parent.GlobalTransform);

                target = Vector3.TransformCoordinate(target, parentTransform);
            }

            if (Vector3.NearEqual(position, target, new Vector3(MathUtil.ZeroTolerance)))
            {
                return;
            }

            var newRotation = Helper.LookAt(position, target, up, axis);

            if (interpolationAmount > 0)
            {
                newRotation = Quaternion.Lerp(rotation, newRotation, interpolationAmount);
            }

            SetRotation(newRotation, updateState);
        }

        /// <inheritdoc/>
        public void RotateTo(Vector3 target, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false)
        {
            RotateTo(target, Vector3.Up, axis, interpolationAmount, updateState);
        }
        /// <inheritdoc/>
        public void RotateTo(Vector3 target, Vector3 up, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false)
        {
            if (Parent != null)
            {
                //Set parameters to local space
                var parentTransform = Matrix.Invert(Parent.GlobalTransform);

                target = Vector3.TransformCoordinate(target, parentTransform);
            }

            if (Vector3.NearEqual(position, target, new Vector3(MathUtil.ZeroTolerance)))
            {
                return;
            }

            var newRotation = Helper.LookAt(position, target, up, axis);

            if (interpolationAmount > 0)
            {
                newRotation = Helper.RotateTowards(rotation, newRotation, interpolationAmount);
            }

            SetRotation(newRotation, updateState);
        }

        /// <inheritdoc/>
        public void SetNormal(Vector3 normal, float interpolationAmount = 0, bool updateState = false)
        {
            Quaternion newRotation;

            float angle = Helper.Angle(Up, normal);
            if (angle != 0)
            {
                Vector3 axis = Vector3.Cross(Up, normal);

                newRotation = Quaternion.RotationAxis(axis, angle) * rotation;
            }
            else
            {
                newRotation = Quaternion.RotationAxis(Vector3.Left, 0f) * rotation;
            }

            if (interpolationAmount > 0)
            {
                newRotation = Quaternion.Lerp(rotation, newRotation, interpolationAmount);
            }

            SetRotation(newRotation, updateState);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{GlobalTransform.GetDescription()}";
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new Manipulator3DState
            {
                LocalTransform = localTransform,
                Rotation = rotation,
                Scaling = scaling,
                Position = position,
                Parent = Parent?.GetState(),
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (state is not Manipulator3DState manipulator3DState)
            {
                return;
            }

            localTransform = manipulator3DState.LocalTransform;
            rotation = manipulator3DState.Rotation;
            scaling = manipulator3DState.Scaling;
            position = manipulator3DState.Position;
            if (manipulator3DState.Parent != null)
            {
                Parent.SetState(manipulator3DState.Parent);
            }
        }
    }
}
