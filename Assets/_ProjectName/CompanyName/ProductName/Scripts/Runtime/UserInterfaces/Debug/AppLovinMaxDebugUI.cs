using CompanyName.ProductName.Scripts.Runtime.Services.AppLovinMaxServices;
using CompanyName.ProductName.Scripts.Runtime.Services.LoggingServices;
using UnityEngine;
using UnityEngine.UI;

namespace CompanyName.ProductName.Scripts.Runtime.UserInterfaces.Debug
{
    public sealed class AppLovinMaxDebugUI : MonoBehaviour
    {
        [SerializeField] private Button showRewardedButton1;
        [SerializeField] private Button showRewardedButton2;
        [SerializeField] private Button showBannerButton;
        [SerializeField] private Button showInterstitialButton;
        [SerializeField] private Button hideBannerButton;
        [SerializeField] private Button showMediationDebuggerButton;
        [SerializeField] private Button showPrivacySettingsButton;

        private void Start()
        {
            showRewardedButton1.onClick.AddListener(ShowRewarded1);
            showRewardedButton2.onClick.AddListener(ShowRewarded2);
            showBannerButton.onClick.AddListener(ShowBanner);
            showInterstitialButton.onClick.AddListener(ShowInterstitial);
            hideBannerButton.onClick.AddListener(HideBanner);
            showMediationDebuggerButton.onClick.AddListener(ShowMediationDebugger);
            showPrivacySettingsButton.onClick.AddListener(ShowPrivacySettings);
        }

        private void OnDestroy()
        {
            showRewardedButton1.onClick.RemoveListener(ShowRewarded1);
            showRewardedButton2.onClick.RemoveListener(ShowRewarded2);
            showBannerButton.onClick.RemoveListener(ShowBanner);
            showInterstitialButton.onClick.RemoveListener(ShowInterstitial);
            hideBannerButton.onClick.RemoveListener(HideBanner);
            showMediationDebuggerButton.onClick.RemoveListener(ShowMediationDebugger);
            showPrivacySettingsButton.onClick.RemoveListener(ShowPrivacySettings);
        }
        
        private void ShowRewarded1() => AppLovinMaxService.Instance?.ShowRewardedByIndex(onComplete: Reward);

        private void ShowRewarded2() => AppLovinMaxService.Instance?.ShowRewardedByIndex(1, Reward);

        private void ShowBanner() => AppLovinMaxService.Instance?.ShowBannerByIndex();

        private void ShowInterstitial() => AppLovinMaxService.Instance?.ShowInterstitialByIndex();

        private void HideBanner() => AppLovinMaxService.Instance?.HideBannerByIndex();

        private void ShowMediationDebugger() => AppLovinMaxService.Instance?.ShowMediationDebugger();

        private void ShowPrivacySettings() => AppLovinMaxService.Instance?.ShowPrivacySettings();

        private void Reward(MaxSdkBase.Reward reward) => LoggerService.Info($"Reward {reward.Label} {reward.Amount}");
    }
}