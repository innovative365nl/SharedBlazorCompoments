using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ExampleApp;
using Innovative.Blazor.Components.Common.Composer;
using ExampleApp.Pages;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.RegisterInnovativeComponents()
       .AddCustomLocalizer<AppDomain>()
       .AddScoped<IAttributeState, AttributeState>()
       ;

await builder.Build()
             .RunAsync()
             .ConfigureAwait(false);
