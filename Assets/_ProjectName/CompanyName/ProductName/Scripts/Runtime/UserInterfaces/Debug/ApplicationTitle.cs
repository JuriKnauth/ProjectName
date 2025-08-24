using TMPro;
using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.UserInterfaces.Debug
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class ApplicationTitle : MonoBehaviour
    {
        private void Awake()
        {
            TextMeshProUGUI textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.SetText($"{Application.productName} {Application.version}");
        }
    }
}