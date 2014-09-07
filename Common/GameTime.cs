using System;
using System.Diagnostics;

namespace Common
{
    /// <summary>
    /// Tiempo de juego
    /// </summary>
    public class GameTime
    {
        private Stopwatch watch = new Stopwatch();
        private TimeSpan previousElapsedTime;

        /// <summary>
        /// Tiempo transcurrido desde la última actualización
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }
        /// <summary>
        /// Tiempo transcurrido total
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                return this.watch.Elapsed;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameTime()
        {
            this.Start();
        }

        /// <summary>
        /// Resetea el cronómetro.
        /// </summary>
        public void Reset()
        {
            this.previousElapsedTime = this.ElapsedTime = TimeSpan.Zero;

            this.watch.Reset();
        }
        /// <summary>
        /// Inicia el cronómetro.
        /// </summary>
        public void Start()
        {
            this.previousElapsedTime = this.ElapsedTime = TimeSpan.Zero;

            this.watch.Start();
        }
        /// <summary>
        /// Para el cronómetro.
        /// </summary>
        public void Stop()
        {
            this.watch.Stop();

            this.Update();
        }
        /// <summary>
        /// Actualiza la foto temporal del cronómetro.
        /// </summary>
        public void Update()
        {
            TimeSpan elapsedTime = this.watch.Elapsed;

            this.ElapsedTime = elapsedTime - this.previousElapsedTime;

            this.previousElapsedTime = elapsedTime;
        }
    }
}
