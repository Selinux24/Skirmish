namespace Engine
{
    /// <summary>
    /// Volume interface
    /// </summary>
    public interface IVolume
    {
        /// <summary>
        /// Gets the volume geometry of the instance
        /// </summary>
        /// <param name="full">Gets full geometry</param>
        /// <returns>Returns the volume geometry of the instance</returns>
        Triangle[] GetVolume(bool full);
    }
}
