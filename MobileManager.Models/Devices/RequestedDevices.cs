using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MobileManager.Models.Devices.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MobileManager.Models.Devices
{
    /// <summary>
    /// Requested devices.
    /// </summary>
    public class RequestedDevices
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>The device identifier.</value>
        public string DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        /// <value>The type of the device.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the name of the device.
        /// </summary>
        /// <value>The name of the device.</value>
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the device properties
        /// </summary>
        public List<DeviceProperties> Properties { get; set; }

        /// <summary>
        /// Gets or sets the reservation reference identifier.
        /// </summary>
        /// <value>The reservation reference identifier.</value>
        [JsonIgnore]
        [ForeignKey("ReservationId")]
        public string ReservationId { get; set; }
    }
}
