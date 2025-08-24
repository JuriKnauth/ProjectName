using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CompanyName.ProductName.Scripts.Runtime.AppLovinMaxServices;
using CompanyName.ProductName.Scripts.Runtime.Services.BootManagerServices;
using CompanyName.ProductName.Scripts.Runtime.Services.LoggingServices;
using CompanyName.ProductName.Scripts.Runtime.Utilities;
using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.Services.AppLovinMaxServices
{
    public sealed class AppLovinMaxService : Singleton<AppLovinMaxService>, IBootableService
    {
        private const int maxAttempts = 6;
        private static string[] _interstitialAdUnitIds;
        private static string[] _rewardedAdUnitIds;
        private static string[] _bannerAdUnitIds;

        private static bool _callbacksRegistered;

        private static readonly Dictionary<string, Retry> _retrys =
            new Dictionary<string, Retry>();

        private void OnDestroy() => UnRegisterCallbacks();

        public string Name => nameof(AppLovinMaxService);
        public bool IsReady { get; private set; }

        public IEnumerator Initialize()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += MaxSdkCallbacksOnOnSdkInitializedEvent;

            MaxSdk.InitializeSdk();

            while (!IsReady)
            {
                yield return null;
            }

            yield return null;
        }

        public event Action<string, MaxSdkBase.AdInfo> OnInterstitialAdLoadedEvent;
        public event Action<string, MaxSdkBase.ErrorInfo> OnInterstitialAdFailedEvent;
        public event Action<string, MaxSdkBase.ErrorInfo, MaxSdkBase.AdInfo> OnInterstitialAdDisplayFailedEvent;
        public event Action<string, MaxSdkBase.AdInfo> OnInterstitialAdHiddenEvent;
        public event Action<string, MaxSdkBase.AdInfo> OnInterstitialAdRevenuePaidEvent;

        public event Action<string, MaxSdkBase.AdInfo> OnRewardedAdLoadedEvent;
        public event Action<string, MaxSdkBase.ErrorInfo> OnRewardedAdFailedEvent;
        public event Action<string, MaxSdkBase.ErrorInfo, MaxSdkBase.AdInfo> OnRewardedAdDisplayFailedEvent;
        public event Action<string, MaxSdkBase.AdInfo> OnRewardedAdDisplayedEvent;
        public event Action<string, MaxSdkBase.AdInfo> OnRewardedAdClickedEvent;
        public event Action<string, MaxSdkBase.AdInfo> OnRewardedAdHiddenEvent;
        public event Action<string, MaxSdkBase.Reward, MaxSdkBase.AdInfo> OnRewardedAdReceivedRewardEvent;
        public event Action<string, MaxSdkBase.AdInfo> OnRewardedAdRevenuePaidEvent;

        public event Action<string, MaxSdkBase.AdInfo> OnBannerAdLoadedEvent;
        public event Action<string, MaxSdkBase.ErrorInfo> OnBannerAdFailedEvent;
        public event Action<string, MaxSdkBase.AdInfo> OnBannerAdClickedEvent;
        public event Action<string, MaxSdkBase.AdInfo> OnBannerAdRevenuePaidEvent;

        public static IEnumerator Setup(AppLovinMaxServiceSettings appLovinMaxServiceSettings)
        {
            if (appLovinMaxServiceSettings == null)
            {
                LoggerService.Warning($"{nameof(AppLovinMaxServiceSettings)} is null");
                yield break;
            }

            _interstitialAdUnitIds = appLovinMaxServiceSettings.InterstitialAdUnitIds;
            _rewardedAdUnitIds = appLovinMaxServiceSettings.RewardedAdUnitIds;
            _bannerAdUnitIds = appLovinMaxServiceSettings.BannerAdUnitIds;

            InitializeInterstitialAds();
            InitializeRewardedAds();
            InitializeBannerAds();

            yield return null;
        }

        private static void MaxSdkCallbacksOnOnSdkInitializedEvent(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            LoggerService.Info("MAX SDK Initialized");

            UnRegisterCallbacks();
            RegisterCallbacks();

            Instance.IsReady = true;
        }

        private static void RegisterCallbacks()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += InterstitialAdLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += InterstitialAdFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialAdDisplayFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += InterstitialAdHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += InterstitialAdRevenuePaidEvent;

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += RewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += RewardedAdFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += RewardedAdDisplayFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += RewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += RewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += RewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += RewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += RewardedAdRevenuePaidEvent;

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += BannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += BannerAdFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += BannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += BannerAdRevenuePaidEvent;
        }

        private static void UnRegisterCallbacks()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= InterstitialAdLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= InterstitialAdFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= InterstitialAdDisplayFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= InterstitialAdHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= InterstitialAdRevenuePaidEvent;

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= RewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= RewardedAdFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= RewardedAdDisplayFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= RewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= RewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= RewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= RewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= RewardedAdRevenuePaidEvent;

            MaxSdkCallbacks.Banner.OnAdLoadedEvent -= BannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= BannerAdFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent -= BannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= BannerAdRevenuePaidEvent;
        }

        public void ShowMediationDebugger()
        {
            LoggerService.Info("ShowMediationDebugger");
            MaxSdk.ShowMediationDebugger();
        }

        public void ShowPrivacySettings() => MaxSdk.CmpService.ShowCmpForExistingUser(error =>
        {
            if (error != null)
                LoggerService.Warning("ShowCmpForExistingUser failed: " + error);
            else
                LoggerService.Info("ShowCmpForExistingUser");
        });

        private static void InitializeInterstitialAds()
        {
            for (int i = 0; i < _interstitialAdUnitIds.Length; i++)
            {
                LoadInterstitial(_interstitialAdUnitIds[i]);
            }
        }

        private static void InterstitialAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info($"{nameof(InterstitialAdLoadedEvent)} adUnitId: {adUnitId}");
            Instance?.OnInterstitialAdLoadedEvent?.Invoke(adUnitId, adInfo);

            ResetRetry(adUnitId);
        }

        private static void InterstitialAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            LoggerService.Warning(
                $"{nameof(InterstitialAdFailedEvent)} adUnitId: {adUnitId} errorInfo {errorInfo.Code}: {errorInfo.Message}");

            Instance?.OnInterstitialAdFailedEvent?.Invoke(adUnitId, errorInfo);

            Retry(adUnitId, () => LoadInterstitial(adUnitId));
        }

        private static void InterstitialAdDisplayFailedEvent(
            string adUnitId,
            MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Warning(
                $"{nameof(InterstitialAdDisplayFailedEvent)} adUnitId: {adUnitId} errorInfo {errorInfo.Code}: {errorInfo.Message}");

            Instance?.OnInterstitialAdDisplayFailedEvent?.Invoke(adUnitId, errorInfo, adInfo);
            LoadInterstitial(adUnitId);
        }

        private static void InterstitialAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info($"{nameof(InterstitialAdHiddenEvent)} adUnitId: {adUnitId}");
            Instance?.OnInterstitialAdHiddenEvent?.Invoke(adUnitId, adInfo);
            LoadInterstitial(adUnitId);
        }

        private static void InterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info($"{nameof(InterstitialAdRevenuePaidEvent)} adUnitId: {adUnitId}");
            Instance?.OnInterstitialAdRevenuePaidEvent?.Invoke(adUnitId, adInfo);
        }

        private static void InitializeRewardedAds()
        {
            for (int i = 0; i < _rewardedAdUnitIds.Length; i++)
            {
                LoadRewarded(_rewardedAdUnitIds[i]);
            }
        }

        private static void RewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info($"{nameof(RewardedAdLoadedEvent)} adUnitId: {adUnitId}");
            Instance?.OnRewardedAdLoadedEvent?.Invoke(adUnitId, adInfo);

            ResetRetry(adUnitId);
        }

        private static void RewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            LoggerService.Warning(
                $"{nameof(RewardedAdFailedEvent)} adUnitId: {adUnitId} errorInfo {errorInfo.Code}: {errorInfo.Message}");

            Instance?.OnRewardedAdFailedEvent?.Invoke(adUnitId, errorInfo);

            Retry(adUnitId, () => LoadRewarded(adUnitId));
        }

        private static void RewardedAdDisplayFailedEvent(
            string adUnitId,
            MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Warning(
                $"{nameof(RewardedAdDisplayFailedEvent)} adUnitId: {adUnitId} errorInfo {errorInfo.Code}: {errorInfo.Message}");

            Instance?.OnRewardedAdDisplayFailedEvent?.Invoke(adUnitId, errorInfo, adInfo);

            LoadRewarded(adUnitId);
        }

        private static void RewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info($"{nameof(RewardedAdDisplayedEvent)} adUnitId: {adUnitId}");
            Instance?.OnRewardedAdDisplayedEvent?.Invoke(adUnitId, adInfo);
        }

        private static void RewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info($"{nameof(RewardedAdClickedEvent)} adUnitId: {adUnitId}");
            Instance?.OnRewardedAdClickedEvent?.Invoke(adUnitId, adInfo);
        }

        private static void RewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info($"{nameof(RewardedAdHiddenEvent)} adUnitId: {adUnitId}");
            Instance?.OnRewardedAdHiddenEvent?.Invoke(adUnitId, adInfo);

            LoadRewarded(adUnitId);
        }

        private static void RewardedAdReceivedRewardEvent(
            string adUnitId,
            MaxSdkBase.Reward reward,
            MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info(
                $"{nameof(RewardedAdReceivedRewardEvent)} adUnitId: {adUnitId} rewardLabel: {reward.Label}");

            Instance?.OnRewardedAdReceivedRewardEvent?.Invoke(adUnitId, reward, adInfo);
        }

        private static void RewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info($"{nameof(RewardedAdRevenuePaidEvent)} adUnitId: {adUnitId}");
            Instance?.OnRewardedAdRevenuePaidEvent?.Invoke(adUnitId, adInfo);
        }

        private static void InitializeBannerAds()
        {
            for (int i = 0; i < _bannerAdUnitIds.Length; i++)
            {
                LoadBanner(_bannerAdUnitIds[i]);
            }
        }

        private static void BannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info($"{nameof(BannerAdLoadedEvent)} adUnitId: {adUnitId}");
            Instance?.OnBannerAdLoadedEvent?.Invoke(adUnitId, adInfo);
        }

        private static void BannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            LoggerService.Warning(
                $"{nameof(BannerAdFailedEvent)} adUnitId: {adUnitId} errorInfo {errorInfo.Code}: {errorInfo.Message}");

            Instance?.OnBannerAdFailedEvent?.Invoke(adUnitId, errorInfo);
        }

        private static void BannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info($"{nameof(BannerAdClickedEvent)} adUnitId: {adUnitId}");
            Instance?.OnBannerAdClickedEvent?.Invoke(adUnitId, adInfo);
        }

        private static void BannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoggerService.Info($"{nameof(BannerAdRevenuePaidEvent)} adUnitId: {adUnitId}");
            Instance?.OnBannerAdRevenuePaidEvent?.Invoke(adUnitId, adInfo);
        }

        private static void LoadInterstitial(string adUnitId = null)
        {
            adUnitId ??= _interstitialAdUnitIds.FirstOrDefault();
            if (string.IsNullOrEmpty(adUnitId))
            {
                LoggerService.Warning($"No Interstitial found. adUnitId: {adUnitId}");
                return;
            }

            MaxSdk.LoadInterstitial(adUnitId);
        }

        private static void LoadRewarded(string adUnitId = null)
        {
            adUnitId ??= _rewardedAdUnitIds.FirstOrDefault();
            if (string.IsNullOrEmpty(adUnitId))
            {
                LoggerService.Warning($"No Rewarded found. adUnitId: {adUnitId}");
                return;
            }

            MaxSdk.LoadRewardedAd(adUnitId);
        }

        private static void LoadBanner(
            string adUnitId = null,
            MaxSdkBase.AdViewPosition bannerPosition = MaxSdkBase.AdViewPosition.TopCenter,
            Color? color = null)
        {
            adUnitId ??= _bannerAdUnitIds.FirstOrDefault();
            color ??= Color.clear;
            if (string.IsNullOrEmpty(adUnitId))
            {
                LoggerService.Warning($"No Banner found. adUnitId: {adUnitId}");
                return;
            }

            MaxSdk.CreateBanner(adUnitId, new MaxSdkBase.AdViewConfiguration(bannerPosition));
            MaxSdk.HideBanner(adUnitId);
            MaxSdk.SetBannerBackgroundColor(adUnitId, color.Value);
        }

        public bool ShowInterstitialByIndex(int addUnitIndex = 0)
        {
            if (addUnitIndex < 0 || addUnitIndex >= _interstitialAdUnitIds.Length)
            {
                LoggerService.Warning("AddUnitIndex out of range.");
                return false;
            }

            string adUnitId = _interstitialAdUnitIds[addUnitIndex];
            return ShowInterstitial(adUnitId);
        }

        public bool ShowInterstitial(string adUnitId = null)
        {
            adUnitId ??= _interstitialAdUnitIds.FirstOrDefault();
            if (string.IsNullOrEmpty(adUnitId))
            {
                LoggerService.Warning($"No Interstitial found. adUnitId: {adUnitId}");
                return false;
            }

            if (!MaxSdk.IsInterstitialReady(adUnitId))
            {
                LoggerService.Warning($"Interstitial not ready. adUnitId: {adUnitId}");
                return false;
            }

            MaxSdk.ShowInterstitial(adUnitId);
            return true;
        }

        public bool ShowRewardedByIndex(
            int addUnitIndex = 0,
            Action<MaxSdkBase.Reward> onComplete = null,
            Action onAborted = null)
        {
            if (addUnitIndex < 0 || addUnitIndex >= _rewardedAdUnitIds.Length)
            {
                LoggerService.Warning("AddUnitIndex out of range.");
                return false;
            }

            string adUnitId = _rewardedAdUnitIds[addUnitIndex];
            return ShowRewarded(adUnitId, onComplete, onAborted);
        }

        public bool ShowRewarded(
            string adUnitId = null,
            Action<MaxSdkBase.Reward> onComplete = null,
            Action onAborted = null)
        {
            adUnitId ??= _rewardedAdUnitIds.FirstOrDefault();
            if (string.IsNullOrEmpty(adUnitId))
            {
                LoggerService.Warning($"No Rewarded found. adUnitId: {adUnitId}");
                return false;
            }

            if (!MaxSdk.IsRewardedAdReady(adUnitId))
            {
                LoggerService.Warning($"Rewarded not ready. adUnitId: {adUnitId}");
                return false;
            }

            if (onComplete != null || onAborted != null)
            {
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += Complete;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += Abort;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += Failed;

                void Complete(string id, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo _)
                {
                    if (id != adUnitId) return;
                    Cleanup();
                    onComplete?.Invoke(reward);
                }

                void Abort(string id, MaxSdkBase.AdInfo _ = null)
                {
                    if (id != adUnitId) return;
                    Cleanup();
                    onAborted?.Invoke();
                }

                void Failed(string id, MaxSdkBase.ErrorInfo _, MaxSdkBase.AdInfo __) => Abort(id);

                void Cleanup()
                {
                    MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= Complete;
                    MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= Abort;
                    MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= Failed;
                }
            }

            MaxSdk.ShowRewardedAd(adUnitId);
            return true;
        }


        public bool ShowBannerByIndex(int addUnitIndex = 0)
        {
            if (addUnitIndex < 0 || addUnitIndex >= _bannerAdUnitIds.Length)
            {
                LoggerService.Warning("AddUnitIndex out of range.");
                return false;
            }

            string adUnitId = _bannerAdUnitIds[addUnitIndex];
            return ShowBanner(adUnitId);
        }

        public bool ShowBanner(string adUnitId = null)
        {
            adUnitId ??= _bannerAdUnitIds.FirstOrDefault();
            if (string.IsNullOrEmpty(adUnitId))
            {
                LoggerService.Warning($"No Banner found. adUnitId: {adUnitId}");
                return false;
            }

            MaxSdk.ShowBanner(adUnitId);
            MaxSdk.SetBannerBackgroundColor(adUnitId, Color.black);
            return true;
        }

        public bool HideBannerByIndex(int addUnitIndex = 0)
        {
            if (addUnitIndex < 0 || addUnitIndex >= _bannerAdUnitIds.Length)
            {
                LoggerService.Warning("AddUnitIndex out of range.");
                return false;
            }

            string adUnitId = _bannerAdUnitIds[addUnitIndex];
            return HideBanner(adUnitId);
        }

        public bool HideBanner(string adUnitId = null)
        {
            adUnitId ??= _bannerAdUnitIds.FirstOrDefault();
            if (string.IsNullOrEmpty(adUnitId))
            {
                LoggerService.Warning($"No Banner found. adUnitId: {adUnitId}");
                return false;
            }

            MaxSdk.HideBanner(adUnitId);
            return true;
        }

        private static float NextDelay(string adUnitId)
        {
            _retrys.TryGetValue(adUnitId, out Retry attempt);
            attempt ??= new Retry();
            attempt.Attempt = Mathf.Min(attempt.Attempt + 1, maxAttempts);
            _retrys[adUnitId] = attempt;
            return Mathf.Pow(2, attempt.Attempt);
        }

        private static void ResetRetry(string adUnitId)
        {
            _retrys.TryGetValue(adUnitId, out Retry attempt);
            attempt ??= new Retry();
            attempt.Attempt = 0;
        }

        private static void Retry(string adUnitId, Action action)
        {
            if (!HasInstance)
            {
                return;
            }

            _retrys.TryGetValue(adUnitId, out Retry attempt);
            attempt ??= new Retry();
            Instance.StartDelayed(ref attempt.Coroutine, action, NextDelay(adUnitId));
        }
    }
}