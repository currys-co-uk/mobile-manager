namespace MobileManager.Models.Adb
{
    /// <inheritdoc />
    /// <summary>
    /// Adb command.
    /// </summary>
    public class AdbCommand : IAdbCommand
    {
        /// <inheritdoc />
        public string AndroidDeviceId { get; set; }

        /// <inheritdoc />
        public string Command { get; set; }
    }
}
