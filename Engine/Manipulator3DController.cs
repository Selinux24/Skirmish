using SharpDX;

namespace Engine
{
    /// <summary>
    /// Manipulator controller interface
    /// </summary>
    public class Manipulator3DController
    {
        /// <summary>
        /// Following path
        /// </summary>
        protected IControllerPath path = null;
        /// <summary>
        /// Path time
        /// </summary>
        protected float pathTime = 0f;
        /// <summary>
        /// Gets if the current controller has a initialized path
        /// </summary>
        public bool HasPath
        {
            get
            {
                return this.path != null && this.path.Length > 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Manipulator3DController()
        {

        }

        /// <summary>
        /// Computes current position and orientation in the curve
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator to update</param>
        public virtual void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator)
        {
            if (this.HasPath && this.pathTime <= this.path.Length)
            {
                var newPosition = this.path.GetPosition(this.pathTime);

                if (this.pathTime != 0f)
                {
                    var view = Vector3.Normalize(manipulator.Position - newPosition);

                    manipulator.SetPosition(newPosition);
                    manipulator.LookAt(newPosition + view);
                }

                this.pathTime += gameTime.ElapsedSeconds * manipulator.LinearVelocity;
            }
        }
        /// <summary>
        /// Sets the path to follow
        /// </summary>
        /// <param name="path">Path to follow</param>
        /// <param name="time">Path initial time</param>
        public virtual void Follow(IControllerPath path, float time = 0f)
        {
            this.path = path;
            this.pathTime = time;
        }
    }
}
