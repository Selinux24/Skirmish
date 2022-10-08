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

        public int Nsamples { get; set; }
        public int MaxSamples { get; set; }
        public Vector3[] Vel { get; set; }
        public float[] Ssize { get; set; }
        public float[] Pen { get; set; }
        public float[] Vpen { get; set; }
        public float[] Vcpen { get; set; }
        public float[] Spen { get; set; }
        public float[] Tpen { get; set; }

        public bool Init(int maxSamples)
        {

            MaxSamples = maxSamples;

            Vel = new Vector3[MaxSamples];
            Pen = new float[MaxSamples];
            Ssize = new float[MaxSamples];
            Vpen = new float[MaxSamples];
            Vcpen = new float[MaxSamples];
            Spen = new float[MaxSamples];
            Tpen = new float[MaxSamples];

            return true;
        }
        public void Reset()
        {
            Nsamples = 0;
        }
        public void AddSample(Vector3 vel, float ssize, float pen, float vpen, float vcpen, float spen, float tpen)
        {
            if (Nsamples >= MaxSamples)
            {
                return;
            }

            Vel[Nsamples] = vel;
            Ssize[Nsamples] = ssize;
            Pen[Nsamples] = pen;
            Vpen[Nsamples] = vpen;
            Vcpen[Nsamples] = vcpen;
            Spen[Nsamples] = spen;
            Tpen[Nsamples] = tpen;
            Nsamples++;
        }
        public void NormalizeSamples()
        {
            NormalizeArray(Pen, Nsamples);
            NormalizeArray(Vpen, Nsamples);
            NormalizeArray(Vcpen, Nsamples);
            NormalizeArray(Spen, Nsamples);
            NormalizeArray(Tpen, Nsamples);
        }
    }
}