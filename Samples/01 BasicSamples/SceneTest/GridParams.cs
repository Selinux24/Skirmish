using SharpDX;
using System;

namespace BasicSamples.SceneTest
{
    struct GridParams
    {
        public int RowSize;
        public float AreaSize;
        public float Sx;
        public float Sy;
        public float Sz;
        public int XCount;
        public int ZCount;
        public int XRowCount;
        public int ZRowCount;
        public int BasementRows;

        public readonly (Vector3 position, float angle) GetP(Random prnd, int i, float baseHeight, Vector3 delta)
        {
            float height = (i / RowSize * Sy) + baseHeight + delta.Y - (Sy * BasementRows);

            if ((i % RowSize) < ZRowCount)
            {
                float rx = (i % ZRowCount < ZCount ? -AreaSize - (Sx / 2f) : AreaSize + (Sx / 2f)) + prnd.NextFloat(-1f, 1f);
                float dz = i % ZRowCount < ZCount ? -(Sz / 2f) : (Sz / 2f);

                float x = rx + delta.X;
                float y = height;
                float z = (i % ZCount * Sz) - AreaSize + delta.Z + dz;
                float angle = MathUtil.Pi * prnd.Next(0, 2);

                return (new(x, y, z), angle);
            }
            else
            {
                int ci = i - ZRowCount;
                float rz = (ci % XRowCount < XCount ? -AreaSize - (Sz / 2f) : AreaSize + (Sz / 2f)) + prnd.NextFloat(-1f, 1f);
                float dx = ci % XRowCount < XCount ? (Sx / 2f) : -(Sx / 2f);

                float x = (ci % XCount * Sx) - AreaSize + delta.X + dx;
                float y = height;
                float z = rz + delta.Z;
                float angle = MathUtil.Pi * prnd.Next(0, 2);

                return (new(x, y, z), angle);
            }
        }
    }
}
