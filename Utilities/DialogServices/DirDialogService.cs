using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;

namespace Panoramas_Editor
{
    internal class DirDialogService : IDirectorySelectionDialog
    {
        public SelectedDirectory SelectedDirectory { get; set; }

        public bool OpenBrowsingDialog()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Title = "Выбор папки";
                dialog.IsFolderPicker = true;
                dialog.Multiselect = false;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    SelectedDirectory = new SelectedDirectory(dialog.FileName);
                    return true;
                }
            }
            return false;
        }
    }
}
