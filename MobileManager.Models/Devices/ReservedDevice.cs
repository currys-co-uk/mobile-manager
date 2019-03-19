using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MobileManager.Models.Devices.Interfaces;
using Newtonsoft.Json;

namespace MobileManager.Models.Devices
{
    /// <summary>
    /// Reserved device.
    /// </summary>
    public class ReservedDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Models.Devices.ReservedDevice"/> class.
        /// </summary>
        /// <param name="device">Device.</param>
        public ReservedDevice(IDevice device)
        {
            DeviceId = device.Id;
            AppiumEndpoint = device.AppiumEndpoint;
        }

        /// <inheritdoc />
        public ReservedDevice()
        {
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public String Id { get; set; }

        /// <summary>
        /// Gets or sets the reservation applied reference identifier.
        /// </summary>
        /// <value>The reservation applied reference identifier.</value>
        [JsonIgnore]
        [ForeignKey("ReservationAppliedId")]
        public String ReservationAppliedId { get; set; }

        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>The device identifier.</value>
        public String DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the appium endpoint.
        /// </summary>
        /// <value>The appium endpoint.</value>
        public String AppiumEndpoint { get; set; }
    }
}
