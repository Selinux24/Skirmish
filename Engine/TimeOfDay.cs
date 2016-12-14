using SharpDX;
using System;
using System.Collections.Generic;

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
        /// Color target
        /// </summary>
        protected struct ColorTarget
        {
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <remarks>The elevation targets represent distances from PI/2 radians (strait up).</remarks>
            public static ColorTarget[] GetElevationTargets()
            {
                List<ColorTarget> colors = new List<ColorTarget>();

                // (semicircle in radians) / (number of color target entries);
                float elevation = MathUtil.Pi / 13.0f;

                // High noon at equanox
                colors.Add(new ColorTarget(0, new Color(1.00f, 1.00f, 1.00f), 1f, new Color(1.00f, 1.00f, 1.00f)));

                // Day
                colors.Add(new ColorTarget(elevation * 1, new Color(0.90f, 0.90f, 0.90f, 1f), 1f, new Color(0.90f, 0.90f, 0.90f, 1f)));
                colors.Add(new ColorTarget(elevation * 2, new Color(0.90f, 0.90f, 0.90f, 1f), 1f, new Color(0.90f, 0.90f, 0.90f, 1f)));
                colors.Add(new ColorTarget(elevation * 3, new Color(0.80f, 0.75f, 0.75f, 1f), 1f, new Color(0.80f, 0.75f, 0.75f, 1f)));
                colors.Add(new ColorTarget(elevation * 4, new Color(0.70f, 0.65f, 0.65f, 1f), 1f, new Color(0.70f, 0.65f, 0.65f, 1f)));

                // Dawn and Dusk (3 entries)
                colors.Add(new ColorTarget(elevation * 5, new Color(0.70f, 0.65f, 0.65f, 1f), 3f, new Color(0.80f, 0.60f, 0.30f, 1f)));
                colors.Add(new ColorTarget(elevation * 6, new Color(0.65f, 0.54f, 0.40f, 1f), 2.75f, new Color(0.75f, 0.5f, 0.40f, 1f)));
                colors.Add(new ColorTarget(elevation * 7, new Color(0.55f, 0.45f, 0.25f, 1f), 2.5f, new Color(0.65f, 0.3f, 0.30f, 1f)));

                // Night
                colors.Add(new ColorTarget(elevation * 8, new Color(0.30f, 0.30f, 0.30f, 1f), 1.25f, new Color(0.70f, 0.40f, 0.20f, 1f)));
                colors.Add(new ColorTarget(elevation * 9, new Color(0.25f, 0.25f, 0.30f, 1f), 1f, new Color(0.80f, 0.30f, 0.20f, 1f)));
                colors.Add(new ColorTarget(elevation * 10, new Color(0.25f, 0.25f, 0.40f, 1f), 1f, new Color(0.25f, 0.25f, 0.40f, 1f)));
                colors.Add(new ColorTarget(elevation * 11, new Color(0.20f, 0.20f, 0.35f, 1f), 1f, new Color(0.20f, 0.20f, 0.35f, 1f)));

                // Midnight at equanox.
                colors.Add(new ColorTarget(MathUtil.Pi, new Color(0.15f, 0.15f, 0.20f, 1f), 1f, new Color(0.15f, 0.15f, 0.20f, 1f)));

                return colors.ToArray();
            }

            /// <summary>
            /// Maximum elevation
            /// </summary>
            public float Elevation;
            /// <summary>
            /// Sun color
            /// </summary>
            public Color SunColor;
            /// <summary>
            /// 6 is max
            /// </summary>
            public float BandModidifer;
            /// <summary>
            /// Sun band color
            /// </summary>
            public Color SunBandColor;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="elevation">Elevation</param>
            /// <param name="sunColor">Color</param>
            /// <param name="sunBandModifier">Band modifier</param>
            /// <param name="sunBandColor">Band color</param>
            public ColorTarget(float elevation, Color sunColor, float sunBandModifier, Color sunBandColor)
            {
                this.Elevation = elevation;
                this.SunColor = sunColor;
                this.BandModidifer = sunBandModifier;
                this.SunBandColor = sunBandColor;
            }
        }

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
        /// Current sun color
        /// </summary>
        private Color currentSunColor;
        /// <summary>
        /// Current band color
        /// </summary>
        private Color currentBandColor;

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
        /// Animation time
        /// </summary>
        protected float AnimateTime;
        /// <summary>
        /// Animation speed
        /// </summary>
        protected float AnimateSpeed;
        /// <summary>
        /// Color targets by elevation
        /// </summary>
        protected List<ColorTarget> ColorTargets = new List<ColorTarget>();

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
        /// Current sun color
        /// </summary>
        public Color SunColor
        {
            get
            {
                return this.currentSunColor;
            }
        }
        /// <summary>
        /// Current sun band color
        /// </summary>
        public Color SunBandColor
        {
            get
            {
                return this.currentBandColor;
            }
        }

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
            this.AnimateTime = 0.0f;
            this.AnimateSpeed = 0.0f;

            this.ColorTargets.AddRange(ColorTarget.GetElevationTargets());
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed seconds</param>
        public void Update(float elapsedSeconds)
        {
            if (this.Animate)
            {
                this.UpdateAnimation(elapsedSeconds);
            }

            this.UpdatePosition();

            this.UpdateColors();
        }
        /// <summary>
        /// Updates animation data
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed seconds</param>
        private void UpdateAnimation(float elapsedSeconds)
        {
            float current = this.TimeOfDayValue * 360.0f;
            float next = (current + (this.AnimateSpeed * elapsedSeconds)) % 360.0f;

            // Clamp to make sure we don't pass the target time.
            if (next >= this.AnimateTime)
            {
                next = this.AnimateTime;
                this.Animate = false;
            }

            // Set the new time of day.
            this.TimeOfDayValue = next / 360.0f;
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
                float normElevation = MathUtil.Pi * this.Elevation / (2 * CalcElevation(0.0f, sunDecline, 0.0f));
                if (this.Azimuth > MathUtil.Pi)
                {
                    normElevation = MathUtil.Pi - normElevation;
                }
                else if (this.Elevation < 0)
                {
                    normElevation = MathUtil.TwoPi + normElevation;
                }

                this.NextElevation = normElevation;
            }
        }
        /// <summary>
        /// Updates current sun and band colors with elevation and azimuth states
        /// </summary>
        private void UpdateColors()
        {
            if (this.ColorTargets.Count == 0)
            {
                this.currentSunColor = Color.Black;
                this.currentBandColor = Color.Black;
            }

            if (this.ColorTargets.Count == 1)
            {
                this.currentSunColor = ColorTargets[0].SunColor;
                this.currentBandColor = ColorTargets[0].SunBandColor;
            }

            //Simple check
            if (this.ColorTargets[0].Elevation != 0.0f)
            {
                throw new Exception("First elevation must be 0.0 radians");
            }
            if (this.ColorTargets[this.ColorTargets.Count - 1].Elevation != MathUtil.Pi)
            {
                throw new Exception("Last elevation must be PI");
            }

            float elevation = MathUtil.Clamp(MathUtil.TwoPi - this.NextElevation, 0.0f, MathUtil.Pi);

            //Find targets
            for (int count = 0; count < this.ColorTargets.Count - 1; count++)
            {
                var one = this.ColorTargets[count];
                var two = this.ColorTargets[count + 1];

                if (elevation >= one.Elevation && elevation <= two.Elevation)
                {
                    //Interpolate
                    float div = two.Elevation - one.Elevation;

                    //catch bad input divide by zero
                    if (Math.Abs(div) < 0.01f)
                    {
                        div = 0.01f;
                    }

                    float phase = (elevation - one.Elevation) / div;

                    this.currentSunColor = Color.Lerp(one.SunColor, two.SunColor, phase);
                    this.currentBandColor = Color.Lerp(one.SunBandColor, two.SunBandColor, phase);

                    break;
                }
            }
        }

        /// <summary>
        /// Sets time of day
        /// </summary>
        /// <param name="time">Time (0 to 1)</param>
        /// <param name="update">Sets wheter update internal state or not</param>
        public void SetTimeOfDay(float time, bool update = false)
        {
            this.TimeOfDayValue = (float)Math.Abs(time) % 1.0f;

            if (update)
            {
                this.UpdatePosition();

                this.UpdateColors();
            }
        }
        /// <summary>
        /// Begins animation cycle
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="speed">Animation speed</param>
        public void BeginAnimation(float startTime, float speed)
        {
            this.Animate = false;

            this.AnimateTime = MathUtil.Clamp(startTime, 0.0f, 360.0f);

            float current = this.TimeOfDayValue * 360.0f;
            float target = this.AnimateTime;
            if (target < current)
            {
                target += 360.0f;
            }

            if (!MathUtil.IsZero(target - current))
            {
                this.AnimateSpeed = speed;
                this.Animate = true;
            }
        }
    }
}
