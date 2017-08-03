
namespace Engine
{
    /// <summary>
    /// An interface that defines a class containing a cost associated with the instance.
    /// </summary>
    public interface IValueWithCost
    {
        /// <summary>
        /// Gets the cost of this instance.
        /// </summary>
        float TotalCost { get; }
    }
}
