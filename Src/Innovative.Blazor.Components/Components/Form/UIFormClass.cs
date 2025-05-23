using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components;

[AttributeUsage(validOn: AttributeTargets.Class, Inherited = false)]
public sealed class UIFormClass(string title) : UIClass
{
    /// <summary>
    ///     Returns the title of the form.
    /// </summary>
    /// <remarks>
    ///     The title is used as a key to get the resource string. If the resource is not found it will be used as a caption.
    /// </remarks>
    public string Title { get; } = title;
}
