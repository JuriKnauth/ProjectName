using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CompanyName.ProductName.Scripts.Runtime.UserInterfaces.Debug
{
    public sealed class DebugPrinter : MonoBehaviour
    {
        [SerializeField] private Toggle _autoScrollToggle;
        [SerializeField] private TextMeshProUGUI _autoScrollText;
        [SerializeField] private TextMeshProUGUI _logText;
        [SerializeField] private ScrollRect _scrollRect;

        [SerializeField] private Color _colorError;
        [SerializeField] private Color _colorAssert;
        [SerializeField] private Color _colorWarning;
        [SerializeField] private Color _colorLog;
        [SerializeField] private Color _colorException;

        [SerializeField] private int _maxLines = 1024;

        private readonly List<string> _lines = new List<string>(1024);
        private readonly Queue<string> _pending = new Queue<string>();

        private readonly object _queueLock = new object();
        private readonly StringBuilder _stringBuilder = new StringBuilder(8192);

        private bool _autoScroll = true;
        private string _hexAssert;

        private string _hexError;
        private string _hexException;
        private string _hexLog;
        private string _hexWarning;
        private volatile int _pendingCount;

        private void Awake()
        {
            CacheHex();
            if (_autoScrollToggle != null)
            {
                _autoScrollToggle.isOn = _autoScroll;
                _autoScrollToggle.onValueChanged.AddListener(OnAutoScrollChanged);
            }

            if (_scrollRect != null)
                _scrollRect.verticalNormalizedPosition = 1f;
        }

        private void Update()
        {
            if (_pendingCount == 0) return;

            DrainPendingToLines();

            _stringBuilder.Clear();
            for (int i = 0; i < _lines.Count; i++)
            {
                _stringBuilder.AppendLine(_lines[i]);
            }

            if (_logText != null)
                _logText.SetText(_stringBuilder);

            if (_autoScroll && _scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                ScrollBottom();
            }
        }

        private void OnEnable() => Application.logMessageReceivedThreaded += HandleLogThreaded;

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= HandleLogThreaded;
            if (_autoScrollToggle != null)
                _autoScrollToggle.onValueChanged.RemoveListener(OnAutoScrollChanged);
        }

        private void CacheHex()
        {
            _hexError = ColorUtility.ToHtmlStringRGB(_colorError);
            _hexAssert = ColorUtility.ToHtmlStringRGB(_colorAssert);
            _hexWarning = ColorUtility.ToHtmlStringRGB(_colorWarning);
            _hexLog = ColorUtility.ToHtmlStringRGB(_colorLog);
            _hexException = ColorUtility.ToHtmlStringRGB(_colorException);
        }

        private void HandleLogThreaded(string condition, string stackTrace, LogType type)
        {
            string hex = type switch
                         {
                             LogType.Error => _hexError,
                             LogType.Assert => _hexAssert,
                             LogType.Warning => _hexWarning,
                             LogType.Log => _hexLog,
                             LogType.Exception => _hexException,
                             _ => _hexLog
                         };

            condition = condition?.Replace("<", "&lt;").Replace(">", "&gt;");
            // stackTrace = stackTrace?.Replace("<", "&lt;").Replace(">", "&gt;");

            string line = $"<color=#{hex}>{DateTime.Now:HH:mm:ss.fff} {type}: \"{condition}\"\n</color>";
            // string line = $"<color=#{hex}>{DateTime.Now:HH:mm:ss.fff} {type}: \"{condition}\"\n{stackTrace}\n</color>";

            lock (_queueLock)
            {
                _pending.Enqueue(line);
                _pendingCount = _pending.Count;
            }
        }

        private void DrainPendingToLines()
        {
            lock (_queueLock)
            {
                while (_pending.Count > 0)
                {
                    _lines.Add(_pending.Dequeue());
                    if (_lines.Count > _maxLines)
                        _lines.RemoveAt(0);
                }

                _pendingCount = 0;
            }
        }

        [ContextMenu(nameof(ScrollBottom))]
        private void ScrollBottom() => _scrollRect.verticalNormalizedPosition = 0f;

        [ContextMenu(nameof(ScrollTop))]
        private void ScrollTop() => _scrollRect.verticalNormalizedPosition = 1f;

        private void OnAutoScrollChanged(bool isOn)
        {
            _autoScroll = isOn;
            if (_autoScrollText != null)
            {
                _autoScrollText.SetText($"AutoScroll: {(_autoScroll ? "On" : "Off")}");
            }
        }

        [ContextMenu(nameof(ClearLogs))]
        public void ClearLogs()
        {
            _lines.Clear();
            _stringBuilder.Clear();
            if (_logText != null) _logText.SetText(string.Empty);
        }
    }
}