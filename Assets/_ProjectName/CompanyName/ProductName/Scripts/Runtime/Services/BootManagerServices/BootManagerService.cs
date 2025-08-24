using System.Collections;
using CompanyName.ProductName.Scripts.Runtime.Services.ApplicationServices;
using CompanyName.ProductName.Scripts.Runtime.Services.LoadingOverlayServices;
using CompanyName.ProductName.Scripts.Runtime.Services.LoggingServices;
using CompanyName.ProductName.Scripts.Runtime.Services.SceneManagerServices;
using CompanyName.ProductName.Scripts.Runtime.SettingServices;
using CompanyName.ProductName.Scripts.Runtime.Utilities;
using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.Services.BootManagerServices
{
    public sealed class BootManagerService : MonoBehaviour
    {
        private Coroutine _bootCoroutine;

        private void Start() => _bootCoroutine = this.RestartCoroutine(ref _bootCoroutine,BootCoroutine());

        private IEnumerator BootCoroutine()
        {
            yield return null;

            yield return LoadingOverlayService.Instance?.Initialize();

            LoadingOverlayService.Instance?.Enqueue(
                new LoadingCommand(
                    new[]
                    {
                        new LoadingStep(
                            $"Loading {nameof(SettingServices)}...",
                            () => SettingsService.Instance?.Initialize()
                        ),
                        new LoadingStep(
                            $"Loading {nameof(ApplicationService)}...",
                            () => ApplicationService.Instance?.Initialize()
                        ),
                        new LoadingStep(
                            $"Loading {nameof(LoggerService)}...",
                            () => LoggerService.Setup(SettingsService.Instance?.Settings
                                                                     ?.Get<SettingsSectionLoggerService>())
                        ),
                        new LoadingStep(
                            "Loading UserInterface Scene...",
                            () => SceneManagerService.LoadSceneAsync(1, useLoadingOverlay: true)
                        )
                    }
                )
            );
        }
    }
}