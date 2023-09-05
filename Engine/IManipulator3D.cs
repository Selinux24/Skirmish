using SharpDX;

namespace Engine
{
    /// <summary>
    /// 3D manipulator interface
    /// </summary>
    public interface IManipulator3D : ITransform
    {
        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void Update(GameTime gameTime);
        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="force">If true, local transforms were forced to update</param>
        void UpdateInternals(bool force);

        /// <summary>
        /// Increments position component using the specified delta vector
        /// </summary>
        /// <param name="delta">Delta vector</param>
        void Move(Vector3 delta);
        /// <summary>
        /// Increments position component delta length along v vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="v">Direction vector</param>
        /// <param name="delta">Delta distance</param>
        void Move(GameTime gameTime, Vector3 v, float delta = 1f);
        /// <summary>
        /// Increments position component d distance along forward vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="delta">Delta distance</param>
        void MoveForward(GameTime gameTime, float delta = 1f);
        /// <summary>
        /// Increments position component d distance along backward vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="delta">Delta distance</param>
        void MoveBackward(GameTime gameTime, float delta = 1f);
        /// <summary>
        /// Increments position component d distance along left vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="delta">Delta distance</param>
        void MoveLeft(GameTime gameTime, float delta = 1f);
        /// <summary>
        /// Increments position component d distance along right vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="delta">Delta distance</param>
        void MoveRight(GameTime gameTime, float delta = 1f);
        /// <summary>
        /// Increments position component d distance along up vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="delta">Delta distance</param>
        void MoveUp(GameTime gameTime, float delta = 1f);
        /// <summary>
        /// Increments position component d distance along down vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="delta">Delta distance</param>
        void MoveDown(GameTime gameTime, float delta = 1f);

        /// <summary>
        /// Increments rotation component by axis
        /// </summary>
        /// <param name="deltaYaw">Yaw (Y) amount (radians)</param>
        /// <param name="deltaPitch">Pitch (X) amount (radians)</param>
        /// <param name="deltaRoll">Roll (Z) amount (radians)</param>
        void Rotate(float deltaYaw, float deltaPitch, float deltaRoll);
        /// <summary>
        /// Increments rotation yaw (Y) to the left
        /// </summary>
        /// <param name="delta">Delta (radians)</param>
        void YawLeft(GameTime gameTime, float delta = Helper.Radian);
        /// <summary>
        /// Increments rotation yaw (Y) to the right
        /// </summary>
        /// <param name="delta">Delta (radians)</param>
        void YawRight(GameTime gameTime, float delta = Helper.Radian);
        /// <summary>
        /// Increments rotation pitch (X) up
        /// </summary>
        /// <param name="delta">Delta (radians)</param>
        void PitchUp(GameTime gameTime, float delta = Helper.Radian);
        /// <summary>
        /// Increments rotation pitch (X) down
        /// </summary>
        /// <param name="delta">Delta (radians)</param>
        void PitchDown(GameTime gameTime, float delta = Helper.Radian);
        /// <summary>
        /// Increments rotation roll (Z) left
        /// </summary>
        /// <param name="delta">Delta (radians)</param>
        void RollLeft(GameTime gameTime, float delta = Helper.Radian);
        /// <summary>
        /// Increments rotation roll (Z) right
        /// </summary>
        /// <param name="delta">Delta (radians)</param>
        void RollRight(GameTime gameTime, float delta = Helper.Radian);

        /// <summary>
        /// Increments scaling the specified scale delta value
        /// </summary>
        /// <param name="delta">Scale delta</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        void Scale(Vector3 delta, Vector3? minSize = null, Vector3? maxSize = null);
        /// <summary>
        /// Increments scaling the specified scale delta value
        /// </summary>
        /// <param name="delta">Scale delta (percent 0 to x)</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        void Scale(GameTime gameTime, float delta, Vector3? minSize = null, Vector3? maxSize = null);
        /// <summary>
        /// Increments scaling the specified scale delta value
        /// </summary>
        /// <param name="deltaScaleX">X axis scale delta (percent 0 to x)</param>
        /// <param name="deltaScaleY">Y axis scale delta (percent 0 to x)</param>
        /// <param name="deltaScaleZ">Z axis scale delta (percent 0 to x)</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        void Scale(GameTime gameTime, float deltaScaleX, float deltaScaleY, float deltaScaleZ, Vector3? minSize = null, Vector3? maxSize = null);
        /// <summary>
        /// Increments scaling the specified scale delta value
        /// </summary>
        /// <param name="delta">Scale delta</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        void Scale(GameTime gameTime, Vector3 delta, Vector3? minSize = null, Vector3? maxSize = null);

