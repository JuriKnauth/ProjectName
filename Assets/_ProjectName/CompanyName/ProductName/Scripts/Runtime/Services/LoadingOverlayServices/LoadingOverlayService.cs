using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CompanyName.ProductName.Scripts.Runtime.Services.BootManagerServices;
using CompanyName.ProductName.Scripts.Runtime.Services.LoggingServices;
using CompanyName.ProductName.Scripts.Runtime.Utilities;
using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.Services.LoadingOverlayServices
{
    public sealed class LoadingOverlayService : Singleton<LoadingOverlayService>, IBootableService
    {
        private const string PrefabPath = "Prefabs/UserInterfaces/LoadingOverlayView";

        private readonly Queue<LoadingCommand> _queue = new Queue<LoadingCommand>();
        private bool _isRunning;
        private Coroutine _runner;

        private LoadingOverlayView _view;

        public string Name => nameof(LoadingOverlayService);
        public bool IsReady { get; private set; }

        public IEnumerator Initialize()
        {
            yield return EnsureViewRoutine();

            _view.ResetProgress();

            IsReady = true;

            yield return null;
        }

        public event Action<LoadingCommand, int, int> OnCommandStarted;
        public event Action<LoadingCommand, int, int> OnCommandFinished;
        public event Action OnAllCommandsFinished;

        public void Enqueue(LoadingCommand command)
        {
            _queue.Enqueue(command);
            TryRun();
        }

        public void EnqueueRange(IEnumerable<LoadingCommand> commands)
        {
            if (commands == null)
            {
                return;
            }

            foreach (LoadingCommand c in commands)
            {
                _queue.Enqueue(c);
            }

            TryRun();
        }

        public void SetLoadingSlider(string sceneName, int sceneIndex, float progress)
        {
            if (_view == null)
            {
                return;
            }

            sceneName = sceneName.Length > 3
                ? sceneName.Substring(3)
                : sceneName;

            _view.SetTitle($"Loading {sceneName}...");
            _view.SetProgress(progress);
        }

        public void ClearQueue() => _queue.Clear();

        private void TryRun()
        {
            if (_isRunning)
            {
                return;
            }

            _runner = this.RestartCoroutine(ref _runner,RunQueue());
        }

        private IEnumerator RunQueue()
        {
            _isRunning = true;

            yield return EnsureView();

            if (_view != null)
            {
                _view.ShowInstant();
                _view.ResetProgress();
            }

            int totalCommands = _queue.Count;
            int commandIndex = 0;

            while (_queue.Count > 0)
            {
                commandIndex++;
                LoadingCommand command = _queue.Dequeue();

                LoadingStep[] steps = command.Steps;
                if (steps == null || steps.Length == 0)
                {
                    continue;
                }

                OnCommandStarted?.Invoke(command, commandIndex, totalCommands);

                float syntheticTotal = steps.Sum(s => Mathf.Max(0f, s.MaxDurationSeconds));
                float syntheticStart = Time.time;

                for (int i = 0; i < steps.Length; i++)
                {
                    LoadingStep step = steps[i];

                    if (_view != null)
                    {
                        _view.SetTitle(step.Name);
                        _view.SetProgress(0f);
                    }

                    Coroutine progressPump = StartCoroutine(ProgressPump(step, syntheticStart, syntheticTotal));

                    if (step.AsyncAction != null)
                    {
                        IEnumerator routine = null;

                        try
                        {
                            routine = step.AsyncAction.Invoke();
                        }
                        catch (Exception ex)
                        {
                            LoggerService.Error(
                                $"[{nameof(LoadingOverlayService)}] Exception creating step '{step.Name}': {ex}");
                        }

                        if (routine != null)
                        {
                            yield return routine;
                        }
                    }
                    else
                    {
                        float start = Time.time;
                        float maxDur = Mathf.Max(0f, step.MaxDurationSeconds);
                        Func<bool> condition = step.Condition;

                        if (condition == null && maxDur <= 0f)
                        {
                            yield return null;
                        }
                        else
                        {
                            while (true)
                            {
                                bool doneByCondition = condition != null && condition.Invoke();
                                bool doneByTime = maxDur > 0f && Time.time - start >= maxDur;

                                if (condition == null && doneByTime || doneByCondition)
                                {
                                    break;
                                }

                                yield return null;
                            }
                        }
                    }

                    if (step.ProgressGetter != null && _view != null)
                    {
                        _view.SetProgress(1f);
                    }

                    if (progressPump != null)
                    {
                        StopCoroutine(progressPump);
                    }

                    try
                    {
                        step.OnCompleted?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        LoggerService.Error(
                            $"[{nameof(LoadingOverlayService)}] Exception in OnCompleted for '{step.Name}': {ex}");
                    }

                    yield return null;
                }

                OnCommandFinished?.Invoke(command, commandIndex, totalCommands);
            }

            if (_view != null)
            {
                yield return _view.FadeOut();
                _view.SetTitle(string.Empty);
                _view.ResetProgress();
            }

            _isRunning = false;
            _runner = null;
            OnAllCommandsFinished?.Invoke();
        }

        private IEnumerator ProgressPump(LoadingStep step, float syntheticStartTime, float syntheticTotalDuration)
        {
            while (true)
            {
                if (_view == null)
                {
                    yield return null;
                    continue;
                }

                if (step.ProgressGetter != null)
                {
                    float p = Mathf.Clamp01(step.ProgressGetter());
                    _view.SetProgress(p);
                }
                else if (syntheticTotalDuration > 0.0001f)
                {
                    float elapsed = Time.time - syntheticStartTime;
                    float p = Mathf.Clamp01(elapsed / syntheticTotalDuration);
                    _view.SetProgress(p);
                }
                else
                {
                    _view.SetIndeterminate(Time.unscaledTime);
                }

                yield return null;
            }
        }

        private IEnumerator EnsureView()
        {
            if (_view != null)
            {
                yield break;
            }

            ResourceRequest request = Resources.LoadAsync<GameObject>(PrefabPath);
            yield return request;

            GameObject prefab = request.asset as GameObject;
            if (prefab == null)
            {
                LoggerService.Error($"[{nameof(LoadingOverlayService)}] Missing prefab at Resources/{PrefabPath}");
                yield break;
            }

            GameObject go = Instantiate(prefab);
            DontDestroyOnLoad(go);

            _view = go.GetComponent<LoadingOverlayView>();
            if (_view == null)
            {
                LoggerService.Error(
                    $"[{nameof(LoadingOverlayService)}] Prefab is missing {nameof(LoadingOverlayView)} component.");
            }
        }

        private IEnumerator EnsureViewRoutine()
        {
            yield return EnsureView();
        }
    }
}