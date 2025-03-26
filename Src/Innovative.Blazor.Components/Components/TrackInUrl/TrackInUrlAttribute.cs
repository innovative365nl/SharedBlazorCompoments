using System.Diagnostics.CodeAnalysis;

namespace Innovative.Blazor.Components.Components.TrackInUrl;

[ExcludeFromCodeCoverage]
[AttributeUsage(validOn: AttributeTargets.Property)]
public sealed class TrackInUrlAttribute : Attribute
{}