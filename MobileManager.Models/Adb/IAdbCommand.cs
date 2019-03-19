namespace MobileManager.Models.Adb
{
    /// <summary>
    /// Adb command.
    /// </summary>
    public interface IAdbCommand
    {
        /// <summary>
        /// Android device Id.
        /// </summary>
        string AndroidDeviceId { get; set; }

        /// <summary>
        /// Gets the adb command.
        /// </summary>
        /// <value>The command.</value>
        string Command { get; set; }
    }
}
