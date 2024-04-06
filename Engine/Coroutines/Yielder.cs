using Engine.Common;
using System.Collections;
using System.Collections.Generic;

namespace Engine.Coroutines
{
    /// <summary>
    /// Coroutine yielder
    /// </summary>
    /// <see href="https://github.com/rozgo/Unity.Coroutine/blob/master/Coroutine.cs"/>
    public class Yielder : IUpdatable
    {
        /// <summary>
        /// coroutine list
        /// </summary>
        private readonly List<Coroutine> coroutines = new();

        /// <inheritdoc/>
        public string Id { get; private set; }
        /// <inheritdoc/>
        public string Name { get; set; }
        /// <inheritdoc/>
        public bool Active { get; set; } = true;
        /// <inheritdoc/>
        public Scene Scene { get; private set; }
        /// <inheritdoc/>
        public SceneObjectUsages Usage { get; set; } = SceneObjectUsages.None;
        /// <inheritdoc/>
        public int Layer { get; set; } = 1;
        /// <inheritdoc/>
        public bool HasOwner { get; private set; }
        /// <inheritdoc/>
        public ISceneObject Owner { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Yielder(Scene scene)
        {
            Name = Id = $"{nameof(Yielder)}";

            Scene = scene;
        }

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
            int i = 0;
            while (i < coroutines.Count)
            {
                var coroutine = coroutines[i];
                if (coroutine.MoveNext())
                {
                    ++i;
                }
                else if (coroutines.Count > 1)
                {
                    coroutines[i] = coroutines[^1];
                    coroutines.RemoveAt(coroutines.Count - 1);
                }
                else
                {
                    coroutines.Clear();
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public void EarlyUpdate(UpdateContext context)
        {
            ProcessCoroutines();
        }
        /// <inheritdoc/>
        public void Update(UpdateContext context)
        {
            //Not applicable
        }
        /// <inheritdoc/>
        public void LateUpdate(UpdateContext context)
        {
            //Not applicable
        }
    }
}
