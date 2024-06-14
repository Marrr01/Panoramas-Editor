using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal class EditorVM : ObservableObject
    {
        private ExecutionSetupVM _executionSetupVM;

        public ImageSettings ImageSettings
        {
            get => _executionSetupVM.SelectedSettings;
        }
        public BitmapImage? Bitmap
        {
            get => ImageSettings.CompressedBitmapImage;
        }
        public EditorVM(ExecutionSetupVM executionSetupVM)
        {
            _executionSetupVM = executionSetupVM;
            ImageSettings.CompressedBitmapImageChanged += (s, e) => OnPropertyChanged(nameof(Bitmap));

            ImageCenterSelector = new ImageCenterSelector();
            HandleTabChangedEventCommand = new RelayCommand<TabControl>((tc) => HandleTabChangedEvent(tc));
        }

        public IRelayCommand<TabControl> HandleTabChangedEventCommand { get; }
        public void HandleTabChangedEvent(TabControl tabControl)
        {
            if (tabControl.SelectedIndex == 0)
            {
                ImageCenterSelector = new ImageCenterSelector();
                OnPropertyChanged(nameof(ImageCenterSelector));
            }
            if (tabControl.SelectedIndex == 1)
            {
                Preview = new Preview();
                OnPropertyChanged(nameof(Preview));
            }
        }

        public UserControl ImageCenterSelector { get; set; }
        public UserControl Preview { get; set; }

        //private UserControl _imageCenterSelector;
        //public UserControl ImageCenterSelector
        //{
        //    get => _imageCenterSelector;
        //    set
        //    {
        //        _imageCenterSelector = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private UserControl _preview;
        //public UserControl Preview
        //{
        //    get => _preview;
        //    set
        //    {
        //        _preview = value;
        //        OnPropertyChanged();
        //    }
        //}
    }
}
