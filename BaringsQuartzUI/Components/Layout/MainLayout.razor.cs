namespace BaringsQuartzUI.Components.Layout;

public sealed partial class MainLayout
{
    private bool _drawerOpen = true;

    private void DrawerToggle() => this._drawerOpen = !this._drawerOpen;
}
