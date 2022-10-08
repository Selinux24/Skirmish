using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// Time of day
    /// </summary>
    /// <remarks>
    /// From Torque3D
    /// https://github.com/GarageGames/Torque3D/blob/development/Engine/source/environment/timeOfDay.h
    /// https://github.com/GarageGames/Torque3D/blob/development/Engine/source/environment/timeOfDay.cpp
    /// </remarks>
    public class TimeOfDay
    {
        /// <summary>
        /// Gets elevation
        /// </summary>
        /// <param name="lattitude">Lattidude</param>
        /// <param name="decline">Sun decline</param>
        /// <param name="meridianAngle">Meridian angle</param>
        /// <returns>Returns elevation in radians</returns>
        /// <remarks>0 to 2PI</remarks>
        public static float CalcElevation(float lattitude, float decline, float meridianAngle)
        {
            return (float)(Math.Asin(Math.Sin(lattitude) * Math.Sin(decline) + Math.Cos(lattitude) * Math.Cos(decline) * Math.Cos(meridianAngle)));
        }
        /// <summary>
        /// Gets azimuth
        /// </summary>
        /// <param name="lattitude">Lattidude</param>
        /// <param name="decline">Sun decline</param>
        /// <param name="meridianAngle">Meridian angle</param>
        /// <returns>Returns azimuth in radians</returns>
        /// <remarks>0 to PI</remarks>
        public static float CalcAzimuth(float lattitude, float decline, float meridianAngle)
        {
            return (float)(Math.Atan2(Math.Sin(meridianAngle), Math.Cos(meridianAngle) * Math.Sin(lattitude) - Math.Tan(decline) * Math.Cos(lattitude))) + MathUtil.Pi;
        }
        /// <summary>
        /// Gets the sun light direction based upon elevation and azimuth angles
        /// </summary>
        /// <param name="elevation">Elevation in radians</param>
        /// <param name="azimuth">Azimuth in radians</param>
        public static Vector3 CalcLightDirection(float elevation, float azimuth)
        {
            Matrix rot = Matrix.RotationYawPitchRoll(azimuth, elevation, 0);

            return Vector3.TransformNormal(Vector3.Up, rot);
        }

        /// <summary>
        /// The 0-360 normalized elevation for the previous update.                                            
        /// </summary>
        protected float PreviuosElevation;
        /// <summary>
        /// The 0-360 normalized elevation for the next update.
        /// </summary>
        protected float NextElevation;

        protected float StartTimeOfDayValue;
        /// <summary>
        /// The zero to one time of day where zero is the start of a day and one is the end.	
        /// </summary>
        protected float TimeOfDayValue;
        /// <summary>
        /// Used to specify an azimuth that will stay constant throughout the day cycle.
        /// </summary>
        protected float AzimuthOverride;
        /// <summary>
        /// Animate flag
        /// </summary>
        protected bool Animate;
        /// <summary>
        /// Animation speed
        /// </summary>
        protected float AnimateSpeed;

        /// <summary>
        /// Angle between global equator and tropic in radians
        /// </summary>
        public float SunDecline { get; set; }
        /// <summary>
        /// Angle from true north of celestial object in radians
        /// </summary>
        public float Azimuth { get; set; }
        /// <summary>
        /// Angle from horizon of celestial object in radians
        /// </summary> 
        public float Elevation { get; set; }
        /// <summary>
        /// Meridian angle in radians
        /// </summary> 
        public float MeridianAngle
        {
            get { return TimeOfDayValue * MathUtil.TwoPi; }
        }
        /// <summary>
        /// Angle from true north of celestial object in degrees
        /// </summary>
        public float AzimuthDegrees
        {
            get
            {
                return MathUtil.RadiansToDegrees(Azimuth);
            }
            set
            {
                Azimuth = MathUtil.DegreesToRadians(value);
            }
        }
        /// <summary>
        /// Angle from horizon of celestial object in degrees
        /// </summary> 
        public float ElevationDegrees
        {
            get
            {
                return MathUtil.RadiansToDegrees(Elevation);
            }
            set
            {
                Elevation = MathUtil.DegreesToRadians(value);
            }
        }
        /// <summary>
        /// Angle between global equator and tropic in degrees
        /// </summary>
        public float SunDeclineDegrees
        {
            get
            {
                return MathUtil.RadiansToDegrees(SunDecline);
            }
            set
            {
                SunDecline = MathUtil.DegreesToRadians(value);
            }
        }
        /// <summary>
        /// Meridian angle in degrees
        /// </summary> 
        public float MeridianAngleDegrees
        {
            get
            {
                return TimeOfDayValue * 360.0f;
            }
        }
        /// <summary>
        /// Current hour of day
        /// </summary>
        public TimeSpan HourOfDay
        {
            get
            {
                return TimeSpan.FromDays(TimeOfDayValue);
            }
        }
        /// <summary>
        /// Gets whether the instance is animating the day cycle
        /// </summary>
        public bool Updated
        {
            get
            {
                return PreviuosElevation != Elevation;
            }
        }

        /// <summary>
        /// Gets the ligth direction base upon elevation and azimuth angles
        /// </summary>
        public Vector3 LightDirection { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TimeOfDay()
        {
            TimeOfDayValue = 0.0f;
            PreviuosElevation = 0;
            NextElevation = 0;
            AzimuthOverride = 1.0f;

            SunDecline = MathUtil.DegreesToRadians(23.44f);
            Elevation = 0.0f;
            Azimuth = 0.0f;

            Animate = false;
            AnimateSpeed = 0f;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            if (Animate)
            {
                TimeOfDayValue = StartTimeOfDayValue + (AnimateSpeed * gameTime.TotalSeconds);
                TimeOfDayValue %= 1f;
            }

            UpdatePosition();
        }
        /// <summary>
        /// Updates elevation and azimuth states
        /// </summary>
        private void UpdatePosition()
        {
            PreviuosElevation = NextElevation;

            if (AzimuthOverride != 0f)
            {
                Elevation = MeridianAngle;
                Azimuth = AzimuthOverride;

                //Already normalized
                NextElevation = Elevation;
            }
            else
            {
                //Simplified azimuth/elevation calculation.

                //Get sun decline and meridian angle (in radians)
                float sunDecline = SunDecline;
                float meridianAngle = MeridianAngle;

                //Calculate the elevation and azimuth (in radians)
                Elevation = CalcElevation(0.0f, sunDecline, meridianAngle);
                Azimuth = CalcAzimuth(0.0f, sunDecline, meridianAngle);

                //Calculate normalized elevation (0=sunrise, PI/2=zenith, PI=sunset, 3PI/4=nadir)
                float normalizedElevation = MathUtil.Pi * Elevation / (2 * CalcElevation(0.0f, sunDecline, 0.0f));
                if (Azimuth > MathUtil.Pi)
                {
                    normalizedElevation = MathUtil.Pi - normalizedElevation;
                }
                else if (Elevation < 0)
                {
                    normalizedElevation = MathUtil.TwoPi + normalizedElevation;
                }

                NextElevation = normalizedElevation;
            }

            LightDirection = CalcLightDirection(Elevation, Azimuth);
        }

        /// <summary>
        /// Sets time of day
        /// </summary>
        /// <param name="time">Time (0 to 1)</param>
        /// <param name="update">Sets whether update internal state or not</param>
        public void SetTimeOfDay(float time, bool update = false)
        {
            StartTimeOfDayValue = Math.Abs(time) % 1.0f;

            if (update)
            {
                UpdatePosition();
            }
        }
        /// <summary>
        /// Sets time of day
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="update">Sets whether update internal state or not</param>
        public void SetTimeOfDay(TimeSpan time, bool update = false)
        {
            SetTimeOfDay((float)time.TotalDays % 1.0f, update);
        }
        /// <summary>
        /// Sets time of day
        /// </summary>
        /// <param name="ticks">Ticks</param>
        /// <param name="update">Sets whether update internal state or not</param>
        public void SetTimeOfDay(long ticks, bool update = false)
        {
            SetTimeOfDay(new TimeSpan(ticks), update);
        }
        /// <summary>
        /// Sets time of day
        /// </summary>
        /// <param name="hours">Hours</param>
        /// <param name="minutes">Minutes</param>
        /// <param name="seconds">Seconds</param>
        /// <param name="update">Sets whether update internal state or not</param>
        public void SetTimeOfDay(int hours, int minutes, int seconds, bool update = false)
        {
            SetTimeOfDay(new TimeSpan(hours, minutes, seconds), update);
        }
        /// <summary>
        /// Sets time of day
        /// </summary>
        /// <param name="days">Days</param>
        /// <param name="hours">Hours</param>
        /// <param name="minutes">Minutes</param>
        /// <param name="seconds">Seconds</param>
        /// <param name="update">Sets whether update internal state or not</param>
        public void SetTimeOfDay(int days, int hours, int minutes, int seconds, bool update = false)
        {
            SetTimeOfDay(new TimeSpan(days, hours, minutes, seconds), update);
        }
        /// <summary>
        /// Sets time of day
        /// </summary>
        /// <param name="days">Days</param>
        /// <param name="hours">Hours</param>
        /// <param name="minutes">Minutes</param>
        /// <param name="seconds">Seconds</param>
        /// <param name="milliseconds">Milliseconds</param>
        /// <param name="update">Sets whether update internal state or not</param>
        public void SetTimeOfDay(int days, int hours, int minutes, int seconds, int milliseconds, bool update = false)
        {
            SetTimeOfDay(new TimeSpan(days, hours, minutes, seconds, milliseconds), update);
        }
        /// <summary>
        /// Begins animation cycle
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="speed">Animation speed</param>
        public void BeginAnimation(float startTime, float speed = 1f)
        {
            AnimateSpeed = speed * 0.001f;
            Animate = true;

            SetTimeOfDay(startTime / 360.0f, true);
        }
        /// <summary>
        /// Begins animation cycle
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="speed">Animation speed</param>
        public void BeginAnimation(TimeSpan startTime, float speed = 1f)
        {
            AnimateSpeed = speed * 0.001f;
            Animate = true;

            SetTimeOfDay(startTime, true);
        }
        /// <summary>
        /// Begins animation cycle
        /// </summary>
        /// <param name="ticks">Ticks</param>
        /// <param name="speed">Animation speed</param>
        public void BeginAnimation(long ticks, float speed = 1f)
        {
            BeginAnimation(new TimeSpan(ticks), speed);
        }
        /// <summary>
        /// Begins animation cycle
        /// </summary>
        /// <param name="hours">Hours</param>
        /// <param name="minutes">Minutes</param>
        /// <param name="seconds">Seconds</param>
        /// <param name="speed">Animation speed</param>
        public void BeginAnimation(int hours, int minutes, int seconds, float speed = 1f)
        {
            BeginAnimation(new TimeSpan(hours, minutes, seconds), speed);
        }
        /// <summary>
        /// Begins animation cycle
        /// </summary>
        /// <param name="days">Days</param>
        /// <param name="hours">Hours</param>
        /// <param name="minutes">Minutes</param>
        /// <param name="seconds">Seconds</param>
        /// <param name="speed">Animation speed</param>
        public void BeginAnimation(int days, int hours, int minutes, int seconds, float speed = 1f)
        {
            BeginAnimation(new TimeSpan(days, hours, minutes, seconds), speed);
        }
        /// <summary>
        /// Begins animation cycle
        /// </summary>
        /// <param name="days">Days</param>
        /// <param name="hours">Hours</param>
        /// <param name="minutes">Minutes</param>
        /// <param name="seconds">Seconds</param>
        /// <param name="milliseconds">Milliseconds</param>
        /// <param name="speed">Animation speed</param>
        public void BeginAnimation(int days, int hours, int minutes, int seconds, int milliseconds, float speed = 1f)
        {
            BeginAnimation(new TimeSpan(days, hours, minutes, seconds, milliseconds), speed);
        }

        /// <summary>
        /// Gets the text representation of the internal state
        /// </summary>
        public override string ToString()
        {
            return string.Format("Azimuth: {0:0.00}; Sun decline: {2:0.00}; Elevation: {1:0.00}; Hour: {3:hh\\:mm\\:ss};", AzimuthDegrees, ElevationDegrees, SunDeclineDegrees, HourOfDay);
        }
    }
}
