using System.Collections.Generic;
using System.Linq;
using Innovative.Blazor.Components.Localizer;
using Microsoft.Extensions.Localization;
using Moq;

namespace Innovative.Blazor.Components.Tests.Localizer;


public class InnovativeStringLocalizerTests
{
    [Fact]
    public void IndexerUsesFallbackWhenPrimaryNotFound()
    {
        const string key = "TestKey";
        var primaryLocalizedResult = new LocalizedString(key, string.Empty, resourceNotFound: true);
        var fallbackLocalizedResult = new LocalizedString(key, "Fallback value", resourceNotFound: false);

        var primaryLocalizerMock = new Mock<IStringLocalizer>();
        primaryLocalizerMock.Setup(l => l[key])
            .Returns(primaryLocalizedResult);

        var fallbackLocalizerMock = new Mock<IStringLocalizer>();
        fallbackLocalizerMock.Setup(l => l[key])
            .Returns(fallbackLocalizedResult);

        var factoryMock = new Mock<IStringLocalizerFactory>();
        factoryMock.Setup(f => f.Create(typeof(DummyResource)))
            .Returns(primaryLocalizerMock.Object);
        factoryMock.Setup(f => f.Create(typeof(FallbackResource)))
            .Returns(fallbackLocalizerMock.Object);

        var localizer = new InnovativeStringLocalizer<DummyResource>(factoryMock.Object, typeof(FallbackResource));

        var result = localizer[key];

        Assert.Equal("Fallback value", result.Value);
    }

    [Fact]
    public void IndexerUsesPrimaryWhenFound()
    {
        // Arrange
        const string key = "TestKey";
        var primaryLocalizedResult = new LocalizedString(key, "Primary value", resourceNotFound: false);
        var fallbackLocalizedResult = new LocalizedString(key, "Fallback value", resourceNotFound: false);

        var primaryLocalizerMock = new Mock<IStringLocalizer>();
        primaryLocalizerMock.Setup(l => l[key])
            .Returns(primaryLocalizedResult);

        var fallbackLocalizerMock = new Mock<IStringLocalizer>();

        fallbackLocalizerMock.Setup(l => l[key])
            .Returns(fallbackLocalizedResult);

        var factoryMock = new Mock<IStringLocalizerFactory>();
        factoryMock.Setup(f => f.Create(typeof(DummyResource)))
            .Returns(primaryLocalizerMock.Object);
        factoryMock.Setup(f => f.Create(typeof(FallbackResource)))
            .Returns(fallbackLocalizerMock.Object);

        var localizer = new InnovativeStringLocalizer<DummyResource>(factoryMock.Object, typeof(FallbackResource));

        var result = localizer[key];

        Assert.Equal("Primary value", result.Value);
    }

    [Fact]
    public void GetAllStringsReturnsMergedValuesFromPrimaryAndFallback()
    {
        var primaryStrings = new List<LocalizedString>
        {
            new LocalizedString("Test1", "Primary value 1", resourceNotFound: false),
            new LocalizedString("Test2", "Primary value 2", resourceNotFound: false)
        };

        var fallbackStrings = new List<LocalizedString>
        {
            new LocalizedString("Test2", "Fallback value 2", resourceNotFound: false),
            new LocalizedString("Test3", "Fallback value 3", resourceNotFound: false)
        };

        var primaryLocalizerMock = new Mock<IStringLocalizer>();
        primaryLocalizerMock.Setup(l => l.GetAllStrings(It.IsAny<bool>()))
            .Returns(primaryStrings);

        var fallbackLocalizerMock = new Mock<IStringLocalizer>();
        fallbackLocalizerMock.Setup(l => l.GetAllStrings(It.IsAny<bool>()))
            .Returns(fallbackStrings);

        var factoryMock = new Mock<IStringLocalizerFactory>();
        factoryMock.Setup(f => f.Create(typeof(DummyResource)))
            .Returns(primaryLocalizerMock.Object);
        factoryMock.Setup(f => f.Create(typeof(FallbackResource)))
            .Returns(fallbackLocalizerMock.Object);

        var localizer = new InnovativeStringLocalizer<DummyResource>(factoryMock.Object, typeof(FallbackResource));

        var allStrings = localizer.GetAllStrings(includeParentCultures: false).ToList();

        Assert.Equal(3, allStrings.Count);
        Assert.Contains(allStrings, ls => ls.Name == "Test1" && ls.Value == "Primary value 1");
        Assert.Contains(allStrings, ls => ls.Name == "Test2" && ls.Value == "Primary value 2");
        Assert.Contains(allStrings, ls => ls.Name == "Test3" && ls.Value == "Fallback value 3");
    }
    
       [Fact]
        public void SetFallbackLocalizerUpdatesFallbackBehavior()
        {
            const string key = "TestKey";
            var primaryResult = new LocalizedString(key, string.Empty, resourceNotFound: true);
            var fallbackResult = new LocalizedString(key, "Fallback value", resourceNotFound: false);
            
            var primaryMock = new Mock<IStringLocalizer>();
            primaryMock.Setup(l => l[key]).Returns(primaryResult);
            
            var fallbackMock = new Mock<IStringLocalizer>();
            fallbackMock.Setup(l => l[key]).Returns(fallbackResult);
            
            var factoryMock = new Mock<IStringLocalizerFactory>();
            factoryMock.Setup(f => f.Create(typeof(DummyResource))).Returns(primaryMock.Object);
            
            var localizer = new InnovativeStringLocalizer<DummyResource>(factoryMock.Object, fallbackResourceType: null);
            
            var resultBefore = localizer[key];
            Assert.True(resultBefore.ResourceNotFound);
            
            localizer.SetFallbackLocalizer(fallbackMock.Object);
            
            var resultAfter = localizer[key];
            Assert.False(resultAfter.ResourceNotFound);
            Assert.Equal("Fallback value", resultAfter.Value);
        }
        
        [Fact]
        public void SetResourceTypeUpdatesPrimaryLocalizer()
        {
            const string key = "TestKey";
            var originalResult = new LocalizedString(key, "Original primary value", resourceNotFound: false);
            var newResult = new LocalizedString(key, "New primary value", resourceNotFound: false);
            
            var originalMock = new Mock<IStringLocalizer>();
            originalMock.Setup(l => l[key]).Returns(originalResult);
            
            var newLocalizerMock = new Mock<IStringLocalizer>();
            newLocalizerMock.Setup(l => l[key]).Returns(newResult);
            
            var factoryMock = new Mock<IStringLocalizerFactory>();
            factoryMock.Setup(f => f.Create(typeof(DummyResource))).Returns(originalMock.Object);
            factoryMock.Setup(f => f.Create(typeof(AlternateResource))).Returns(newLocalizerMock.Object);
            
            var localizer = new InnovativeStringLocalizer<DummyResource>(factoryMock.Object, fallbackResourceType: null);
            
            var initialResult = localizer[key];
            Assert.Equal("Original primary value", initialResult.Value);
            
            localizer.SetResourceType(typeof(AlternateResource));
            
            var updatedResult = localizer[key];
            Assert.Equal("New primary value", updatedResult.Value);
        }
        private sealed class DummyResource { }
        private sealed class AlternateResource { }
    
        private sealed  class FallbackResource { }

    }


