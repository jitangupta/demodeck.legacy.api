using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Newtonsoft.Json;

namespace Demodeck.Legacy.Api.Infrastructure
{
    /// <summary>
    /// Simple console logger for Kubernetes deployment.
    /// Outputs JSON-formatted logs to stdout (Info/Warning) and stderr (Error/Fatal).
    /// LogMonitor captures these for Kubernetes log aggregation.
    /// </summary>
    public static class AppLogger
    {
        private static readonly string ServiceName;
        private static readonly string Environment;
        private static readonly string Version;

        static AppLogger()
        {
            ServiceName = System.Environment.GetEnvironmentVariable("AppSettings__ServiceName")
                ?? ConfigurationManager.AppSettings["ServiceName"]
                ?? "Demodeck.Legacy.Api";

            Environment = System.Environment.GetEnvironmentVariable("AppSettings__Environment")
                ?? ConfigurationManager.AppSettings["Environment"]
                ?? "Development";

            Version = System.Environment.GetEnvironmentVariable("AppSettings__Version")
                ?? ConfigurationManager.AppSettings["Version"]
                ?? "1.0.0";
        }

        public static void Info(string message, object data = null)
        {
            WriteLog("Information", message, data, Console.Out);
        }

        public static void Warn(string message, object data = null)
        {
            WriteLog("Warning", message, data, Console.Out);
        }

        public static void Error(string message, object data = null)
        {
            WriteLog("Error", message, data, Console.Error);
        }

        public static void Error(Exception ex, string message, object data = null)
        {
            WriteLog("Error", message, data, Console.Error, ex);
        }

        public static void Fatal(Exception ex, string message, object data = null)
        {
            WriteLog("Fatal", message, data, Console.Error, ex);
        }

        public static void Debug(string message, object data = null)
        {
            WriteLog("Debug", message, data, Console.Out);
        }

        private static void WriteLog(string level, string message, object data, System.IO.TextWriter writer, Exception ex = null)
        {
            var correlationId = HttpContext.Current?.Items["CorrelationId"]?.ToString();

            var logEntry = new Dictionary<string, object>
            {
                ["@t"] = DateTime.UtcNow.ToString("O"),
                ["@l"] = level,
                ["@m"] = message,
                ["ServiceName"] = ServiceName,
                ["Environment"] = Environment,
                ["Version"] = Version,
                ["MachineName"] = System.Environment.MachineName
            };

            if (!string.IsNullOrEmpty(correlationId))
            {
                logEntry["CorrelationId"] = correlationId;
            }

            if (data != null)
            {
                logEntry["Data"] = data;
            }

            if (ex != null)
            {
                logEntry["@x"] = ex.ToString();
                logEntry["ExceptionType"] = ex.GetType().FullName;
                logEntry["ExceptionMessage"] = ex.Message;
            }

            var json = JsonConvert.SerializeObject(logEntry, Formatting.None);
            writer.WriteLine(json);
        }
    }
}
