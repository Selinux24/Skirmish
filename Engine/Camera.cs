using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// Camera 3D
    /// </summary>
    public class Camera : IManipulator, IDisposable
    {
        /// <summary>
        /// Creates an isometric camera
        /// </summary>
        /// <param name="axis">Isometric axis</param>
        /// <param name="interest">Interest point</param>
        /// <param name="distance">Distance from viewer to interest point</param>
        /// <returns>Returns the new camera</returns>
        public static Camera CreateIsometric(IsometricAxis axis, Vector3 interest, float distance)
        {
            Camera cam = new Camera();

            cam.SetIsometric(axis, interest, distance);

            return cam;
        }
        /// <summary>
        /// Creates a free camera
        /// </summary>
        /// <param name="position">Viewer position</param>
        /// <param name="interest">Interest point</param>
        /// <returns>Returns the new camera</returns>
        public static Camera CreateFree(Vector3 position, Vector3 interest)
        {
            return new Camera
            {
                mode = CameraModes.Free,
                position = position,
                interest = interest
            };
        }
        /// <summary>
        /// Creates 2D camera
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="interest">Interest</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns new 2D camera</returns>
        public static Camera CreateOrtho(Vector3 position, Vector3 interest, int width, int height)
        {
            Camera camera = new Camera
            {
                Position = position,
                Interest = interest
            };

            camera.SetLens(width, height);

            return camera;
        }

        /// <summary>
        /// Position
        /// </summary>
        private Vector3 position;
        /// <summary>
        /// Interest
        /// </summary>
        private Vector3 interest;
        /// <summary>
        /// Field of view angle
        /// </summary>
        private float fieldOfView = MathUtil.PiOverFour;
        /// <summary>
        /// Near plane distance
        /// </summary>
        private float nearPlaneDistance = 1f;
        /// <summary>
        /// Far plane distance
        /// </summary>
        private float farPlaneDistance = 100f;
        /// <summary>
        /// Aspect relation
        /// </summary>
        private float aspectRelation = 1f;
        /// <summary>
        /// Viewport width
        /// </summary>
        private float viewportWidth = 0f;
        /// <summary>
        /// Viewport height
        /// </summary>
        private float viewportHeight = 0f;
        /// <summary>
        /// Camera mode
        /// </summary>
        private CameraModes mode;

        /// <summary>
        /// Forward vector
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                if (this.mode == CameraModes.FreeIsometric)
                {
                    return this.isoMetricForward;
                }
                else if (mode == CameraModes.Free)
                {
                    return Vector3.Normalize(this.Interest - this.Position);
                }
                else if (mode == CameraModes.FirstPerson || mode == CameraModes.ThirdPerson)
                {
                    var v = this.Interest - this.Position;

                    return Vector3.Normalize(new Vector3(v.X, 0, v.Z));
                }
                else
                {
                    return Vector3.Normalize(this.Interest - this.Position);
                }
            }
        }
        /// <summary>
        /// Backward vector
        /// </summary>
        public Vector3 Backward
        {
            get
            {
                if (this.mode == CameraModes.FreeIsometric)
                {
                    return this.isoMetricBackward;
                }
                else if (mode == CameraModes.Free)
                {
                    return -this.Forward;
                }
                else if (mode == CameraModes.FirstPerson || mode == CameraModes.ThirdPerson)
                {
                    return -this.Forward;
                }
                else
                {
                    return -this.Forward;
                }
            }
        }
        /// <summary>
        /// Left vector
        /// </summary>
        public Vector3 Left
        {
            get
            {
                if (this.mode == CameraModes.FreeIsometric)
                {
                    return this.isoMetricLeft;
                }
                else if (mode == CameraModes.Free)
                {
                    return Vector3.Cross(this.Forward, Vector3.Up);
                }
                else if (mode == CameraModes.FirstPerson || mode == CameraModes.ThirdPerson)
                {
                    return Vector3.Cross(this.Forward, Vector3.Up);
                }
                else
                {
                    return Vector3.Cross(this.Forward, Vector3.Up);
                }
            }
        }
        /// <summary>
        /// Right vector
        /// </summary>
        public Vector3 Right
        {
            get
            {
                if (this.mode == CameraModes.FreeIsometric)
                {
                    return this.isoMetricRight;
                }
                else if (mode == CameraModes.Free)
                {
                    return -Left;
                }
                else if (mode == CameraModes.FirstPerson || mode == CameraModes.ThirdPerson)
                {
                    return -Left;
                }
                else
                {
                    return -Left;
                }
            }
        }
        /// <summary>
        /// Up vector
        /// </summary>
        public Vector3 Up
        {
            get
            {
                if (this.mode == CameraModes.FreeIsometric)
                {
                    return new Vector3(0f, 1f, 0f);
                }
                else if (mode == CameraModes.Free)
                {
                    return Vector3.Cross(this.Left, this.Forward);
                }
                else if (mode == CameraModes.FirstPerson || mode == CameraModes.ThirdPerson)
                {
                    return Vector3.Up;
                }
                else
                {
                    return Vector3.Cross(this.Left, this.Forward);
                }
            }
        }
        /// <summary>
        /// Down vector
        /// </summary>
        public Vector3 Down
        {
            get
            {
                if (this.mode == CameraModes.FreeIsometric)
                {
                    return new Vector3(0f, -1f, 0f);
                }
                else if (mode == CameraModes.Free)
                {
                    return -Up;
                }
                else if (mode == CameraModes.FirstPerson || mode == CameraModes.ThirdPerson)
                {
                    return -Up;
                }
                else
                {
                    return -Up;
                }
            }
        }
        /// <summary>
        /// Velocity vector
        /// </summary>
        public Vector3 Velocity { get; private set; }

        #region Isometric

        /// <summary>
        /// Isometric axis
        /// </summary>
        private IsometricAxis isometricAxis = IsometricAxis.NW;
        /// <summary>
        /// Isometric current forward
        /// </summary>
        private Vector3 isoMetricForward = new Vector3(-1f, 0f, -1f);
        /// <summary>
        /// Isometric current backward
        /// </summary>
        private Vector3 isoMetricBackward = new Vector3(1f, 0f, 1f);
        /// <summary>
        /// Isometric current left
        /// </summary>
        private Vector3 isoMetricLeft = new Vector3(1f, 0f, -1f);
        /// <summary>
        /// Isometric current right
        /// </summary>
        private Vector3 isoMetricRight = new Vector3(-1f, 0f, 1f);

        #endregion

        #region Translations

        /// <summary>
        /// Translation mode
        /// </summary>
        private CameraTranslations translationMode = CameraTranslations.None;
        /// <summary>
        /// Translation destination
        /// </summary>
        private Vector3 translationInterest = Vector3.Zero;
        /// <summary>
        /// Radius for slow aproximation delta
        /// </summary>
        private readonly float translationRadius = 10f;
        /// <summary>
        /// Modifier applied to delta out of the radius
        /// </summary>
        private readonly float translationOutOfRadius = 0.2f;
        /// <summary>
        /// Modifier applied to delta into the radius
        /// </summary>
        private readonly float translationIntoRadius = 0.01f;

        #endregion

        /// <summary>
        /// Gets or sets camera mode
        /// </summary>
        public CameraModes Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                this.mode = value;

                if (this.mode == CameraModes.FreeIsometric)
                {
                    this.SetIsometric(IsometricAxis.SE, Vector3.Zero, this.ZoomMin * 2f);
                }
                else if (this.mode == CameraModes.Free)
                {
                    this.SetFree(this.position, this.interest);
                }
                else if (this.mode == CameraModes.Ortho)
                {
                    this.SetOrtho();
                }
            }
        }
        /// <summary>
        /// Camera viewer position
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return this.Following != null ? this.Following.Position : this.position;
            }
            set
            {
                this.position = value;

                if (this.Following != null) this.Following = null;
            }
        }
        /// <summary>
        /// Camera interest
        /// </summary>
        public Vector3 Interest
        {
            get
            {
                return this.Following != null ? this.Following.Interest : this.interest;
            }
            set
            {
                this.interest = value;

                if (this.Following != null) this.Following = null;
            }
        }
        /// <summary>
        /// Camera direction
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                return Vector3.Normalize(this.Interest - this.Position);
            }
        }
        /// <summary>
        /// Movement delta (20.5f)
        /// </summary>
        public float MovementDelta { get; set; } = 20.5f;
        /// <summary>
        /// Slow movement delta (1.0f)
        /// </summary>
        public float SlowMovementDelta { get; set; } = 1.0f;
        /// <summary>
        /// Rotation delta (0.25f)
        /// </summary>
        public float RotationDelta { get; set; } = 0.25f;
        /// <summary>
        /// Slow rotation delta (0.10f)
        /// </summary>
        public float SlowRotationDelta { get; set; } = 0.10f;
        /// <summary>
        /// Maximum zoom value
        /// </summary>
        public float ZoomMax { get; set; } = 200f;
        /// <summary>
        /// Minimum zoom value
        /// </summary>
        public float ZoomMin { get; set; } = 15f;
        /// <summary>
        /// Zoom delta
        /// </summary>
        public float ZoomDelta { get; set; } = 100f;
        /// <summary>
        /// Zoom movement delta
        /// </summary>
        public float SlowZoomDelta { get; set; } = 40f;
        /// <summary>
        /// Gets or sets field of view value
        /// </summary>
        public float FieldOfView
        {
            get
            {
                return this.fieldOfView;
            }
            set
            {
                this.fieldOfView = value;

                this.UpdateLens();
            }
        }
        /// <summary>
        /// Gets or sets near plane distance
        /// </summary>
        public float NearPlaneDistance
        {
            get
            {
                return this.nearPlaneDistance;
            }
            set
            {
                this.nearPlaneDistance = value;

                this.UpdateLens();
            }
        }
        /// <summary>
        /// Gets or sets far plane distance
        /// </summary>
        public float FarPlaneDistance
        {
            get
            {
                return this.farPlaneDistance;
            }
            set
            {
                this.farPlaneDistance = value;

                this.UpdateLens();
            }
        }
        /// <summary>
        /// Gets or sets aspect relation
        /// </summary>
        public float AspectRelation
        {
            get
            {
                return this.aspectRelation;
            }
            set
            {
                this.aspectRelation = value;

                this.UpdateLens();
            }
        }
        /// <summary>
        /// Perspective view matrix
        /// </summary>
        public Matrix View { get; private set; }
        /// <summary>
        /// Perspective projection matrix
        /// </summary>
        public Matrix Projection { get; private set; }
        /// <summary>
        /// Camera frustum
        /// </summary>
        public BoundingFrustum Frustum { get; private set; }
        /// <summary>
        /// Following object
        /// </summary>
        public IFollower Following { get; set; }
        /// <summary>
        /// Gets or sets whether the camera must invert the Y-delta mouse coordinate
        /// </summary>
        public bool InvertY { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected Camera()
        {
            this.position = Vector3.One;
            this.interest = Vector3.Zero;

            this.View = Matrix.LookAtLH(
                this.position,
                this.interest,
                Vector3.UnitY);

            this.Projection = Matrix.Identity;

            this.InvertY = false;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Camera()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {

        }

        /// <summary>
        /// Sets dimensions of viewport
        /// </summary>
        /// <param name="width">Viewport width</param>
        /// <param name="height">Viewport height</param>
        public void SetLens(int width, int height)
        {
            this.viewportWidth = width;
            this.viewportHeight = height;

            this.aspectRelation = this.viewportWidth / this.viewportHeight;

            this.UpdateLens();
        }
        /// <summary>
        /// Updates camera state
        /// </summary>
        public void Update(GameTime gameTime)
        {
            this.UpdateTranslations(gameTime);

            if (this.mode == CameraModes.Ortho)
            {
                this.View = Matrix.LookAtLH(
                    this.Position,
                    this.Interest,
                    Vector3.UnitZ);
            }
            else
            {
                this.View = Matrix.LookAtLH(
                    this.Position,
                    this.Interest,
                    Vector3.UnitY);
            }

            this.Frustum = new BoundingFrustum(this.View * this.Projection);
        }
        /// <summary>
        /// Sets previous isometrix axis
        /// </summary>
        public void PreviousIsometricAxis()
        {
            if (this.mode == CameraModes.FreeIsometric)
            {
                int current = (int)this.isometricAxis;
                int previous;

                if (current <= 0)
                {
                    previous = 3;
                }
                else
                {
                    previous = current - 1;
                }

                this.SetIsometric((IsometricAxis)previous, this.Interest, Vector3.Distance(this.Position, this.Interest));
            }
        }
        /// <summary>
        /// Sets next isometrix axis
        /// </summary>
        public void NextIsometricAxis()
        {
            if (this.mode == CameraModes.FreeIsometric)
            {
                int current = (int)this.isometricAxis;
                int next;

                if (current >= 3)
                {
                    next = 0;
                }
                else
                {
                    next = current + 1;
                }

                this.SetIsometric((IsometricAxis)next, this.Interest, Vector3.Distance(this.Position, this.Interest));
            }
        }
        /// <summary>
        /// Move forward
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveForward(GameTime gameTime, bool slow)
        {
            this.Move(gameTime, this.Forward, slow);
        }
        /// <summary>
        /// Move backward
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveBackward(GameTime gameTime, bool slow)
        {
            this.Move(gameTime, this.Backward, slow);
        }
        /// <summary>
        /// Move left
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveLeft(GameTime gameTime, bool slow)
        {
            this.Move(gameTime, this.Left, slow);
        }
        /// <summary>
        /// Move right
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveRight(GameTime gameTime, bool slow)
        {
            this.Move(gameTime, this.Right, slow);
        }
        /// <summary>
        /// Rotate up
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateUp(GameTime gameTime, bool slow)
        {
            this.Rotate(gameTime, this.Left, slow);
        }
        /// <summary>
        /// Rotate down
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateDown(GameTime gameTime, bool slow)
        {
            this.Rotate(gameTime, this.Right, slow);
        }
        /// <summary>
        /// Rotate left
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateLeft(GameTime gameTime, bool slow)
        {
            this.Rotate(gameTime, this.Down, slow);
        }
        /// <summary>
        /// Rotate right
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateRight(GameTime gameTime, bool slow)
        {
            this.Rotate(gameTime, this.Up, slow);
        }
        /// <summary>
        /// Rotate camera by mouse
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="deltaX">X mouse delta</param>
        /// <param name="deltaY">Y mouse delta</param>
        public void RotateMouse(GameTime gameTime, float deltaX, float deltaY)
        {
            if (deltaX != 0f)
            {
                this.Rotate(this.Up, gameTime.ElapsedSeconds * deltaX * 10f);
            }
            if (deltaY != 0f)
            {
                if (this.InvertY) deltaY = -deltaY;

                this.Rotate(this.Left, gameTime.ElapsedSeconds * -deltaY * 10f, true, -85, 85);
            }
        }
        /// <summary>
        /// Zoom in
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void ZoomIn(GameTime gameTime, bool slow)
        {
            this.Zoom(gameTime, true, slow);
        }
        /// <summary>
        /// Zoom out
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void ZoomOut(GameTime gameTime, bool slow)
        {
            this.Zoom(gameTime, false, slow);
        }

        /// <summary>
        /// Sets camera to free mode
        /// </summary>
        /// <param name="newPosition">New position</param>
        /// <param name="newInterest">New interest point</param>
        private void SetFree(Vector3 newPosition, Vector3 newInterest)
        {
            this.StopTranslations();

            this.Position = newPosition;
            this.Interest = newInterest;

            this.mode = CameraModes.Free;
        }
        /// <summary>
        /// Sets camera to isometric mode
        /// </summary>
        /// <param name="axis">Isometrix axis</param>
        /// <param name="newInterest">Interest point</param>
        /// <param name="distance">Distance to interest point from viewer point</param>
        private void SetIsometric(IsometricAxis axis, Vector3 newInterest, float distance)
        {
            this.StopTranslations();

            Vector3 tmpPosition = Vector3.Zero;

            if (axis == IsometricAxis.NW)
            {
                tmpPosition = new Vector3(1, 1, 1);
                this.isoMetricForward = new Vector3(-1f, 0f, -1f);
                this.isoMetricBackward = new Vector3(1f, 0f, 1f);
                this.isoMetricLeft = new Vector3(1f, 0f, -1f);
                this.isoMetricRight = new Vector3(-1f, 0f, 1f);
            }
            else if (axis == IsometricAxis.NE)
            {
                tmpPosition = new Vector3(-1, 1, 1);
                this.isoMetricForward = new Vector3(1f, 0f, -1f);
                this.isoMetricBackward = new Vector3(-1f, 0f, 1f);
                this.isoMetricLeft = new Vector3(1f, 0f, 1f);
                this.isoMetricRight = new Vector3(-1f, 0f, -1f);
            }
            else if (axis == IsometricAxis.SW)
            {
                tmpPosition = new Vector3(1, 1, -1);
                this.isoMetricForward = new Vector3(-1f, 0f, 1f);
                this.isoMetricBackward = new Vector3(1f, 0f, -1f);
                this.isoMetricLeft = new Vector3(-1f, 0f, -1f);
                this.isoMetricRight = new Vector3(1f, 0f, 1f);
            }
            else if (axis == IsometricAxis.SE)
            {
                tmpPosition = new Vector3(-1, 1, -1);
                this.isoMetricForward = new Vector3(1f, 0f, 1f);
                this.isoMetricBackward = new Vector3(-1f, 0f, -1f);
                this.isoMetricLeft = new Vector3(-1f, 0f, 1f);
                this.isoMetricRight = new Vector3(1f, 0f, -1f);
            }

            this.mode = CameraModes.FreeIsometric;
            this.isometricAxis = axis;

            this.Position = Vector3.Normalize(tmpPosition) * distance;
            this.Position += newInterest;
            this.Interest = newInterest;
        }
        /// <summary>
        /// Sets camero to ortho mode
        /// </summary>
        private void SetOrtho()
        {
            this.StopTranslations();

            this.Position = Vector3.Up;
            this.Interest = Vector3.Zero;

            this.mode = CameraModes.Ortho;
        }
        /// <summary>
        /// Update projections
        /// </summary>
        private void UpdateLens()
        {
            if (this.mode == CameraModes.Ortho)
            {
                this.Projection = Matrix.OrthoLH(
                    this.viewportWidth,
                    this.viewportHeight,
                    this.nearPlaneDistance,
                    this.farPlaneDistance);
            }
            else
            {
                this.Projection = Matrix.PerspectiveFovLH(
                    this.fieldOfView,
                    this.aspectRelation,
                    this.nearPlaneDistance,
                    this.farPlaneDistance);
            }
        }

        /// <summary>
        /// Move camera to position
        /// </summary>
        /// <param name="x">X position component</param>
        /// <param name="y">Y position component</param>
        /// <param name="z">Z position component</param>
        /// <param name="translation">Translation mode</param>
        public void Goto(float x, float y, float z, CameraTranslations translation = CameraTranslations.None)
        {
            this.Goto(new Vector3(x, y, z), translation);
        }
        /// <summary>
        /// Move camera to position
        /// </summary>
        /// <param name="newPosition">New position</param>
        /// <param name="translation">Translation mode</param>
        public void Goto(Vector3 newPosition, CameraTranslations translation = CameraTranslations.None)
        {
            Vector3 diff = newPosition - this.Position;

            if (translation != CameraTranslations.None)
            {
                this.StartTranslations(translation, this.Interest + diff);
            }
            else
            {
                this.StopTranslations();

                this.Position += diff;
                this.Interest += diff;
            }
        }
        /// <summary>
        /// Center camera in new interest
        /// </summary>
        /// <param name="x">X position component</param>
        /// <param name="y">Y position component</param>
        /// <param name="z">Z position component</param>
        /// <param name="translation">Translation mode</param>
        public void LookTo(float x, float y, float z, CameraTranslations translation = CameraTranslations.None)
        {
            this.LookTo(new Vector3(x, y, z), translation);
        }
        /// <summary>
        /// Center camera in new interest
        /// </summary>
        /// <param name="newInterest">New interest point</param>
        /// <param name="translation">Translation mode</param>
        public void LookTo(Vector3 newInterest, CameraTranslations translation = CameraTranslations.None)
        {
            if (translation != CameraTranslations.None)
            {
                this.StartTranslations(translation, newInterest);
            }
            else
            {
                this.StopTranslations();

                if (this.mode == CameraModes.FreeIsometric)
                {
                    Vector3 diff = newInterest - this.Interest;

                    this.Position += diff;
                    this.Interest += diff;
                }
                else
                {
                    this.Interest = newInterest;
                }
            }
        }
        /// <summary>
        /// Movement
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="vector">Movement vector</param>
        /// <param name="slow">Slow movement</param>
        private void Move(GameTime gameTime, Vector3 vector, bool slow)
        {
            this.StopTranslations();

            float delta = (slow) ? this.SlowMovementDelta : this.MovementDelta;

            this.Velocity = vector * delta * gameTime.ElapsedSeconds;
            if (this.Velocity != Vector3.Zero)
            {
                this.Position += this.Velocity;
                this.Interest += this.Velocity;
            }
        }
        /// <summary>
        /// Rotation
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="axis">Rotation axis</param>
        /// <param name="slow">Slow movement</param>
        private void Rotate(GameTime gameTime, Vector3 axis, bool slow)
        {
            float degrees = (slow) ? this.SlowRotationDelta : this.RotationDelta;

            this.Rotate(axis, degrees * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Rotation
        /// </summary>
        /// <param name="axis">Rotation axis</param>
        /// <param name="degrees">Degrees</param>
        /// <param name="clampY">Sets if the current roation must be clamped around the Y vector</param>
        /// <param name="clampFrom">Clamp from angle in degrees</param>
        /// <param name="clampTo">Clamp to angle in degrees</param>
        private void Rotate(Vector3 axis, float degrees, bool clampY = false, float clampFrom = 0, float clampTo = 0)
        {
            this.StopTranslations();

            //Smooth rotation
            Quaternion sourceRotation = Quaternion.RotationAxis(axis, 0);
            Quaternion targetRotation = Quaternion.RotationAxis(axis, MathUtil.DegreesToRadians(degrees));
            Quaternion r = Quaternion.Lerp(sourceRotation, targetRotation, 0.5f);

            Vector3 curDir = Vector3.Normalize(this.Interest - this.Position);
            Vector3 newDir = Vector3.Transform(curDir, r);

            if (clampY)
            {
                float newAngle = Helper.Angle(Vector3.Up, newDir) - MathUtil.PiOverTwo;
                if (newAngle >= MathUtil.DegreesToRadians(clampFrom) && newAngle <= MathUtil.DegreesToRadians(clampTo))
                {
                    this.Interest = this.position + newDir;
                }
            }
            else
            {
                this.Interest = this.position + newDir;
            }
        }
        /// <summary>
        /// Zoom
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="zoomIn">True if camera goes in. False otherwise</param>
        /// <param name="slow">Slow movement</param>
        private void Zoom(GameTime gameTime, bool zoomIn, bool slow)
        {
            this.StopTranslations();

            float delta = (slow) ? this.SlowZoomDelta : this.ZoomDelta;
            float zooming = (zoomIn) ? +delta : -delta;

            if (zooming != 0f)
            {
                float s = gameTime.ElapsedSeconds;

                Vector3 newPosition = this.Position + (this.Direction * zooming * s);

                float distance = Vector3.Distance(newPosition, this.Interest);
                if (distance < this.ZoomMax && distance > this.ZoomMin)
                {
                    this.Position = newPosition;
                }
            }
        }

        /// <summary>
        /// Starts automatic translations
        /// </summary>
        /// <param name="translation">Translation mode</param>
        /// <param name="newInterest">New interest point</param>
        private void StartTranslations(CameraTranslations translation, Vector3 newInterest)
        {
            this.translationMode = translation;
            this.translationInterest = newInterest;
        }
        /// <summary>
        /// Stops all current automatic translations
        /// </summary>
        private void StopTranslations()
        {
            if (this.translationMode != CameraTranslations.None)
            {
                this.translationMode = CameraTranslations.None;
                this.translationInterest = Vector3.Zero;
            }
        }
        /// <summary>
        /// Performs translation to target
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="newInterest">New interest</param>
        /// <param name="radius">Radius to decelerate</param>
        /// <param name="slow">Slow</param>
        private void UpdateTranslations(GameTime gameTime)
        {
            if (this.translationMode != CameraTranslations.None)
            {
                Vector3 diff = this.translationInterest - this.Interest;
                Vector3 pos = this.Position + diff;
                Vector3 dir = pos - this.Position;

                float distanceToTarget = dir.Length();
                float distanceThisMove = 0f;

                if (this.translationMode == CameraTranslations.UseDelta)
                {
                    distanceThisMove = this.MovementDelta * gameTime.ElapsedSeconds;
                }
                else if (this.translationMode == CameraTranslations.UseSlowDelta)
                {
                    distanceThisMove = this.SlowMovementDelta * gameTime.ElapsedSeconds;
                }
                else if (this.translationMode == CameraTranslations.Quick)
                {
                    distanceThisMove = distanceToTarget * this.translationOutOfRadius;
                }

                Vector3 movingVector;
                if (distanceThisMove >= distanceToTarget)
                {
                    //This movement goes beyond the destination.
                    movingVector = Vector3.Normalize(dir) * distanceToTarget * this.translationIntoRadius;
                }
                else if (distanceToTarget < this.translationRadius)
                {
                    //Into slow radius
                    movingVector = Vector3.Normalize(dir) * distanceThisMove * (distanceToTarget / this.translationRadius);
                }
                else
                {
                    //On flight
                    movingVector = Vector3.Normalize(dir) * distanceThisMove;
                }

                if (movingVector != Vector3.Zero)
                {
                    this.Position += movingVector;
                    this.Interest += movingVector;
                }

                if (Vector3.NearEqual(this.Interest, this.translationInterest, new Vector3(0.1f, 0.1f, 0.1f)))
                {
                    this.StopTranslations();
                }
            }
        }
    }
}
