namespace BaringsQuartzUI.Components.Layout;

using Microsoft.AspNetCore.Components;

public sealed partial class NavMenu
{
    [Inject]
    private NavigationManager? NavigationManager { get; set; }

    [Parameter]
    public EventCallback<string> OnMenuItemSelected { get; set; }

    private void MenuItemClickedHandler(string itemName, string href)
    {
        _ = this.OnMenuItemSelected.InvokeAsync(itemName);
        this.NavigationManager!.NavigateTo(href);
    }
}
