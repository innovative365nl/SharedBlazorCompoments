using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Innovative.Blazor.Components.Components.Dialog;
using Innovative.Blazor.Components.Services;
using ManagementPortal.Website.Components.Example;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace Innovative.Blazor.Components.Tests.Dialog;


public partial class InnovativeDialogServiceTest
{

    private PersonModel _person = new()
    {
        FirstName = "John",
        LastName = "Doe",
        IsActive = true,
        BirthDate = new DateTime(1993, 5, 12)
    };

    private List<string> actionLog = new();
    [Inject] private InnovativeDialogService DialogService { get; set; }

    protected override void OnInitialized()
    {
        _person.UpdatePasswordAction = id => 
        {
            var logEntry = $"Password updated with ID: {id} at {DateTime.Now:HH:mm:ss}";
            actionLog.Add(logEntry);
            Console.WriteLine(logEntry);
            StateHasChanged();
        };
        
        // _person.ControlePasswordAction = result => 
        // {
        //     var logEntry = $"Password control result: {result} at {DateTime.Now:HH:mm:ss}";
        //     actionLog.Add(logEntry);
        //     Console.WriteLine(logEntry);
        //     StateHasChanged();
        // };

        base.OnInitialized();
    }

    private async Task OpenPersonDialog()
    {
        var result = await DialogService.OpenDynamicFormDialog(
            _person);

        if (result != null)
        {
            _person = result;
            
            StateHasChanged();
        }
    }
}

[UIFormClass( title: nameof(Examples.DialogService_Person), ResourceType = typeof(Examples))]
public class PersonModel
{
    [UIFormField(Name = "First Name")]
    public string FirstName { get; set; }

    [UIFormField(Name = "Last Name")]
    public string LastName { get; set; }

    [UIFormField(Name = "Age")] public int? Age => DateTime.Now.Year - BirthDate.Value.Year;

    // [UIFormField(Name = "Is Active", ViewComponent = typeof(CustomBoolStyle))]
    public bool IsActive { get; set; }

    [UIFormField(Name = "Birth Date")]
    public DateTime? BirthDate { get; set; }

    [UIFormField(Name = "Description", UseWysiwyg = true)]
    public string Description { get; set; }

    [UIFormViewAction(Name = "Update Password", Order = 1)]
    public Action<int> UpdatePasswordAction { get; set; }

    // [UIFormViewAction(Name = "Control Password", Order = 1, CustomComponent = typeof(PasswordUpdateComponent))]
    //  public Action<int> ControlePasswordAction { get; set; }
}