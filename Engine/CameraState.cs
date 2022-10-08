using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Engine
{
    /// <summary>
    /// Camera state
    /// </summary>
    public class CameraState : IGameState
    {
        /// <summary>
        /// Position
        /// </summary>
        public Position3 Position { get; set; }
        /// <summary>
        /// Interest
        /// </summary>
        public Direction3 Interest { get; set; }
        /// <summary>
        /// Field of view angle
        /// </summary>
        public float FieldOfView { get; set; }
        /// <summary>
        /// Near plane distance
        /// </summary>
        public float NearPlaneDistance { get; set; }
        /// <summary>
        /// Far plane distance
        /// </summary>
        public float FarPlaneDistance { get; set; }
        /// <summary>
        /// Aspect relation
        /// </summary>
        public float AspectRelation { get; set; }
        /// <summary>
        /// Viewport width
        /// </summary>
        public float ViewportWidth { get; set; }
        /// <summary>
        /// Viewport height
        /// </summary>
        public float ViewportHeight { get; set; }
        /// <summary>
        /// Camera mode
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public CameraModes Mode { get; set; }
        /// <summary>
        /// Velocity vector
        /// </summary>
        public Direction3 Velocity { get; set; }
        /// <summary>
        /// Isometric axis
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public IsometricAxis IsometricAxis { get; set; }
        /// <summary>
        /// Isometric current forward
        /// </summary>
        public Direction3 IsoMetricForward { get; set; }
        /// <summary>
        /// Isometric current backward
        /// </summary>
        public Direction3 IsoMetricBackward { get; set; }
        /// <summary>
        /// Isometric current left
        /// </summary>
        public Direction3 IsoMetricLeft { get; set; }
        /// <summary>
        /// Isometric current right
        /// </summary>
        public Direction3 IsoMetricRight { get; set; }
        /// <summary>
        /// Translation mode
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public CameraTranslations TranslationMode { get; set; }
        /// <summary>
        /// Translation destination
        /// </summary>
        public Direction3 TranslationInterest { get; set; }
        /// <summary>
        /// Movement delta
        /// </summary>
        public float MovementDelta { get; set; }
        /// <summary>
        /// Slow movement delta
        /// </summary>
        public float SlowMovementDelta { get; set; }
        /// <summary>
        /// Rotation delta
        /// </summary>
        public float RotationDelta { get; set; }
        /// <summary>
        /// Slow rotation delta
        /// </summary>
        public float SlowRotationDelta { get; set; }
        /// <summary>
        /// Maximum zoom value
        /// </summary>
        public float ZoomMax { get; set; }
        /// <summary>
        /// Minimum zoom value
        /// </summary>
        public float ZoomMin { get; set; }
        /// <summary>
        /// Zoom delta
        /// </summary>
        public float ZoomDelta { get; set; }
        /// <summary>
        /// Zoom movement delta
        /// </summary>
        public float SlowZoomDelta { get; set; }
        /// <summary>
        /// Perspective view matrix
        /// </summary>
        public Matrix4X4 View { get; set; }
        /// <summary>
        /// Perspective projection matrix
        /// </summary>
        public Matrix4X4 Projection { get; set; }
        /// <summary>
        /// Camera frustum
        /// </summary>
        public Matrix4X4 Frustum { get; set; }
        /// <summary>
        /// Following object id
        /// </summary>
        public int Following { get; set; }
        /// <summary>
        /// Gets or sets whether the camera must invert the Y-delta mouse coordinate
        /// </summary>
        public bool InvertY { get; set; }
        /// <summary>
        /// Gets or sets the camera radius, for collision detection
        /// </summary>
        public float CameraRadius { get; set; }
    }
}
