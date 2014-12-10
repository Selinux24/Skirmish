using System;
using SharpDX;

namespace Engine
{
    /// <summary>
    /// Cámara
    /// </summary>
    public class Camera : IDisposable
    {
        /// <summary>
        /// Crea una cámara isométrica
        /// </summary>
        /// <param name="game">Juego</param>
        /// <param name="axis">Eje isométrico</param>
        /// <param name="interest">Punto de interés</param>
        /// <param name="distance">Distancia al punto de interés</param>
        /// <returns>Devuelve la cámara creada</returns>
        public static Camera CreateIsometric(IsometricAxis axis, Vector3 interest, float distance)
        {
            Camera cam = new Camera();

            cam.SetIsometric(axis, interest, distance);

            return cam;
        }
        /// <summary>
        /// Crea una cámara libre
        /// </summary>
        /// <param name="position">Posición</param>
        /// <param name="interest">Punto de vista</param>
        /// <returns>Devuelve la cámara creada</returns>
        public static Camera CreateFree(Vector3 position, Vector3 interest)
        {
            Camera cam = new Camera();

            cam.mode = CameraModes.Free;
            cam.Position = position;
            cam.Interest = interest;

            return cam;
        }

        /// <summary>
        /// Campo de visión
        /// </summary>
        private float fieldOfView = MathUtil.PiOverFour;
        /// <summary>
        /// Distancia al plano cercano
        /// </summary>
        private float nearPlaneDistance = 1f;
        /// <summary>
        /// Distancia al plano lejano
        /// </summary>
        private float farPlaneDistance = 100f;
        /// <summary>
        /// Relación de aspecto
        /// </summary>
        private float aspectRelation = 1f;
        /// <summary>
        /// Anchura de la pantalla
        /// </summary>
        private float viewportWidth = 0f;
        /// <summary>
        /// Altura de la pantalla
        /// </summary>
        private float viewportHeight = 0f;
        /// <summary>
        /// Modo de funcionamiento de la cámara
        /// </summary>
        private CameraModes mode;

        #region Free

        /// <summary>
        /// Vector isométrico avanzar cámara
        /// </summary>
        private Vector3 freeForward
        {
            get
            {
                return Vector3.Normalize(this.Interest - this.Position);
            }
        }
        /// <summary>
        /// Vector isométrico retroceder cámara
        /// </summary>
        private Vector3 freeBackward
        {
            get
            {
                return -this.freeForward;
            }
        }
        /// <summary>
        /// Vector isométrico desplazar izquierda
        /// </summary>
        private Vector3 freeLeft
        {
            get
            {
                return Vector3.Cross(this.freeForward, Vector3.Up);
            }
        }
        /// <summary>
        /// Vector isométrico desplazar derecha
        /// </summary>
        private Vector3 freeRight
        {
            get
            {
                return -this.freeLeft;
            }
        }
        /// <summary>
        /// Vector isométrico desplazar izquierda
        /// </summary>
        private Vector3 freeUp
        {
            get
            {
                return Vector3.Cross(this.freeLeft, this.freeForward);
            }
        }
        /// <summary>
        /// Vector isométrico desplazar derecha
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
        /// Eje isométrico
        /// </summary>
        private IsometricAxis isometricAxis = IsometricAxis.NE;
        /// <summary>
        /// Vector isométrico avanzar cámara
        /// </summary>
        private Vector3 isoMetricForward = new Vector3(-1f, 0f, -1f);
        /// <summary>
        /// Vector isométrico retroceder cámara
        /// </summary>
        private Vector3 isoMetricBackward = new Vector3(1f, 0f, 1f);
        /// <summary>
        /// Vector isométrico desplazar izquierda
        /// </summary>
        private Vector3 isoMetricLeft = new Vector3(1f, 0f, -1f);
        /// <summary>
        /// Vector isométrico desplazar derecha
        /// </summary>
        private Vector3 isoMetricRight = new Vector3(-1f, 0f, 1f);

