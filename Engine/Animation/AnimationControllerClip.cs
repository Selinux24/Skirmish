
namespace Engine.Animation
{
    /// <summary>
    /// Animation controller clip
    /// </summary>
    public struct AnimationControllerClip
    {
        /// <summary>
        /// Animation clip index in the skinning data animation list
        /// </summary>
        public int Index;
        /// <summary>
        /// Clip duration in the controller
        /// </summary>
        /// <remarks>Not the clip duration in the skinning data animation list</remarks>
        public float Duration;
        /// <summary>
        /// Do loop
        /// </summary>
        public bool Loop;
    }
}
