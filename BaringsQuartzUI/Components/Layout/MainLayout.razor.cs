namespace BaringsQuartzUI.Components.Layout;

public sealed partial class MainLayout
{
    private bool _drawerOpen;

    private void DrawerToggle() => this._drawerOpen = !this._drawerOpen;
}
