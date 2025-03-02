namespace BaringsQuartzUI.Components.Layout;

using MudBlazor;
using System.Threading.Tasks;

public sealed partial class MainLayout
{
    private MudThemeProvider? themeProvider;

    private bool drawerOpen;
    private string? pageTitle = " - Home";
    private bool isDarkMode = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this.isDarkMode = await this.themeProvider!.GetSystemPreference();
            this.StateHasChanged();
        }
    }

    private void DrawerToggle(string? pageTitle)
    {
        if (!string.IsNullOrWhiteSpace(pageTitle))
        {
            this.pageTitle = $" - {pageTitle}";
        }

        this.drawerOpen = !this.drawerOpen;
    }

    private void DarlModeToggle() => this.isDarkMode = !this.isDarkMode;
}
