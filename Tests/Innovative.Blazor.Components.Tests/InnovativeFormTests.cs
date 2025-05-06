using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;
using Innovative.Blazor.Components.Tests.TestBase;
using Moq;

namespace Innovative.Blazor.Components.Tests;

public class InnovativeFormTests : LocalizedTestBase
{
    private readonly Mock<ISidepanelService> customDialogServiceMock;
    private readonly Mock<SidePanelComponent<TestFormModel>> dialogMock;

    public InnovativeFormTests()
    {
        customDialogServiceMock = new Mock<ISidepanelService>();
        dialogMock = new Mock<SidePanelComponent<TestFormModel>>(customDialogServiceMock.Object);
    }

    [Fact]
    public void When_OnParametersSet_ItShouldInitializeFormValues()
    {
        // Arrange
        var model = new TestFormModel
        {
            StringProperty = "Test",
            IntProperty = 42,
            BoolProperty = true,
            DateProperty = new DateTime(2023, 1, 1)
        };

        var component = new InnovativeForm<TestFormModel>(LocalizerFactoryMock.Object)
        {
            Model = model,
            ParentDialog = dialogMock.Object
        };

        // Act
        component.CallOnParametersSet();

        // Assert
        Assert.Equal(model.StringProperty, component.GetFormValue("StringProperty"));
        Assert.Equal(model.IntProperty, component.GetFormValue("IntProperty"));
        Assert.Equal(model.BoolProperty, component.GetFormValue("BoolProperty"));
        Assert.Equal(model.DateProperty, component.GetFormValue("DateProperty"));
    }

    [Fact]
    public async Task When_OnFormSubmit_ItShouldUpdateModelWithFormValues()
    {
        // Arrange
        var model = new TestFormModel
                    {
                        StringProperty = "Test",
                        IntProperty = 42,
                        BoolProperty = true,
                        DateProperty = new DateTime(2023, 1, 1)
                    };

        var component = new InnovativeForm<TestFormModel>(LocalizerFactoryMock.Object)
        {
            Model = model,
            ParentDialog = dialogMock.Object,
        };

        component.CallOnParametersSet();
        component.SetFormValue(nameof(model.StringProperty), "Updated");
        component.SetFormValue(nameof(model.IntProperty), 100);
        component.SetFormValue(nameof(model.BoolProperty), false);
        var testDate = new DateTime(2023, 5, 5);
        component.SetFormValue(nameof(model.DateProperty), testDate);

        // Act
        await component.OnFormSubmit();

        // Assert
        Assert.Equal("Updated", model.StringProperty);
        Assert.Equal(100, model.IntProperty);
        Assert.False(model.BoolProperty);
        Assert.Equal(testDate, model.DateProperty);
    }

    [Fact]
    public async Task When_OnFormReset_ItShouldNotUpdateModelWithFormValues()
    {
        // Arrange
        var actual = new TestFormModel
                    {
                        StringProperty = "Test",
                        IntProperty = 42,
                        BoolProperty = true,
                        DateProperty = new DateTime(2023, 1, 1)
                    };

        var expected = new TestFormModel
                    {
                        StringProperty = actual.StringProperty,
                        IntProperty = actual.IntProperty,
                        BoolProperty = actual.BoolProperty,
                        DateProperty = actual.DateProperty
                    };

        var component = new InnovativeForm<TestFormModel>(LocalizerFactoryMock.Object)
                        {
                            Model = actual,
                            ParentDialog = dialogMock.Object,
                        };

        component.CallOnParametersSet();
        component.SetFormValue(nameof(actual.StringProperty), "Updated");
        component.SetFormValue(nameof(actual.IntProperty), 100);
        component.SetFormValue(nameof(actual.BoolProperty), true);
        var testDate = new DateTime(2023, 5, 5);
        component.SetFormValue(nameof(actual.DateProperty), testDate);

        // Act
        await component.OnFormReset();

        // Assert
        Assert.Equal(expected.StringProperty, actual.StringProperty);
        Assert.Equal(expected.IntProperty, actual.IntProperty);
        Assert.Equal(expected.BoolProperty, actual.BoolProperty);
        Assert.Equal(expected.DateProperty, actual.DateProperty);
    }

    [Fact]
    public void OrganizePropertiesByGroupsGroupsPropertiesCorrectly()
    {
        // Arrange
        var component = new InnovativeForm<TestFormGroupModel>(LocalizerFactoryMock.Object)
        {
            Model = new TestFormGroupModel(),
            ParentDialog = new Mock<SidePanelComponent<TestFormGroupModel>>(customDialogServiceMock.Object).Object
        };

        // Act
        component.CallOnParametersSet();

        // Assert
        var orderedGroups = component.GetOrderedColumnGroups();
        Assert.Equal(2, orderedGroups!.Count);

        var group1 = orderedGroups.FirstOrDefault(g => g.Key == "Group1");
        Assert.Single(group1.Value);
        Assert.Equal("GroupedProperty1", group1.Value[0].Name);

        var group2 = orderedGroups.FirstOrDefault(g => g.Key == "Group2");
        Assert.Single(group2.Value);
        Assert.Equal("GroupedProperty2", group2.Value[0].Name);
        Assert.Single(component.GetUngroupedProperties()!);
        Assert.Equal("UngroupedProperty", component.GetUngroupedProperties()!.First().Name);
    }

