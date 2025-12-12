using System.ComponentModel.DataAnnotations;

namespace ExampleApp.Attributes;

public sealed class AbsoluteUriAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
        => value is null
        || Uri.TryCreate(value.ToString(), UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
}
