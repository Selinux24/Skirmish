
namespace Engine.Audio
{
    /// <summary>
    /// Game audio event handler
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event arguments</param>
    public delegate void GameAudioHandler(object sender, GameAudioEventArgs e);

    /// <summary>
    /// Game audio progress event handler
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event arguments</param>
    public delegate void GameAudioProgressHandler(object sender, GameAudioProgressEventArgs e);
}
