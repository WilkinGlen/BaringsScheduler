namespace BaringsQuartzUI.Components.Controls.Shared;

using Microsoft.AspNetCore.Components;
using MudBlazor;

public sealed partial class Spinner
{
    [Parameter]
    public Size Size { get; set; } = Size.Large;

    [Parameter]
    public Color Color { get; set; } = Color.Primary;

    [Parameter]
    public string Message { get; set; } = "Loading, please wait...";
}
