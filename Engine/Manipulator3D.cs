using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// 3D manipulator
    /// </summary>
    public class Manipulator3D : IManipulator3D
    {
        /// <inheritdoc/>
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
        public IManipulator3D Parent { get; set; }

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
        public Manipulator3D(IManipulator3D parent) : this()
        {
            Parent = parent;
        }

        /// <inheritdoc/>
        public virtual void Update(IGameTime gameTime)
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
            var prePos = localTransform.TranslationVector;
            Velocity = position - prePos;

            if (!transformUpdateNeeded)
            {
                return;
            }

            var sca = Matrix.Scaling(scaling);
            var rot = Matrix.RotationQuaternion(rotation);
            var tra = Matrix.Translation(position);

            localTransform = sca * rot * tra;

            Forward = -rot.Forward;
            Backward = -rot.Backward;
            Left = rot.Left;
            Right = rot.Right;
            Up = rot.Up;
            Down = rot.Down;

            transformUpdateNeeded = false;

            Updated?.Invoke(this, new());

            FrameCounters.PickCounters.TransformUpdatesPerFrame++;
        }
        /// <inheritdoc/>
        public virtual void Reset()
        {
            SetTransform(Matrix.Identity);
        }

        /// <inheritdoc/>
        public void Move(IGameTime gameTime, Vector3 direction, float velocity = 1f)
        {
            float time = gameTime.ElapsedSeconds;

            if (MathUtil.IsZero(time))
            {
                return;
            }

            if (MathUtil.IsZero(velocity))
            {
                return;
            }

            if (Vector3.NearEqual(direction, Vector3.Zero, Helper.ZeroToleranceVector))
            {
                return;
            }

            var delta = Vector3.Normalize(direction) * velocity * time;

            var newPosition = position + delta;

            SetPosition(newPosition);
        }
        /// <inheritdoc/>
        public void MoveForward(IGameTime gameTime, float velocity = 1f)
        {
            Move(gameTime, Forward, -velocity);
        }
        /// <inheritdoc/>
        public void MoveBackward(IGameTime gameTime, float velocity = 1f)
        {
            Move(gameTime, Backward, -velocity);
        }
        /// <inheritdoc/>
        public void MoveLeft(IGameTime gameTime, float velocity = 1f)
        {
            Move(gameTime, Left, -velocity);
        }
        /// <inheritdoc/>
        public void MoveRight(IGameTime gameTime, float velocity = 1f)
        {
            Move(gameTime, Right, -velocity);
        }
        /// <inheritdoc/>
        public void MoveUp(IGameTime gameTime, float velocity = 1f)
        {
            Move(gameTime, Up, velocity);
        }
        /// <inheritdoc/>
        public void MoveDown(IGameTime gameTime, float velocity = 1f)
        {
            Move(gameTime, Down, velocity);
        }

        /// <inheritdoc/>
        public void Rotate(IGameTime gameTime, float yaw, float pitch, float roll)
        {
            float time = gameTime.ElapsedSeconds;

            if (MathUtil.IsZero(time))
            {
                return;
            }

            if (MathUtil.IsZero(yaw) && MathUtil.IsZero(pitch) && MathUtil.IsZero(roll))
            {
                return;
            }

            float deltaYaw = yaw * time;
            float deltaPitch = pitch * time;
            float deltaRoll = roll * time;

            var newRotation = rotation * Quaternion.RotationYawPitchRoll(deltaYaw, deltaPitch, deltaRoll);

            SetRotation(newRotation);
        }
        /// <inheritdoc/>
        public void YawLeft(IGameTime gameTime, float yaw = 1f)
        {
            Rotate(gameTime, -yaw, 0, 0);
        }
        /// <inheritdoc/>
        public void YawRight(IGameTime gameTime, float yaw = 1f)
        {
            Rotate(gameTime, yaw, 0, 0);
        }
        /// <inheritdoc/>
        public void PitchUp(IGameTime gameTime, float pitch = 1f)
        {
            Rotate(gameTime, 0, pitch, 0);
        }
        /// <inheritdoc/>
        public void PitchDown(IGameTime gameTime, float pitch = 1f)
        {
            Rotate(gameTime, 0, -pitch, 0);
        }
        /// <inheritdoc/>
        public void RollLeft(IGameTime gameTime, float roll = 1f)
        {
            Rotate(gameTime, 0, 0, -roll);
        }
        /// <inheritdoc/>
        public void RollRight(IGameTime gameTime, float roll = 1f)
        {
            Rotate(gameTime, 0, 0, roll);
        }

        /// <inheritdoc/>
        public void Scale(IGameTime gameTime, float scaling)
        {
            Scale(gameTime, new Vector3(scaling));
        }
        /// <inheritdoc/>
        public void Scale(IGameTime gameTime, float scalingX, float scalingY, float scalingZ)
        {
            Scale(gameTime, new Vector3(scalingX, scalingY, scalingZ));
        }
        /// <inheritdoc/>
        public void Scale(IGameTime gameTime, Vector3 scaling)
        {
            float time = gameTime.ElapsedSeconds;

            if (MathUtil.IsZero(time))
            {
                return;
            }

            if (MathUtil.IsZero(scaling.X) && MathUtil.IsZero(scaling.Y) && MathUtil.IsZero(scaling.Z))
            {
                return;
            }

            var deltaScaling = scaling * time;

            var newScale = this.scaling + deltaScaling;

            SetScaling(newScale);
        }

        /// <inheritdoc/>
        public void SetPosition(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }
        /// <inheritdoc/>
        public void SetPosition(Vector3 position)
        {
            if (this.position == position)
            {
                return;
            }

            this.position = position;

            transformUpdateNeeded = true;

            UpdateLocalTransform();
        }

        /// <inheritdoc/>
        public void SetRotation(Vector3 rotationAxis, float rotationAngle)
        {
            SetRotation(Quaternion.RotationAxis(rotationAxis, rotationAngle));
        }
        /// <inheritdoc/>
        public void SetRotation(float yaw, float pitch, float roll)
        {
            SetRotation(Quaternion.RotationYawPitchRoll(yaw, pitch, roll));
        }
        /// <inheritdoc/>
        public void SetRotation(Quaternion rotation)
        {
            if (this.rotation == rotation)
            {
                return;
            }

            this.rotation = rotation;

            transformUpdateNeeded = true;

            UpdateLocalTransform();
        }

        /// <inheritdoc/>
        public void SetScaling(float scaling)
        {
            SetScaling(new Vector3(scaling));
        }
        /// <inheritdoc/>
        public void SetScaling(float scalingX, float scalingY, float scalingZ)
        {
            SetScaling(new Vector3(scalingX, scalingY, scalingZ));
        }
        /// <inheritdoc/>
        public void SetScaling(Vector3 scaling)
        {
            if (this.scaling == scaling)
            {
                return;
            }

            this.scaling = scaling;

            transformUpdateNeeded = true;

            UpdateLocalTransform();
        }

        /// <inheritdoc/>
        public void SetTransform(Vector3 position, Vector3 rotationAxis, float rotationAngle, float scaling)
        {
            SetTransform(position, Quaternion.RotationAxis(rotationAxis, rotationAngle), scaling);
        }
        /// <inheritdoc/>
        public void SetTransform(Vector3 position, Vector3 rotationAxis, float rotationAngle, Vector3 scaling)
        {
            SetTransform(position, Quaternion.RotationAxis(rotationAxis, rotationAngle), scaling);
        }
        /// <inheritdoc/>
        public void SetTransform(Vector3 position, float yaw, float pitch, float roll, float scaling)
        {
            SetTransform(position, Quaternion.RotationYawPitchRoll(yaw, pitch, roll), scaling);
        }
        /// <inheritdoc/>
        public void SetTransform(Vector3 position, float yaw, float pitch, float roll, Vector3 scaling)
        {
            SetTransform(position, Quaternion.RotationYawPitchRoll(yaw, pitch, roll), scaling);
        }
        /// <inheritdoc/>
        public void SetTransform(Vector3 position, Quaternion rotation, float scaling)
        {
            SetTransform(position, rotation, new Vector3(scaling, scaling, scaling));
        }
        /// <inheritdoc/>
        public void SetTransform(Vector3 position, Quaternion rotation, Vector3 scaling)
        {
            this.scaling = scaling;
            this.rotation = rotation;
            this.position = position;

            transformUpdateNeeded = true;

            UpdateLocalTransform();
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
        public void LookAt(Vector3 target)
        {
            LookAt(target, Vector3.Up);
        }
        /// <inheritdoc/>
        public void LookAt(Vector3 target, Vector3 up)
        {
            if (Parent != null)
            {
                //Set parameters to local space
                var parentTransform = Matrix.Invert(Parent.GlobalTransform);

                target = Vector3.TransformCoordinate(target, parentTransform);
            }

            if (Vector3.NearEqual(position, target, Helper.ZeroToleranceVector))
            {
                return;
            }

            var newRotation = Helper.LookAt(position, target, up, Axis.None);

            SetRotation(newRotation);
        }

        /// <inheritdoc/>
        public void RotateTo(Vector3 target, Axis axis = Axis.Y, float interpolationAmount = 0)
        {
            RotateTo(target, Vector3.Up, axis, interpolationAmount);
        }
        /// <inheritdoc/>
        public void RotateTo(Vector3 target, Vector3 up, Axis axis = Axis.Y, float interpolationAmount = 0)
        {
            if (Parent != null)
            {
                //Set parameters to local space
                var parentTransform = Matrix.Invert(Parent.GlobalTransform);

                target = Vector3.TransformCoordinate(target, parentTransform);
            }

            if (Vector3.NearEqual(position, target, Helper.ZeroToleranceVector))
            {
                return;
            }

            var newRotation = Helper.LookAt(position, target, up, axis);

            if (interpolationAmount > 0)
            {
                newRotation = Helper.RotateTowards(rotation, newRotation, interpolationAmount);
            }

            SetRotation(newRotation);
        }

        /// <inheritdoc/>
        public void SetNormal(Vector3 normal, float interpolationAmount = 0)
        {
            float angle = Helper.AngleSigned(Up, normal);

            var axis = MathUtil.IsZero(angle % MathUtil.Pi) ? Vector3.Left : Vector3.Cross(Up, normal);

            var newRotation = Quaternion.RotationAxis(axis, angle) * rotation;

            if (interpolationAmount > 0)
            {
                newRotation = Quaternion.Lerp(rotation, newRotation, interpolationAmount);
            }

            SetRotation(newRotation);
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
