using Microsoft.WindowsAPICodePack.Dialogs;

namespace Panoramas_Editor;

public class SaveTableDialogService
{
    public string SelectedFilePath { get; private set; }

    public bool OpenBrowsingDialog()
    {
        using (var dialog = new CommonSaveFileDialog())
        {
            dialog.Title = "Сохранение в таблицу Excel";
            dialog.DefaultExtension = "xlsx";
            dialog.AlwaysAppendDefaultExtension = true;
            dialog.EnsureValidNames = true;
            
            var excelFilter = new CommonFileDialogFilter("Excel", ".xlsx");
            dialog.Filters.Add(excelFilter);

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SelectedFilePath = dialog.FileName;
                return true;
            }
        }
        return false;
    }
}