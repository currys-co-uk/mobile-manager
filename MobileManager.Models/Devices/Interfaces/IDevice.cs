using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MobileManager.Models.Devices.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MobileManager.Models.Devices.Interfaces
{
    /// <summary>
    /// Device.
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:MobileManager.Models.Devices.Interfaces.IDevice"/> is available.
        /// </summary>
        /// <value><c>true</c> if available; otherwise, <c>false</c>.</value>
        bool Available { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        DeviceType Type { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        DeviceStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the appium endpoint.
        /// </summary>
        /// <value>The appium endpoint.</value>
        String AppiumEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The properties.</value>
        [NotMapped]
        List<DeviceProperties> Properties { get; set; }
    }
}
