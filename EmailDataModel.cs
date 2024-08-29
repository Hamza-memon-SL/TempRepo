using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Alert_2.Model
{
    public partial class EmailDataModel
    {
        public string? JobDate { get; set; }
        public string? TotalPNS { get; set; }
        public string? TotalBatch { get; set; }
        public string? PushNotificationSuccessful { get; set; }
        public string? PushNotificationFailed { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public bool IsSuccess { get; set; }
    }
}
