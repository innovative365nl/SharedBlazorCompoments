#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Innovative.Blazor.Components.Components.Dialog;
using Innovative.Blazor.Components.Services;
using Innovative.Blazor.Components.Tests.TestBase;
using Microsoft.AspNetCore.Components;
using Moq;

#endregion

namespace Innovative.Blazor.Components.Tests;

public class DynamicFormViewTests : LocalizedTestBase
{
    private readonly Mock<ICustomDialogService> _customDialogServiceMock;
    private readonly Mock<RightSideDialog<TestFormModel>> _dialogMock;

    public DynamicFormViewTests()
    {
        _customDialogServiceMock = new Mock<ICustomDialogService>();
        _dialogMock = new Mock<RightSideDialog<TestFormModel>>(_customDialogServiceMock.Object);
    }


    [Fact]
    public void OnParametersSetInitializesFormValues()
    {
        // Arrange
        var model = new TestFormModel
        {
            StringProperty = "Test",
            IntProperty = 42,
            BoolProperty = true,
            DateProperty = new DateTime(2023, 1, 1)
        };

        var component = new DynamicFormView<TestFormModel>(LocalizerFactoryMock.Object)
        {
            Model = model,
            ParentDialog = _dialogMock.Object
        };

        // Act
        component.CallOnParametersSet();

        // Assert
        Assert.Equal("Test", component.GetFormValue("StringProperty"));
        Assert.Equal(42, component.GetFormValue("IntProperty"));
        Assert.Equal(true, component.GetFormValue("BoolProperty"));
        Assert.Equal(new DateTime(2023, 1, 1), component.GetFormValue("DateProperty"));
    }

    [Fact]
    public async Task OnSubmitPressedUpdatesModelWithFormValues()
    {
        // Arrange
        var model = new TestFormModel();
        bool saveCallbackInvoked = false;

        var component = new DynamicFormView<TestFormModel>(LocalizerFactoryMock.Object)
        {
            Model = model,
            ParentDialog = _dialogMock.Object,
            OnSave = EventCallback.Factory.Create<TestFormModel>(this, _ => saveCallbackInvoked = true)
        };

        component.CallOnParametersSet();
        component.SetFormValue("StringProperty", "Updated");
        component.SetFormValue("IntProperty", 100);
        component.SetFormValue("BoolProperty", true);
        var testDate = new DateTime(2023, 5, 5);
        component.SetFormValue("DateProperty", testDate);

        // Act
        await component.OnSubmitPressed();

        // Assert
        Assert.Equal("Updated", model.StringProperty);
        Assert.Equal(100, model.IntProperty);
        Assert.True(model.BoolProperty);
        Assert.Equal(testDate, model.DateProperty);
        Assert.True(saveCallbackInvoked);
    }

    [Fact]
    public async Task OnCancelPressedInvokesCancelCallback()
    {
        // Arrange
        bool cancelCallbackInvoked = false;
        var component = new DynamicFormView<TestFormModel>(LocalizerFactoryMock.Object)
        {
            Model = new TestFormModel(),
            ParentDialog = _dialogMock.Object,
            OnCancel = EventCallback.Factory.Create(this, () => cancelCallbackInvoked = true)
        };

        // Act
        await component.OnCancelPressed();

        // Assert
        Assert.True(cancelCallbackInvoked);
    }

    [Fact]
    public void OrganizePropertiesByGroupsGroupsPropertiesCorrectly()
    {
        // Arrange
        var component = new DynamicFormView<TestFormGroupModel>(LocalizerFactoryMock.Object)
        {
            Model = new TestFormGroupModel(),
            ParentDialog = new Mock<RightSideDialog<TestFormGroupModel>>(_customDialogServiceMock.Object).Object
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
    public void GetColumnWidthClassReturnsCorrectClass()
    {
        // Arrange
        var component = new DynamicFormView<TestFormColumnWidthModel>(LocalizerFactoryMock.Object)
        {
            Model = new TestFormColumnWidthModel(),
            ParentDialog = new Mock<RightSideDialog<TestFormColumnWidthModel>>(_customDialogServiceMock.Object).Object
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
[UIFormClass("testFormGroupModel", ColumnOrder = new[] { "Group1", "Group2" })]
public class TestFormGroupModel
{
    [UIFormField(name:nameof(GroupedProperty1), ColumnGroup = "Group1")] public string GroupedProperty1 { get; set; } = string.Empty;

    [UIFormField(name:nameof(GroupedProperty2), ColumnGroup = "Group2")] public string GroupedProperty2 { get; set; } = string.Empty;

    [UIFormField(name: nameof(UngroupedProperty))] public string UngroupedProperty { get; set; } = string.Empty;
}

[UIFormClass("TestFormColumnWidthModel",
    ColumnWidthNames = new[] { "Col1", "Col2" },
    ColumnWidthValues = new[] { 3, 0 })]
public class TestFormColumnWidthModel
{
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
    public static void CallOnInitialized<T>(this DynamicFormView<T> component)
    {
        var method = typeof(DynamicFormView<T>).GetMethod("OnInitialized",
            BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(component, null);
    }

    public static void CallOnParametersSet<T>(this DynamicFormView<T> component)
    {
        var method = typeof(DynamicFormView<T>).GetMethod("OnParametersSet",
            BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(component, null);
    }

    public static object? GetFormValue<T>(this DynamicFormView<T> component, string propertyName)
    {
        var formValuesField = typeof(DynamicFormView<T>).GetField("_formValues",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var formValues = (Dictionary<string, object>)formValuesField?.GetValue(component)!;
        return formValues?.TryGetValue(propertyName, out var value) == true ? value : null;
    }

    public static void SetFormValue<T>(this DynamicFormView<T> component, string propertyName, object value)
    {
        var setValueMethod = typeof(DynamicFormView<T>).GetMethod("SetValue",
            BindingFlags.NonPublic | BindingFlags.Instance);
        setValueMethod?.Invoke(component, new[] { propertyName, value });
    }

    public static IReadOnlyCollection<PropertyInfo>? GetUngroupedProperties<T>(this DynamicFormView<T> component)
    {
        var property = typeof(DynamicFormView<T>).GetProperty("UngroupedProperties",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (IReadOnlyCollection<PropertyInfo>)property?.GetValue(component)!;
    }

    public static IReadOnlyCollection<KeyValuePair<string, List<PropertyInfo>>>? GetOrderedColumnGroups<T>(
        this DynamicFormView<T> component)
    {
        var property = typeof(DynamicFormView<T>).GetProperty("OrderedColumnGroups",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return (IReadOnlyCollection<KeyValuePair<string, List<PropertyInfo>>>)property?.GetValue(component)!;
    }

    public static string CallGetColumnWidthClass<T>(this DynamicFormView<T> component, string columnGroup)
    {
        var method = typeof(DynamicFormView<T>).GetMethod("GetColumnWidthClass",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        return (string)method!.Invoke(component, new[] { columnGroup })! ?? string.Empty;
    }
}