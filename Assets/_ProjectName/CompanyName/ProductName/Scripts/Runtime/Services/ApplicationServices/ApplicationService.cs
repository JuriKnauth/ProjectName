using System;
using System.Collections;
using CompanyName.ProductName.Scripts.Runtime.Services.BootManagerServices;
using CompanyName.ProductName.Scripts.Runtime.Services.LoggingServices;
using CompanyName.ProductName.Scripts.Runtime.Utilities;
using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.Services.ApplicationServices
{
    public sealed class ApplicationService : Singleton<ApplicationService>, IBootableService
    {
        public bool IsFocused { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsQuitting { get; private set; }

        private void OnDestroy() => Application.lowMemory -= LowMemory;

        private void OnApplicationFocus(bool hasFocus)
        {
            if (IsFocused == hasFocus)
            {
                return;
            }

            IsFocused = hasFocus;
            OnFocused?.Invoke(hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (IsPaused == pauseStatus)
            {
                return;
            }

            IsPaused = pauseStatus;
            OnPaused?.Invoke(pauseStatus);
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            if (IsQuitting)
            {
                return;
            }

            IsQuitting = true;
            OnQuit?.Invoke();
        }

        public string Name => nameof(ApplicationService);
        public bool IsReady { get; private set; }

        public IEnumerator Initialize()
        {
            IsReady = true;
            yield return null;
        }

        public event Action<bool> OnFocused;
        public event Action<bool> OnPaused;
        public event Action OnQuit;
        public event Action OnLowMemory;

        protected override void OnFirstInstance() => Application.lowMemory += LowMemory;

        private void LowMemory()
        {
            LoggerService.Warning("Low Memory");
            OnLowMemory?.Invoke();
        }
    }
}