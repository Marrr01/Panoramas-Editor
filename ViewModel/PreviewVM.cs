using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal class PreviewVM : ObservableObject
    {
        private ExecutionSetupVM _executionSetupVM;

        private IImageEditor _imageEditor;
        private IImageReader _imageReader;
        private SelectedDirectory _tempFilesDirectory { get => new SelectedDirectory(App.Current.Configuration["temp"]); }
        public ImageSettings ImageSettings
        {
            get => _executionSetupVM.SelectedSettings;
        }

        public SelectedImage Preview
        {
            get => ImageSettings.Preview;
            set
            {
                ImageSettings.Preview = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage _previewBitmapImage;
        public BitmapImage PreviewBitmapImage
        {
            get => _previewBitmapImage;
            set
            {
                _previewBitmapImage = value;
                OnPropertyChanged();
            }
        }

        private bool _isCenterShown;
        public bool IsCenterShown
        {
            get => _isCenterShown;
            set
            {
                _isCenterShown = value;
                OnPropertyChanged();
            }
        }

        #region Actual - значения изображения в окне
        private double actualHeight_;
        public double ActualHeight_
        {
            get => actualHeight_;
            set
            {
                actualHeight_ = value;
                OnPropertyChanged(nameof(ActualVerticalCenter));
            }
        }

        public double ActualVerticalCenter
        {
            get => Math.Round(ActualHeight_ / 2, 0);
        }

        private double actualWidth_;
        public double ActualWidth_
        {
            get => actualWidth_;
            set
            {
                actualWidth_ = value;
                OnPropertyChanged(nameof(ActualHorizontalCenter));
            }
        }

        public double ActualHorizontalCenter
        {
            get => Math.Round(ActualWidth_ / 2, 0);
        }
        #endregion

        private Task _imageEditing;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        public PreviewVM(ExecutionSetupVM executionSetupVM, 
                         IImageEditor imageEditor,
                         IImageReader imageReader)
        {
            _executionSetupVM = executionSetupVM;
            _imageEditor = imageEditor;
            _imageReader = imageReader;

            if (Preview == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;

                _imageEditing = Task.Run(() =>
                {
                    try
                    {
                        Preview = _imageEditor.EditCompressedImage(_tempFilesDirectory, ImageSettings, _cancellationToken);
                        PreviewBitmapImage = _imageReader.ReadAsBitmapImage(Preview);
                    }
                    catch (OperationCanceledException)
                    {
                        //CustomMessageBox.ShowMessage("Загрузка предпросмотра отменена"); // удалить потом
                    }
                    catch (Exception)
                    {
                        //CustomMessageBox.ShowError("Ошибка");
                    }
                    finally
                    {
                        _cancellationTokenSource.Dispose();
                    }
                }, _cancellationToken);
            }
            else
            {
                Task.Run(() => PreviewBitmapImage = _imageReader.ReadAsBitmapImage(Preview));
            }

            // Отмена загрузки предпросмотра срабатывает, если таб айтем в Editor.xaml сменяется
            HandleClosedEventCommand = new RelayCommand(HandleClosedEvent);
            // Отмена загрузки предпросмотра срабатывает, если главное окно закрывается
            App.Current.Dispatcher.ShutdownStarted += (s, e) => HandleClosedEvent();
        }

        public IRelayCommand HandleClosedEventCommand { get; }

        public void HandleClosedEvent()
        {
            if (_imageEditing != null && !_imageEditing.IsCompleted && _cancellationToken.CanBeCanceled)
            {
                _cancellationTokenSource.Cancel();
            }
        }
    }
}
