
namespace Engine.Common
{
    /// <summary>
    /// Pick counters
    /// </summary>
    public class PickCounters
    {
        /// <summary>
        /// Updates per frame
        /// </summary>
        public int TransformUpdatesPerFrame { get; set; }
        /// <summary>
        /// Picking test per frame
        /// </summary>
        public int PicksPerFrame { get; private set; }
        /// <summary>
        /// Total picking time cost per frame
        /// </summary>
        public float PickingTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average picking time cost
        /// </summary>
        public float PickingAverageTime { get; private set; }
        /// <summary>
        /// Box volume tests per frame
        /// </summary>
        public float VolumeBoxTestPerFrame { get; private set; }
        /// <summary>
        /// Total box volume tests time cost
        /// </summary>
        public float VolumeBoxTestTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average box volume tests time cost
        /// </summary>
        public float VolumeBoxTestAverageTime { get; private set; }
        /// <summary>
        /// Sphere volume tests per frame
        /// </summary>
        public float VolumeSphereTestPerFrame { get; private set; }
        /// <summary>
        /// Total sphere volume tests time cost
        /// </summary>
        public float VolumeSphereTestTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average sphere volume tests time cost
        /// </summary>
        public float VolumeSphereTestAverageTime { get; private set; }
        /// <summary>
        /// Frustum volume tests per frame
        /// </summary>
        public float VolumeFrustumTestPerFrame { get; private set; }
        /// <summary>
        /// Total frustum volume tests time cost
        /// </summary>
        public float VolumeFrustumTestTotalTimePerFrame { get; private set; }
        /// <summary>
        /// Average frustum volume tests time cost
        /// </summary>
        public float VolumeFrustumTestAverageTime { get; private set; }

        /// <summary>
        /// Adds a pick
        /// </summary>
        /// <param name="delta">Delta</param>
        public void AddPick(float delta)
        {
            PicksPerFrame++;
            PickingTotalTimePerFrame += delta;
            PickingAverageTime = PickingTotalTimePerFrame / PicksPerFrame;
        }
        /// <summary>
        /// Adds a frustum test
        /// </summary>
        /// <param name="delta">Delta</param>
        public void AddVolumeFrustumTest(float delta)
        {
            VolumeFrustumTestPerFrame++;
            VolumeFrustumTestTotalTimePerFrame += delta;
            VolumeFrustumTestAverageTime = VolumeFrustumTestTotalTimePerFrame / VolumeFrustumTestPerFrame;
        }
        /// <summary>
        /// Adds a box test
        /// </summary>
        /// <param name="delta">Delta</param>
        public void AddVolumeBoxTest(float delta)
        {
            VolumeBoxTestPerFrame++;
            VolumeBoxTestTotalTimePerFrame += delta;
            VolumeBoxTestAverageTime = VolumeBoxTestTotalTimePerFrame / VolumeBoxTestPerFrame;
        }
        /// <summary>
        /// Adds a sphere test
        /// </summary>
        /// <param name="delta">Delta</param>
        public void AddVolumeSphereTest(float delta)
        {
            VolumeSphereTestPerFrame++;
            VolumeSphereTestTotalTimePerFrame += delta;
            VolumeSphereTestAverageTime = VolumeSphereTestTotalTimePerFrame / VolumeSphereTestPerFrame;
        }

        /// <summary>
        /// Reset all counters to zero.
        /// </summary>
        public void Reset()
        {
            TransformUpdatesPerFrame = 0;

            PicksPerFrame = 0;
            PickingTotalTimePerFrame = 0f;
            PickingAverageTime = 0f;
            VolumeBoxTestPerFrame = 0;
            VolumeBoxTestTotalTimePerFrame = 0f;
            VolumeBoxTestAverageTime = 0f;
            VolumeSphereTestPerFrame = 0;
            VolumeSphereTestTotalTimePerFrame = 0f;
            VolumeSphereTestAverageTime = 0f;
            VolumeFrustumTestPerFrame = 0;
            VolumeFrustumTestTotalTimePerFrame = 0f;
            VolumeFrustumTestAverageTime = 0f;
        }
    }
}
