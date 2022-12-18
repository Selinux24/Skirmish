using System.Collections;

namespace Engine.Coroutines
{
    /// <summary>
    /// Yield instruction
    /// </summary>
    /// <see href="https://github.com/rozgo/Unity.Coroutine/blob/master/Coroutine.cs"/>
    public class YieldInstruction
    {
        /// <summary>
        /// Internal routine
        /// </summary>
        internal IEnumerator Routine { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected YieldInstruction()
        {

        }

        /// <summary>
        /// Starts the routine
        /// </summary>
        internal void Start()
        {
            Routine?.MoveNext();
        }
        /// <summary>
        /// Moves to the next iteration
        /// </summary>
        internal bool MoveNext()
        {
            if (Routine == null)
            {
                return false;
            }

            if (Routine.Current is YieldInstruction yieldInstruction)
            {
                if (yieldInstruction.MoveNext())
                {
                    return true;
                }
                else if (Routine.MoveNext())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Routine.MoveNext())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
