namespace BaringsQuartzUI.Components.Layout;

public sealed partial class MainLayout
{
    bool _drawerOpen = true;

    void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }
}
