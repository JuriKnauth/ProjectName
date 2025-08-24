namespace CompanyName.ProductName.Scripts.Runtime.Services.LoadingOverlayServices
{
    public readonly struct LoadingCommand
    {
        public LoadingStep[] Steps { get; }

        public LoadingCommand(LoadingStep[] steps) => Steps = steps;
    }
}