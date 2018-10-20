using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Updating context
    /// </summary>
    public class UpdateContext
    {
        /// <summary>
        /// Context name
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// Game time
        /// </summary>
        public GameTime GameTime { get; set; }
        /// <summary>
        /// View matrix
        /// </summary>
        public Matrix View { get; set; }
        /// <summary>
        /// Projection matrix
        /// </summary>
        public Matrix Projection { get; set; }
        /// <summary>
        /// Projection near plane distance
        /// </summary>
        public float NearPlaneDistance { get; set; }
        /// <summary>
        /// Projection far plane distance
        /// </summary>
        public float FarPlaneDistance { get; set; }
        /// <summary>
        /// View * projection matrix
        /// </summary>
        public Matrix ViewProjection { get; set; }
        /// <summary>
        /// Camera culling volume
        /// </summary>
        public CullingVolumeCamera CameraVolume { get; set; }
        /// <summary>
        /// Eye position
        /// </summary>
        public Vector3 EyePosition { get; set; }
        /// <summary>
        /// Eye view direction
        /// </summary>
        public Vector3 EyeDirection { get; set; }
        /// <summary>
        /// Lights
        /// </summary>
        public SceneLights Lights { get; set; }
    }
}
