#region

using System.Diagnostics.CodeAnalysis;

#endregion

namespace Innovative.Blazor.Components.Components;

[ExcludeFromCodeCoverage]
[AttributeUsage(validOn: AttributeTargets.Property)]
public sealed class TrackInUrlAttribute : Attribute
{
}