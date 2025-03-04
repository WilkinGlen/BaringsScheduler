namespace BaringsQuartzUI.Components.Controls.Dialogs.CommonDialogs;

using MudBlazor;

public interface ICommonDialogsService
{
    Task<bool> GetConfirmationAsync(string titleText, string contentText, DialogOptions? options =null);
}

public sealed class CommonDialogsService(IDialogService dialogService) : ICommonDialogsService
{
    public async Task<bool> GetConfirmationAsync(string titleText, string contentText, DialogOptions? options = null)
    {
        var parameters = new DialogParameters<ConfirmDialog>
        {
            { x => x.ContentText, contentText }
        };
        var dialog = await dialogService.ShowAsync<ConfirmDialog>(titleText, parameters, options);
        var dialogResult = await dialog.Result;
        return dialogResult?.Canceled == false;
    }
}
