using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;

namespace Panoramas_Editor
{
    internal class ImageDialogService : IDialogService
    {
        public List<SelectedFile> SelectedFiles { get; set; }

        public ImageDialogService()
        {
            SelectedFiles = new List<SelectedFile>(50);
        }

        public bool OpenBrowsingDialog()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Title = "Выбор изображений";
                dialog.Multiselect = true;
                dialog.Filters.Add(new CommonFileDialogFilter("Изображения", ImageSettings.ValidExtensionsAsString())) ;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    SelectedFiles.Clear();
                    foreach (var file in dialog.FileNames)
                    {
                        SelectedFiles.Add(new SelectedFile(file));
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
