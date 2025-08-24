using System;
using System.Collections;
using CompanyName.ProductName.Scripts.Runtime.Configurations;
using CompanyName.ProductName.Scripts.Runtime.Services.BootManagerServices;
using CompanyName.ProductName.Scripts.Runtime.Services.LoggingServices;
using CompanyName.ProductName.Scripts.Runtime.Utilities;
using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.SettingServices
{
    public sealed class SettingsService : Singleton<SettingsService>, IBootableService
    {
        [SerializeField] private Settings explicitAsset;
        [SerializeField] private string resourcesPath = "Runtime/Configurations/Settings";

        public Settings Settings { get; private set; }
        public string Name => nameof(SettingsService);
        public bool IsReady { get; private set; }

        public event Action<Settings> OnSettingsUpdated;
        
        public T Get<T>() where T : SettingsSection
        {
            if (!Settings) return null;
            return Settings.Get<T>();
        }
        
        public IEnumerator Initialize()
        {
            Load();
            IsReady = true;
            yield break;
        }

        public void Reload() => Load();

        private void Load()
        {
            Settings asset = explicitAsset
                ? explicitAsset
                : Resources.Load<Settings>(resourcesPath);

            if (!asset)
            {
                LoggerService.Warning("[SettingsService] No Settings found, using defaults.");
                asset = ScriptableObject.CreateInstance<Settings>();
            }

            if (Settings == asset) return;
            Settings = asset;
            OnSettingsUpdated?.Invoke(Settings);
        }
    }
}