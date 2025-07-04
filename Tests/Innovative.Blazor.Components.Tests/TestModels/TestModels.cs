using System;
using Innovative.Blazor.Components.Components;

namespace Innovative.Blazor.Components.Tests.TestModels
{
    /// <summary>
    ///     Resource class used for tests.
    /// </summary>
    public class TestResources
    {
    }

    /// <summary>
    ///     Standard test model with UIGridField attributes.
    /// </summary>
    public class TestModel
    {
        [UIGridField(IsSortable = true)]
        public string? TestProperty { get; set; }

        [UIGridField(CustomComponentType= typeof(TestCustomComponent), Parameters = ["CustomParam:test"])]
        public string? CustomProperty { get; set; }
    }

    /// <summary>
    ///     Test model with mixed properties - some with UIGridField attributes, some without.
    /// </summary>
    public class TestModelWithMixedAttributes
    {
        [UIGridField] public string? PropertyWithAttribute { get; set; }

        public string? PropertyWithoutAttribute { get; set; }
    }

    /// <summary>
    ///     Test model with UIGridClass attribute.
    /// </summary>
    [UIGridClass(AllowSorting = false, DefaultSortField = "TestProperty", ResourceType = typeof(TestResources))]
    public class TestModelWithGridClass
    {
        [UIGridField(IsSortable = true)]
        public string? TestProperty { get; set; }

        [UIGridField] public string? AnotherProperty { get; set; }
    }

    /// <summary>
    ///     Model for testing forms with form attributes.
    /// </summary>
    [UIFormClass(title: "Test Form", ResourceType = typeof(TestResources))]
    public class TestDynamicFormModel
    {
        [UIFormField(name: "Test Property")] public string? TestProperty { get; set; }

        public string? CustomProperty { get; set; }

        [UIFormViewAction(name: "Test Action")]
        public Action? TestAction { get; set; }
    }

    /// <summary>
    ///     Resource class for specific form testing.
    /// </summary>
    public class TestResourcesClass
    {
    }
}
