using System;
using System.Collections;
using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.Utilities
{
    public static class MonoBehaviourExtensions
    {
        public static void StopAndClear(this MonoBehaviour owner, ref Coroutine handle)
        {
            if (!owner)
            {
                handle = null;
                return;
            }

            if (handle != null) owner.StopCoroutine(handle);
            handle = null;
        }

        public static Coroutine RestartCoroutine(this MonoBehaviour owner, ref Coroutine handle, IEnumerator routine)
        {
            if (!owner || routine == null) return handle;
            if (handle != null) owner.StopCoroutine(handle);
            handle = owner.StartCoroutine(routine);
            return handle;
        }

        public static Coroutine StartDelayed(
            this MonoBehaviour owner,
            ref Coroutine handle,
            IEnumerator routine,
            float delay,
            bool unscaled = true)
        {
            if (!owner || routine == null) return handle;
            if (handle != null) owner.StopCoroutine(handle);
            handle = owner.StartCoroutine(DelayThenRun(owner, routine, delay, unscaled));
            return handle;
        }

        public static Coroutine StartDelayed(
            this MonoBehaviour owner,
            ref Coroutine handle,
            Action action,
            float delay,
            bool unscaled = true)
        {
            if (!owner || action == null) return handle;
            if (handle != null) owner.StopCoroutine(handle);
            handle = owner.StartCoroutine(DelayThenInvoke(owner, action, delay, unscaled));
            return handle;
        }

        private static IEnumerator DelayThenRun(MonoBehaviour owner, IEnumerator routine, float delay, bool unscaled)
        {
            if (delay > 0f)
            {
                if (unscaled) yield return new WaitForSecondsRealtime(delay);
                else yield return new WaitForSeconds(delay);
            }

            if (!owner) yield break;
            yield return routine;
        }

        private static IEnumerator DelayThenInvoke(MonoBehaviour owner, Action action, float delay, bool unscaled)
        {
            if (delay > 0f)
            {
                if (unscaled) yield return new WaitForSecondsRealtime(delay);
                else yield return new WaitForSeconds(delay);
            }

            if (!owner) yield break;
            action();
        }
    }
}