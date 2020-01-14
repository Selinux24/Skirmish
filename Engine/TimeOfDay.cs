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
            get { return this.TimeOfDayValue * MathUtil.TwoPi; }
        }
        /// <summary>
        /// Angle from true north of celestial object in degrees
        /// </summary>
        public float AzimuthDegrees
        {
            get
            {
                return MathUtil.RadiansToDegrees(this.Azimuth);
            }
            set
            {
                this.Azimuth = MathUtil.DegreesToRadians(value);
            }
        }
        /// <summary>
        /// Angle from horizon of celestial object in degrees
        /// </summary> 
        public float ElevationDegrees
        {
            get
            {
                return MathUtil.RadiansToDegrees(this.Elevation);
            }
            set
            {
                this.Elevation = MathUtil.DegreesToRadians(value);
            }
        }
        /// <summary>
        /// Angle between global equator and tropic in degrees
        /// </summary>
        public float SunDeclineDegrees
        {
            get
            {
                return MathUtil.RadiansToDegrees(this.SunDecline);
            }
            set
            {
                this.SunDecline = MathUtil.DegreesToRadians(value);
            }
        }
        /// <summary>
        /// Meridian angle in degrees
        /// </summary> 
        public float MeridianAngleDegrees
        {
            get
            {
                return this.TimeOfDayValue * 360.0f;
            }
        }
        /// <summary>
        /// Current hour of day
        /// </summary>
        public TimeSpan HourOfDay
        {
            get
            {
                return TimeSpan.FromDays(this.TimeOfDayValue);
            }
        }
        /// <summary>
        /// Gets whether the instance is animating the day cycle
        /// </summary>
        public bool Updated
        {
            get
            {
                return this.PreviuosElevation != this.Elevation;
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
            this.TimeOfDayValue = 0.0f;
            this.PreviuosElevation = 0;
            this.NextElevation = 0;
            this.AzimuthOverride = 1.0f;

            this.SunDecline = MathUtil.DegreesToRadians(23.44f);
            this.Elevation = 0.0f;
            this.Azimuth = 0.0f;

            this.Animate = false;
            this.AnimateSpeed = 0f;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            if (this.Animate)
            {
                this.TimeOfDayValue += this.AnimateSpeed * gameTime.ElapsedSeconds;
                this.TimeOfDayValue %= 1f;
            }

            this.UpdatePosition();
        }
        /// <summary>
        /// Updates elevation and azimuth states
        /// </summary>
        private void UpdatePosition()
        {
            this.PreviuosElevation = this.NextElevation;

            if (this.AzimuthOverride != 0f)
            {
                this.Elevation = this.MeridianAngle;
                this.Azimuth = this.AzimuthOverride;

                //Already normalized
                this.NextElevation = this.Elevation;
            }
            else
            {
                //Simplified azimuth/elevation calculation.

                //Get sun decline and meridian angle (in radians)
                float sunDecline = this.SunDecline;
                float meridianAngle = this.MeridianAngle;

                //Calculate the elevation and azimuth (in radians)
                this.Elevation = CalcElevation(0.0f, sunDecline, meridianAngle);
                this.Azimuth = CalcAzimuth(0.0f, sunDecline, meridianAngle);

                //Calculate normalized elevation (0=sunrise, PI/2=zenith, PI=sunset, 3PI/4=nadir)
                float normalizedElevation = MathUtil.Pi * this.Elevation / (2 * CalcElevation(0.0f, sunDecline, 0.0f));
                if (this.Azimuth > MathUtil.Pi)
                {
                    normalizedElevation = MathUtil.Pi - normalizedElevation;
                }
                else if (this.Elevation < 0)
                {
                    normalizedElevation = MathUtil.TwoPi + normalizedElevation;
                }

                this.NextElevation = normalizedElevation;
            }

            this.LightDirection = CalcLightDirection(this.Elevation, this.Azimuth);
        }

        /// <summary>
        /// Sets time of day
        /// </summary>
        /// <param name="time">Time (0 to 1)</param>
        /// <param name="update">Sets whether update internal state or not</param>
        public void SetTimeOfDay(float time, bool update = false)
        {
            this.TimeOfDayValue = Math.Abs(time) % 1.0f;

            if (update)
            {
                this.UpdatePosition();
            }
        }
        /// <summary>
        /// Sets time of day
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="update">Sets whether update internal state or not</param>
        public void SetTimeOfDay(TimeSpan time, bool update = false)
        {
            this.SetTimeOfDay((float)time.TotalDays % 1.0f, update);
        }
        /// <summary>
        /// Begins animation cycle
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="speed">Animation speed</param>
        public void BeginAnimation(float startTime, float speed)
        {
            this.AnimateSpeed = speed * 0.001f;
            this.Animate = true;

            this.SetTimeOfDay(startTime / 360.0f, true);
        }
        /// <summary>
        /// Begins animation cycle
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="speed">Animation speed</param>
        public void BeginAnimation(TimeSpan startTime, float speed)
        {
            this.AnimateSpeed = speed * 0.001f;
            this.Animate = true;

            this.SetTimeOfDay(startTime, true);
        }

        /// <summary>
        /// Gets the text representation of the internal state
        /// </summary>
        public override string ToString()
        {
            return string.Format("Azimuth: {0:0.00}; Sun decline: {2:0.00}; Elevation: {1:0.00}; Hour: {3:hh\\:mm\\:ss};", this.AzimuthDegrees, this.ElevationDegrees, this.SunDeclineDegrees, this.HourOfDay);
        }
    }
}
