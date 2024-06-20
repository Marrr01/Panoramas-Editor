using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Controls;

namespace Panoramas_Editor
{
    internal class EditorVM : ObservableObject
    {
        private ExecutionSetupVM _executionSetupVM;
        public ImageSettings ImageSettings { get; }
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
            ImageSettings = _executionSetupVM.SelectedSettings;

            CompressedChangedHandler = delegate (object? s, EventArgs e)
            {
                // null если изображение было выбрано, но его удалили из списка до окончания процесса сжатия
                if (ImageSettings != null)
                {
                    OnPropertyChanged(nameof(Compressed));
                }
            };
            ImageSettings.CompressedChanged += CompressedChangedHandler;

            ImageCenterSelector = new ImageCenterSelector();
            HandleTabChangedEventCommand = new RelayCommand<TabControl>((tc) => HandleTabChangedEvent(tc));
            HandleUnloadedEventCommand = new RelayCommand(HandleUnloadedEvent);
        }

        EventHandler CompressedChangedHandler;
        public IRelayCommand HandleUnloadedEventCommand { get; }

        public void HandleUnloadedEvent()
        {
            ImageSettings.CompressedChanged -= CompressedChangedHandler;
        }

        public void HandleTabChangedEvent(TabControl tabControl)
        {
            if (tabControl.SelectedIndex == 0)
            {
                ImageCenterSelector = new ImageCenterSelector();
                OnPropertyChanged(nameof(ImageCenterSelector));
                GC.Collect();
            }
            if (tabControl.SelectedIndex == 1)
            {
                Preview = new Preview();
                OnPropertyChanged(nameof(Preview));
                GC.Collect();
            }
        }
    }
}
