#pragma warning disable CA2007

using System.Globalization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace ExampleApp.Extensions;

public static class WebAssemblyHostExtension
{
    public static async Task SetDefaultCultureAsync(this WebAssemblyHost host)
    {
        ArgumentNullException.ThrowIfNull(argument: host);

        IJSRuntime jsInterop = host.Services.GetRequiredService<IJSRuntime>();
        var appCulture = await GetAppCultureAsync(jsInterop);
        var culture = GetCultureInfo(appCulture);

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    private static async Task<string?> GetAppCultureAsync(IJSRuntime jsInterop)
    {
        string? result = null;
        try
        {
            result = await jsInterop.InvokeAsync<string>(identifier: "exampleAppCulture.get");
        }
        catch (JSDisconnectedException) { }
        catch (JSException) { }
        return result;
    }

    private static CultureInfo GetCultureInfo(string? name)
    {
        string cultureName = string.IsNullOrWhiteSpace(value: name) ? "en-US" : name;
        CultureInfo result;
        try
        {
            result = new CultureInfo(name: cultureName, useUserOverride: true);
        }
        catch (CultureNotFoundException)
        {
            result = new CultureInfo(name: "en-US", useUserOverride: true);
        }
        return result;
    }
}
