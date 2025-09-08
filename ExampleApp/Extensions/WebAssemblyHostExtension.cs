#pragma warning disable CA2007

using System.Globalization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace ExampleApp.Extensions;

public static class WebAssemblyHostExtension
{
    public static async Task SetDefaultCultureAsync(this WebAssemblyHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        IJSRuntime jsInterop = host.Services.GetRequiredService<IJSRuntime>();
        string? result = await jsInterop.InvokeAsync<string>("blazorCulture.get");

        var culture = string.IsNullOrEmpty(result)
                          ? new CultureInfo("en-US", true)
                          : new CultureInfo(result, true);

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
