using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ExampleApp.Components;

public partial class CultureSelector(NavigationManager navManager, IJSRuntime jsRuntime)
{
    private readonly CultureInfo[] cultures =
    [
        new CultureInfo("en-US"),
        new CultureInfo("de-DE"),
        new CultureInfo("nl-NL")
    ];

    internal CultureInfo Culture
    {
        get => CultureInfo.CurrentCulture;
        set
        {
            if (Equals(CultureInfo.CurrentCulture, value))
                return;

            var js = (IJSInProcessRuntime)jsRuntime;
            js.InvokeVoid("exampleAppCulture.set", value.Name);
            navManager.NavigateTo(navManager.Uri, true);
        }
    }
}
