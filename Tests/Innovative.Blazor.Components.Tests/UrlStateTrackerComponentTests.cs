using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Innovative.Blazor.Components.Components.TrackInUrl;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.WebUtilities;

namespace Innovative.Blazor.Components.Tests;

  public class UrlStateTrackerComponentTests : TestContext
    {
        [Fact]
        public void Component_Initializes_With_Default_Values_When_No_URL_Parameters()
        {
            var navigationManager = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
            navigationManager?.NavigateTo("https://example.com/");
            
            var cut = RenderComponent<TestUrlStateTrackerComponent>();
            
            cut.MarkupMatches("<div>TestProperty: InitialValue, TestIntProperty: 42</div>");
        }
        
        [Fact]
        public void Component_Reads_Values_From_URL_On_Initialization()
        {
            var state = new Dictionary<string, object>
            {
                ["TestProperty"] = "FromUrl",
                ["TestIntProperty"] = 99
            };
            
            var json = JsonSerializer.Serialize(state);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            
            var navigationManager = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
            navigationManager?.NavigateTo($"https://example.com/?dataset={base64}");
            
            var cut = RenderComponent<TestUrlStateTrackerComponent>();
            
            cut.MarkupMatches("<div>TestProperty: FromUrl, TestIntProperty: 99</div>");
        }
        
        [Fact]
public void UpdateQueryStringParameter_PreservesExistingParameters()
{
    // Arrange
    var navigationManager = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
    navigationManager?.NavigateTo("https://example.com/?existing=value&other=123");
    
    var cut = RenderComponent<TestUrlStateTrackerComponent>();
    
    // Act
    cut.Instance.UpdateTrackedProperty("NewValue");
    
    // Assert
    var uri = new Uri(navigationManager?.Uri);
    var query = QueryHelpers.ParseQuery(uri.Query);
    
    // Verify dataset parameter was added
    Assert.True(query.TryGetValue("dataset", out var base64Value));
    
    // Verify existing parameters were preserved
    Assert.True(query.TryGetValue("existing", out var existingValue));
    Assert.Equal("value", existingValue);
    
    Assert.True(query.TryGetValue("other", out var otherValue));
    Assert.Equal("123", otherValue);
    
    // Verify the tracked data was correctly encoded
    var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64Value));
    var state = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
    Assert.Equal("NewValue", state["TestProperty"].GetString());
}

[Fact]
public void UpdateQueryStringParameter_UpdatesExistingDatasetParameter()
{
    // Arrange
    var initialState = new Dictionary<string, object>
    {
        ["TestProperty"] = "InitialValue",
        ["TestIntProperty"] = 42
    };
    
    var json = JsonSerializer.Serialize(initialState);
    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    
    var navigationManager = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
    navigationManager?.NavigateTo($"https://example.com/?dataset={base64}&other=param");
    
    var cut = RenderComponent<TestUrlStateTrackerComponent>();
    
    // Act
    cut.Instance.UpdateIntProperty(100);
    
    // Assert
    var uri = new Uri(navigationManager?.Uri);
    var query = QueryHelpers.ParseQuery(uri.Query);
    
    // Verify dataset parameter was updated
    Assert.True(query.TryGetValue("dataset", out var updatedBase64));
    Assert.NotEqual(base64, updatedBase64.ToString());
    
    // Verify the other parameter was preserved
    Assert.True(query.TryGetValue("other", out var otherValue));
    Assert.Equal("param", otherValue);
    
    // Verify the dataset was correctly updated
    var updatedJson = Encoding.UTF8.GetString(Convert.FromBase64String(updatedBase64));
    var updatedState = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(updatedJson);
    
    Assert.Equal("InitialValue", updatedState["TestProperty"].GetString());
    Assert.Equal(100, updatedState["TestIntProperty"].GetInt32());
}
        
        [Fact]
        public void Property_Change_Updates_URL()
        {
            var navigationManager = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
            navigationManager?.NavigateTo("https://example.com/");
            
            var cut = RenderComponent<TestUrlStateTrackerComponent>();
            
            cut.Instance.UpdateTrackedProperty("UpdatedValue");
            
            var uri = new Uri(navigationManager?.Uri);
            var query = QueryHelpers.ParseQuery(uri.Query);
            
            Assert.True(query.TryGetValue("dataset", out var base64Value));
            
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64Value));
            var state = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            
            Assert.Equal("UpdatedValue", state["TestProperty"].GetString());
            Assert.Equal(42, state["TestIntProperty"].GetInt32());
        }
        
        [Fact]
        public void URL_Change_Updates_Properties()
        {
            var navigationManager = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;
            navigationManager?.NavigateTo("https://example.com/");
            
            var cut = RenderComponent<TestUrlStateTrackerComponent>();
            
            cut.MarkupMatches("<div>TestProperty: InitialValue, TestIntProperty: 42</div>");
            
            var state = new Dictionary<string, object>
            {
                ["TestProperty"] = "ChangedFromUrl",
                ["TestIntProperty"] = 123
            };
            
            var json = JsonSerializer.Serialize(state);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            
            navigationManager?.NavigateTo($"https://example.com/?dataset={base64}");
            
            cut.MarkupMatches("<div>TestProperty: ChangedFromUrl, TestIntProperty: 123</div>");
        }
    }
    
  public class TestUrlStateTrackerComponent : UrlStateTrackerComponent
  {
      [Parameter]
      [TrackInUrl]
      public string TestProperty { get; set; } = "InitialValue";
    
      [Parameter]
      [TrackInUrl]
      public int TestIntProperty { get; set; } = 42;
    
      [Parameter]
      public string UntrackedProperty { get; set; } = "Untracked";
    
      public void UpdateTrackedProperty(string value)
      {
          TestProperty = value;
          OnPropertyPossiblyChanged();
      }
    
      public void UpdateIntProperty(int value)
      {
          TestIntProperty = value;
          OnPropertyPossiblyChanged();
      }
    
      protected override void BuildRenderTree(RenderTreeBuilder builder)
      {
          builder.OpenElement(0, "div");
          builder.AddContent(1, $"TestProperty: {TestProperty}, TestIntProperty: {TestIntProperty}");
          builder.CloseElement();
      }
  }