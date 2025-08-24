using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CompanyName.ProductName.Scripts.Runtime.Services.LoadingOverlayServices
{
    public sealed class LoadingOverlayView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Slider _slider;
        [SerializeField] private TextMeshProUGUI _title;

        [SerializeField] private float _fadeSeconds = 0.12f;
        [SerializeField] private float _indeterminateSpeed = 1.0f;

        private void Awake()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = false;
            }

            if (_slider != null)
            {
                _slider.minValue = 0f;
                _slider.maxValue = 1f;
                _slider.value = 0f;
            }

            if (LoadingOverlayService.HasInstance)
            {
                LoadingOverlayService.Instance.OnCommandStarted -= OnCommandStarted;
                LoadingOverlayService.Instance.OnCommandStarted += OnCommandStarted;
            }
        }

        private void OnDestroy()
        {
            if (LoadingOverlayService.HasInstance)
            {
                LoadingOverlayService.Instance.OnCommandStarted -= OnCommandStarted;
            }
        }

        private void OnCommandStarted(LoadingCommand command, int commandIndex, int totalCommands) =>
            SetTitle($"{commandIndex}/{totalCommands} {command}");

        public void SetTitle(string text)
        {
            if (_title == null)
            {
                return;
            }

            if (_title.text == text)
            {
                return;
            }

            _title.text = string.IsNullOrEmpty(text)
                ? "Loading…"
                : text;
        }

        public void SetProgress(float value)
        {
            if (_slider == null)
            {
                return;
            }

            float clamped = Mathf.Clamp01(value);
            if (!Mathf.Approximately(_slider.value, clamped))
            {
                _slider.value = clamped;
            }
        }

        public void SetIndeterminate(float time)
        {
            if (_slider == null)
            {
                return;
            }

            float t = Mathf.PingPong(time * _indeterminateSpeed, 1f);
            if (!Mathf.Approximately(_slider.value, t))
            {
                _slider.value = t;
            }
        }

        public void ShowInstant()
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }

        public IEnumerator FadeOut()
        {
            if (_canvasGroup == null)
            {
                yield break;
            }

            float t = 0f;
            float d = Mathf.Max(0.01f, _fadeSeconds);
            while (t < d)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = 1f - Mathf.Clamp01(t / d);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        public void ResetProgress() => SetProgress(0f);
    }
}