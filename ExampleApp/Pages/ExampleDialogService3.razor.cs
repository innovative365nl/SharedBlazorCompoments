using System.Reflection.PortableExecutable;
using ExampleApp.Translations;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Enumerators;
using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Pages;

public partial class ExampleDialogService3(IInnovativeSidePanelService sidePanelService) : ComponentBase
{
    private SingleColumnModel singleColumn = new();
    private DoubleColumnModel doubleColumn = new();
    private MultiColumnModel multiColumn = new();

    public async Task OpenOneColumnDialog()
    {
        await sidePanelService
              .OpenInDisplayMode(singleColumn)
              .ConfigureAwait(false);
    }

    public async Task OpenDoubleColumDialog()
    {
        await sidePanelService
              .OpenInDisplayMode(doubleColumn)
              .ConfigureAwait(false);
    }

    public async Task OpenMultiColumnDialog()
    {
        await sidePanelService
              .OpenInDisplayMode(multiColumn)
              .ConfigureAwait(false);
    }

    public async Task OpenMultiColumnDialogLarge()
    {
        await sidePanelService
              .OpenInDisplayMode(multiColumn, width: SideDialogWidth.Large)
              .ConfigureAwait(false);
    }

    public async Task OpenMultiColumnDialogExtraLarge()
    {
        await sidePanelService
              .OpenInDisplayMode(multiColumn, width: SideDialogWidth.ExtraLarge)
              .ConfigureAwait(false);
    }
}

[UIFormClass(title: "Single Column Form", ResourceType = typeof(Example))]
public class SingleColumnModel : FormModel
{
    private const string FirstColumn = "Name";
    public SingleColumnModel()
    {
        AddViewColumn(name: FirstColumn, order: 1, width: 12, offset: 0);
    }

    [UIFormField(name: "Field1", ColumnGroup = FirstColumn)]
    public string Field1 { get; set; } = "Field1";
    [UIFormField(name: "Field2", ColumnGroup = FirstColumn)]
    public string Field2 { get; set; } = "Field2";
    [UIFormField(name: "Field3", ColumnGroup = FirstColumn)]
    public string Field3 { get; set; } = "Field3";
    [UIFormField(name: "Field4", ColumnGroup = FirstColumn)]
    public string Field4 { get; set; } = "Field4";
}

[UIFormClass(title: "Double Column Form", ResourceType = typeof(Example))]
public class DoubleColumnModel : FormModel
{
    private const string FirstColumn = "First";
    private const string SecondColumn = "Second";
    public DoubleColumnModel()
    {
        AddViewColumn(name: FirstColumn, order: 1, width: 6, offset: 0);
        AddViewColumn(name: SecondColumn, order: 1, width: 6, offset: 0);
    }

    [UIFormField(name: "FieldLeft1", ColumnGroup = FirstColumn)]
    public string Field1 { get; set; } = "Field1";
    [UIFormField(name: "FieldLeft2", ColumnGroup = FirstColumn)]
    public string Field2 { get; set; } = "Field2";
    [UIFormField(name: "FieldRight3", ColumnGroup = SecondColumn)]
    public string Field3 { get; set; } = "Field3";
    [UIFormField(name: "FieldRight4", ColumnGroup = SecondColumn)]
    public string Field4 { get; set; } = "Field4";
}

[UIFormClass(title: "Multi Column Form", ResourceType = typeof(Example))]
public class MultiColumnModel : FormModel
{
    private const string FirstColumn = "First";
    private const string SecondColumn = "Second";
    private const string ThirdColumn = "Third";
    private const string FullWidthColumn = "Fourth";
    private const string BottomLeftColumn = "BottomLeft";
    private const string BottomRightColumn = "BottomRight";
    public MultiColumnModel()
    {
        AddViewColumn(name: FirstColumn, order: 1, width: 3, offset: 0);
        AddViewColumn(name: SecondColumn, order: 1, width: 3, offset: 0);
        AddViewColumn(name: ThirdColumn, order: 1, width: 6, offset: 0);
        AddViewColumn(name: FullWidthColumn, order: 1, width: 12, offset: 0);;
        AddViewColumn(name: BottomLeftColumn, order: 1, width: 6, offset: 0);
        AddViewColumn(name: BottomRightColumn, order: 1, width: 6, offset: 0);
    }

    [UIFormField(name: "FieldLeft1", ColumnGroup = FirstColumn)]
    public string Field1 { get; set; } = "Field1";
    [UIFormField(name: "FieldLeft2", ColumnGroup = SecondColumn)]
    public string Field2 { get; set; } = "Field2";
    [UIFormField(name: "FieldRight50%3", ColumnGroup = ThirdColumn, DisplayParameters = ["style=background-color: orange;"])]
    public string Field3 { get; set; } = "Field3";
    [UIFormField(name: "FieldBottomFullWidth4", ColumnGroup = FullWidthColumn, FormParameters = ["style=background-color: pink;"], DisplayParameters = ["style=background-color: pink;"])]
    public string Field4 { get; set; } = "Field4";
    [UIFormField("FieldInt50%width", ColumnGroup = BottomLeftColumn)]
    public int Field5 { get; set; } = 5;
    [UIFormField("FieldInt50%width", ColumnGroup = BottomRightColumn)]
    public DateTime Field6 { get; set; } = DateTime.Today;
}
