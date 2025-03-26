using System;
using System.Collections.Generic;
using Bunit;
using Innovative.Blazor.Components.Components.Grid;
using Radzen;

namespace Innovative.Blazor.Components.Tests.TestHelpers
{
    /// <summary>
    /// Helper class for testing InnovativeGrid components.
    /// Provides methods for rendering grid components with various configurations.
    /// </summary>
    public static class GridTestHelper
    {
        /// <summary>
        /// Helper method to render a grid component with common parameters to reduce code duplication.
        /// </summary>
        public static IRenderedComponent<InnovativeGrid<T>> RenderGridComponent<T>(
            this TestContext? testContext,
            IEnumerable<T> data,
            string? title = null,
            bool isLoading = false,
            bool enableRowSelection = false,
            DataGridSelectionMode selectionMode = DataGridSelectionMode.Single,
            Action<IEnumerable<T>>? onSelectionChanged = null,
            GridHeight minHeightOption = GridHeight.Minimal) where T : class
        {
            if (testContext != null)
                return testContext.RenderComponent<InnovativeGrid<T>>(parameters =>
                {
                    parameters.Add(p => p.Data, data);

                    if (title != null)
                        parameters.Add(p => p.Title, title);

                    if (isLoading)
                        parameters.Add(p => p.IsLoading, true);

                    if (enableRowSelection)
                        parameters.Add(p => p.EnableRowSelection, true);

                    parameters.Add(p => p.SelectionMode, selectionMode);

                    if (onSelectionChanged != null)
                        parameters.Add<IEnumerable<T>>(p => p.OnSelectionChanged, onSelectionChanged);

                    parameters.Add(p => p.MinHeightOption, minHeightOption);
                });
            return null!;
        }
    }
}