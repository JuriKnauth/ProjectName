using System;
using System.Collections.Generic;
using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.Configurations
{
    [CreateAssetMenu(fileName = "Settings",
        menuName = "Company/Product/Configurations/Settings")]
    public sealed class Settings : ScriptableObject
    {
        [SerializeField] private List<SettingsSection> sections = new List<SettingsSection>();

        private Dictionary<Type, SettingsSection> _cache;

        public T Get<T>() where T : SettingsSection
        {
            if (_cache == null || _cache.Count == 0) RebuildCache();
            return _cache != null && _cache.TryGetValue(typeof(T), out SettingsSection s)
                ? (T)s
                : null;
        }

        private void OnEnable() => RebuildCache();

        private void RebuildCache()
        {
            if (_cache == null) _cache = new Dictionary<Type, SettingsSection>();
            else _cache.Clear();

            foreach (SettingsSection s in sections)
            {
                if (!s) continue;
                Type t = s.GetType();
                if (!_cache.ContainsKey(t)) _cache[t] = s;
            }
        }
    }
}