        #endregion

        /// <summary>
        /// Modo de funcionamiento de la cámara
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
            }
        }
        /// <summary>
        /// Posición
        /// </summary>
        public Vector3 Position = new Vector3(1f);
        /// <summary>
        /// Punto de interés
        /// </summary>
        public Vector3 Interest = Vector3.Zero;
        /// <summary>
        /// Dirección
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                return Vector3.Normalize(this.Position - this.Interest);
            }
        }
        /// <summary>
        /// Variación de movimiento
        /// </summary>
        public float MovementDelta = 35f;
        /// <summary>
        /// Variación de movimiento lento
        /// </summary>
        public float SlowMovementDelta = 1f;
        /// <summary>
        /// Variación de rotación
        /// </summary>
        public float RotationDelta = 0.25f;
        /// <summary>
        /// Variación de rotación lenta
        /// </summary>
        public float SlowRotationDelta = 0.025f;
        /// <summary>
        /// Valor máximo del zoom
        /// </summary>
        public float ZoomMax = 200f;
        /// <summary>
        /// Valor mínimo del zoom
        /// </summary>
        public float ZoomMin = 15f;
        /// <summary>
        /// Campo de visión
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
        /// Distancia al plano cercano
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
        /// Distancia al plano lejano
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
        /// Relación de aspecto
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
        /// Matriz vista
        /// </summary>
        public Matrix PerspectiveView { get; private set; }
        /// <summary>
        /// Matriz proyección
        /// </summary>
        public Matrix PerspectiveProjection { get; private set; }
        /// <summary>
        /// Matriz vista
        /// </summary>
        public Matrix OrthoView { get; private set; }
        /// <summary>
        /// Matriz proyección ortométrica
        /// </summary>
        public Matrix OrthoProjection { get; private set; }
        /// <summary>
        /// Cono de visión de la cámara
        /// </summary>
        public BoundingFrustum Frustum { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected Camera()
        {
            this.PerspectiveView = Matrix.LookAtLH(
                this.Position,
                this.Interest,
                Vector3.UnitY);

            this.PerspectiveProjection = Matrix.Identity;

            this.OrthoView = Matrix.LookAtLH(
                Vector3.BackwardLH,
                Vector3.Zero,
                Vector3.UnitY);

            this.OrthoProjection = Matrix.Identity;
        }
        /// <summary>
        /// Establece el modo de visualización de la cámara (proyección)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetLens(int width, int height)
        {
            this.viewportWidth = width;
            this.viewportHeight = height;

            this.aspectRelation = this.viewportWidth / this.viewportHeight;

            this.UpdateLens();
        }
        /// <summary>
        /// Liberar recursos
        /// </summary>
        public void Dispose()
        {

        }
        /// <summary>
        /// Actualiza el estado de la cámara
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        public void Update()
        {
            this.PerspectiveView = Matrix.LookAtLH(
                this.Position,
                this.Interest,
                Vector3.UnitY);

            this.Frustum = BoundingFrustum.FromCamera(
                this.Position,
                this.Direction,
                Vector3.UnitY,
                this.fieldOfView,
                this.nearPlaneDistance,
                this.farPlaneDistance,
                this.aspectRelation);
        }
        /// <summary>
        /// Avanzar al eje anterior
        /// </summary>
        public void PreviousIsometricAxis()
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

            this.SetIsometricAxis((IsometricAxis)previous);
        }
        /// <summary>
        /// Avanzar al siguiente eje
        /// </summary>
        public void NextIsometricAxis()
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

            this.SetIsometricAxis((IsometricAxis)next);
        }
        /// <summary>
        /// Mueve hacia adelante
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
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
        /// Mueve hacia atrás
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
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
        /// Mueve hacia la izquierda
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
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
        /// Mueve hacia la derecha
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
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
        /// Rotar hacia arriba
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
        public void RotateUp(GameTime gameTime, bool slow)
        {
            if (this.mode == CameraModes.Free)
            {
                this.Rotate(gameTime, this.freeLeft, slow);
            }
        }
        /// <summary>
        /// Rotar hacia abajo
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
        public void RotateDown(GameTime gameTime, bool slow)
        {
            if (this.mode == CameraModes.Free)
            {
                this.Rotate(gameTime, this.freeRight, slow);
            }
        }
        /// <summary>
        /// Rotar hacia la izquierda
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
        public void RotateLeft(GameTime gameTime, bool slow)
        {
            if (this.mode == CameraModes.Free)
            {
                this.Rotate(gameTime, this.freeDown, slow);
            }
        }
        /// <summary>
        /// Rotar hacia la derecha
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
        public void RotateRight(GameTime gameTime, bool slow)
        {
            if (this.mode == CameraModes.Free)
            {
                this.Rotate(gameTime, this.freeUp, slow);
            }
        }
        /// <summary>
        /// Rotación con ratón
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="deltaX">Variación en X</param>
        /// <param name="deltaY">Variación en Y</param>
        public void RotateMouse(GameTime gameTime, float deltaX, float deltaY)
        {
            if (deltaX != 0f)
                this.Rotate(this.freeUp, gameTime.ElapsedSeconds * deltaX * 10f);
            if (deltaY != 0f)
                this.Rotate(this.freeLeft, gameTime.ElapsedSeconds * -deltaY * 10f);
        }
        /// <summary>
        /// Acercar cámara
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
        public void ZoomIn(GameTime gameTime, bool slow)
        {
            this.Zoom(gameTime, true, slow);
        }
        /// <summary>
        /// Alejar cámara
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
        public void ZoomOut(GameTime gameTime, bool slow)
        {
            this.Zoom(gameTime, false, slow);
        }
        /// <summary>
        /// Centra la cámara en el punto especificado
        /// </summary>
        /// <param name="newInterest">Nuevo punto de interés</param>
        public void CenterPoint(Vector3 newInterest)
        {
            if (this.mode == CameraModes.FreeIsometric)
            {
                Vector3 diff = newInterest - this.Interest;

                this.Interest += diff;
                this.Position += diff;
            }
            else
            {
                this.Interest = newInterest;
            }
        }

        /// <summary>
        /// Establece la configuración de cámara libre
        /// </summary>
        /// <param name="interest">Punte de interés</param>
        /// <param name="distance">Distancia al punto de interés</param>
        private void SetFree(Vector3 interest, float distance)
        {
            Vector3 diff = interest - this.Interest;
            this.Interest += diff;
            this.Position += diff;
            this.Position = Vector3.Normalize(this.Position) * distance;

            this.mode = CameraModes.Free;
        }
        /// <summary>
        /// Establece la configuración isométrica
        /// </summary>
        /// <param name="axis">Eje</param>
        /// <param name="interest">Punte de interés</param>
        /// <param name="distance">Distancia al punto de interés</param>
        private void SetIsometric(IsometricAxis axis, Vector3 interest, float distance)
        {
            Vector3 diff = interest - this.Interest;
            this.Interest += diff;
            this.Position += diff;
            this.Position = Vector3.Normalize(this.Position) * distance;

            this.mode = CameraModes.FreeIsometric;
            this.SetIsometricAxis(axis);
        }
        /// <summary>
        /// Establece el nuevo eje
        /// </summary>
        /// <param name="axis">Eje</param>
        private void SetIsometricAxis(IsometricAxis axis)
        {
            this.isometricAxis = axis;

            float distance = (this.Position - this.Interest).Length();

            Vector3 tmpPosition = Vector3.Zero;
            Vector3 tmpInterest = Vector3.Zero;

            if (axis == IsometricAxis.NW)
            {
                //Norte
                tmpPosition = new Vector3(1, 1, 1);
                this.isoMetricForward = new Vector3(-1f, 0f, -1f);
                this.isoMetricBackward = new Vector3(1f, 0f, 1f);
                this.isoMetricLeft = new Vector3(1f, 0f, -1f);
                this.isoMetricRight = new Vector3(-1f, 0f, 1f);
            }
            else if (axis == IsometricAxis.NE)
            {
                //Sur
                tmpPosition = new Vector3(-1, 1, -1);
                this.isoMetricForward = new Vector3(1f, 0f, 1f);
                this.isoMetricBackward = new Vector3(-1f, 0f, -1f);
                this.isoMetricLeft = new Vector3(-1f, 0f, 1f);
                this.isoMetricRight = new Vector3(1f, 0f, -1f);
            }
            else if (axis == IsometricAxis.SE)
            {
                //Este
                tmpPosition = new Vector3(-1, 1, 1);
                this.isoMetricForward = new Vector3(1f, 0f, -1f);
                this.isoMetricBackward = new Vector3(-1f, 0f, 1f);
                this.isoMetricLeft = new Vector3(1f, 0f, 1f);
                this.isoMetricRight = new Vector3(-1f, 0f, -1f);
            }
            else if (axis == IsometricAxis.SW)
            {
                //Oeste
                tmpPosition = new Vector3(1, 1, -1);
                this.isoMetricForward = new Vector3(-1f, 0f, 1f);
                this.isoMetricBackward = new Vector3(1f, 0f, -1f);
                this.isoMetricLeft = new Vector3(-1f, 0f, -1f);
                this.isoMetricRight = new Vector3(1f, 0f, 1f);
            }

            Vector3 diff = this.Interest - tmpInterest;
            this.Position = Vector3.Normalize(tmpPosition) * distance;
            this.Position += diff;
        }
        /// <summary>
        /// Movimiento
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="vector">Vector de movimiento</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
        private void Move(GameTime gameTime, Vector3 vector, bool slow)
        {
            Vector3 movingVector = Vector3.Zero;
            float delta = (slow) ? this.SlowMovementDelta : this.MovementDelta;

            vector *= delta;
            movingVector += vector;

            if (movingVector != Vector3.Zero)
            {
                movingVector *= gameTime.ElapsedSeconds;

                this.Position += movingVector;
                this.Interest += movingVector;
            }
        }
        /// <summary>
        /// Rotación
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="axis">Eje de rotación</param>
        /// <param name="slow">Indica si la rotación es lenta</param>
        private void Rotate(GameTime gameTime, Vector3 axis, bool slow)
        {
            float degrees = (slow) ? this.SlowRotationDelta : this.RotationDelta;

            this.Rotate(axis, degrees);
        }
        /// <summary>
        /// Rotación
        /// </summary>
        /// <param name="axis">Eje de rotación</param>
        /// <param name="degrees">Ángulo de rotación en grados</param>
        private void Rotate(Vector3 axis, float degrees)
        {
            Quaternion r = Quaternion.RotationAxis(axis, MathUtil.DegreesToRadians(degrees));

            Vector3 fw = Vector3.Transform(this.freeForward, r);

            this.Interest = this.Position + fw;
        }
        /// <summary>
        /// Zoom
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        /// <param name="zoomIn">Indica si la cámara se aleja o se acerca. Verdadero si se acerca</param>
        /// <param name="slow">Indica si el movimiento es lento</param>
        private void Zoom(GameTime gameTime, bool zoomIn, bool slow)
        {
            float delta = delta = (slow) ? this.SlowMovementDelta : this.MovementDelta;
            float zooming = (zoomIn) ? -delta : +delta;

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
        /// Update projections
        /// </summary>
        private void UpdateLens()
        {
            this.PerspectiveProjection = Matrix.PerspectiveFovLH(
                this.fieldOfView,
                this.aspectRelation,
                this.nearPlaneDistance,
                this.farPlaneDistance);

            this.OrthoProjection = Matrix.OrthoLH(
                this.viewportWidth,
                this.viewportHeight,
                this.nearPlaneDistance,
                this.farPlaneDistance);
        }
    }
}
