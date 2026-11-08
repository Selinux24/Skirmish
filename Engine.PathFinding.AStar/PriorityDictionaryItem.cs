
namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Represents an element stored in a priority queue
    /// </summary>
    /// <typeparam name="TValue">Object type</typeparam>
    /// <typeparam name="TPriority">Priority value type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="value">Value</param>
    /// <param name="priority">Priority</param>
    public class PriorityDictionaryItem<TValue, TPriority>(TValue value, TPriority priority)
    {
        /// <summary>
        /// Gets the value of the element
        /// </summary>
        public TValue Value { get; protected set; } = value;
        /// <summary>
        /// Gets the priority associated with this item
        /// </summary>
        public TPriority Priority { get; protected set; } = priority;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Priority {Priority}; Value {Value}";
        }
    }
}
