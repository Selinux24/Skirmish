using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// 3D Camera
    /// </summary>
    public class Camera : ITransform, IIntersectable, IHasGameState, IDisposable
    {
        /// <summary>
        /// Creates a free camera
        /// </summary>
        /// <param name="position">Viewer position</param>
        /// <param name="interest">Interest point</param>
        /// <param name="width">Viewport Width</param>
        /// <param name="height">Viewport Height</param>
        /// <returns>Returns the new camera</returns>
        public static Camera CreateFree(Vector3 position, Vector3 interest, int width, int height)
        {
            var cam = new Camera();

            cam.SetFree(position, interest, width, height);

            return cam;
        }
        /// <summary>
        /// Creates an isometric camera
        /// </summary>
        /// <param name="axis">Isometric axis</param>
        /// <param name="interest">Interest point</param>
        /// <param name="distance">Distance from viewer to interest point</param>
        /// <param name="width">Viewport Width</param>
        /// <param name="height">Viewport Height</param>
        /// <returns>Returns the new camera</returns>
        public static Camera CreateIsometric(IsometricAxis axis, Vector3 interest, float distance, int width, int height)
        {
            var cam = new Camera();

            cam.SetIsometric(axis, interest, distance, width, height);

            return cam;
        }
        /// <summary>
        /// Creates 2D camera
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="interest">Interest</param>
        /// <param name="width">Viewport Width</param>
        /// <param name="height">Viewport Height</param>
        /// <returns>Returns new 2D camera</returns>
        public static Camera CreateOrtho(Vector3 position, Vector3 interest, int width, int height)
        {
            var cam = new Camera();

            cam.SetOrtho(position, interest, width, height);

            return cam;
        }
        /// <summary>
        /// Creates 2D camera
        /// </summary>
        /// <param name="area">Area of interest (extents of a bounding box)</param>
        /// <param name="nearPlane">Near plane distance</param>
        /// <param name="width">Viewport Width</param>
        /// <param name="height">Viewport Height</param>
        /// <returns>Returns new 2D camera</returns>
        public static Camera CreateOrtho(BoundingBox area, float nearPlane, float width, float height)
        {
            var cam = new Camera();

            cam.SetOrtho(area, nearPlane, width, height);

            return cam;
        }

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
        /// Velocity
        /// </summary>
        private Vector3 velocity;
        /// <summary>
        /// Position
        /// </summary>
        private Vector3 position;
        /// <summary>
        /// Interest
        /// </summary>
        private Vector3 interest;

        /// <summary>
        /// Next position
        /// </summary>
        private Vector3 nextPosition;
        /// <summary>
        /// Next interest
        /// </summary>
        private Vector3 nextInterest;
        /// <summary>
        /// Update needed flag
        /// </summary>
        private bool updateNeeded = false;

        #region Isometric

        /// <summary>
        /// Isometric axis
        /// </summary>
        private IsometricAxis isometricAxis = IsometricAxis.NW;
        /// <summary>
        /// Isometric distance to interest
        /// </summary>
        private float isometricDistanceToInterest = 50f;
        /// <summary>
        /// Isometric current forward
        /// </summary>
        private Vector3 isoMetricForward = new(-1f, 0f, -1f);
        /// <summary>
        /// Isometric current backward
        /// </summary>
        private Vector3 isoMetricBackward = new(1f, 0f, 1f);
        /// <summary>
        /// Isometric current left
        /// </summary>
        private Vector3 isoMetricLeft = new(1f, 0f, -1f);
        /// <summary>
        /// Isometric current right
        /// </summary>
        private Vector3 isoMetricRight = new(-1f, 0f, 1f);

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

        /// <inheritdoc/>
        public Vector3 Velocity
        {
            get
            {
                return velocity;
            }
        }
        /// <inheritdoc/>
        /// <remarks>Point of view</remarks>
        public Vector3 Position
        {
            get
            {
                return position;
            }
        }
        /// <inheritdoc/>
        /// <remarks>Rotation quaternion of direction from up vector</remarks>
        public Quaternion Rotation
        {
            get
            {
                return Helper.RotateFromDirection(Direction, Up);
            }
        }
        /// <inheritdoc/>
        /// <remarks>Scale is always one</remarks>
        public Vector3 Scaling
        {
            get
            {
                return Vector3.One;
            }
        }
        /// <summary>
        /// Camera interest
        /// </summary>
        public Vector3 Interest
        {
            get
            {
                return interest;
            }
        }
        /// <summary>
        /// Camera direction
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                return Vector3.Normalize(interest - position);
            }
        }
        /// <inheritdoc/>
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
                    return Direction;
                }
                else if (mode == CameraModes.FirstPerson || mode == CameraModes.ThirdPerson)
                {
                    var dir = Direction;

                    return Vector3.Normalize(new Vector3(dir.X, 0, dir.Z));
                }
                else
                {
                    return Direction;
                }
            }
        }
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        /// <remarks>The view * projection transform</remarks>
        public Matrix LocalTransform
        {
            get
            {
                return ViewProjection;
            }
        }
        /// <inheritdoc/>
        /// <remarks>The view * projection transform</remarks>
        public Matrix GlobalTransform
        {
            get
            {
                return ViewProjection;
            }
        }
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
                if (mode == value)
                {
                    return;
                }

                mode = value;

                if (mode == CameraModes.FreeIsometric)
                {
                    SetIsometric(isometricAxis, interest, isometricDistanceToInterest);
                }
                else if (mode == CameraModes.Free)
                {
                    SetFree(position, interest);
                }
                else if (mode == CameraModes.Ortho)
                {
                    SetOrtho(position, interest);
                }
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
        /// View * projection matrix
        /// </summary>
        public Matrix ViewProjection { get; private set; }
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

            View = Matrix.LookAtLH(position, interest, Vector3.UnitY);
            Projection = Matrix.Identity;
            ViewProjection = View * Projection;

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
        /// <inheritdoc/>
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
        /// Sets dimensions of viewport
        /// </summary>
        /// <param name="width">Viewport width</param>
        /// <param name="height">Viewport height</param>
        /// <param name="area">Area of interest</param>
        /// <param name="nearPlane">Near plane distance</param>
        public void SetLens(float width, float height, BoundingBox area, float nearPlane)
        {
            aspectRelation = height / width;

            var extents = area.Size;
            viewportWidth = extents.X / aspectRelation;
            viewportHeight = extents.Z;
            nearPlaneDistance = nearPlane;
            farPlaneDistance = extents.Y + nearPlane;

            UpdateLens();
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
        /// Updates camera state
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (Following != null)
            {
                Following.Update(gameTime);
                nextPosition = Following.Position;
                nextInterest = Following.Interest;
            }

            UpdateTranslations(gameTime);

            if (updateNeeded)
            {
                velocity = Vector3.Zero;
                position = nextPosition;
                interest = nextInterest;
                updateNeeded = false;
            }

            var upVector = mode == CameraModes.Ortho ? Vector3.UnitZ : Vector3.UnitY;
            View = Matrix.LookAtLH(position, interest, upVector);

            ViewProjection = View * Projection;

            Frustum = new BoundingFrustum(ViewProjection);
        }
        /// <summary>
        /// Performs translation to target
        /// </summary>
        /// <param name="gameTime">Game time</param>
        private void UpdateTranslations(GameTime gameTime)
        {
            if (translationMode == CameraTranslations.None)
            {
                return;
            }

            var diff = translationInterest - nextInterest;
            var pos = nextPosition + diff;
            var dir = pos - nextPosition;

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
                nextPosition += movingVector;
                nextInterest += movingVector;

                updateNeeded = true;
            }

            if (Vector3.NearEqual(nextInterest, translationInterest, new Vector3(0.1f, 0.1f, 0.1f)))
            {
                StopTranslations();
            }
        }

        /// <summary>
        /// Sets previous isometrix axis
        /// </summary>
        public void PreviousIsometricAxis()
        {
            if (mode != CameraModes.FreeIsometric)
            {
                return;
            }

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

            SetIsometric((IsometricAxis)previous, interest, Vector3.Distance(position, interest));
        }
        /// <summary>
        /// Sets next isometrix axis
        /// </summary>
        public void NextIsometricAxis()
        {
            if (mode != CameraModes.FreeIsometric)
            {
                return;
            }

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

            SetIsometric((IsometricAxis)next, interest, Vector3.Distance(position, interest));
        }

        /// <summary>
        /// Sets the camera position
        /// </summary>
        /// <param name="position">Position</param>
        public void SetPosition(Vector3 position)
        {
            nextPosition = position;
            updateNeeded = true;
        }
        /// <summary>
        /// Sets the camera point of interest
        /// </summary>
        /// <param name="interest">Point of interest</param>
        public void SetInterest(Vector3 interest)
        {
            nextInterest = interest;
            updateNeeded = true;
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
        /// Move up
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveUp(GameTime gameTime, bool slow)
        {
            Move(gameTime, Up, slow);
        }
        /// <summary>
        /// Move down
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="slow">Slow movement</param>
        public void MoveDown(GameTime gameTime, bool slow)
        {
            Move(gameTime, Down, slow);
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
        /// Creates a free camera
        /// </summary>
        /// <param name="position">Viewer position</param>
        /// <param name="interest">Interest point</param>
        /// <param name="width">Viewport Width</param>
        /// <param name="height">Viewport Height</param>
        /// <returns>Returns the new camera</returns>
        public void SetFree(Vector3 position, Vector3 interest, int width, int height)
        {
            SetFree(position, interest);
            SetLens(width, height);
        }
        /// <summary>
        /// Sets camera to free mode
        /// </summary>
        /// <param name="newPosition">New position</param>
        /// <param name="newInterest">New interest point</param>
        private void SetFree(Vector3 newPosition, Vector3 newInterest)
        {
            StopTranslations();

            nextPosition = newPosition;
            nextInterest = newInterest;
            updateNeeded = true;

            mode = CameraModes.Free;
        }
        /// <summary>
        /// Creates an isometric camera
        /// </summary>
        /// <param name="axis">Isometric axis</param>
        /// <param name="interest">Interest point</param>
        /// <param name="distance">Distance from viewer to interest point</param>
        /// <param name="width">Viewport Width</param>
        /// <param name="height">Viewport Height</param>
        /// <returns>Returns the new camera</returns>
        public void SetIsometric(IsometricAxis axis, Vector3 interest, float distance, int width, int height)
        {
            SetIsometric(axis, interest, distance);
            SetLens(width, height);
        }
        /// <summary>
        /// Sets camera to isometric mode
        /// </summary>
        /// <param name="axis">Isometrix axis</param>
        /// <param name="newInterest">Interest point</param>
        /// <param name="distanceToInterest">Distance to interest point from viewer point</param>
        private void SetIsometric(IsometricAxis axis, Vector3 newInterest, float distanceToInterest)
        {
            StopTranslations();

            Vector3 camPosition = Vector3.Zero;

            if (axis == IsometricAxis.NW)
            {
                camPosition = new Vector3(1, 1, 1);
                isoMetricForward = new Vector3(-1f, 0f, -1f);
                isoMetricBackward = new Vector3(1f, 0f, 1f);
                isoMetricLeft = new Vector3(1f, 0f, -1f);
                isoMetricRight = new Vector3(-1f, 0f, 1f);
            }
            else if (axis == IsometricAxis.NE)
            {
                camPosition = new Vector3(-1, 1, 1);
                isoMetricForward = new Vector3(1f, 0f, -1f);
                isoMetricBackward = new Vector3(-1f, 0f, 1f);
                isoMetricLeft = new Vector3(1f, 0f, 1f);
                isoMetricRight = new Vector3(-1f, 0f, -1f);
            }
            else if (axis == IsometricAxis.SW)
            {
                camPosition = new Vector3(1, 1, -1);
                isoMetricForward = new Vector3(-1f, 0f, 1f);
                isoMetricBackward = new Vector3(1f, 0f, -1f);
                isoMetricLeft = new Vector3(-1f, 0f, -1f);
                isoMetricRight = new Vector3(1f, 0f, 1f);
            }
            else if (axis == IsometricAxis.SE)
            {
                camPosition = new Vector3(-1, 1, -1);
                isoMetricForward = new Vector3(1f, 0f, 1f);
                isoMetricBackward = new Vector3(-1f, 0f, -1f);
                isoMetricLeft = new Vector3(-1f, 0f, 1f);
                isoMetricRight = new Vector3(1f, 0f, -1f);
            }

            mode = CameraModes.FreeIsometric;
            isometricAxis = axis;
            isometricDistanceToInterest = distanceToInterest;

            nextPosition = Vector3.Normalize(camPosition) * isometricDistanceToInterest;
            nextPosition += newInterest;
            nextInterest = newInterest;
            updateNeeded = true;
        }
        /// <summary>
        /// Creates 2D camera
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="interest">Interest</param>
        /// <param name="width">Viewport Width</param>
        /// <param name="height">Viewport Height</param>
        /// <returns>Returns new 2D camera</returns>
        public void SetOrtho(Vector3 position, Vector3 interest, int width, int height)
        {
            SetOrtho(position, interest);
            SetLens(width, height);
        }
        /// <summary>
        /// Creates 2D camera
        /// </summary>
        /// <param name="area">Area of interest (extents of a bounding box)</param>
        /// <param name="nearPlane">Near plane distance</param>
        /// <param name="width">Viewport Width</param>
        /// <param name="height">Viewport Height</param>
        /// <returns>Returns new 2D camera</returns>
        public void SetOrtho(BoundingBox area, float nearPlane, float width, float height)
        {
            //Sets the position over the maximum area height, plus the near plane distance
            var eyePosition = new Vector3(0, area.Size.Y + nearPlane, 0);
            var eyeInterest = Vector3.Zero;

            SetOrtho(eyePosition, eyeInterest);
            SetLens(width, height, area, nearPlane);
        }
        /// <summary>
        /// Sets camero to ortho mode
        /// </summary>
        /// <param name="newPosition">New position</param>
        /// <param name="newInterest">New interest point</param>
        private void SetOrtho(Vector3 newPosition, Vector3 newInterest)
        {
            StopTranslations();

            nextPosition = newPosition;
            nextInterest = newInterest;
            updateNeeded = true;

            mode = CameraModes.Ortho;
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
            Vector3 diff = newPosition - nextPosition;

            if (translation != CameraTranslations.None)
            {
                StartTranslations(translation, nextInterest + diff);
            }
            else
            {
                StopTranslations();

                nextPosition += diff;
                nextInterest += diff;
                updateNeeded = true;
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
                    var diff = newInterest - nextInterest;

                    nextPosition += diff;
                    nextInterest += diff;
                    updateNeeded = true;
                }
                else
                {
                    nextInterest = newInterest;
                    updateNeeded = true;
                }
            }
        }
        /// <summary>
        /// Movement
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="vector">Movement vector</param>
        /// <param name="slow">Slow movement</param>
        public void Move(GameTime gameTime, Vector3 vector, bool slow)
        {
            StopTranslations();

            float delta = slow ? SlowMovementDelta : MovementDelta;

            var nextVelocity = vector * delta * gameTime.ElapsedSeconds;
            if (nextVelocity != Vector3.Zero)
            {
                nextPosition += nextVelocity;
                nextInterest += nextVelocity;
            }
            updateNeeded = true;
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

            Vector3 curDir = Vector3.Normalize(nextInterest - nextPosition);
            Vector3 newDir = Vector3.Transform(curDir, r);

            if (clampY)
            {
                float newAngle = Helper.Angle(Vector3.Up, newDir) - MathUtil.PiOverTwo;
                if (newAngle >= MathUtil.DegreesToRadians(clampFrom) && newAngle <= MathUtil.DegreesToRadians(clampTo))
                {
                    nextInterest = nextPosition + newDir;
                    updateNeeded = true;
                }
            }
            else
            {
                nextInterest = nextPosition + newDir;
                updateNeeded = true;
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

            float delta = slow ? SlowZoomDelta : ZoomDelta;
            float zooming = zoomIn ? +delta : -delta;

            if (zooming != 0f)
            {
                float s = gameTime.ElapsedSeconds;

                var dir = Vector3.Normalize(nextInterest - nextPosition);
                Vector3 newPosition = nextPosition + (dir * zooming * s);

                float distance = Vector3.Distance(newPosition, nextInterest);
                if (distance < ZoomMax && distance > ZoomMin)
                {
                    nextPosition = newPosition;
                    updateNeeded = true;
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
            translationMode = CameraTranslations.None;
            translationInterest = Vector3.Zero;
        }

        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectable other, IntersectDetectionMode detectionModeOther)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, other, detectionModeOther);
        }
        /// <inheritdoc/>
        public bool Intersects(IntersectDetectionMode detectionModeThis, ICullingVolume volume)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, volume);
        }
        /// <inheritdoc/>
        public ICullingVolume GetIntersectionVolume(IntersectDetectionMode detectionMode)
        {
            return detectionMode switch
            {
                IntersectDetectionMode.Sphere => (IntersectionVolumeSphere)BoundingSphere.FromPoints(Frustum.GetCorners()),
                IntersectDetectionMode.Box => (IntersectionVolumeAxisAlignedBox)BoundingBox.FromPoints(Frustum.GetCorners()),
                _ => (IntersectionVolumeFrustum)Frustum
            };
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new CameraState
            {
                Position = position,
                Interest = interest,
                FieldOfView = fieldOfView,
                NearPlaneDistance = nearPlaneDistance,
                FarPlaneDistance = farPlaneDistance,
                AspectRelation = aspectRelation,
                ViewportWidth = viewportWidth,
                ViewportHeight = viewportHeight,
                Mode = mode,
                Velocity = velocity,
                IsometricAxis = isometricAxis,
                IsometricDistanceToInterest = isometricDistanceToInterest,
                IsoMetricForward = isoMetricForward,
                IsoMetricBackward = isoMetricBackward,
                IsoMetricLeft = isoMetricLeft,
                IsoMetricRight = isoMetricRight,
                TranslationMode = translationMode,
                TranslationInterest = translationInterest,
                MovementDelta = MovementDelta,
                SlowMovementDelta = SlowMovementDelta,
                RotationDelta = RotationDelta,
                SlowRotationDelta = SlowRotationDelta,
                ZoomMax = ZoomMax,
                ZoomMin = ZoomMin,
                ZoomDelta = ZoomDelta,
                SlowZoomDelta = SlowZoomDelta,
                View = View,
                Projection = Projection,
                Frustum = Frustum.Matrix,
                Following = -1,
                InvertY = InvertY,
                CameraRadius = CameraRadius,
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (state is not CameraState cameraState)
            {
                return;
            }

            position = cameraState.Position;
            interest = cameraState.Interest;
            fieldOfView = cameraState.FieldOfView;
            nearPlaneDistance = cameraState.NearPlaneDistance;
            farPlaneDistance = cameraState.FarPlaneDistance;
            aspectRelation = cameraState.AspectRelation;
            viewportWidth = cameraState.ViewportWidth;
            viewportHeight = cameraState.ViewportHeight;
            mode = cameraState.Mode;
            velocity = cameraState.Velocity;
            isometricAxis = cameraState.IsometricAxis;
            isometricDistanceToInterest = cameraState.IsometricDistanceToInterest;
            isoMetricForward = cameraState.IsoMetricForward;
            isoMetricBackward = cameraState.IsoMetricBackward;
            isoMetricLeft = cameraState.IsoMetricLeft;
            isoMetricRight = cameraState.IsoMetricRight;
            translationMode = cameraState.TranslationMode;
            translationInterest = cameraState.TranslationInterest;
            MovementDelta = cameraState.MovementDelta;
            SlowMovementDelta = cameraState.SlowMovementDelta;
            RotationDelta = cameraState.RotationDelta;
            SlowRotationDelta = cameraState.SlowRotationDelta;
            ZoomMax = cameraState.ZoomMax;
            ZoomMin = cameraState.ZoomMin;
            ZoomDelta = cameraState.ZoomDelta;
            SlowZoomDelta = cameraState.SlowZoomDelta;
            View = cameraState.View;
            Projection = cameraState.Projection;
            ViewProjection = cameraState.View * cameraState.Projection;
            Frustum = new BoundingFrustum(cameraState.Frustum);
            Following = null;
            InvertY = cameraState.InvertY;
            CameraRadius = cameraState.CameraRadius;
        }
    }
}
