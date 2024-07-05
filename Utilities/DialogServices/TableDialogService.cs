using Microsoft.WindowsAPICodePack.Dialogs;

namespace Panoramas_Editor
{
    internal class TableDialogService
    {
        public SelectedFile SelectedFile { get; private set; }

        public bool OpenBrowsingDialog()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Title = "Выбор таблицы Excel";
                dialog.Multiselect = false;
                dialog.Filters.Add(new CommonFileDialogFilter("Excel", "xlsx")) ;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    SelectedFile = new SelectedFile(dialog.FileName);
                    return true;
                }
            }
            return false;
        }
    }
}