namespace Innovative.Blazor.Components.Components;

public interface INotifyFormValueChanged
{
    void OnFormValueChanged(string propertyName, object? value);
}
