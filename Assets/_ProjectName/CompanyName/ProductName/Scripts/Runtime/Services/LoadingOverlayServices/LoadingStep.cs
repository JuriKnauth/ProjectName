using System;
using System.Collections;

namespace CompanyName.ProductName.Scripts.Runtime.Services.LoadingOverlayServices
{
    public readonly struct LoadingStep
    {
        public string Name { get; }
        public float MaxDurationSeconds { get; }

        public Func<IEnumerator> AsyncAction { get; }
        public Func<bool> Condition { get; }
        public Func<float> ProgressGetter { get; }
        public Action OnCompleted { get; }

        public LoadingStep(
            string name,
            Func<IEnumerator> asyncAction = null,
            Func<float> progressGetter = null,
            Action onCompleted = null,
            float maxDurationSeconds = 0f,
            Func<bool> condition = null)
        {
            Name = name;
            AsyncAction = asyncAction;
            ProgressGetter = progressGetter;
            OnCompleted = onCompleted;
            MaxDurationSeconds = maxDurationSeconds;
            Condition = condition;
        }
    }
}