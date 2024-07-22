using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Panoramas_Editor
{
    internal class PreviewVM : ObservableObject
    {
        private Logger _logger => App.Current.Logger;
        private ExecutionSetupVM _executionSetupVM;
        private IImageEditor _imageEditor;
        public ImageSettings ImageSettings { get; }

        private BitmapSource _preview;
        public BitmapSource Preview
        {
            get => _preview;
            set
            {
                _preview = value;
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

        public double ActualVerticalCenter => Math.Round(ActualHeight_ / 2, 0);

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

        public double ActualHorizontalCenter => Math.Round(ActualWidth_ / 2, 0);
        #endregion

        private Task _previewLoading;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        public PreviewVM(ExecutionSetupVM executionSetupVM,
                         IImageEditor imageEditor)
        {
            _executionSetupVM = executionSetupVM;
            ImageSettings = _executionSetupVM.SelectedSettings;
            _imageEditor = imageEditor;
            IsCenterShown = true;

            UpdatePreview();

            #region события
            SettingsChangedHandler = delegate (object? s, EventArgs e)
            {
                Preview = null;

                // Предполагаем, что последняя загрузка превью не завершилась
                try
                {
                    _cancellationTokenSource.Cancel();
                    Task.Run(() =>
                    {
                        _previewLoading.Wait();
                        Preview = null;
                        UpdatePreview();
                    });
                }
                catch (ObjectDisposedException)
                {
                    UpdatePreview();
                }
            };
            ImageSettings.HorizontalOffsetChanged += SettingsChangedHandler;
            ImageSettings.VerticalOffsetChanged += SettingsChangedHandler;

            // Отмена загрузки предпросмотра срабатывает, если таб айтем в Editor.xaml сменяется
            HandleUnloadedEventCommand = new RelayCommand(HandleUnloadedEvent);

            // Отмена загрузки предпросмотра срабатывает, если главное окно закрывается
            ShutdownStartedHandler = delegate (object? s, EventArgs e)
            {
                HandleUnloadedEvent();
            };
            App.Current.Dispatcher.ShutdownStarted += ShutdownStartedHandler;
            #endregion
        }

        EventHandler SettingsChangedHandler;
        private void UpdatePreview()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _previewLoading = Task.Run(() =>
            {
                try
                { 
                    Preview = _imageEditor.GetPreview(ImageSettings, _cancellationToken);
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                { 
                    _logger.Error($"Не удалось загрузить предпросмотр: {ex.Message}");
                }
                finally
                { 
                    _cancellationTokenSource.Dispose();
                }
            });
        }

        private EventHandler ShutdownStartedHandler;
        public IRelayCommand HandleUnloadedEventCommand { get; }

        public void HandleUnloadedEvent()
        {
            if (_previewLoading != null && !_previewLoading.IsCompleted && _cancellationToken.CanBeCanceled)
            {
                _cancellationTokenSource.Cancel();
            }
            if (App.Current != null)
            {
                App.Current.Dispatcher.ShutdownStarted -= ShutdownStartedHandler;
            }
            ImageSettings.HorizontalOffsetChanged -= SettingsChangedHandler;
            ImageSettings.VerticalOffsetChanged -= SettingsChangedHandler;
        }
    }
}
