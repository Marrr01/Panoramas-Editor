using Microsoft.WindowsAPICodePack.Dialogs;

namespace Panoramas_Editor
{
    internal class TableFileDialogService
    {
        public SelectedFile selectedFile { get; set; }

        public bool OpenBrowsingDialog()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Title = "Выбор таблицы";
                dialog.Multiselect = false;
                dialog.Filters.Add(new CommonFileDialogFilter("Exel Таблица", "xlsx")) ;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    selectedFile = new SelectedFile(dialog.FileName);
                    return true;
                }
            }
            return false;
        }
    }
}