    [Fact]
    public void When_GetColumnWidthClass_ItShouldReturnsCorrectClassName()
    {
        // Arrange
        var component = new InnovativeForm<TestFormColumnWidthModel>(LocalizerFactoryMock.Object)
        {
            Model = new TestFormColumnWidthModel(),
            ParentDialog = new Mock<SidePanelComponent<TestFormColumnWidthModel>>(customDialogServiceMock.Object).Object
        };

        // Act
        var width1 = component.CallGetColumnWidthClass("Col1");
        var width2 = component.CallGetColumnWidthClass("Col2");
        var widthNonExistent = component.CallGetColumnWidthClass("NonExistent");

        // Assert
        Assert.Equal("column-span-3", width1);
        Assert.Equal("", width2); // 0 width should return empty string
        Assert.Equal("", widthNonExistent);
    }
}

// Test classes
[UIFormClass("testFormGroupModel")]
public class TestFormGroupModel
{
    [UIFormField(name:nameof(GroupedProperty1), ColumnGroup = "Group1")] public string GroupedProperty1 { get; set; } = string.Empty;

    [UIFormField(name:nameof(GroupedProperty2), ColumnGroup = "Group2")] public string GroupedProperty2 { get; set; } = string.Empty;

    [UIFormField(name: nameof(UngroupedProperty))] public string UngroupedProperty { get; set; } = string.Empty;
}

[UIFormClass("TestFormColumnWidthModel")]
public class TestFormColumnWidthModel: FormModel
{
    public TestFormColumnWidthModel()
    {
        AddViewColumn("Col1", 0, 3, 0);
        AddViewColumn("Col2", 0, 0, 0);
    }

    [UIFormField(name:nameof(Property1), ColumnGroup = "Col1")] public string Property1 { get; set; } = string.Empty;

    [UIFormField(name: nameof(Property2), ColumnGroup = "Col2")] public string Property2 { get; set; } = string.Empty;
}

public class TestFormModel
{
    [UIFormField(name: nameof(StringProperty))] public string StringProperty { get; set; } = string.Empty;

    [UIFormField(name: nameof(IntProperty))] public int IntProperty { get; set; }

    [UIFormField(name: nameof(BoolProperty))] public bool BoolProperty { get; set; }

    [UIFormField(name: nameof(DateProperty))] public DateTime DateProperty { get; set; }
}

// Extension methods to access private methods/properties for testing
public static class DynamicFormViewTestExtensions
{
    public static void CallOnParametersSet<T>(this InnovativeForm<T> component)
    {
        var method = typeof(InnovativeForm<T>).GetMethod("OnParametersSet",
                                                                    BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(component, null);
    }

    public static object? GetFormValue<T>(this InnovativeForm<T> component, string propertyName)
    {
        var formValuesField = typeof(InnovativeForm<T>).GetField("formValues",
                                                                            BindingFlags.NonPublic | BindingFlags.Instance);
        var formValues = (Dictionary<string, object>)formValuesField?.GetValue(component)!;
        return formValues?.TryGetValue(propertyName, out var value) == true ? value : null;
    }

    public static void SetFormValue<T>(this InnovativeForm<T> component, string propertyName, object value)
    {
        var setValueMethod = typeof(InnovativeForm<T>).GetMethod("SetValue",
                                                                            BindingFlags.NonPublic | BindingFlags.Instance);
        setValueMethod?.Invoke(component, new[] { propertyName, value });
    }

    public static IReadOnlyCollection<PropertyInfo>? GetUngroupedProperties<T>(this InnovativeForm<T> component)
    {
        var property = typeof(InnovativeForm<T>).GetProperty("ungroupedProperties",
                                                                        BindingFlags.NonPublic | BindingFlags.Instance);
        return (IReadOnlyCollection<PropertyInfo>)property?.GetValue(component)!;
    }

    public static IReadOnlyCollection<KeyValuePair<string, List<PropertyInfo>>>? GetOrderedColumnGroups<T>(
        this InnovativeForm<T> component)
    {
        var property = typeof(InnovativeForm<T>).GetProperty("OrderedColumnGroups",
                                                                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return (IReadOnlyCollection<KeyValuePair<string, List<PropertyInfo>>>)property?.GetValue(component)!;
    }

    public static string CallGetColumnWidthClass<T>(this InnovativeForm<T> component, string columnGroup)
    {
        var method = typeof(InnovativeForm<T>).GetMethod("GetColumnWidthClass",
                                                                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        return (string)method!.Invoke(component, [columnGroup])!;
    }
}
