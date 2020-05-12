using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wsNotifierService.DataTransferObjects
{
    public class SendSmsRequestDTO
    {
        public string ReceiverPhone { get;  set; }
        public string SmsText { get;  set; }
    }
}
