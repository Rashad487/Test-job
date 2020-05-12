using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using wsNotifierService.DataTransferObjects;
using XBLogger;

namespace wsNotifierService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SMSController : ControllerBase
    {
        [HttpPost]
        [Route("SendSms")]
        public SendSmsResponseDTO SendSms([FromBody]SendSmsRequestDTO request)
        {
            SendSmsResponseDTO result = new SendSmsResponseDTO();
            try
            {
                Dictionary<string, object> output = new Dictionary<string, object>() { { "p_result", null } };
                Utils.NewConnection().ExecuteProcedure($"{Constants.ConsPackageName}.SEND_SMS", null, new
                {
                    P_SEND_TO = request.ReceiverPhone,
                    P_TEXT = request.SmsText
                }, output
                   );
                string res = Convert.ToString(output["p_result"]);
                if (res.Contains("OK"))
                {
                    result.IsSuccess = true;
                }else
                {
                    throw new Exception(res);
                }

                
            }
            catch (Exception ex)
            {

                result.IsSuccess = false;
                string guid = Guid.NewGuid().ToString();
                result.Error = new Error() { Code = "XB-SMS-001", Message = "Call Support with this Error ID:" + guid };
                Logger.GetInstance.LogError(ex, LogId: guid);

            }
            return result;
        }
    }
}