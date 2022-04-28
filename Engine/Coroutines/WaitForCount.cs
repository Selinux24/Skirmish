using System.Collections;

namespace Engine.Coroutines
{
    /// <summary>
    /// Wait for count routine
    /// </summary>
    /// <see href="https://github.com/rozgo/Unity.Coroutine/blob/master/Coroutine.cs"/>
    public sealed class WaitForCount : YieldInstruction
    {
        /// <summary>
        /// Counter
        /// </summary>
        int count = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="count">Frames to wait</param>
        public WaitForCount(int count) : base()
        {
            this.count = count;
            Routine = Count();
        }

        /// <summary>
        /// Routine
        /// </summary>
        IEnumerator Count()
        {
            while (--count >= 0)
            {
                yield return true;
            }
        }
    }
}
