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
        /// <summary>
        /// Ligth position
        /// </summary>
        public Vector3 Position = Vector3.Zero;
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius = 1f;

        public SceneLightPoint()
            : base()
        {
            
        }

        public static float OneTestDistance(float radius, float intensity, float cutoff, float distance)
        {
            float dn = (distance / radius) + 1f;
            float attenuation = intensity / (dn * dn);

            attenuation -= cutoff;
            attenuation *= 1f / (1f - cutoff);
            attenuation = Math.Max(attenuation, 0f);

            return attenuation;
        }
        public static float OneGetMaxDistance(float radius, float intensity, float cutoff)
        {
            float res = 0f;

            res = radius * ((float)Math.Sqrt(intensity / cutoff) - 1f);

            return res;
        }
        public static Tuple<float, float>[] OneTestComplete(float radius, float intensity, float cutoff, float pass)
        {
            List<Tuple<float, float>> values = new List<Tuple<float, float>>();

            for (float distance = 0; distance < 1000f; distance += pass)
            {
                float attenuation = OneTestDistance(radius, intensity, cutoff, distance);

                values.Add(new Tuple<float, float>(distance, attenuation));

                if (attenuation == 0f) break;
            }

            return values.ToArray();
        }

        public static float TwoTestDistance(float radius, float intensity, float maxDistance, float distance)
        {
            float f = distance / maxDistance;
            float denom = 1 - (f * f);
            if (denom > 0)
            {
                float d = distance / (1 - (f * f));
                float dn = (d / radius) + 1f;
                return intensity / (dn * dn);
            }
            else
            {
                return 0f;
            }
        }
        public static Tuple<float, float>[] TwoTestComplete(float radius, float intensity, float maxDistance, float pass)
        {
            List<Tuple<float, float>> values = new List<Tuple<float, float>>();

            for (float distance = 0; distance < maxDistance * 2f; distance += pass)
            {
                float attenuation = TwoTestDistance(radius, intensity, maxDistance, distance);

                values.Add(new Tuple<float, float>(distance, attenuation));
            }

            return values.ToArray();
        }
    }
}
