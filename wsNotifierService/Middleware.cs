using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using XBLogger;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;


/// <summary>
/// VERSION 1.0.4
/// ADDED LOGID TO ROOT ERROR  HANDLER
/// 
/// VERSION 1.0.3
/// LAST CHANGED 19.11.2019
/// </summary>

namespace XBAspNetCoreMiddleware
{
    public class AppSettings<T> where T : new()
    {
        private const string DEFAULT_FILENAME = "xb_aspnet_core_settings.xml";

        public void Save(string fileName = DEFAULT_FILENAME)
        {
            var serializer = new XmlSerializer(this.GetType());
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
            using (var writer = XmlWriter.Create(fileName, settings))
            {
                serializer.Serialize(writer, this);
            }
        }



        public T Load(string fileName = DEFAULT_FILENAME)
        {
            T t = new T();
            if (File.Exists(fileName))
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var reader = XmlReader.Create(fileName))
                {
                    t = (T)serializer.Deserialize(reader);
                }
                return t;
            }
            else
            {
                Save();
                return Load();
            }
        }



    }

    public interface ISettings
    {
        bool IsMiddlewareEnabled { get; }
        bool IsRootExceptionHandlingEnabled { get; }
        bool IsFullLoggingEnabled(string path);

    }
    public class Settings : AppSettings<Settings>, ISettings
    {
        public bool IsMiddlewareEnabled { get; set; } = true;
        public bool IsRootExceptionHandlingEnabled { get; set; } = true;

        public List<string> FullLogEnabledPaths { get; set; } = new List<string> { "/api/somepathtobeinculude" };

        public bool IsFullLoggingEnabled(string path)
        {
            return FullLogEnabledPaths.Contains(path) || FullLogEnabledPaths.Contains("*")|| EnabledByLike(path);
        }

        private bool EnabledByLike(string path)
        {
            try
            {
                foreach (var item in FullLogEnabledPaths)
                {
                    if (item.StartsWith("*") && item.EndsWith("*"))
                    {
                        if (path.Contains(item.Replace("*", "")))
                        {
                            return true;
                        }
                    }
                    else if (item.StartsWith("*"))
                    {
                        if (path.EndsWith(item.Replace("*", "")))
                        {
                            return true;
                        }
                    }
                    else if (item.EndsWith("*"))
                    {
                        if (path.StartsWith(item.Replace("*", "")))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.GetInstance.LogError(ex);
            }

            return false;
        }
    }

    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ISettings _settings;

        public ApiLoggingMiddleware(RequestDelegate next, ISettings settings)
        {
            _next = next;
            _settings = settings;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var originalBodyStream = httpContext.Response.Body;
            try
            {
                var request = httpContext.Request;
                if (request.Path.StartsWithSegments(new PathString("/api")))
                {
                    var stopWatch = Stopwatch.StartNew();
                    var requestTime = DateTime.Now;
                    var requestBodyContent = await ReadRequestBody(request);
                    originalBodyStream = httpContext.Response.Body;
                    using (var responseBody = new MemoryStream())
                    {
                        var response = httpContext.Response;
                        response.Body = responseBody;
                        await _next(httpContext);
                        stopWatch.Stop();

                        string responseBodyContent = null;
                        responseBodyContent = await ReadResponseBody(response);
                        await responseBody.CopyToAsync(originalBodyStream);
                        string remoteIpAddress = GetIp(request);

                        await SafeLog(requestTime,
                            stopWatch.ElapsedMilliseconds,
                            response.StatusCode,
                            request.Method,
                            request.Path,
                            request.QueryString.ToString(),
                            requestBodyContent,
                            responseBodyContent,
                            remoteIpAddress);

                    }
                }
                else
                {
                    await _next(httpContext);
                    var responseBody = new MemoryStream();
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            catch (Exception ex)
            {
                Logger.GetInstance.LogError(ex);
                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseBody = new MemoryStream();
                await responseBody.CopyToAsync(originalBodyStream);
            }

        }

        private static string GetIp(HttpRequest request)
        {
            try
            {
                string remoteIpAddress = request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                if (request.Headers.ContainsKey("X-Forwarded-For"))
                    remoteIpAddress = request.Headers["X-Forwarded-For"];
                return remoteIpAddress;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();//.EnableRewind();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);
            return bodyAsText;
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var bodyAsText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return bodyAsText;
        }



        private async Task SafeLog(DateTime requestTime,
                            long responseMillis,
                            int statusCode,
                            string method,
                            string path,
                            string queryString,
                            string requestBody,
                            string responseBody,
                            string ipaddress)
        {
            try
            {
                string request_xml = string.Empty;
                if (_settings.IsFullLoggingEnabled(path))
                {
                    request_xml = string.Format(
                  @"<Request>
                       <RequestTime>{0}</RequestTime>
                       <ResponseMillis>{1}</ResponseMillis>
                       <StatusCode>{2}</StatusCode>
                       <Method>{3}</Method>
                       <Path>{4}</Path>
                       <QueryString>{5}</QueryString>  
                       <IpAddress>{6}</IpAddress>
                       <RequestBody><![CDATA[{7}]]></RequestBody>
                       <ResponseBody><![CDATA[{8}]]></ResponseBody>
                    </Request>
                ", requestTime, responseMillis, statusCode, method, path, queryString, ipaddress, requestBody, responseBody);
                }
                else
                {
                    request_xml = string.Format(
                   @"<Request>
                       <RequestTime>{0}</RequestTime>
                       <ResponseMillis>{1}</ResponseMillis>
                       <StatusCode>{2}</StatusCode>
                       <Method>{3}</Method>
                       <Path>{4}</Path>
                       <QueryString>{5}</QueryString>   
                       <IpAddress>{6}</IpAddress>
                    </Request>
                ", requestTime, responseMillis, statusCode, method, path, queryString, ipaddress);

                }
                Logger.GetInstance.LogEvent("ServiceResultLogs", EventType.Info, request_xml);
            }
            catch (Exception ex)
            {
                Logger.GetInstance.LogError(ex);
            }
           


        }


    }


    public static class XBMiddleware
    {
        public static void RegisterXBMiddleware(this IApplicationBuilder app)
        {
            try
            {

                ISettings _settings = new Settings().Load();

                Logger.GetInstance.LogEvent("RegisterXBMiddleware", EventType.Info, "");

                if (_settings.IsMiddlewareEnabled)
                {
                    app.UseMiddleware<ApiLoggingMiddleware>(_settings);
                }

                if (_settings.IsRootExceptionHandlingEnabled)
                {
                    app.UseExceptionHandler(a => a.Run(async context =>
                    {
                        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                        var exception = feature.Error;
                        string guid = Guid.NewGuid().ToString();
                        Logger.GetInstance.LogError(exception,LogId: guid);
                        var result = JsonConvert.SerializeObject(new { XBRootHandledError = "Send this LogId for analyzing eror details. LogId:"+ guid });
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(result);
                    }));
                }
            }
            catch (System.Exception ex)
            {
                Logger.GetInstance.LogError(ex);
            }




        }
    }



}
