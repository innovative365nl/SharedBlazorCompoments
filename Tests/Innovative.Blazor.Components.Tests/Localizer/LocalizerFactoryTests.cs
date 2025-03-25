// File: InnovativeStringLocalizerFactoryTests.cs

using System;
using System.Collections.Generic;
using Innovative.Blazor.Components.Localizer;
using Microsoft.Extensions.Localization;

namespace Innovative.Blazor.Components.Tests.Localizer;

public class InnovativeStringLocalizerFactoryTests
{
    [Fact]
    public void Create_Returns_InnovativeStringLocalizer_Instance()
    {
        var services = new ServiceCollection();
        var dummyLocalizerFactory = new DummyStringLocalizerFactory();
        services.AddSingleton<IStringLocalizerFactory>(dummyLocalizerFactory);
        services.AddSingleton<IInnovativeStringLocalizerFactory, InnovativeStringLocalizerFactory>();

        var serviceProvider = services.BuildServiceProvider();
        var localizerFactory = serviceProvider.GetRequiredService<IInnovativeStringLocalizerFactory>();

        var localizer = localizerFactory.Create(typeof(DummyResource));

        Assert.NotNull(localizer);
        Assert.IsType<InnovativeStringLocalizer<DummyResource>>(localizer);
    }
}

public class DummyStringLocalizerFactory : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource)
    {
        return new DummyStringLocalizer();
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return new DummyStringLocalizer();
    }
}
public class DummyStringLocalizer : IStringLocalizer
{
    public LocalizedString this[string name] => new LocalizedString(name, $"Dummy value for {name}", resourceNotFound: false);

    public LocalizedString this[string name, params object[] arguments] => new LocalizedString(name, $"Dummy value for {name}", resourceNotFound: false);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return new[] { new LocalizedString("Dummy", "Dummy value", resourceNotFound: false) };
    }
}