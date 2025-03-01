namespace BaringsQuartzUI.Components.Layout;

public sealed partial class MainLayout
{
    private bool drawerOpen;
    private string? pageTitle = " - Home";

    private void DrawerToggle(string? pageTitle)
    {
        if ((!string.IsNullOrWhiteSpace(pageTitle)))
        {
            this.pageTitle = $" - {pageTitle}";
        }

        this.drawerOpen = !this.drawerOpen;
    }
}
