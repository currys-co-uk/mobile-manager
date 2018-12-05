using System;
using MobileManager.Models.Devices.Enums;

namespace MobileManager.Models.Devices
{
    /// <summary>
    /// Device factory.
    /// </summary>
    public class DeviceFactory
    {
        /// <summary>
        /// New device Factory
        /// </summary>
        /// <returns>The device.</returns>
        /// <param name="id">Identifier.</param>
        /// <param name="name">Name.</param>
        /// <param name="available">If set to <c>true</c> available.</param>
        /// <param name="type">Type.</param>
        /// <param name="status">Status.</param>
        public Device NewDevice(String id, String name, Boolean available, DeviceType type, DeviceStatus status)
        {
            return new Device(id, name, available, type, status);
        }
    }
}
