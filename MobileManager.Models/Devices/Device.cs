using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MobileManager.Models.Devices.Enums;
using MobileManager.Models.Devices.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MobileManager.Models.Devices
{
    /// <inheritdoc />
    /// <summary>
    /// Device.
    /// </summary>
    public class Device : IDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Devices.Device"/> class.
        /// </summary>
        internal Device()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Devices.Device"/> class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="name">Name.</param>
        /// <param name="available">If set to <c>true</c> available.</param>
        /// <param name="type">Type.</param>
        /// <param name="status">Status.</param>
        public Device(string id, string name, bool available, DeviceType type, DeviceStatus status)
        {
            Id = id;
            Name = name;
            Available = available;
            Type = type;
            Status = status;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        public string Id { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:MobileManager.Devices.Device" /> is available.
        /// </summary>
        /// <value><c>true</c> if available; otherwise, <c>false</c>.</value>
        public bool Available { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceType Type { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceStatus Status { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the appium endpoint.
        /// </summary>
        /// <value>The appium endpoint.</value>
        public string AppiumEndpoint { get; set; } = "";

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the devices properties.
        /// </summary>
        /// <value>The requested devices.</value>
        [Required] //<======= Forces Cascade delete
        [ForeignKey("DeviceRefId")]
        public List<DeviceProperties> Properties { get; set; }

        public override string ToString()
        {
            return $"Id: [{Id}], Name: [{Name}], Type: [{Type}], Status: [{Status}], Available: [{Available}]";
        }
    }
}
