using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Point light
    /// </summary>
    public class SceneLightPoint : SceneLight
    {
        private float constant = 0f;
        private float linear = 0f;
        private float exponential = 0f;

        /// <summary>
        /// Ligth position
        /// </summary>
        public Vector3 Position = Vector3.Zero;
        /// <summary>
        /// Stores the three attenuation constants in the format (a0, a1, a2) that control how light intensity falls off with distance
        /// </summary>
        /// <remarks>
        /// Constant weaken (1,0,0)
        /// Inverse distance weaken (0,1,0)
        /// Inverse square law (0,0,1)
        /// </remarks>
        public float Constant
        {
            get
            {
                return this.constant;
            }
            set
            {
                this.constant = value;

                this.UpdateRange();
            }
        }

        public float Linear
        {
            get
            {
                return this.linear;
            }
            set
            {
                this.linear = value;

                this.UpdateRange();
            }
        }

        public float Exponential
        {
            get
            {
                return this.exponential;
            }
            set
            {
                this.exponential = value;

                this.UpdateRange();
            }
        }

        public float Range { get; private set; }

        public SceneLightPoint()
            : base()
        {
            this.UpdateRange();
        }

        public static Tuple<float, float>[] Test(SceneLightPoint light, float limit, float pass)
        {
            List<Tuple<float, float>> values = new List<Tuple<float, float>>();

            for (float distance = 0; distance < limit; distance += pass)
            {
                float attenuation =
                    light.Constant +
                    light.Linear * distance +
                    light.Exponential * distance * distance;

                attenuation = 1f / Math.Max(1f, attenuation);

                values.Add(new Tuple<float, float>(distance, attenuation));

                if (attenuation == 0f)
                {
                    break;
                }
            }

            return values.ToArray();
        }

        public void UpdateRange()
        {
            float a = this.exponential;
            float b = this.linear;
            float c = this.constant;

            if (a != 0)
            {
                //ax^2 + bx + c = 0
                float discriminant = (b * b) - (4f * a * c);

                float x1 = (-b + (float)(Math.Sqrt(discriminant))) / (2f * a);
                float x2 = (-b - (float)(Math.Sqrt(discriminant))) / (2f * a);

                this.Range = Math.Max(x1, x2);
            }
            else if (b != 0)
            {
                //bx + c = 0
                this.Range = -c / b;
            }
            else
            {
                this.Range = c;
            }
        }
    }
}
