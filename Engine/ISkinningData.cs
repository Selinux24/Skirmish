using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Animation;
    using Engine.Content.Persistence;

    /// <summary>
    /// Skinning data interface
    /// </summary>
    public interface ISkinningData
    {
        /// <summary>
        /// Time step
        /// </summary>
        float TimeStep { get; }
        /// <summary>
        /// Resource index
        /// </summary>
        uint ResourceIndex { get; set; }
        /// <summary>
        /// Resource offset
        /// </summary>
        uint ResourceOffset { get; set; }
        /// <summary>
        /// Resource size
        /// </summary>
        uint ResourceSize { get; set; }

        /// <summary>
        /// On resources updated event
        /// </summary>
        event EventHandler OnResourcesUpdated;

        /// <summary>
        /// Initializes the skinning data instance
        /// </summary>
        /// <param name="jointAnimations">Joint animation list</param>
        /// <param name="animationDescription">Animation description</param>
        void Initialize(IEnumerable<JointAnimation> jointAnimations, AnimationFile animationDescription);

        /// <summary>
        /// Gets the specified animation offset
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipName">Clip name</param>
        /// <param name="animationOffset">Animation offset</param>
        void GetAnimationOffset(float time, string clipName, out uint animationOffset);
        /// <summary>
        /// Gets the index of the specified clip in the animation collection
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <returns>Returns the index of the clip by name</returns>
        int GetClipIndex(string clipName);
        /// <summary>
        /// Gets the clip offset in animation palette
        /// </summary>
        /// <param name="clipIndex">Clip index</param>
        /// <returns>Returns the clip offset in animation palette</returns>
        uint GetClipOffset(int clipIndex);
        /// <summary>
        /// Gets the duration of the specified by index clip
        /// </summary>
        /// <param name="clipIndex">Clip index</param>
        /// <returns>Returns the duration of the clip</returns>
        float GetClipDuration(int clipIndex);

        /// <summary>
        /// Gets the base pose transformation list
        /// </summary>
        /// <returns>Returns the base transformation list</returns>
        IEnumerable<Matrix> GetPoseBase();
        /// <summary>
        /// Gets the transform list of the pose at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipName">Clip mame</param>
        /// <returns>Returns the resulting transform list</returns>
        IEnumerable<Matrix> GetPoseAtTime(float time, string clipName);
        /// <summary>
        /// Gets the transform list of the pose at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipIndex">Clip index</param>
        /// <returns>Returns the resulting transform list</returns>
        IEnumerable<Matrix> GetPoseAtTime(float time, int clipIndex);
        /// <summary>
        /// Gets the transform list of the pose's combination at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipName1">First clip name</param>
        /// <param name="clipName2">Second clip name</param>
        /// <param name="offset1">Time offset for first clip</param>
        /// <param name="offset2">Time offset from second clip</param>
        /// <param name="factor">Interpolation factor</param>
        /// <returns>Returns the resulting transform list</returns>
        IEnumerable<Matrix> GetPoseAtTime(float time, string clipName1, string clipName2, float offset1, float offset2, float factor);
        /// <summary>
        /// Gets the transform list of the pose's combination at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipIndex1">First clip index</param>
        /// <param name="clipIndex2">Second clip index</param>
        /// <param name="offset1">Time offset for first clip</param>
        /// <param name="offset2">Time offset from second clip</param>
        /// <param name="factor">Interpolation factor</param>
        /// <returns>Returns the resulting transform list</returns>
        IEnumerable<Matrix> GetPoseAtTime(float time, int clipIndex1, int clipIndex2, float offset1, float offset2, float factor);

        /// <summary>
        /// Packs current instance into a Vector4 array
        /// </summary>
        /// <returns>Returns the packed skinning data</returns>
        /// <remarks>This method must stay synchronized with InitializeOffsets</remarks>
        IEnumerable<Vector4> Pack();

        /// <summary>
        /// Updates the resource data
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="offset">Offset</param>
        /// <param name="size">Size</param>
        void UpdateResource(uint index, uint offset, uint size);
    }
}
