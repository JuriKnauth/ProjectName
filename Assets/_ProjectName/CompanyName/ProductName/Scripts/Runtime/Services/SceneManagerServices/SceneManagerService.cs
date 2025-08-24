using System;
using System.Collections;
using System.IO;
using CompanyName.ProductName.Scripts.Runtime.Services.LoadingOverlayServices;
using CompanyName.ProductName.Scripts.Runtime.Services.LoggingServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CompanyName.ProductName.Scripts.Runtime.Services.SceneManagerServices
{
    public static class SceneManagerService
    {
        public static event Action<string, int> OnSceneLoadStarted;
        public static event Action<string, int, float> OnSceneLoadProgress;
        public static event Action<string, int> OnSceneLoadFinished;

        public static IEnumerator LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            bool allowSceneActivation = true,
            bool useLoadingOverlay = false)
        {
            if (!IsSceneNameValid(sceneName)) yield break;

            int index = GetBuildIndexForSceneName(sceneName);
            yield return LoadSceneAsyncCore(
                sceneName,
                index,
                mode,
                allowSceneActivation,
                null,
                useLoadingOverlay);
        }

        public static IEnumerator LoadSceneAsync(
            int sceneIndex,
            LoadSceneMode mode = LoadSceneMode.Single,
            bool allowSceneActivation = true,
            bool useLoadingOverlay = false)
        {
            if (!IsSceneIndexValid(sceneIndex)) yield break;

            string name = GetSceneNameForBuildIndex(sceneIndex);
            yield return LoadSceneAsyncCore(
                name,
                sceneIndex,
                mode,
                allowSceneActivation,
                null,
                useLoadingOverlay);
        }

        private static bool IsSceneNameValid(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName)) return true;
            LoggerService.Warning("scene name is null or empty");
            return false;
        }

        private static bool IsSceneIndexValid(int sceneIndex)
        {
            if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings) return true;
            LoggerService.Warning("scene index is out of range");
            return false;
        }

        public static IEnumerator LoadSceneAsyncControlled(
            string sceneName,
            Func<bool> canActivate,
            LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (!IsSceneNameValid(sceneName)) yield break;

            int index = GetBuildIndexForSceneName(sceneName);
            yield return LoadSceneAsyncCore(sceneName, index, mode, false, canActivate);
        }

        private static IEnumerator LoadSceneAsyncCore(
            string sceneName,
            int sceneIndex,
            LoadSceneMode mode,
            bool allowSceneActivation,
            Func<bool> canActivate,
            bool useLoadingOverlay)
        {
            if (useLoadingOverlay && LoadingOverlayService.HasInstance)
            {
                OnSceneLoadProgress -= LoadingOverlayService.Instance.SetLoadingSlider;
                OnSceneLoadProgress += LoadingOverlayService.Instance.SetLoadingSlider;
            }

            yield return LoadSceneAsyncCore(sceneName, sceneIndex, mode, allowSceneActivation, canActivate);

            if (useLoadingOverlay && LoadingOverlayService.HasInstance)
            {
                OnSceneLoadProgress -= LoadingOverlayService.Instance.SetLoadingSlider;
            }
        }

        private static IEnumerator LoadSceneAsyncCore(
            string sceneName,
            int sceneIndex,
            LoadSceneMode mode,
            bool allowSceneActivation,
            Func<bool> canActivate)
        {
            OnSceneLoadStarted?.Invoke(sceneName, sceneIndex);

            AsyncOperation asyncOperation = sceneIndex >= 0
                ? SceneManager.LoadSceneAsync(sceneIndex, mode)
                : SceneManager.LoadSceneAsync(sceneName, mode);

            if (asyncOperation == null)
            {
                LoggerService.Error("scene load operation is null");
                yield break;
            }

            asyncOperation.allowSceneActivation = canActivate == null && allowSceneActivation;

            float last = float.NegativeInfinity;

            if (canActivate == null)
            {
                while (!asyncOperation.isDone)
                {
                    float p = Mathf.Clamp01(asyncOperation.progress);
                    if (!Mathf.Approximately(p, last))
                    {
                        OnSceneLoadProgress?.Invoke(sceneName, sceneIndex, p);
                        last = p;
                    }

                    yield return null;
                }

                if (!Mathf.Approximately(last, 1f))
                {
                    OnSceneLoadProgress?.Invoke(sceneName, sceneIndex, 1f);
                }
            }
            else
            {
                while (asyncOperation.progress < 0.9f)
                {
                    float p = Mathf.Clamp01(asyncOperation.progress);
                    if (!Mathf.Approximately(p, last))
                    {
                        OnSceneLoadProgress?.Invoke(sceneName, sceneIndex, p);
                        last = p;
                    }

                    yield return null;
                }

                if (!Mathf.Approximately(last, 1f))
                {
                    OnSceneLoadProgress?.Invoke(sceneName, sceneIndex, 1f);
                }

                while (!canActivate())
                {
                    yield return null;
                }

                asyncOperation.allowSceneActivation = true;

                while (!asyncOperation.isDone)
                {
                    yield return null;
                }
            }

            OnSceneLoadFinished?.Invoke(sceneName, sceneIndex);
        }

        private static int GetBuildIndexForSceneName(string sceneName)
        {
            int count = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < count; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(name, sceneName, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string GetSceneNameForBuildIndex(int sceneIndex)
        {
            if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                return string.Empty;
            }

            string path = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            return Path.GetFileNameWithoutExtension(path);
        }
    }
}