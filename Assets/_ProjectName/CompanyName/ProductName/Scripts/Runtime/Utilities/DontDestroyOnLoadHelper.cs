using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.Utilities
{
    public sealed class DontDestroyOnLoadHelper : MonoBehaviour
    {
        private void Awake() => DontDestroyOnLoad(gameObject);
    }
}