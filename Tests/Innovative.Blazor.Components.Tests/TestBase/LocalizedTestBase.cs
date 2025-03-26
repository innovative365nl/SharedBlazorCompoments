using System;
using Innovative.Blazor.Components.Localizer;
using Microsoft.Extensions.Localization;
using Moq;
using Bunit;

namespace Innovative.Blazor.Components.Tests.TestBase
{
    /// <summary>
    /// Base class for tests that require localization services.
    /// Provides common setup for localizer mocks.
    /// </summary>
    public abstract class LocalizedTestBase : TestContext
    {
        protected LocalizedTestBase()
        {
            // Setup common mocks for localization
            LocalizerMock = new Mock<IInnovativeStringLocalizer>();
            LocalizerFactoryMock = new Mock<IInnovativeStringLocalizerFactory>();


            // Setup localizer factory to return our localizer mock
            LocalizerFactoryMock
                .Setup(f => f.Create(It.IsAny<Type>()))
                .Returns(LocalizerMock.Object);

            // Setup localizer to return the key as the value by default
            LocalizerMock
                .Setup(l => l[It.IsAny<string>()])
                .Returns((string name) => new LocalizedString(name, name));

            // Register the localization services
            Services.AddSingleton(LocalizerFactoryMock.Object);
            Services.AddSingleton(LocalizerMock.Object);
        }

        protected Mock<IInnovativeStringLocalizerFactory> LocalizerFactoryMock { get; set; }

        protected Mock<IInnovativeStringLocalizer> LocalizerMock { get; set; }

        /// <summary>
        /// Configure the localizer mock to return a specific value for a key.
        /// </summary>
        /// <param name="key">The localization key</param>
        /// <param name="value">The localized value to return</param>
        protected void SetupLocalizedString(string key, string value)
        {
            LocalizerMock
                .Setup(l => l[key])
                .Returns(new LocalizedString(key, value, resourceNotFound: false));
        }
    }
}