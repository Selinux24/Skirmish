using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Camera 3D
    /// </summary>
    public class Camera : IManipulator, IIntersectable, IDisposable
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
                if (mode == CameraModes.FreeIsometric)
                {
                    return isoMetricForward;
                }
                else if (mode == CameraModes.Free)
                {
                    return Vector3.Normalize(Interest - Position);
                }
                else if (mode == CameraModes.FirstPerson || mode == CameraModes.ThirdPerson)
                {
                    var v = Interest - Position;

                    return Vector3.Normalize(new Vector3(v.X, 0, v.Z));
                }
                else
                {
                    return Vector3.Normalize(Interest - Position);
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
                if (mode == CameraModes.FreeIsometric)
                {
                    return isoMetricBackward;
                }
                else if (mode == CameraModes.Free)
                {
                    return -Forward;
                }
                else if (mode == CameraModes.FirstPerson || mode == CameraModes.ThirdPerson)
                {
                    return -Forward;
                }
                else
                {
                    return -Forward;
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
                if (mode == CameraModes.FreeIsometric)
                {
                    return isoMetricLeft;
                }
                else if (mode == CameraModes.Free)
                {
                    return Vector3.Cross(Forward, Vector3.Up);
                }
                else if (mode == CameraModes.FirstPerson || mode == CameraModes.ThirdPerson)
                {
                    return Vector3.Cross(Forward, Vector3.Up);
                }
                else
                {
                    return Vector3.Cross(Forward, Vector3.Up);
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
                if (mode == CameraModes.FreeIsometric)
                {
                    return isoMetricRight;
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
                if (mode == CameraModes.FreeIsometric)
                {
                    return new Vector3(0f, 1f, 0f);
                }
                else if (mode == CameraModes.Free)
                {
                    return Vector3.Cross(Left, Forward);
                }
                else if (mode == CameraModes.FirstPerson || mode == CameraModes.ThirdPerson)
                {
                    return Vector3.Up;
                }
                else
                {
                    return Vector3.Cross(Left, Forward);
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
                if (mode == CameraModes.FreeIsometric)
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
                return mode;
            }
            set
            {
                mode = value;

                if (mode == CameraModes.FreeIsometric)
                {
                    SetIsometric(IsometricAxis.SE, Vector3.Zero, ZoomMin * 2f);
                }
                else if (mode == CameraModes.Free)
                {
                    SetFree(position, interest);
                }
                else if (mode == CameraModes.Ortho)
                {
                    SetOrtho();
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
                return Following?.Position ?? position;
            }
            set
            {
                position = value;
            }
        }
        /// <summary>
        /// Camera interest
        /// </summary>
        public Vector3 Interest
        {
            get
            {
                return Following?.Interest ?? interest;
            }
            set
            {
                interest = value;
            }
        }
        /// <summary>
        /// Camera direction
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                return Vector3.Normalize(Interest - Position);
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
                return fieldOfView;
            }
            set
            {
                fieldOfView = value;

                UpdateLens();
            }
        }
        /// <summary>
        /// Gets or sets near plane distance
        /// </summary>
        public float NearPlaneDistance
        {
            get
            {
                return nearPlaneDistance;
            }
            set
            {
                nearPlaneDistance = value;

                UpdateLens();
            }
        }
        /// <summary>
        /// Gets or sets far plane distance
        /// </summary>
        public float FarPlaneDistance
        {
            get
            {
                return farPlaneDistance;
            }
            set
            {
                farPlaneDistance = value;

                UpdateLens();
            }
        }
        /// <summary>
        /// Gets or sets aspect relation
        /// </summary>
        public float AspectRelation
        {
            get
            {
                return aspectRelation;
            }
            set
            {
                aspectRelation = value;

                UpdateLens();
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
        /// Gets or sets the camera radius, for collision detection
        /// </summary>
        public float CameraRadius { get; set; } = 1f;

        /// <summary>
        /// Constructor
        /// </summary>
        protected Camera()
        {
            position = Vector3.One;
            interest = Vector3.Zero;

            View = Matrix.LookAtLH(
                position,
                interest,
                Vector3.UnitY);

            Projection = Matrix.Identity;

            InvertY = false;
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
            viewportWidth = width;
            viewportHeight = height;

            aspectRelation = viewportWidth / viewportHeight;

            UpdateLens();
        }
        /// <summary>
        /// Updates camera state
        /// </summary>
        public void Update(GameTime gameTime)
        {
            Velocity = Vector3.Zero;

            Following?.Update(gameTime);

            UpdateTranslations(gameTime);

            if (mode == CameraModes.Ortho)
            {
                View = Matrix.LookAtLH(
                    Position,
                    Interest,
                    Vector3.UnitZ);
            }
            else
            {
                View = Matrix.LookAtLH(
                    Position,
                    Interest,
                    Vector3.UnitY);
            }

            Frustum = new BoundingFrustum(View * Projection);
        }
        /// <summary>
        /// Sets previous isometrix axis
        /// </summary>
        public void PreviousIsometricAxis()
        {
            if (mode == CameraModes.FreeIsometric)
            {
                int current = (int)isometricAxis;
                int previous;

                if (current <= 0)
                {
                    previous = 3;
                }
                else
                {
                    previous = current - 1;
                }

                SetIsometric((IsometricAxis)previous, Interest, Vector3.Distance(Position, Interest));
            }
        }
        /// <summary>
        /// Sets next isometrix axis
        /// </summary>
        public void NextIsometricAxis()
        {
            if (mode == CameraModes.FreeIsometric)
            {
                int current = (int)isometricAxis;
                int next;

                if (current >= 3)
                {
                    next = 0;
                }
                else
                {
                    next = current + 1;
                }

                SetIsometric((IsometricAxis)next, Interest, Vector3.Distance(Position, Interest));
            }
        }
        /// <summary>
        /// Move forward
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveForward(GameTime gameTime, bool slow)
        {
            Move(gameTime, Forward, slow);
        }
        /// <summary>
        /// Move backward
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveBackward(GameTime gameTime, bool slow)
        {
            Move(gameTime, Backward, slow);
        }
        /// <summary>
        /// Move left
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveLeft(GameTime gameTime, bool slow)
        {
            Move(gameTime, Left, slow);
        }
        /// <summary>
        /// Move right
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveRight(GameTime gameTime, bool slow)
        {
            Move(gameTime, Right, slow);
        }
        /// <summary>
        /// Rotate up
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateUp(GameTime gameTime, bool slow)
        {
            Rotate(gameTime, Left, slow);
        }
        /// <summary>
        /// Rotate down
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateDown(GameTime gameTime, bool slow)
        {
            Rotate(gameTime, Right, slow);
        }
        /// <summary>
        /// Rotate left
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateLeft(GameTime gameTime, bool slow)
        {
            Rotate(gameTime, Down, slow);
        }
        /// <summary>
        /// Rotate right
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void RotateRight(GameTime gameTime, bool slow)
        {
            Rotate(gameTime, Up, slow);
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
                Rotate(Up, gameTime.ElapsedSeconds * deltaX * 10f);
            }
            if (deltaY != 0f)
            {
                if (InvertY) deltaY = -deltaY;

                Rotate(Left, gameTime.ElapsedSeconds * -deltaY * 10f, true, -85, 85);
            }
        }
        /// <summary>
        /// Zoom in
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void ZoomIn(GameTime gameTime, bool slow)
        {
            Zoom(gameTime, true, slow);
        }
        /// <summary>
        /// Zoom out
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void ZoomOut(GameTime gameTime, bool slow)
        {
            Zoom(gameTime, false, slow);
        }

        /// <summary>
        /// Sets camera to free mode
        /// </summary>
        /// <param name="newPosition">New position</param>
        /// <param name="newInterest">New interest point</param>
        private void SetFree(Vector3 newPosition, Vector3 newInterest)
        {
            StopTranslations();

            Position = newPosition;
            Interest = newInterest;

            mode = CameraModes.Free;
        }
        /// <summary>
        /// Sets camera to isometric mode
        /// </summary>
        /// <param name="axis">Isometrix axis</param>
        /// <param name="newInterest">Interest point</param>
        /// <param name="distance">Distance to interest point from viewer point</param>
        private void SetIsometric(IsometricAxis axis, Vector3 newInterest, float distance)
        {
            StopTranslations();

            Vector3 tmpPosition = Vector3.Zero;

            if (axis == IsometricAxis.NW)
            {
                tmpPosition = new Vector3(1, 1, 1);
                isoMetricForward = new Vector3(-1f, 0f, -1f);
                isoMetricBackward = new Vector3(1f, 0f, 1f);
                isoMetricLeft = new Vector3(1f, 0f, -1f);
                isoMetricRight = new Vector3(-1f, 0f, 1f);
            }
            else if (axis == IsometricAxis.NE)
            {
                tmpPosition = new Vector3(-1, 1, 1);
                isoMetricForward = new Vector3(1f, 0f, -1f);
                isoMetricBackward = new Vector3(-1f, 0f, 1f);
                isoMetricLeft = new Vector3(1f, 0f, 1f);
                isoMetricRight = new Vector3(-1f, 0f, -1f);
            }
            else if (axis == IsometricAxis.SW)
            {
                tmpPosition = new Vector3(1, 1, -1);
                isoMetricForward = new Vector3(-1f, 0f, 1f);
                isoMetricBackward = new Vector3(1f, 0f, -1f);
                isoMetricLeft = new Vector3(-1f, 0f, -1f);
                isoMetricRight = new Vector3(1f, 0f, 1f);
            }
            else if (axis == IsometricAxis.SE)
            {
                tmpPosition = new Vector3(-1, 1, -1);
                isoMetricForward = new Vector3(1f, 0f, 1f);
                isoMetricBackward = new Vector3(-1f, 0f, -1f);
                isoMetricLeft = new Vector3(-1f, 0f, 1f);
                isoMetricRight = new Vector3(1f, 0f, -1f);
            }

            mode = CameraModes.FreeIsometric;
            isometricAxis = axis;

            Position = Vector3.Normalize(tmpPosition) * distance;
            Position += newInterest;
            Interest = newInterest;
        }
        /// <summary>
        /// Sets camero to ortho mode
        /// </summary>
        private void SetOrtho()
        {
            StopTranslations();

            Position = Vector3.Up;
            Interest = Vector3.Zero;

            mode = CameraModes.Ortho;
        }
        /// <summary>
        /// Update projections
        /// </summary>
        private void UpdateLens()
        {
            if (mode == CameraModes.Ortho)
            {
                Projection = Matrix.OrthoLH(
                    viewportWidth,
                    viewportHeight,
                    nearPlaneDistance,
                    farPlaneDistance);
            }
            else
            {
                Projection = Matrix.PerspectiveFovLH(
                    fieldOfView,
                    aspectRelation,
                    nearPlaneDistance,
                    farPlaneDistance);
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
            Goto(new Vector3(x, y, z), translation);
        }
        /// <summary>
        /// Move camera to position
        /// </summary>
        /// <param name="newPosition">New position</param>
        /// <param name="translation">Translation mode</param>
        public void Goto(Vector3 newPosition, CameraTranslations translation = CameraTranslations.None)
        {
            Vector3 diff = newPosition - Position;

            if (translation != CameraTranslations.None)
            {
                StartTranslations(translation, Interest + diff);
            }
            else
            {
                StopTranslations();

                Position += diff;
                Interest += diff;
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
            LookTo(new Vector3(x, y, z), translation);
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
                StartTranslations(translation, newInterest);
            }
            else
            {
                StopTranslations();

                if (mode == CameraModes.FreeIsometric)
                {
                    Vector3 diff = newInterest - Interest;

                    Position += diff;
                    Interest += diff;
                }
                else
                {
                    Interest = newInterest;
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
            StopTranslations();

            float delta = (slow) ? SlowMovementDelta : MovementDelta;

            Velocity = vector * delta * gameTime.ElapsedSeconds;
            if (Velocity != Vector3.Zero)
            {
                Position += Velocity;
                Interest += Velocity;
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
            float degrees = (slow) ? SlowRotationDelta : RotationDelta;

            Rotate(axis, degrees * gameTime.ElapsedSeconds);
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
            StopTranslations();

            //Smooth rotation
            Quaternion sourceRotation = Quaternion.RotationAxis(axis, 0);
            Quaternion targetRotation = Quaternion.RotationAxis(axis, MathUtil.DegreesToRadians(degrees));
            Quaternion r = Quaternion.Lerp(sourceRotation, targetRotation, 0.5f);

            Vector3 curDir = Vector3.Normalize(Interest - Position);
            Vector3 newDir = Vector3.Transform(curDir, r);

            if (clampY)
            {
                float newAngle = Helper.Angle(Vector3.Up, newDir) - MathUtil.PiOverTwo;
                if (newAngle >= MathUtil.DegreesToRadians(clampFrom) && newAngle <= MathUtil.DegreesToRadians(clampTo))
                {
                    Interest = position + newDir;
                }
            }
            else
            {
                Interest = position + newDir;
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
            StopTranslations();

            float delta = (slow) ? SlowZoomDelta : ZoomDelta;
            float zooming = (zoomIn) ? +delta : -delta;

            if (zooming != 0f)
            {
                float s = gameTime.ElapsedSeconds;

                Vector3 newPosition = Position + (Direction * zooming * s);

                float distance = Vector3.Distance(newPosition, Interest);
                if (distance < ZoomMax && distance > ZoomMin)
                {
                    Position = newPosition;
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
            translationMode = translation;
            translationInterest = newInterest;
        }
        /// <summary>
        /// Stops all current automatic translations
        /// </summary>
        private void StopTranslations()
        {
            if (translationMode != CameraTranslations.None)
            {
                translationMode = CameraTranslations.None;
                translationInterest = Vector3.Zero;
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
            if (translationMode != CameraTranslations.None)
            {
                Vector3 diff = translationInterest - Interest;
                Vector3 pos = Position + diff;
                Vector3 dir = pos - Position;

                float distanceToTarget = dir.Length();
                float distanceThisMove = 0f;

                if (translationMode == CameraTranslations.UseDelta)
                {
                    distanceThisMove = MovementDelta * gameTime.ElapsedSeconds;
                }
                else if (translationMode == CameraTranslations.UseSlowDelta)
                {
                    distanceThisMove = SlowMovementDelta * gameTime.ElapsedSeconds;
                }
                else if (translationMode == CameraTranslations.Quick)
                {
                    distanceThisMove = distanceToTarget * translationOutOfRadius;
                }

                Vector3 movingVector;
                if (distanceThisMove >= distanceToTarget)
                {
                    //This movement goes beyond the destination.
                    movingVector = Vector3.Normalize(dir) * distanceToTarget * translationIntoRadius;
                }
                else if (distanceToTarget < translationRadius)
                {
                    //Into slow radius
                    movingVector = Vector3.Normalize(dir) * distanceThisMove * (distanceToTarget / translationRadius);
                }
                else
                {
                    //On flight
                    movingVector = Vector3.Normalize(dir) * distanceThisMove;
                }

                if (movingVector != Vector3.Zero)
                {
                    Position += movingVector;
                    Interest += movingVector;
                }

                if (Vector3.NearEqual(Interest, translationInterest, new Vector3(0.1f, 0.1f, 0.1f)))
                {
                    StopTranslations();
                }
            }
        }

        /// <summary>
        /// Gets whether the sphere intersects with the current object
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="result">Picking results</param>
        /// <returns>Returns true if intersects</returns>
        public bool Intersects(IntersectionVolumeSphere sphere, out PickingResult<Triangle> result)
        {
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            var bsph = new BoundingSphere(position, Math.Max(1f, CameraRadius));
            if (bsph.Intersects(sphere))
            {
                float distance = Vector3.Distance(position, sphere.Position);

                result.Distance = distance;
                result.Position = Vector3.Normalize(position - sphere.Position) * distance * 0.5f;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets whether the actual object have intersection with the intersectable or not
        /// </summary>
        /// <param name="detectionModeThis">Detection mode for this object</param>
        /// <param name="other">Other intersectable</param>
        /// <param name="detectionModeOther">Detection mode for the other object</param>
        /// <returns>Returns true if have intersection</returns>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectable other, IntersectDetectionMode detectionModeOther)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, other, detectionModeOther);
        }
        /// <summary>
        /// Gets whether the actual object have intersection with the volume or not
        /// </summary>
        /// <param name="detectionModeThis">Detection mode for this object</param>
        /// <param name="volume">Volume</param>
        /// <returns>Returns true if have intersection</returns>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectionVolume volume)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, volume);
        }

        /// <summary>
        /// Gets the intersection volume based on the specified detection mode
        /// </summary>
        /// <param name="detectionMode">Detection mode</param>
        /// <returns>Returns an intersection volume</returns>
        public IIntersectionVolume GetIntersectionVolume(IntersectDetectionMode detectionMode)
        {
            if (detectionMode == IntersectDetectionMode.Sphere)
            {
                var bsph = new BoundingSphere(position, Math.Max(1f, CameraRadius));

                return (IntersectionVolumeSphere)bsph;
            }
            else
            {
                return (IntersectionVolumeFrustum)Frustum;
            }
        }
    }
}
