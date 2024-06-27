using SharpDX;
using System;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Obstacle avoidance debug data
    /// </summary>
    public struct ObstacleAvoidanceDebugData
    {
        /// <summary>
        /// Sample data
        /// </summary>
        struct Data
        {
            public Vector3 Vel;
            public float Ssize;
            public float Pen;
            public float Vpen;
            public float Vcpen;
            public float Spen;
            public float Tpen;

            public static void NormalizeArray(Data[] arr, int n)
            {
                var aPen = arr.Select(a => a.Pen).ToArray();
                var aVpen = arr.Select(a => a.Vpen).ToArray();
                var aVcpen = arr.Select(a => a.Vcpen).ToArray();
                var aSpen = arr.Select(a => a.Spen).ToArray();
                var aTpen = arr.Select(a => a.Tpen).ToArray();

                NormalizeArray(aPen, n);
                NormalizeArray(aVpen, n);
                NormalizeArray(aVcpen, n);
                NormalizeArray(aSpen, n);
                NormalizeArray(aTpen, n);

                for (int i = 0; i < n; i++)
                {
                    arr[i].Pen = aPen[i];
                    arr[i].Vpen = aVpen[i];
                    arr[i].Vcpen = aVcpen[i];
                    arr[i].Spen = aSpen[i];
                    arr[i].Tpen = aTpen[i];
                }
            }
            private static void NormalizeArray(float[] arr, int n)
            {
                // Normalize penaly range.
                float min = float.MaxValue;
                float max = float.MinValue;
                for (int i = 0; i < n; ++i)
                {
                    min = MathF.Min(min, arr[i]);
                    max = MathF.Max(max, arr[i]);
                }

                float range = max - min;
                float s = range > 0.001f ? (1.0f / range) : 1;
                for (int i = 0; i < n; ++i)
                {
                    arr[i] = MathUtil.Clamp((arr[i] - min) * s, 0.0f, 1.0f);
                }
            }
        }

        /// <summary>
        /// Data array
        /// </summary>
        private readonly Data[] data;

        /// <summary>
        /// Number of samples
        /// </summary>
        public int Nsamples { get; set; }
        /// <summary>
        /// Maximum samples
        /// </summary>
        public int MaxSamples { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxSamples">Maximum samples</param>
        public ObstacleAvoidanceDebugData(int maxSamples)
        {
            MaxSamples = maxSamples;

            data = new Data[MaxSamples];
        }

        /// <summary>
        /// Resets the debug data
        /// </summary>
        public void Reset()
        {
            Nsamples = 0;
        }
        /// <summary>
        /// Adds a sample
        /// </summary>
        /// <param name="vel"></param>
        /// <param name="ssize"></param>
        /// <param name="pen"></param>
        /// <param name="vpen"></param>
        /// <param name="vcpen"></param>
        /// <param name="spen"></param>
        /// <param name="tpen"></param>
        public void AddSample(Vector3 vel, float ssize, float pen, float vpen, float vcpen, float spen, float tpen)
        {
            if (Nsamples >= MaxSamples)
            {
                return;
            }

            data[Nsamples++] = new()
            {
                Vel = vel,
                Ssize = ssize,
                Pen = pen,
                Vpen = vpen,
                Vcpen = vcpen,
                Spen = spen,
                Tpen = tpen,
            };
        }
        /// <summary>
        /// Normalizes the samples
        /// </summary>
        public readonly void NormalizeSamples()
        {
            Data.NormalizeArray(data, Nsamples);
        }
    }
}