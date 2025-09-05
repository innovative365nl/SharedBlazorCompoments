using System.Globalization;
using ExampleApp;
using ExampleApp.Pages;
using Innovative.Blazor.Components.Common.Composer;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.RegisterInnovativeComponents()
       .AddCustomLocalizer<AppDomain>()
       .AddLogging()
       .AddScoped<IAttributeState, AttributeState>()
       .AddScoped<DialogService>()
       ;

var culture = new CultureInfo("nl-NL");
Thread.CurrentThread.CurrentCulture = culture;
Thread.CurrentThread.CurrentUICulture = culture;

await builder.Build()
             .RunAsync()
             .ConfigureAwait(false);
