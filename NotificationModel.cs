using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Alert_2.Model
{
    public partial class MessageDataModel
    {
        public Message? Messages { get; set; }
        public string? MobileNo { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AccountNo { get; set; }
        public string? AlertID { get; set; }

    }
}
