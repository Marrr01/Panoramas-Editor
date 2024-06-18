using System.Collections.Generic;

namespace Panoramas_Editor
{
    internal interface IFilesSelectionDialog
    {
        public bool OpenBrowsingDialog();  // открытие диалогового окна
        public List<SelectedFile> SelectedFiles { get; set; }
    }
}
