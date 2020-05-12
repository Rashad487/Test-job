using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace wsNotifierService
{

    public class Utils
    {
        public static string getSettingsValue(string rootKey, string subKey)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);
            var root = configurationBuilder.Build();
            return root.GetSection(rootKey).GetSection(subKey).Value;
        }

        public static string GetConnectionString()
        {
            return getSettingsValue("ConnectionStrings", "dbConnection");
        }

        public static SecureOracleConnection NewConnection()
        {
            return new SecureOracleConnection(GetConnectionString());
        }

        public SecureOracleConnection secureOracleConnection()
        {
            return new SecureOracleConnection(GetConnectionString());
        }

        public static XmlDocument ServiceResponse(string body, ResponseInfo responseInfo)
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml($@"
            <mProgress>
               <Response>
                  <ResponseInfo>
                     <Status>{responseInfo.Status}</Status>
                     <ErrorCode>{responseInfo.ErrorCode}</ErrorCode>
                     <ErrorDescription>{responseInfo.ErrorDescription}</ErrorDescription>
                     <ReqId>{responseInfo.ReqId}</ReqId>
                     <ResponseType>{responseInfo.ResponseType}</ResponseType>
                  </ResponseInfo>
                  <ResponseData>
                    {body}
                  </ResponseData>
               </Response>
            </mProgress>
            ");
            return doc;
        }



    }
}
