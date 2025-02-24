namespace BaringsBlazor.Components.Layout;

using MudBlazor;

public sealed partial class MainLayout
{
    bool _drawerOpen = false;

    void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private readonly MudTheme theme = new()
    {
        PaletteLight = new()
        {
            Primary = Colors.Blue.Darken3,
            Secondary = Colors.Red.Darken2,
            AppbarBackground = Colors.Blue.Darken1
        },
        ZIndex = new() { Popover = new ZIndex().Dialog + 1 }
    };
}
