using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;

namespace Panoramas_Editor
{
    internal class ImageDialogService : IImagesSelectionDialog
    {
        public List<SelectedImage> SelectedImages { get; set; }

        public ImageDialogService()
        {
            SelectedImages = new List<SelectedImage>();
        }

        public bool OpenBrowsingDialog()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Title = "Выбор изображений";
                dialog.Multiselect = true;
                dialog.Filters.Add(new CommonFileDialogFilter("Изображения", SelectedImage.ValidExtensionsAsString())) ;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    SelectedImages.Clear();
                    foreach (var file in dialog.FileNames)
                    {
                        SelectedImages.Add(new SelectedImage(file));
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
