using System.Collections;

namespace Engine.Coroutines
{
    /// <summary>
    /// Empty routine that does nothing
    /// </summary>
    public sealed class EmptyCoroutine : YieldInstruction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EmptyCoroutine() : base()
        {
            Routine = Run();
        }

        /// <summary>
        /// Routine
        /// </summary>
        IEnumerator Run()
        {
            yield return true;
        }
    }
}
