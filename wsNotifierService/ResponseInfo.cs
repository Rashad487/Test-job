using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wsNotifierService
{
    public class ResponseInfo
    {
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string ReqId { get; set; }
        public string ResponseType { get; set; }

        public static ResponseInfo SuccessSesponse(string ResponseType)
        {
            return new ResponseInfo() { Status = "1", ResponseType = ResponseType };
        }

        public static ResponseInfo SuccessResponse(RequestInfo requestInfo)
        {
            return new ResponseInfo() { Status = "1", ResponseType = requestInfo.Type, ReqId = requestInfo.ReqID };
        }

        public static ResponseInfo ErrorResponse(string ErrorCode, string ErrorDescription, RequestInfo requestInfo)
        {
            if (requestInfo == null)
            {
                return new ResponseInfo() { Status = "0", ErrorCode = ErrorCode, ErrorDescription = ErrorDescription, ResponseType = string.Empty, ReqId = string.Empty };
            }
            else
                return new ResponseInfo() { Status = "0", ErrorCode = ErrorCode, ErrorDescription = ErrorDescription, ResponseType = requestInfo.Type, ReqId = requestInfo.ReqID };
        }

    }
}
