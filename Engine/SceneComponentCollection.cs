using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Engine
{
    /// <summary>
    /// Scene component collection
    /// </summary>
    public class SceneComponentCollection : IDisposable
    {
        /// <summary>
        /// Collection updated event
        /// </summary>
        public event SceneComponentCollectionUpdatedEventHandler Updated;

        /// <summary>
        /// Components sorting
        /// </summary>
        /// <param name="p1">First component</param>
        /// <param name="p2">Second component</param>
        /// <remarks>
        /// This sorting logic is used by the scene renderers:
        /// - First drawables
        /// - Then drawing layer
        /// - Then blend mode (opaques first)
        /// - Then z.buffer writers (writers first)
        /// </remarks>
        private static int SortComponents(ISceneObject p1, ISceneObject p2)
        {
            //First by type
            bool p1D = p1 is IDrawable;
            bool p2D = p2 is IDrawable;
            int i = p1D.CompareTo(p2D);
            if (i != 0)
            {
                return -i;
            }

            if (!p1D || !p2D)
            {
                return 0;
            }

            IDrawable drawable1 = (IDrawable)p1;
            IDrawable drawable2 = (IDrawable)p2;

            //Then by layer
            i = drawable1.Layer.CompareTo(drawable2.Layer);
            if (i != 0) return i;

            //Then by blend mode
            i = drawable1.BlendMode.CompareTo(drawable2.BlendMode);
            if (i != 0) return i;

            //Then by z-buffer writers
            i = -drawable1.DepthEnabled.CompareTo(drawable2.DepthEnabled);

            return i;
        }

        /// <summary>
        /// Scene component list
        /// </summary>
        private readonly List<ISceneObject> internalComponents = new();
        /// <summary>
        /// Gets the component count
        /// </summary>
        public int Count { get => internalComponents.Count; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        public SceneComponentCollection()
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SceneComponentCollection()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                internalComponents
                    .OfType<IDisposable>()
                    .ToList()
                    .ForEach(x => x.Dispose());

                internalComponents.Clear();
            }
        }

        /// <summary>
        /// Adds component to collection
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <param name="component">Component</param>
        /// <param name="usage">Usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the added component</returns>
        public void AddComponent(ISceneObject component, SceneObjectUsages usage, int layer)
        {
            if (component == null)
            {
                return;
            }

            Monitor.Enter(internalComponents);
            try
            {
                if (internalComponents.Contains(component))
                {
                    return;
                }

                if (internalComponents.Exists(c => component.Id == c.Id))
                {
                    throw new EngineException($"{nameof(SceneComponentCollection)} => The specified component id [{component.Id}] already exists.");
                }

                component.Usage = usage;
                component.Layer = layer;

                internalComponents.Add(component);

                if (internalComponents.Count > 1)
                {
                    internalComponents.Sort(SortComponents);
                }
            }
            catch (EngineException)
            {
                throw;
            }
            finally
            {
                Monitor.Exit(internalComponents);

                FireUpdated(true, new[] { component });
            }
        }
        /// <summary>
        /// Removes and disposes the specified component
        /// </summary>
        /// <param name="component">Component</param>
        public void RemoveComponent(ISceneObject component)
        {
            if (component == null)
            {
                return;
            }

            if (!internalComponents.Contains(component))
            {
                return;
            }

            Monitor.Enter(internalComponents);
            try
            {
                internalComponents.Remove(component);
            }
            finally
            {
                Monitor.Exit(internalComponents);

                FireUpdated(false, new[] { component });
            }

            if (component is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        /// <summary>
        /// Removes and disposes the specified component list
        /// </summary>
        /// <param name="components">List of components</param>
        public void RemoveComponents(IEnumerable<ISceneObject> components)
        {
            if (components?.Any() != true)
            {
                return;
            }

            Monitor.Enter(internalComponents);
            try
            {
                foreach (var component in components)
                {
                    if (internalComponents.Contains(component))
                    {
                        internalComponents.Remove(component);
                    }
                }
            }
            finally
            {
                Monitor.Exit(internalComponents);

                FireUpdated(false, components);
            }

            foreach (var disposable in components.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
        /// <summary>
        /// Fires the updated collection event
        /// </summary>
        /// <param name="added">Added or removed</param>
        /// <param name="components">Component list</param>
        private void FireUpdated(bool added, IEnumerable<ISceneObject> components)
        {
            if (added)
            {
                Updated?.Invoke(this, new SceneComponentCollectionUpdatedEventArgs() { Added = components });
            }
            else
            {
                Updated?.Invoke(this, new SceneComponentCollectionUpdatedEventArgs() { Removed = components });
            }
        }

        /// <summary>
        /// Gets the component collection
        /// </summary>
        /// <returns>Returns the component collection</returns>
        public IEnumerable<ISceneObject> Get()
        {
            return internalComponents
                .AsReadOnly();
        }
        /// <summary>
        /// Gets the component collection which validates the predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Returns the component collection which validates the predicate</returns>
        public IEnumerable<ISceneObject> Get(Predicate<ISceneObject> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return internalComponents
                .FindAll(predicate)
                .AsReadOnly();
        }
        /// <summary>
        /// Gets the component collection which has the specified usage flag
        /// </summary>
        /// <param name="usage">Usage</param>
        /// <returns>Returns the component collection which has the specified usage flag</returns>
        public IEnumerable<ISceneObject> Get(SceneObjectUsages usage)
        {
            if (usage == SceneObjectUsages.None)
            {
                return Get();
            }

            return internalComponents
                .FindAll(c => c.Usage == usage)
                .AsReadOnly();
        }
        /// <summary>
        /// Gets the component collection which has the specified usage flag and  validates the predicate
        /// </summary>
        /// <param name="usage">Usage</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Returns the component collection which has the specified usage flag and  validates the predicate</returns>
        public IEnumerable<ISceneObject> Get(SceneObjectUsages usage, Predicate<ISceneObject> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (usage == SceneObjectUsages.None)
            {
                return Get(predicate);
            }

            return internalComponents
                .FindAll(c => c.Usage == usage && (predicate?.Invoke(c) ?? true))
                .AsReadOnly();
        }

        /// <summary>
        /// Gets the component collection
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>Returns the component collection</returns>
        public IEnumerable<T> Get<T>()
        {
            return internalComponents
                .OfType<T>()
                .ToArray();
        }
        /// <summary>
        /// Gets the component collection which validates the predicate
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="predicate">Predicate</param>
        /// <returns>Returns the component collection which validates the predicate</returns>
        public IEnumerable<T> Get<T>(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return internalComponents
                .OfType<T>()
                .Where(predicate)
                .ToArray();
        }
        /// <summary>
        /// Gets the component collection which has the specified usage flag
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="usage">Usage</param>
        /// <returns>Returns the component collection which has the specified usage flag</returns>
        public IEnumerable<T> Get<T>(SceneObjectUsages usage)
        {
            if (usage == SceneObjectUsages.None)
            {
                return Get<T>();
            }

            return internalComponents
                .Where(c => c.Usage == usage)
                .OfType<T>()
                .ToArray();
        }
        /// <summary>
        /// Gets the component collection which has the specified usage flag and  validates the predicate
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="usage">Usage</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Returns the component collection which has the specified usage flag and  validates the predicate</returns>
        public IEnumerable<T> Get<T>(SceneObjectUsages usage, Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (usage == SceneObjectUsages.None)
            {
                return Get(predicate);
            }

            return internalComponents
                .Where(c => c.Usage == usage)
                .OfType<T>()
                .Where(predicate)
                .ToArray();
        }

        /// <summary>
        /// Gets the first component in the collection
        /// </summary>
        /// <returns>Returns the first component in the collection</returns>
        public ISceneObject First()
        {
            return internalComponents
                .FirstOrDefault();
        }
        /// <summary>
        /// Gets the first component in the collection which validates de predicate
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>Returns the first component in the collection which validates de predicate</returns>
        public ISceneObject First(Predicate<ISceneObject> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return internalComponents
                .Find(predicate);
        }
        /// <summary>
        /// Gets the first component in the collection which has the specified usage flag
        /// </summary>
        /// <param name="usage">Usage</param>
        /// <returns>Returns the first component in the collection which has the specified usage flag</returns>
        public ISceneObject First(SceneObjectUsages usage)
        {
            if (usage == SceneObjectUsages.None)
            {
                return First();
            }

            return internalComponents
                .Find(c => c.Usage == usage);
        }
        /// <summary>
        /// Gets the first component in the collection which has the specified usage flag, and validates de predicate
        /// </summary>
        /// <param name="usage">Usage</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Returns the first component in the collection which has the specified usage flag, and validates de predicate</returns>
        public ISceneObject First(SceneObjectUsages usage, Predicate<ISceneObject> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (usage == SceneObjectUsages.None)
            {
                return First(predicate);
            }

            return internalComponents
                .Find(c => c.Usage == usage && (predicate?.Invoke(c) ?? true));
        }

        /// <summary>
        /// Gets the first component of the specified type in the collection
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>Returns the first component of the specified type in the collection</returns>
        public T First<T>()
        {
            return internalComponents
                .OfType<T>()
                .FirstOrDefault();
        }
        /// <summary>
        /// Gets the first component of the specified type in the collection which validates de predicate
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="predicate">Predicate</param>
        /// <returns>Returns the first component of the specified type in the collection which validates de predicate</returns>
        public T First<T>(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return internalComponents
                .OfType<T>()
                .FirstOrDefault(predicate);
        }
        /// <summary>
        /// Gets the first component of the specified type in the collection which has the specified usage flag
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="usage">Usage</param>
        /// <returns>Returns the first component of the specified type in the collection which has the specified usage flag</returns>
        public T First<T>(SceneObjectUsages usage)
        {
            if (usage == SceneObjectUsages.None)
            {
                return First<T>();
            }

            return internalComponents
                .Where(c => c.Usage == usage)
                .OfType<T>()
                .FirstOrDefault();
        }
        /// <summary>
        /// Gets the first component of the specified type in the collection which has the specified usage flag, and validates de predicate
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="usage">Usage</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Returns the first component of the specified type in the collection which has the specified usage flag, and validates de predicate</returns>
        public T First<T>(SceneObjectUsages usage, Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (usage == SceneObjectUsages.None)
            {
                return First(predicate);
            }

            return internalComponents
                .Where(c => c.Usage == usage)
                .OfType<T>()
                .FirstOrDefault(predicate);
        }

        /// <summary>
        /// Gets the component in the collection by id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Returns the component in the collection by id</returns>
        public ISceneObject ById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            return internalComponents
                .Find(c => c.Id == id);
        }
        /// <summary>
        /// Gets the component in the collection by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Returns the component in the collection by name</returns>
        public ISceneObject ByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return internalComponents
                .Find(c => c.Name == name);
        }
        /// <summary>
        /// Gets the component list in the collection by owner
        /// </summary>
        /// <param name="owner">Owner</param>
        /// <returns>Returns the component list in the collection by owner</returns>
        public IEnumerable<ISceneObject> ByOwner(ISceneObject owner)
        {
            if (owner == null)
            {
                return Enumerable.Empty<ISceneObject>();
            }

            return internalComponents
                .FindAll(c => c.Owner == owner)
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Updated event handler
    /// </summary>
    public delegate void SceneComponentCollectionUpdatedEventHandler(object sender, SceneComponentCollectionUpdatedEventArgs e);

    /// <summary>
    /// Updated event arguments
    /// </summary>
    public class SceneComponentCollectionUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Added components
        /// </summary>
        public IEnumerable<ISceneObject> Added { get; set; }
        /// <summary>
        /// Removed components
        /// </summary>
        public IEnumerable<ISceneObject> Removed { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SceneComponentCollectionUpdatedEventArgs() : base()
        {

        }
    }
}
