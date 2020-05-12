using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wsNotifierService
{
    public class RequestInfo
    {
        public string Type { get; set; }
        public string Version { get; set; }
        public string Source { get; set; }
        public string ReqID { get; set; }

    }
}