        /// <summary>
        /// Sets the position component
        /// </summary>
        /// <param name="x">X component of position</param>
        /// <param name="y">Y component of position</param>
        /// <param name="z">Z component of position</param>
        /// <param name="updateState">Update internal state</param>
        void SetPosition(float x, float y, float z, bool updateState = false);
        /// <summary>
        /// Sets the position component
        /// </summary>
        /// <param name="position">Position component</param>
        /// <param name="updateState">Update internal state</param>
        void SetPosition(Vector3 position, bool updateState = false);

        /// <summary>
        /// Sets the rotation component
        /// </summary>
        /// <param name="yaw">Yaw (Y)</param>
        /// <param name="pitch">Pitch (X)</param>
        /// <param name="roll">Roll (Z)</param>
        /// <param name="updateState">Update internal state</param>
        void SetRotation(float yaw, float pitch, float roll, bool updateState = false);
        /// <summary>
        /// Sets the rotation component
        /// </summary>
        /// <param name="rotation">Rotation component</param>
        /// <param name="updateState">Update internal state</param>
        void SetRotation(Quaternion rotation, bool updateState = false);

        /// <summary>
        /// Sets the scaling component
        /// </summary>
        /// <param name="scale">Scale amount (0 to x)</param>
        /// <param name="updateState">Update internal state</param>
        void SetScale(float scale, bool updateState = false);
        /// <summary>
        /// Sets the scaling component
        /// </summary>
        /// <param name="scaleX">Scale along X axis</param>
        /// <param name="scaleY">Scale along Y axis</param>
        /// <param name="scaleZ">Scale along Z axis</param>
        /// <param name="updateState">Update internal state</param>
        void SetScale(float scaleX, float scaleY, float scaleZ, bool updateState = false);
        /// <summary>
        /// Sets the scaling component
        /// </summary>
        /// <param name="scale">Scale vector</param>
        /// <param name="updateState">Update internal state</param>
        void SetScale(Vector3 scale, bool updateState = false);

        /// <summary>
        /// Sets the position, scaling and rotation components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        /// <param name="updateState">Update internal state</param>
        void SetTransform(Vector3 position, Quaternion rotation, float scale, bool updateState = false);
        /// <summary>
        /// Sets the position, scaling and rotation components
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        /// <param name="updateState">Update internal state</param>
        void SetTransform(Vector3 position, Quaternion rotation, Vector3 scale, bool updateState = false);
        /// <summary>
        /// Sets transform matrix and updates position, rotation and scaling components
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        /// <remarks>The specified transform matrix must be a SRT matrix</remarks>
        void SetTransform(Matrix transform);

        /// <summary>
        /// Look at target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="axis">Relative rotation axis</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        /// <param name="updateState">Update internal state</param>
        void LookAt(Vector3 target, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false);
        /// <summary>
        /// Look at target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="up">Up vector</param>
        /// <param name="axis">Relative rotation axis</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        /// <param name="updateState">Update internal state</param>
        void LookAt(Vector3 target, Vector3 up, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false);

        /// <summary>
        /// Rotate to target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="axis">Relative rotation axis</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        /// <param name="updateState">Update internal state</param>
        void RotateTo(Vector3 target, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false);
        /// <summary>
        /// Rotate to target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="up">Up vector</param>
        /// <param name="axis">Relative rotation axis</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        /// <param name="updateState">Update internal state</param>
        void RotateTo(Vector3 target, Vector3 up, Axis axis = Axis.Y, float interpolationAmount = 0, bool updateState = false);

        /// <summary>
        /// Set model aligned to normal
        /// </summary>
        /// <param name="normal">Normal</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        /// <param name="updateState">Update internal state</param>
        void SetNormal(Vector3 normal, float interpolationAmount = 0, bool updateState = false);
    }
}
