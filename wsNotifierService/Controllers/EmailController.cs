using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using XBLogger;
using static wsNotifierService.Utils;
using static wsNotifierService.Constants;
using Microsoft.AspNetCore.Mvc;
using wsNotifierService.DataTransferObjects;

namespace wsNotifierService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [FormatFilter]
    public class EmailController : ControllerBase
    {

        [HttpPost]
        [Route("SendEmail")]
        public SendEmailResponseDTO SendEmail([FromBody]SendEmailRequestDTO request)
        {
            
            List<string> ConvertedAttachments = new List<string>();
            ConvertedAttachments = ConvertAttachment(request.Attachment);

            SendEmailResponseDTO result = new SendEmailResponseDTO();
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("10.100.0.140");
                Attachment attachment;

                mail.From = new MailAddress("noreply@xalqbank.az");
                mail.To.Add(string.Join(", ", request.Receivers.ToArray()));
                mail.Subject = request.Subject;
                mail.Body = request.Body;
                List<MemoryStream> mss=new List<MemoryStream>();
                foreach (FileAttachment att in request.Attachments)
                {
                    MemoryStream ms = new MemoryStream(Convert.FromBase64String(att.Base64));
                    Attachment k = new Attachment(ms, att.FileName+"."+ att.FileType);
                    mail.Attachments.Add(k);
                    mss.Add(ms);
                }
                SmtpServer.Port = 25;
                SmtpServer.Send(mail);
                foreach (var item in mss)
                {
                    item.Dispose();
                }

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {

                result.IsSuccess = false;
                string guid = Guid.NewGuid().ToString();
                result.Error = new Error() {Code="XB-EMAIL-001",Message="Call Support with this Error ID:"+ guid };
                Logger.GetInstance.LogError(ex, LogId: guid);

            }
            return result;
        }

        public List<string> ConvertAttachment(List<string> Attachments)
        {
            List<string> ConvertedAttachments = new List<string>();

            foreach (string att in Attachments)
            {
                ConvertedAttachments.Add(Convert.FromBase64String(att).ToString());
            }
            return ConvertedAttachments;
        }



        [HttpPost]
        [Route("SendEmailIB")]
        public XmlDocument SendEmailIB([FromBody] XmlDocument value)
        {
            XDocument document = value.ToXDocument();
            RequestInfo requestInfo = document.RequestInfo();

            try
            {
                if (requestInfo.Type == "SEND_EMAIL")
                {
                    SendEmailRequestDTO request=new SendEmailRequestDTO();
                    request.Body = document.XPathSelectElement($"{ConsRequestDatapath}/Body").Value;
                    request.Subject = document.XPathSelectElement($"{ConsRequestDatapath}/Subject").Value;

                    request.Receivers = (from m in document.XPathSelectElements($"{ConsRequestDatapath}/Receivers/string") select m.Value).ToList();

                    request.Attachments= (from m in document.XPathSelectElements($"{ConsRequestDatapath}/Attachments/FileAttachment") select new FileAttachment
                    {
                        FileName = m.Element("FileName").Value,
                        Base64 = m.Element("Base64").Value,
                        FileType = m.Element("FileType").Value
                    }).ToList();

                    SendEmailResponseDTO response = SendEmail(request);
                    if (response.IsSuccess)
                    {
                        return ServiceResponse("", ResponseInfo.SuccessResponse(requestInfo));
                    }
                    else
                    {
                        return ServiceResponse(string.Empty, ResponseInfo.ErrorResponse("APP-ERR-003", response.Error.Message, requestInfo));
                    }

                }
                return ServiceResponse(string.Empty, ResponseInfo.ErrorResponse("APP-ERR-002", $"Invalid Type ({requestInfo.Type})", requestInfo));
            }
            catch (Exception ex)
            {
                return ServiceResponse(string.Empty, ResponseInfo.ErrorResponse("APP-ERR-001", ex.Message, requestInfo));
            }

        }





    }
}