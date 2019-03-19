using System;
using System.ComponentModel.DataAnnotations.Schema;
using MobileManager.Models.Devices.Enums;

namespace MobileManager.Models.Reservations
{
    public class Capabilities
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public String DeviceId { get; set; }
        public DeviceType DeviceType { get; set; }
    }
}
