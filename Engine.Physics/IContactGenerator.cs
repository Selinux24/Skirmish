
namespace Engine.Physics
{
    /// <summary>
    /// Contact generator interface
    /// </summary>
    public interface IContactGenerator
    {
        /// <summary>
        /// First end-point
        /// </summary>
        IContactEndPoint One { get; set; }
        /// <summary>
        /// Second end-point
        /// </summary>
        IContactEndPoint Two { get; set; }

        /// <summary>
        /// Gets whether the contact generator is active or not
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Generate contacts between the bodies included in the contact generator
        /// </summary>
        /// <param name="contactData">Contact data</param>
        /// <param name="limit">Limit of contacts to generate</param>
        /// <returns>Returns the number of contacts generated</returns>
        bool AddContact(ContactResolver contactData, int limit);
    }
}
