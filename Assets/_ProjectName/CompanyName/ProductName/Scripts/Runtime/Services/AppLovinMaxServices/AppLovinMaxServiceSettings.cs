using CompanyName.ProductName.Scripts.Runtime.Configurations;
using UnityEngine;
using UnityEngine.Serialization;

namespace CompanyName.ProductName.Scripts.Runtime.Services.AppLovinMaxServices
{
    public sealed class AppLovinMaxServiceSettings : SettingsSection
    {
        [Header("Android")]
        [SerializeField]
        public string[] AndroidInterstitialAdUnitIds =
            {
                "58f24a034a1ab4d2"
            };

        [SerializeField]
        public string[] AndroidRewardedAdUnitIds =
            {
                "39da376bc3211fc1",
                "67ad4fcfac164253"
            };

        [SerializeField]
        public string[] AndroidBannerAdUnitIds =
            {
                "7d54f8fe50d4b222"
            };
        
        [Header("IOS")]
        [SerializeField]
        public string[] IOSInterstitialAdUnitIds =
            {
                "58f24a034a1ab4d2"
            };

        [SerializeField]
        public string[] IOSRewardedAdUnitIds =
            {
                "39da376bc3211fc1",
                "67ad4fcfac164253"
            };

        [SerializeField]
        private string[] IOSBannerAdUnitIds =
            {
                "7d54f8fe50d4b222"
            };
        
#if UNITY_IOS
        public string[] InterstitialAdUnitIds => IOSInterstitialAdUnitIds;
        public string[] RewardedAdUnitIds => IOSRewardedAdUnitIds;
        public string[] BannerAdUnitIds => IOSBannerAdUnitIds;
#else
        public string[] InterstitialAdUnitIds => AndroidInterstitialAdUnitIds;
        public string[] RewardedAdUnitIds => AndroidRewardedAdUnitIds;
        public string[] BannerAdUnitIds => AndroidBannerAdUnitIds;
#endif
    }
}