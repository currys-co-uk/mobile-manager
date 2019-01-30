using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MobileManager.Models.Logger
{
    public class LogMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ThreadId { get; set; }

        public DateTime Time { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogLevel LogLevel { get; set; }

        public string Message { get; set; }

        public string MethodName { get; set; }

        public string Exception { get; set; }
    }
}
