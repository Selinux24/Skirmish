using System;

namespace Engine.Tween
{
    /// <summary>
    /// A concrete implementation of a tween object.
    /// </summary>
    /// <typeparam name="T">The type to tween.</typeparam>
    public class Tween<T> : ITween<T> where T : struct
    {
        private readonly LerpFunc<T> lerpFunc;
        private ScaleFunc scaleFunc;

        /// <summary>
        /// Gets the current time of the tween.
        /// </summary>
        public float CurrentTime { get; private set; }
        /// <summary>
        /// Gets the duration of the tween.
        /// </summary>
        public float Duration { get; private set; }
        /// <summary>
        /// Gets the current state of the tween.
        /// </summary>
        public TweenState State { get; private set; }
        /// <summary>
        /// Gets the starting value of the tween.
        /// </summary>
        public T StartValue { get; private set; }
        /// <summary>
        /// Gets the ending value of the tween.
        /// </summary>
        public T EndValue { get; private set; }
        /// <summary>
        /// Gets the current value of the tween.
        /// </summary>
        public T CurrentValue { get; private set; }

        /// <summary>
        /// Initializes a new Tween with a given lerp function.
        /// </summary>
        /// <remarks>
        /// C# generics are good but not good enough. We need a delegate to know how to
        /// interpolate between the start and end values for the given type.
        /// </remarks>
        /// <param name="lerpFunc">The interpolation function for the tween type.</param>
        public Tween(LerpFunc<T> lerpFunc)
        {
            this.lerpFunc = lerpFunc;
            State = TweenState.Stopped;
        }

        /// <summary>
        /// Starts a tween.
        /// </summary>
        /// <param name="start">The start value.</param>
        /// <param name="end">The end value.</param>
        /// <param name="duration">The duration of the tween.</param>
        /// <param name="scaleFunc">A function used to scale progress over time.</param>
        public void Start(T start, T end, float duration, ScaleFunc scaleFunc)
        {
            CurrentTime = 0;

            if (duration <= 0)
            {
                throw new ArgumentException("duration must be greater than 0", nameof(duration));
            }
            this.Duration = duration;

            this.scaleFunc = scaleFunc ?? throw new ArgumentNullException(nameof(scaleFunc));

            State = TweenState.Running;

            this.StartValue = start;
            this.EndValue = end;

            UpdateValue();
        }
        /// <summary>
        /// Restarts the tween
        /// </summary>
        public void Restart()
        {
            Start(StartValue, EndValue, Duration, scaleFunc);
        }
        /// <summary>
        /// Restarts the tween
        /// </summary>
        /// <param name="start">Starting value</param>
        /// <param name="end">End value</param>
        public void Restart(T start, T end)
        {
            Start(start, end, Duration, scaleFunc);
        }
        /// <summary>
        /// Pauses the tween.
        /// </summary>
        public void Pause()
        {
            if (State == TweenState.Running)
            {
                State = TweenState.Paused;
            }
        }
        /// <summary>
        /// Resumes the paused tween.
        /// </summary>
        public void Resume()
        {
            if (State == TweenState.Paused)
            {
                State = TweenState.Running;
            }
        }
        /// <summary>
        /// Stops the tween.
        /// </summary>
        /// <param name="stopBehavior">The behavior to use to handle the stop.</param>
        public void Stop(StopBehavior stopBehavior)
        {
            State = TweenState.Stopped;

            if (stopBehavior == StopBehavior.ForceComplete)
            {
                CurrentTime = Duration;
                UpdateValue();
            }
        }
        /// <summary>
        /// Updates the tween.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time to add to the tween.</param>
        public void Update(float elapsedTime)
        {
            if (State != TweenState.Running)
            {
                return;
            }

            CurrentTime += elapsedTime;
            if (CurrentTime >= Duration)
            {
                CurrentTime = Duration;
                State = TweenState.Stopped;
            }

            UpdateValue();
        }
        /// <summary>
        /// Helper that uses the current time, duration, and delegates to update the current value.
        /// </summary>
        private void UpdateValue()
        {
            CurrentValue = lerpFunc(StartValue, EndValue, scaleFunc(CurrentTime / Duration));
        }
    }
}
