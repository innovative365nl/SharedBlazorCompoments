#region

using Microsoft.AspNetCore.Components;

#endregion

namespace Innovative.Blazor.Components.Tests.TestHelpers
{
    /// <summary>
    ///     Test implementation of NavigationManager for testing components
    ///     that interact with navigation.
    /// </summary>
    public class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager()
        {
            Initialize("https://test.com/", "https://test.com/");
        }

        /// <summary>
        ///     Navigate to the specified URI.
        /// </summary>
        /// <param name="uri">The destination URI.</param>
        /// <param name="forceLoad">Not used in this implementation.</param>
        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Uri = uri;
            NotifyLocationChanged(false);
        }
    }
}