
namespace Engine.Physics
{
    /// <summary>
    /// Contact generator interface
    /// </summary>
    public interface IContactGenerator
    {
        /// <summary>
        /// Generate contacts between the bodies included in the contact generator
        /// </summary>
        /// <param name="contactData">Contact data</param>
        /// <param name="limit">Limit of contacts to generate</param>
        /// <returns>Returns the number of contacts generated</returns>
        bool AddContact(ContactResolver contactData, int limit);
    }
}
