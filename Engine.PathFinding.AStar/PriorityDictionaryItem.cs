
namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Represents an element stored in a priority queue
    /// </summary>
    /// <typeparam name="TValue">Object type</typeparam>
    /// <typeparam name="TPriority">Priority value type</typeparam>
    public class PriorityDictionaryItem<TValue, TPriority>
    {
        /// <summary>
        /// Gets the value of the element
        /// </summary>
        public TValue Value { get; protected set; }
        /// <summary>
        /// Gets the priority associated with this item
        /// </summary>
        public TPriority Priority { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="priority">Priority</param>
        public PriorityDictionaryItem(TValue value, TPriority priority)
        {
            this.Value = value;
            this.Priority = priority;
        }
        /// <summary>
        /// Gets the text representation of the element
        /// </summary>
        /// <returns>Returns a string that represents the object</returns>
        public override string ToString()
        {
            return string.Format("Priority {0}; Value {1}", this.Priority, this.Value);
        }
    }
}
