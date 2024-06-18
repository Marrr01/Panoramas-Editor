namespace Panoramas_Editor
{
    internal interface IDirectorySelectionDialog
    {
        public bool OpenBrowsingDialog();  // открытие диалогового окна
        public SelectedDirectory SelectedDirectory { get; set; }
    }
}
