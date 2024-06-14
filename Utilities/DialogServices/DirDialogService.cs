using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;

namespace Panoramas_Editor
{
    internal class DirDialogService : IDialogService
    {
        public List<SelectedFile> SelectedFiles { get; set; }

        public DirDialogService()
        {
            SelectedFiles = new List<SelectedFile>(1);
        }

        public bool OpenBrowsingDialog()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Title = "Выбор папки";
                dialog.IsFolderPicker = true;
                dialog.Multiselect = false;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    SelectedFiles.Clear();
                    SelectedFiles.Add(new SelectedFile(dialog.FileName));
                    return true;
                }
            }
            return false;
        }
    }
}
