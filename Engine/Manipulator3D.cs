﻿using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// 3D manipulator
    /// </summary>
    public class Manipulator3D : IManipulator, IHasGameState
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

        /// <summary>
        /// Parent manipulator
        /// </summary>
        public Manipulator3D Parent { get; set; }
        /// <summary>
        /// Gets Position component
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return position;
            }
        }
        /// <summary>
        /// Gets Scaling component
        /// </summary>
        public Vector3 Scaling
        {
            get
            {
                return scaling;
            }
        }
        /// <summary>
        /// Rotation component
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
        }
        /// <summary>
        /// Gets local transform of controller
        /// </summary>
        public Matrix LocalTransform
        {
            get
            {
                return localTransform;
            }
        }
        /// <summary>
        /// Gets final transform of controller
        /// </summary>
        public Matrix FinalTransform
        {
            get
            {
                if (Parent != null)
                {
                    return localTransform * Parent.FinalTransform;
                }

                return localTransform;
            }
        }
        /// <summary>
        /// Gets Forward vector
        /// </summary>
        public Vector3 Forward { get; private set; }
        /// <summary>
        /// Gets Backward vector
        /// </summary>
        public Vector3 Backward { get; private set; }
        /// <summary>
        /// Gets Left vector
        /// </summary>
        public Vector3 Left { get; private set; }
        /// <summary>
        /// Gets Right vector
        /// </summary>
        public Vector3 Right { get; private set; }
        /// <summary>
        /// Gets Up vector
        /// </summary>
        public Vector3 Up { get; private set; }
        /// <summary>
        /// Gets Down vector
        /// </summary>
        public Vector3 Down { get; private set; }
        /// <summary>
        /// Gets the velocity vector
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// Contructor
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
        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Update(GameTime gameTime)
        {
            UpdateLocalTransform();
        }
        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="force">If true, local transforms were forced to update</param>
        public void UpdateInternals(bool force)
        {
            if (force) transformUpdateNeeded = true;

            UpdateLocalTransform();
        }
        /// <summary>
        /// Update internal state
        /// </summary>
        protected void UpdateLocalTransform()
        {
            Vector3 prePos = localTransform.TranslationVector;
            Velocity = position - prePos;

            if (transformUpdateNeeded)
            {
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

                if (Updated != null)
                {
                    Updated.Invoke(this, new EventArgs());
                }

                Counters.UpdatesPerFrame++;
            }
        }
        /// <summary>
        /// Sets the local transform decomposing position, scale and rotation
        /// </summary>
        /// <param name="newLocalTransform">New local transform</param>
        protected void SetLocalTransform(Matrix newLocalTransform)
        {
            if (newLocalTransform.Decompose(out scaling, out rotation, out position))
            {
                transformUpdateNeeded = true;

                UpdateLocalTransform();
            }
        }

        /// <summary>
        /// Increments position component d length along d vector
        /// </summary>
        /// <param name="d">Distance</param>
        private void Move(Vector3 d)
        {
            if (d != Vector3.Zero)
            {
                position += d;

                transformUpdateNeeded = true;
            }
        }
        /// <summary>
        /// Increments rotation component by axis
        /// </summary>
        /// <param name="yaw">Yaw (Y) amount (radians)</param>
        /// <param name="pitch">Pitch (X) amount (radians)</param>
        /// <param name="roll">Roll (Z) amount (radians)</param>
        public void Rotate(float yaw, float pitch, float roll)
        {
            if (yaw != 0f || pitch != 0f || roll != 0f)
            {
                rotation *= Quaternion.RotationYawPitchRoll(yaw, pitch, roll);

                transformUpdateNeeded = true;
            }
        }

        /// <summary>
        /// Increments position component d distance along forward vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveForward(GameTime gameTime, float d = 1f)
        {
            Move(Forward * d * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along backward vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveBackward(GameTime gameTime, float d = 1f)
        {
            Move(Backward * d * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along left vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveLeft(GameTime gameTime, float d = 1f)
        {
            Move(Left * -d * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along right vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveRight(GameTime gameTime, float d = 1f)
        {
            Move(Right * -d * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along up vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveUp(GameTime gameTime, float d = 1f)
        {
            Move(Up * d * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along down vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveDown(GameTime gameTime, float d = 1f)
        {
            Move(Down * d * gameTime.ElapsedSeconds);
        }

        /// <summary>
        /// Increments rotation yaw (Y) to the left
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void YawLeft(GameTime gameTime, float a = Helper.Radian)
        {
            Rotate(-a * gameTime.ElapsedSeconds, 0, 0);
        }
        /// <summary>
        /// Increments rotation yaw (Y) to the right
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void YawRight(GameTime gameTime, float a = Helper.Radian)
        {
            Rotate(a * gameTime.ElapsedSeconds, 0, 0);
        }
        /// <summary>
        /// Increments rotation pitch (X) up
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void PitchUp(GameTime gameTime, float a = Helper.Radian)
        {
            Rotate(0, a * gameTime.ElapsedSeconds, 0);
        }
        /// <summary>
        /// Increments rotation pitch (X) down
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void PitchDown(GameTime gameTime, float a = Helper.Radian)
        {
            Rotate(0, -a * gameTime.ElapsedSeconds, 0);
        }
        /// <summary>
        /// Increments rotation roll (Z) left
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void RollLeft(GameTime gameTime, float a = Helper.Radian)
        {
            Rotate(0, 0, -a * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments rotation roll (Z) right
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void RollRight(GameTime gameTime, float a = Helper.Radian)
        {
            Rotate(0, 0, a * gameTime.ElapsedSeconds);
        }

        /// <summary>
        /// Clamped scale increment
        /// </summary>
        /// <param name="scale">Scale amount (percent 0 to x)</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        public void Scale(GameTime gameTime, float scale, Vector3? minSize = null, Vector3? maxSize = null)
        {
            Scale(gameTime, new Vector3(scale), minSize, maxSize);
        }
        /// <summary>
        /// Clamped scale increment
        /// </summary>
        /// <param name="scaleX">X axis scale amount (percent 0 to x)</param>
        /// <param name="scaleY">Y axis scale amount (percent 0 to x)</param>
        /// <param name="scaleZ">Z axis scale amount (percent 0 to x)</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        public void Scale(GameTime gameTime, float scaleX, float scaleY, float scaleZ, Vector3? minSize = null, Vector3? maxSize = null)
        {
            Scale(gameTime, new Vector3(scaleX, scaleY, scaleZ), minSize, maxSize);
        }
        /// <summary>
        /// Clamped scale increment
        /// </summary>
        /// <param name="scale">Scaling component</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        public void Scale(GameTime gameTime, Vector3 scale, Vector3? minSize = null, Vector3? maxSize = null)
        {
            Vector3 newScaling = scaling + (scale * gameTime.ElapsedSeconds);

            if (maxSize.HasValue && newScaling.LengthSquared() > maxSize.Value.LengthSquared())
            {
                newScaling = maxSize.Value;
            }

            if (minSize.HasValue && newScaling.LengthSquared() < minSize.Value.LengthSquared())
            {
                newScaling = minSize.Value;
            }

            SetScale(newScaling);
        }

        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="x">X component of position</param>
        /// <param name="y">Y component of position</param>
        /// <param name="z">Z component of position</param>
        /// <param name="updateState">Update internal state</param>
        public void SetPosition(float x, float y, float z, bool updateState = false)
        {
            SetPosition(new Vector3(x, y, z), updateState);
        }
        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="position">Position component</param>
        /// <param name="updateState">Update internal state</param>
        public void SetPosition(Vector3 position, bool updateState = false)
        {
            if (this.position != position)
            {
                this.position = position;

                transformUpdateNeeded = true;

                if (updateState) UpdateLocalTransform();
            }
        }
        /// <summary>
        /// Sets rotation
        /// </summary>
        /// <param name="yaw">Yaw (Y)</param>
        /// <param name="pitch">Pitch (X)</param>
        /// <param name="roll">Roll (Z)</param>
        /// <param name="updateState">Update internal state</param>
        public void SetRotation(float yaw, float pitch, float roll, bool updateState = false)
        {
            SetRotation(Quaternion.RotationYawPitchRoll(yaw, pitch, roll), updateState);
        }
        /// <summary>
        /// Sets rotation
        /// </summary>
        /// <param name="rotation">Rotation component</param>
        /// <param name="updateState">Update internal state</param>
        public void SetRotation(Quaternion rotation, bool updateState = false)
        {
            if (this.rotation != rotation)
            {
                this.rotation = rotation;

                transformUpdateNeeded = true;

                if (updateState) UpdateLocalTransform();
            }
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale amount (0 to x)</param>
        /// <param name="updateState">Update internal state</param>
        public void SetScale(float scale, bool updateState = false)
        {
            SetScale(new Vector3(scale), updateState);
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scaleX">Scale along X axis</param>
        /// <param name="scaleY">Scale along Y axis</param>
        /// <param name="scaleZ">Scale along Z axis</param>
        /// <param name="updateState">Update internal state</param>
        public void SetScale(float scaleX, float scaleY, float scaleZ, bool updateState = false)
        {
            SetScale(new Vector3(scaleX, scaleY, scaleZ), updateState);
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale vector</param>
        /// <param name="updateState">Update internal state</param>
        public void SetScale(Vector3 scale, bool updateState = false)
        {
            if (scaling != scale)
            {
                scaling = scale;

                transformUpdateNeeded = true;

                if (updateState) UpdateLocalTransform();
            }
        }
        /// <summary>
        /// Sets transform matrix
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        public void SetTransform(Matrix transform)
        {
            SetLocalTransform(transform);
        }
        /// <summary>
        /// Look at target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="axis">Relative rotation axis</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        /// <param name="updateState">Update internal state</param>
        public void LookAt(Vector3 target, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false)
        {
            LookAt(target, Vector3.Up, axis, interpolationAmount, updateState);
        }
        /// <summary>
        /// Look at target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="up">Up vector</param>
        /// <param name="axis">Relative rotation axis</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        /// <param name="updateState">Update internal state</param>
        public void LookAt(Vector3 target, Vector3 up, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false)
        {
            if (Parent != null)
            {
                //Set parameters to local space
                var parentTransform = Matrix.Invert(Parent.FinalTransform);

                target = Vector3.TransformCoordinate(target, parentTransform);
            }

            if (!Vector3.NearEqual(position, target, new Vector3(MathUtil.ZeroTolerance)))
            {
                var newRotation = Helper.LookAt(position, target, up, axis);

                if (interpolationAmount > 0)
                {
                    newRotation = Quaternion.Lerp(rotation, newRotation, interpolationAmount);
                }

                SetRotation(newRotation, updateState);
            }
        }
        /// <summary>
        /// Rotate to target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="axis">Relative rotation axis</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        /// <param name="updateState">Update internal state</param>
        public void RotateTo(Vector3 target, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false)
        {
            RotateTo(target, Vector3.Up, axis, interpolationAmount, updateState);
        }
        /// <summary>
        /// Rotate to target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="up">Up vector</param>
        /// <param name="axis">Relative rotation axis</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        /// <param name="updateState">Update internal state</param>
        public void RotateTo(Vector3 target, Vector3 up, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false)
        {
            if (Parent != null)
            {
                //Set parameters to local space
                var parentTransform = Matrix.Invert(Parent.FinalTransform);

                target = Vector3.TransformCoordinate(target, parentTransform);
            }

            if (!Vector3.NearEqual(position, target, new Vector3(MathUtil.ZeroTolerance)))
            {
                var newRotation = Helper.LookAt(position, target, up, axis);

                if (interpolationAmount > 0)
                {
                    newRotation = Helper.RotateTowards(rotation, newRotation, interpolationAmount);
                }

                SetRotation(newRotation, updateState);
            }
        }
        /// <summary>
        /// Set model aligned to normal
        /// </summary>
        /// <param name="normal">Normal</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        /// <param name="updateState">Update internal state</param>
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

        /// <summary>
        /// Gets manipulator text representation
        /// </summary>
        /// <returns>Returns manipulator text description</returns>
        public override string ToString()
        {
            return $"{FinalTransform.GetDescription()}";
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
            if (!(state is Manipulator3DState manipulator3DState))
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
