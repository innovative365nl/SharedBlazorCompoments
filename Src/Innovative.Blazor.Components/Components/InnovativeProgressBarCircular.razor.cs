using Microsoft.AspNetCore.Components;
            using System;
            using System.Collections.Generic;
            
            namespace Innovative.Blazor.Components.Components;
            
            /// <summary>
            /// A circular progress indicator component that supports determinate and indeterminate states.
            /// Use this component to display progress visually with customizable styling options.
            /// </summary>
            public partial class InnovativeProgressBarCircular : ComponentBase
            {
                /// <summary>
                /// Gets or sets the ElementReference for direct DOM manipulation.
                /// This is used internally for accessing the rendered DOM element.
                /// </summary>
                protected ElementReference Element { get; set; }
            
                /// <summary>
                /// Gets or sets the minimal value for the progress range.
                /// This defines the lower bound of the progress scale.
                /// Default value is 0.
                /// </summary>
                [Parameter]
                public double Min { get; set; } = 0;
            
                /// <summary>
                /// Gets or sets the maximum value for the progress range.
                /// This defines the upper bound of the progress scale.
                /// Default value is 100.
                /// </summary>
                [Parameter]
                public double Max { get; set; } = 100;
            
                /// <summary>
                /// Gets or sets the current progress value.
                /// Must be between Min and Max values for proper display.
                /// Default value is 0.
                /// </summary>
                [Parameter]
                public double Value { get; set; } = 0;
            
                /// <summary>
                /// Gets or sets the event callback triggered when the value changes.
                /// Used for two-way binding with @bind-Value.
                /// </summary>
                [Parameter]
                public EventCallback<double> ValueChanged { get; set; }
            
                /// <summary>
                /// Gets or sets the unit displayed after the value.
                /// For example, "%" for percentage or "MB" for megabytes.
                /// Default is "%".
                /// </summary>
                [Parameter]
                public string Unit { get; set; } = "%";
            
                /// <summary>
                /// Gets or sets whether the component is visible.
                /// When false, the component will not render.
                /// Default is true.
                /// </summary>
                [Parameter]
                public bool Visible { get; set; } = true;
            
                /// <summary>
                /// Gets or sets whether to show the value inside the circular progress.
                /// When true, displays the current value and unit in the center.
                /// Default is true.
                /// </summary>
                [Parameter]
                public bool ShowValue { get; set; } = true;
            
                /// <summary>
                /// Gets or sets the template for custom content to display in the center.
                /// When provided, overrides the default value display.
                /// </summary>
                [Parameter]
                public RenderFragment? Template { get; set; }
            
                /// <summary>
                /// Gets or sets the inline CSS style string for the component.
                /// Allows for additional styling beyond the built-in classes.
                /// </summary>
                [Parameter]
                public string? Style { get; set; }
            
                /// <summary>
                /// Gets or sets additional HTML attributes for the component.
                /// Captures any unmatched attributes and passes them to the root element.
                /// </summary>
                [Parameter(CaptureUnmatchedValues = true)]
                public IDictionary<string, object>? Attributes { get; init; }
            
                /// <summary>
                /// Gets or sets the mode of the progress bar (Determinate or Indeterminate).
                /// Determinate shows precise progress while Indeterminate shows an animation.
                /// Default is Determinate.
                /// </summary>
                [Parameter]
                public ProgressBarMode Mode { get; set; } = ProgressBarMode.Determinate;
            
                /// <summary>
                /// Gets or sets the style (color theme) of the progress bar.
                /// Follows standard Bootstrap color conventions.
                /// Default is Primary.
                /// </summary>
                [Parameter]
                public ProgressBarStyle ProgressBarStyle { get; set; } = ProgressBarStyle.Primary;
            
                /// <summary>
                /// Gets or sets the size of the circular progress bar.
                /// Controls the diameter of the component.
                /// Default is Medium.
                /// </summary>
                [Parameter]
                public ProgressBarCircularSize Size { get; set; } = ProgressBarCircularSize.Medium;
            
                /// <summary>
                /// Gets the normalized value between 0 and 1 based on Min and Max.
                /// Used for calculating the circular progress arc length.
                /// </summary>
                protected double NormalizedValue => Math.Min(Math.Max((Value - Min) / (Max - Min), 0), 1);
            
                /// <summary>
                /// Generates the CSS class string for the component based on current parameters.
                /// Combines base classes with mode, style, and size-specific classes.
                /// </summary>
                /// <returns>A space-separated string of CSS classes</returns>
                protected string GetCssClass()
                {
                    var classList = new List<string>
                    {
                        "innovative-progressbar-circular"
                    };
            
                    switch (Mode)
                    {
                        case ProgressBarMode.Determinate:
                            classList.Add("innovative-progressbar-determinate");
                            break;
                        case ProgressBarMode.Indeterminate:
                            classList.Add("innovative-progressbar-indeterminate");
                            break;
                    }
            
                    classList.Add($"innovative-progressbar-{ProgressBarStyle.ToString().ToUpperInvariant()}");
                    classList.Add($"innovative-progressbar-circular-{CircleSize}");
            
                    return string.Join(" ", classList);
                }
            
                /// <summary>
                /// Gets the circle size CSS class suffix based on the Size parameter.
                /// Maps enum values to CSS class suffixes.
                /// </summary>
                protected string CircleSize
                {
                    get
                    {
                        switch (Size)
                        {
                            case ProgressBarCircularSize.Medium:
                                return "md";
                            case ProgressBarCircularSize.Large:
                                return "lg";
                            case ProgressBarCircularSize.Small:
                                return "sm";
                            case ProgressBarCircularSize.ExtraSmall:
                                return "xs";
                            default:
                                return string.Empty;
                        }
                    }
                }
            
                /// <summary>
                /// Generates a unique identifier for the component.
                /// Used to ensure each progress bar instance has a unique ID.
                /// </summary>
                /// <returns>A unique string ID</returns>
                protected static string GetId()
                {
                    return $"innovative-progressbar-{Guid.NewGuid().ToString().Replace("-", "", StringComparison.Ordinal)}";
                }
            }
            
            // Helper extension method for invariant string conversion
            public static class DoubleExtensions
            {
                /// <summary>
                /// Converts a double value to a string using invariant culture formatting.
                /// Ensures consistent number formatting regardless of user's locale settings.
                /// </summary>
                /// <param name="value">The double value to convert</param>
                /// <returns>String representation of the double using invariant culture</returns>
                public static string ToInvariantString(this double value)
                {
                    return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            
            /// <summary>
            /// Mode options for the progress bar.
            /// Determinate shows precise progress, while Indeterminate shows an animation.
            /// </summary>
            public enum ProgressBarMode
            {
                /// <summary>
                /// Shows precise progress with a specific value
                /// </summary>
                Determinate,
                
                /// <summary>
                /// Shows an animation indicating activity without a specific value
                /// </summary>
                Indeterminate
            }
            
            /// <summary>
            /// Style options for the progress bar.
            /// Follows standard Bootstrap color conventions.
            /// </summary>
            public enum ProgressBarStyle
            {
                /// <summary>Main brand color (typically blue)</summary>
                Primary,
                /// <summary>Secondary brand color (typically gray)</summary>
                Secondary,
                /// <summary>Success indication (typically green)</summary>
                Success,
                /// <summary>Informational indication (typically cyan)</summary>
                Info,
                /// <summary>Warning indication (typically yellow)</summary>
                Warning,
                /// <summary>Danger/error indication (typically red)</summary>
                Danger,
                /// <summary>Light color (typically white or off-white)</summary>
                Light,
                /// <summary>Dark color (typically dark gray or black)</summary>
                Dark
            }
            
            /// <summary>
            /// Size options for the circular progress bar.
            /// Controls the diameter of the component.
            /// </summary>
            public enum ProgressBarCircularSize
            {
                /// <summary>Very small size (2rem diameter)</summary>
                ExtraSmall,
                /// <summary>Small size (3rem diameter)</summary>
                Small,
                /// <summary>Medium size (4rem diameter)</summary>
                Medium,
                /// <summary>Large size (6rem diameter)</summary>
                Large
            }