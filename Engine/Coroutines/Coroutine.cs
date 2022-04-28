using System.Collections;

namespace Engine.Coroutines
{
    /// <summary>
    /// Coroutine base class
    /// </summary>
    /// <see href="https://github.com/rozgo/Unity.Coroutine/blob/master/Coroutine.cs"/>
    public class Coroutine : YieldInstruction
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="routine">Routine</param>
        public Coroutine(IEnumerator routine) : base()
        {
            Routine = routine;
        }
    }
}
