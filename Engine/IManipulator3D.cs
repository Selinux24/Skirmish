using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// 3D manipulator interface
    /// </summary>
    public interface IManipulator3D : ITransform, IHasGameState
    {
        /// <summary>
        /// State updated event
        /// </summary>
        event EventHandler Updated;

        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void Update(IGameTime gameTime);
        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="force">If true, local transforms were forced to update</param>
        void UpdateInternals(bool force);
        /// <summary>
        /// Resets the manipulator internal state
        /// </summary>
        void Reset();

        /// <summary>
        /// Increments position component velocity along direction vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="direction">Direction vector</param>
        /// <param name="velocity">Velocity</param>
        void Move(IGameTime gameTime, Vector3 direction, float velocity = 1f);
        /// <summary>
        /// Increments position component velocity along forward vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="velocity">Velocity</param>
        void MoveForward(IGameTime gameTime, float velocity = 1f);
        /// <summary>
        /// Increments position component velocity along backward vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="velocity">Velocity</param>
        void MoveBackward(IGameTime gameTime, float velocity = 1f);
        /// <summary>
        /// Increments position component velocity along left vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="velocity">Velocity</param>
        void MoveLeft(IGameTime gameTime, float velocity = 1f);
        /// <summary>
        /// Increments position component velocity along right vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="velocity">Velocity</param>
        void MoveRight(IGameTime gameTime, float velocity = 1f);
        /// <summary>
        /// Increments position component velocity along up vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="velocity">Velocity</param>
        void MoveUp(IGameTime gameTime, float velocity = 1f);
        /// <summary>
        /// Increments position component velocity along down vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="velocity">Velocity</param>
        void MoveDown(IGameTime gameTime, float velocity = 1f);

        /// <summary>
        /// Increments rotation component
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="yaw">Yaw (Y) amount (radians)</param>
        /// <param name="pitch">Pitch (X) amount (radians)</param>
        /// <param name="roll">Roll (Z) amount (radians)</param>
        void Rotate(IGameTime gameTime, float yaw, float pitch, float roll);
        /// <summary>
        /// Increments rotation yaw (Y) to the left
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="yaw">Yaw (radians)</param>
        void YawLeft(IGameTime gameTime, float yaw = 1f);
        /// <summary>
        /// Increments rotation yaw (Y) to the right
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="delta">Yaw (radians)</param>
        void YawRight(IGameTime gameTime, float yaw = 1f);
        /// <summary>
        /// Increments rotation pitch (X) up
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="pitch">Pitch (radians)</param>
        void PitchUp(IGameTime gameTime, float pitch = 1f);
        /// <summary>
        /// Increments rotation pitch (X) down
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="pitch">Pitch (radians)</param>
        void PitchDown(IGameTime gameTime, float pitch = 1f);
        /// <summary>
        /// Increments rotation roll (Z) left
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="roll">Roll (radians)</param>
        void RollLeft(IGameTime gameTime, float roll = 1f);
        /// <summary>
        /// Increments rotation roll (Z) right
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="roll">Roll (radians)</param>
        void RollRight(IGameTime gameTime, float roll = 1f);

        /// <summary>
        /// Increments scaling the specified scaling value
        /// </summary>
        /// <param name="scaling">Scaling</param>
        void Scale(IGameTime gameTime, float scaling);
        /// <summary>
        /// Increments scaling the specified scaling value
        /// </summary>
        /// <param name="scalingX">X axis scaling (percent 0 to x)</param>
        /// <param name="scalingY">Y axis scaling (percent 0 to x)</param>
        /// <param name="scalingZ">Z axis scaling (percent 0 to x)</param>
        void Scale(IGameTime gameTime, float scalingX, float scalingY, float scalingZ);
        /// <summary>
        /// Increments scaling the specified scaling value
        /// </summary>
        /// <param name="scaling">Scaling</param>
        void Scale(IGameTime gameTime, Vector3 scaling);

        /// <summary>
        /// Sets the position component
        /// </summary>
        /// <param name="x">X component of position</param>
        /// <param name="y">Y component of position</param>
        /// <param name="z">Z component of position</param>
        void SetPosition(float x, float y, float z);
        /// <summary>
        /// Sets the position component
        /// </summary>
        /// <param name="position">Position component</param>
        void SetPosition(Vector3 position);

        /// <summary>
        /// Sets the rotation component
        /// </summary>
        /// <param name="rotationAxis">Rotation axis</param>
        /// <param name="rotationAngle">Rotation angle</param>
        void SetRotation(Vector3 rotationAxis, float rotationAngle);
        /// <summary>
        /// Sets the rotation component
        /// </summary>
        /// <param name="yaw">The yaw of rotation</param>
        /// <param name="pitch">The pitch of rotation</param>
        /// <param name="roll">The roll of rotation</param>
        void SetRotation(float yaw, float pitch, float roll);
        /// <summary>
        /// Sets the rotation component
        /// </summary>
        /// <param name="rotation">Rotation component</param>
        void SetRotation(Quaternion rotation);

        /// <summary>
        /// Sets the scaling component
        /// </summary>
        /// <param name="scaling">Scale amount (0 to x)</param>
        void SetScaling(float scaling);
        /// <summary>
        /// Sets the scaling component
        /// </summary>
        /// <param name="scalingX">Scale along X axis</param>
        /// <param name="scalingY">Scale along Y axis</param>
        /// <param name="scalingZ">Scale along Z axis</param>
        void SetScaling(float scalingX, float scalingY, float scalingZ);
        /// <summary>
        /// Sets the scaling component
        /// </summary>
        /// <param name="scaling">Scale vector</param>
        void SetScaling(Vector3 scaling);

        /// <summary>
        /// Sets the position, scaling and rotation components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotationAxis">Rotation axis</param>
        /// <param name="rotationAngle">Rotation angle</param>
        /// <param name="scaling">Scaling</param>
        void SetTransform(Vector3 position, Vector3 rotationAxis, float rotationAngle, float scaling);
        /// <summary>
        /// Sets the position, scaling and rotation components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotationAxis">Rotation axis</param>
        /// <param name="rotationAngle">Rotation angle</param>
        /// <param name="scaling">Scaling</param>
        void SetTransform(Vector3 position, Vector3 rotationAxis, float rotationAngle, Vector3 scaling);
        /// <summary>
        /// Sets the position, scaling and rotation components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="yaw">The yaw of rotation</param>
        /// <param name="pitch">The pitch of rotation</param>
        /// <param name="roll">The roll of rotation</param>
        /// <param name="scaling">Scaling</param>
        void SetTransform(Vector3 position, float yaw, float pitch, float roll, float scaling);
        /// <summary>
        /// Sets the position, scaling and rotation components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="yaw">The yaw of rotation</param>
        /// <param name="pitch">The pitch of rotation</param>
        /// <param name="roll">The roll of rotation</param>
        /// <param name="scaling">Scaling</param>
        void SetTransform(Vector3 position, float yaw, float pitch, float roll, Vector3 scaling);
        /// <summary>
        /// Sets the position, scaling and rotation components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scaling">Scaling</param>
        void SetTransform(Vector3 position, Quaternion rotation, float scaling);
        /// <summary>
        /// Sets the position, scaling and rotation components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scaling">Scaling</param>
        void SetTransform(Vector3 position, Quaternion rotation, Vector3 scaling);
        /// <summary>
        /// Sets transform matrix and updates position, rotation and scaling components
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        /// <remarks>The specified transform matrix must be a SRT matrix</remarks>
        void SetTransform(Matrix transform);

        /// <summary>
        /// Looks at target
        /// </summary>
        /// <param name="target">Target position</param>
        void LookAt(Vector3 target);
        /// <summary>
        /// Looks at target
        /// </summary>
        /// <param name="target">Target position</param>
        /// <param name="up">Up vector</param>
        void LookAt(Vector3 target, Vector3 up);

        /// <summary>
        /// Rotates to target
        /// </summary>
        /// <param name="target">Target position</param>
        /// <param name="axis">Rotation axis</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        void RotateTo(Vector3 target, Axis axis = Axis.Y, float interpolationAmount = 0);
        /// <summary>
        /// Rotates to target
        /// </summary>
        /// <param name="target">Target position</param>
        /// <param name="up">Up vector</param>
        /// <param name="axis">Rotation axis</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        void RotateTo(Vector3 target, Vector3 up, Axis axis = Axis.Y, float interpolationAmount = 0);

        /// <summary>
        /// Set model aligned to normal
        /// </summary>
        /// <param name="normal">Normal</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        void SetNormal(Vector3 normal, float interpolationAmount = 0);
    }
}
