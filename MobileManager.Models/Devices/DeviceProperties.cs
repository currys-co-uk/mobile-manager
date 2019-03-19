using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileManager.Models.Devices
{
    /// <summary>
    /// Device properties.
    /// </summary>
    public class DeviceProperties
    {
        internal DeviceProperties()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Models.Devices.DeviceProperties"/> class.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public DeviceProperties(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[JsonIgnore]
        public String Id { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }
    }
}
