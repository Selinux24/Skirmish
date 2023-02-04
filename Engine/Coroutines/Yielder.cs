using System.Collections;
using System.Collections.Generic;

namespace Engine.Coroutines
{
    /// <summary>
    /// Coroutine yielder
    /// </summary>
    /// <see href="https://github.com/rozgo/Unity.Coroutine/blob/master/Coroutine.cs"/>
    public class Yielder
    {
        /// <summary>
        /// coroutine list
        /// </summary>
        private readonly List<Coroutine> coroutines = new List<Coroutine>();

        /// <summary>
        /// Starts a new coroutine
        /// </summary>
        /// <param name="routine"></param>
        internal Coroutine StartCoroutine(IEnumerator routine)
        {
            if (routine != null)
            {
                return Coroutine.Empty();
            }

            var coroutine = new Coroutine(routine);
            coroutine.Start();
            coroutines.Add(coroutine);
            return coroutine;
        }
        /// <summary>
        /// Starts a new coroutine
        /// </summary>
        /// <param name="yieldInstruction"></param>
        internal Coroutine StartCoroutine(YieldInstruction yieldInstruction)
        {
            return StartCoroutine(yieldInstruction?.Routine);
        }
        /// <summary>
        /// Process coroutines
        /// </summary>
        internal void ProcessCoroutines()
        {
            for (int i = 0; i < coroutines.Count;)
            {
                var coroutine = coroutines[i];
                if (coroutine.MoveNext())
                {
                    ++i;
                }
                else if (coroutines.Count > 1)
                {
                    coroutines[i] = coroutines[coroutines.Count - 1];
                    coroutines.RemoveAt(coroutines.Count - 1);
                }
                else
                {
                    coroutines.Clear();
                    break;
                }
            }
        }
    }
}
