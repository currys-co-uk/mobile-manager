namespace MobileManager.Models.Devices.Enums
{
    /// <summary>
    /// Device status.
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>
        /// The unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The online.
        /// </summary>
        Online,

        /// <summary>
        /// The offline.
        /// </summary>
        Offline,

        /// <summary>
        /// The locked.
        /// </summary>
        Locked,

        /// <summary>
        /// The locked offline.
        /// </summary>
        LockedOffline,

        /// <summary>
        /// The initialize.
        /// </summary>
        Initialize,

        /// <summary>
        /// The failed to initialize.
        /// </summary>
        FailedToInitialize
    }
}
