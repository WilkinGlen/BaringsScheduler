namespace BaringsQuartzUI.Components.Controls.Dialogs.CommonDialogs;

using Microsoft.AspNetCore.Components;
using MudBlazor;

public sealed partial class ConfirmDialog
{
    [CascadingParameter] 
    private IMudDialogInstance? MudDialog { get; set; }

    [Parameter]
    public string? ContentText { get; set; }

    private void Submit () => this.MudDialog!.Close(DialogResult.Ok(true));

    private void Cancel() => this.MudDialog!.Cancel();
}
