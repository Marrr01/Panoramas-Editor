using System.Collections.Generic;

namespace Panoramas_Editor
{
    internal interface IDialogService
    {
        public bool OpenBrowsingDialog();  // открытие диалогового окна

        public List<SelectedFile> SelectedFiles { get; set; }
    }
}
