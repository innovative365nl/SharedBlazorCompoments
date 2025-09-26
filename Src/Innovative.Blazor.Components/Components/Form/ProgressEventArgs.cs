namespace Innovative.Blazor.Components.Components;

public sealed class ProgressEventArgs : EventArgs
{
    public ProgressEventArgs(string message)
    {
        Message = message ?? string.Empty;
    }

    public string Message { get; }
}
