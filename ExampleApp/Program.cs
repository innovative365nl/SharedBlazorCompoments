#pragma warning disable CA2007

using ExampleApp;
using ExampleApp.Extensions;
using ExampleApp.Pages;
using ExampleApp.Translations;
using Innovative.Blazor.Components.Common.Composer;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services
       .RegisterInnovativeComponents()
       .AddLocalization()
       .AddCustomLocalizer<Example>()
       .AddLogging()
       .AddScoped<IAttributeState, AttributeState>()
       .AddScoped<DialogService>()
       ;

var app = builder.Build();
await app.SetDefaultCultureAsync();
await app.RunAsync();
