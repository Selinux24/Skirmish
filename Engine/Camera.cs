using SharpDX;

namespace Engine
{
    /// <summary>
    /// Camera 3D
    /// </summary>
    public class Camera
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
            Camera cam = new Camera();

            cam.mode = CameraModes.Free;
            cam.position = position;
            cam.interest = interest;

            return cam;
        }
        /// <summary>
        /// Creates 2D camera
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="interest">Interest</param>
        /// <param name="up">Up vector</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns new 2D camera</returns>
        public static Camera CreateOrtho(Vector3 position, Vector3 interest, Vector3 up, int width, int height)
        {
            Camera camera = new Camera();

            camera.Position = position;
            camera.Interest = interest;
            camera.Up = up;

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

        #region Free

        /// <summary>
        /// Forward free vector
        /// </summary>
        private Vector3 freeForward
        {
            get
            {
                return Vector3.Normalize(this.Interest - this.Position);
            }
        }
        /// <summary>
        /// Backward free vector
        /// </summary>
        private Vector3 freeBackward
        {
            get
            {
                return -this.freeForward;
            }
        }
        /// <summary>
        /// Left free vector
        /// </summary>
        private Vector3 freeLeft
        {
            get
            {
                return Vector3.Cross(this.freeForward, Vector3.Up);
            }
        }
        /// <summary>
        /// Right free vector
        /// </summary>
        private Vector3 freeRight
        {
            get
            {
                return -this.freeLeft;
            }
        }
        /// <summary>
        /// Up free vector
        /// </summary>
        private Vector3 freeUp
        {
            get
            {
                return Vector3.Cross(this.freeLeft, this.freeForward);
            }
        }
        /// <summary>
        /// Down free vector
        /// </summary>
        private Vector3 freeDown
        {
            get
            {
                return -this.freeUp;
            }
        }

        #endregion

        #region Isometric

        /// <summary>
        /// Isometric axis
        /// </summary>
        private IsometricAxis isometricAxis = IsometricAxis.NW;
        /// <summary>
        /// Forward isometric vector
        /// </summary>
        private Vector3 isoMetricForward = new Vector3(-1f, 0f, -1f);
        /// <summary>
        /// Backward isometric vector
        /// </summary>
        private Vector3 isoMetricBackward = new Vector3(1f, 0f, 1f);
        /// <summary>
        /// Left isometric vector
        /// </summary>
        private Vector3 isoMetricLeft = new Vector3(1f, 0f, -1f);
        /// <summary>
        /// Right isometric vector
        /// </summary>
        private Vector3 isoMetricRight = new Vector3(-1f, 0f, 1f);
        /// <summary>
        /// Up isometric vector
        /// </summary>
        private Vector3 isoMetricUp = new Vector3(0f, 1f, 0f);
        /// <summary>
        /// Down isometric vector
        /// </summary>
        private Vector3 isoMetricDown = new Vector3(0f, -1f, 0f);

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
        private float translationRadius = 10f;
        /// <summary>
        /// Modifier applied to delta out of the radius
        /// </summary>
        private float translationOutOfRadius = 0.2f;
        /// <summary>
        /// Modifier applied to delta into the radius
        /// </summary>
        private float translationIntoRadius = 0.01f;

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
                    this.SetFree(Vector3.Zero, this.ZoomMin * 2f);
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
        /// Up vector
        /// </summary>
        public Vector3 Up { get; set; }
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
        /// Movement delta
        /// </summary>
        public float MovementDelta = 35f;
        /// <summary>
        /// Slow movement delta
        /// </summary>
        public float SlowMovementDelta = 1f;
        /// <summary>
        /// Rotation delta
        /// </summary>
        public float RotationDelta = 0.25f;
        /// <summary>
        /// Slow rotation delta
        /// </summary>
        public float SlowRotationDelta = 0.025f;
        /// <summary>
        /// Maximum zoom value
        /// </summary>
        public float ZoomMax = 200f;
        /// <summary>
        /// Minimum zoom value
        /// </summary>
        public float ZoomMin = 15f;
        /// <summary>
        /// Zoom delta
        /// </summary>
        public float ZoomDelta = 100f;
        /// <summary>
        /// Zoom movement delta
        /// </summary>
        public float SlowZoomDelta = 25f;
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
        public FollowingObject Following { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected Camera()
        {
            this.position = Vector3.One;
            this.interest = Vector3.Zero;
            this.Up = Vector3.Up;

            this.View = Matrix.LookAtLH(
                this.position,
                this.interest,
                Vector3.UnitY);

            this.Projection = Matrix.Identity;
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
        /// Resource disposing
        /// </summary>
        public void Dispose()
        {

        }
        /// <summary>
        /// Updates camera state
        /// </summary>
        public void Update(GameTime gameTime)
        {
            this.UpdateTranslations(gameTime);

            this.View = Matrix.LookAtLH(
                this.Position,
                this.Interest,
                this.Up);

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
                int previous = 0;

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
                int next = 0;

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
            if (this.mode == CameraModes.FreeIsometric)
            {
                this.Move(gameTime, this.isoMetricForward, slow);
            }
            else if (this.mode == CameraModes.Free)
            {
                this.Move(gameTime, this.freeForward, slow);
            }
        }
        /// <summary>
        /// Move backward
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveBackward(GameTime gameTime, bool slow)
        {
            if (this.mode == CameraModes.FreeIsometric)
            {
                this.Move(gameTime, this.isoMetricBackward, slow);
            }
            else if (this.mode == CameraModes.Free)
            {
                this.Move(gameTime, this.freeBackward, slow);
            }
        }
        /// <summary>
        /// Move left
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveLeft(GameTime gameTime, bool slow)
        {
            if (this.mode == CameraModes.FreeIsometric)
            {
                this.Move(gameTime, this.isoMetricLeft, slow);
            }
            else if (this.mode == CameraModes.Free)
            {
                this.Move(gameTime, this.freeLeft, slow);
            }
        }
        /// <summary>
        /// Move right
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveRight(GameTime gameTime, bool slow)
        {
            if (this.mode == CameraModes.FreeIsometric)
            {
                this.Move(gameTime, this.isoMetricRight, slow);
            }
            else if (this.mode == CameraModes.Free)
            {
                this.Move(gameTime, this.freeRight, slow);
            }
        }
        /// <summary>
        /// Rotate up
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateUp(GameTime gameTime, bool slow)
        {
            if (this.mode == CameraModes.Free)
            {
                this.Rotate(gameTime, this.freeLeft, slow);
            }
        }
        /// <summary>
        /// Rotate down
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateDown(GameTime gameTime, bool slow)
        {
            if (this.mode == CameraModes.Free)
            {
                this.Rotate(gameTime, this.freeRight, slow);
            }
        }
        /// <summary>
        /// Rotate left
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateLeft(GameTime gameTime, bool slow)
        {
            if (this.mode == CameraModes.Free)
            {
                this.Rotate(gameTime, this.freeDown, slow);
            }
        }
        /// <summary>
        /// Rotate right
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateRight(GameTime gameTime, bool slow)
        {
            if (this.mode == CameraModes.Free)
            {
                this.Rotate(gameTime, this.freeUp, slow);
            }
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
                this.Rotate(this.freeUp, gameTime.ElapsedSeconds * deltaX * 10f);
            if (deltaY != 0f)
                this.Rotate(this.freeLeft, gameTime.ElapsedSeconds * -deltaY * 10f);
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
        /// <param name="newInterest">Interest point</param>
        /// <param name="distance">Distance to interest point from viewer point</param>
        private void SetFree(Vector3 newInterest, float distance)
        {
            this.StopTranslations();

            Vector3 diff = newInterest - this.Interest;
            this.Interest += diff;
            this.Position += diff;
            this.Position = Vector3.Normalize(this.Position) * distance;

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
            this.Up = Vector3.UnitZ;

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

            Vector3 movingVector = vector * delta * gameTime.ElapsedSeconds;
            if (movingVector != Vector3.Zero)
            {
                this.Position += movingVector;
                this.Interest += movingVector;
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

            this.Rotate(axis, degrees);
        }
        /// <summary>
        /// Rotation
        /// </summary>
        /// <param name="axis">Rotation axis</param>
        /// <param name="degrees">Degrees</param>
        private void Rotate(Vector3 axis, float degrees)
        {
            this.StopTranslations();

            Quaternion r = Quaternion.RotationAxis(axis, MathUtil.DegreesToRadians(degrees));

            Vector3 fw = Vector3.Transform(this.freeForward, r);

            this.Interest = this.position + fw;
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

            float delta = delta = (slow) ? this.SlowZoomDelta : this.ZoomDelta;
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
            this.translationMode = CameraTranslations.None;
            this.translationInterest = Vector3.Zero;
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

                Vector3 movingVector = Vector3.Zero;
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

    /// <summary>
    /// Automatic camera translation modes
    /// </summary>
    public enum CameraTranslations
    {
        /// <summary>
        /// No translation
        /// </summary>
        None,
        /// <summary>
        /// Use current camera movement delta
        /// </summary>
        UseDelta,
        /// <summary>
        /// Use current camera slow movement delta
        /// </summary>
        UseSlowDelta,
        /// <summary>
        /// Quick
        /// </summary>
        Quick,
    }

    public interface FollowingObject
    {
        Vector3 Position { get; }

        Vector3 Interest { get; }
    }

    public class FollowingManipulator : FollowingObject
    {
        private Manipulator3D manipulator;

        private Vector3 positionOffset = Vector3.Zero;
        private Vector3 viewOffset = Vector3.ForwardLH;

        public Vector3 Position
        {
            get
            {
                return Vector3.TransformCoordinate(this.positionOffset, this.manipulator.LocalTransform);
            }
        }

        public Vector3 Interest
        {
            get
            {
                return this.Position + Vector3.TransformNormal(this.viewOffset, this.manipulator.LocalTransform);
            }
        }

        public FollowingManipulator(Manipulator3D manipulator)
        {
            this.manipulator = manipulator;
        }

        public FollowingManipulator(Manipulator3D manipulator, Vector3 position, Vector3 view)
        {
            this.manipulator = manipulator;
            this.positionOffset = position;
            this.viewOffset = view;
        }
    }
}
