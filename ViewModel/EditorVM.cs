using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        public SelectedImage? Compressed
        {
            get => ImageSettings.Compressed;
        }
        public UserControl ImageCenterSelector { get; set; }
        public UserControl Preview { get; set; }
        public IRelayCommand<TabControl> HandleTabChangedEventCommand { get; }

        public EditorVM(ExecutionSetupVM executionSetupVM)
        {
            _executionSetupVM = executionSetupVM;
            ImageSettings.CompressedChanged += (s, e) =>
            {
                // null если изображение было выбрано, но его удалили из списка до окончания процесса сжатия
                if (ImageSettings != null)
                {
                    OnPropertyChanged(nameof(Compressed));
                }
            };

            ImageCenterSelector = new ImageCenterSelector();
            HandleTabChangedEventCommand = new RelayCommand<TabControl>((tc) => HandleTabChangedEvent(tc));
        }

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
    }
}
