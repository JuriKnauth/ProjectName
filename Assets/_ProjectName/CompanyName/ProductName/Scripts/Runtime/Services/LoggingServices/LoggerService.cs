using System.Collections;
using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.Services.LoggingServices
{
    public static class LoggerService
    {
        public static LogLevel MinimumLevel { get; set; } = LogLevel.Info;

        [HideInCallstack]
        public static void Trace(string message) => Log(LogLevel.Trace, message);

        [HideInCallstack]
        public static void Debug(string message) => Log(LogLevel.Debug, message);

        [HideInCallstack]
        public static void Info(string message) => Log(LogLevel.Info, message);

        [HideInCallstack]
        public static void Warning(string message) => Log(LogLevel.Warning, message);

        [HideInCallstack]
        public static void Error(string message) => Log(LogLevel.Error, message);

        [HideInCallstack]
        public static void Trace(string message, GameObject gameObject) => Log(LogLevel.Trace, message, gameObject);

        [HideInCallstack]
        public static void Debug(string message, GameObject gameObject) => Log(LogLevel.Debug, message, gameObject);

        [HideInCallstack]
        public static void Info(string message, GameObject gameObject) => Log(LogLevel.Info, message, gameObject);

        [HideInCallstack]
        public static void Warning(string message, GameObject gameObject) => Log(LogLevel.Warning, message, gameObject);

        [HideInCallstack]
        public static void Error(string message, GameObject gameObject) => Log(LogLevel.Error, message, gameObject);

        public static IEnumerator Setup(LoggerServiceSettings loggerServiceSettings)
        {
            if (loggerServiceSettings == null)
            {
                Warning($"{nameof(LoggerServiceSettings)} is null");
                yield break;
            }

            MinimumLevel = loggerServiceSettings.MinimumLogLevel;

            yield return null;
        }

        [HideInCallstack]
        private static void Log(LogLevel level, string message, GameObject gameObject = null)
        {
            if (MinimumLevel == LogLevel.None || level < MinimumLevel)
            {
                return;
            }

            switch (level)
            {
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(message, gameObject);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message, gameObject);
                    break;
                default:
                    UnityEngine.Debug.Log(message, gameObject);
                    break;
            }
        }
    }
}