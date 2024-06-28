using Microsoft.WindowsAPICodePack.Dialogs;

namespace Panoramas_Editor;

public class DirTableFileDialogService
{
    public string SelectedFilePath { get; set; }
    private bool result;

    public DirTableFileDialogService()
    {
        result = false;
    }

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
                result = true;
            }
        }
        return result;
    }
}