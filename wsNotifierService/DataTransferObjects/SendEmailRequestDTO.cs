using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace wsNotifierService.DataTransferObjects
{
    public class SendEmailRequestDTO
    {
        public string Body { get;  set; }
        public string Subject { get; set; }

        public List<string> Receivers { get; set; }

        public List<FileAttachment> Attachments { get; set; }

    }


    public class FileAttachment
    {
        public string Base64 { get; set; }
        public string FileName { get; set; }

        public string FileType { get; set; }
    }



}
