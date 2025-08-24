using CompanyName.ProductName.Scripts.Runtime.Configurations;
using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.Services.LoggingServices
{
    public sealed class LoggerServiceSettings : SettingsSection
    {
        [field: SerializeField] public LogLevel MinimumLogLevel { get; private set; } = LogLevel.Info;
    }
}