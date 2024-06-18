using System.Collections.Generic;

namespace Panoramas_Editor
{
    internal interface IImagesSelectionDialog
    {
        public bool OpenBrowsingDialog();  // открытие диалогового окна
        public List<SelectedImage> SelectedImages { get; set; }
    }
}
