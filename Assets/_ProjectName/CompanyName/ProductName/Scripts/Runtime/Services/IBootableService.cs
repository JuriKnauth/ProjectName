using System.Collections;

namespace CompanyName.ProductName.Scripts.Runtime.Services.BootManagerServices
{
    public interface IBootableService
    {
        string Name { get; }
        bool IsReady { get; }
        IEnumerator Initialize();
    }
}