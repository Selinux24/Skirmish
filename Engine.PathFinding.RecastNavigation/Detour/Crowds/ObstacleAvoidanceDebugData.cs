using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class ObstacleAvoidanceDebugData
    {
        public static void NormalizeArray(float[] arr, int n)
        {
            // Normalize penaly range.
            float minPen = float.MaxValue;
            float maxPen = float.MinValue;
            for (int i = 0; i < n; ++i)
            {
                minPen = Math.Min(minPen, arr[i]);
                maxPen = Math.Max(maxPen, arr[i]);
            }

            float penRange = maxPen - minPen;
            float s = penRange > 0.001f ? (1.0f / penRange) : 1;
            for (int i = 0; i < n; ++i)
            {
                arr[i] = MathUtil.Clamp((arr[i] - minPen) * s, 0.0f, 1.0f);
            }
        }

        public int m_nsamples { get; set; }
        public int m_maxSamples { get; set; }
        public Vector3[] m_vel { get; set; }
        public float[] m_ssize { get; set; }
        public float[] m_pen { get; set; }
        public float[] m_vpen { get; set; }
        public float[] m_vcpen { get; set; }
        public float[] m_spen { get; set; }
        public float[] m_tpen { get; set; }

        public bool Init(int maxSamples)
        {

            m_maxSamples = maxSamples;

            m_vel = new Vector3[m_maxSamples];
            m_pen = new float[m_maxSamples];
            m_ssize = new float[m_maxSamples];
            m_vpen = new float[m_maxSamples];
            m_vcpen = new float[m_maxSamples];
            m_spen = new float[m_maxSamples];
            m_tpen = new float[m_maxSamples];

            return true;
        }
        public void Reset()
        {
            m_nsamples = 0;
        }
        public void AddSample(Vector3 vel, float ssize, float pen, float vpen, float vcpen, float spen, float tpen)
        {
            if (m_nsamples >= m_maxSamples)
            {
                return;
            }

            m_vel[m_nsamples] = vel;
            m_ssize[m_nsamples] = ssize;
            m_pen[m_nsamples] = pen;
            m_vpen[m_nsamples] = vpen;
            m_vcpen[m_nsamples] = vcpen;
            m_spen[m_nsamples] = spen;
            m_tpen[m_nsamples] = tpen;
            m_nsamples++;
        }
        public void NormalizeSamples()
        {
            NormalizeArray(m_pen, m_nsamples);
            NormalizeArray(m_vpen, m_nsamples);
            NormalizeArray(m_vcpen, m_nsamples);
            NormalizeArray(m_spen, m_nsamples);
            NormalizeArray(m_tpen, m_nsamples);
        }
    }
}