using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace wsNotifierService
{
    public static class ExtensionUtilMethods
    {
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        public static RequestInfo RequestInfo(this XDocument doc)
        {

            RequestInfo info = (from m in doc.Element("mProgress").Element("Request").Elements("RequestInfo")
                select new RequestInfo
                {
                    Type = m.Element("Type").Value,
                    ReqID = m.Element("ReqId").Value,
                    Source = m.Element("Source").Value,
                    Version = m.Element("Version").Value

                }).Single();
            return info;
        }

        public static string ToXmlTag(this object value, string tag)
        {
            if (value == null || value == DBNull.Value)
            {
                return $"<{tag} />";
            }
            else
            {
                return $"<{tag}>{value}</{tag}>";
            }
        }

        public static string ToXmlTagString(this object value, string tag)
        {
            if (value == null || value == DBNull.Value)
            {
                return $"<{tag} />";
            }
            else
            {
                return $"<{tag}>{ReplaceBadCharacters(Convert.ToString(value))}</{tag}>";
            }
        }

        private static object ReplaceBadCharacters(string v)
        {
            string result = v;

            result = result.Replace(">", string.Empty).Replace("<", string.Empty).Replace("&", "and");

            return result;
        }

        public static string ToSameXmlTagString(this DataRow row, string tag)
        {
            return row[tag].ToXmlTagString(tag);

        }

        public static string ToStringNoNull(this object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return $"";
            }
            else
            {
                return value.ToString();
            }
        }

        public static DateTime ToDate(this string value)
        {
            if (value == null) return DateTime.MinValue;
            return DateTime.ParseExact(value, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        }


    }
}
