﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
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
        public ImageSettings ImageSettings { get; }

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

        private Task _previewLoading;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        public PreviewVM(ExecutionSetupVM executionSetupVM,
                         IImageEditor imageEditor,
                         IImageReader imageReader)
        {
            _executionSetupVM = executionSetupVM;
            ImageSettings = _executionSetupVM.SelectedSettings;
            _imageEditor = imageEditor;
            _imageReader = imageReader;

            UpdatePreview();

            #region события
            SettingsChangedHandler = delegate (object? s, EventArgs e)
            {
                PreviewBitmapImage = null;

                // Предполагаем, что последняя загрузка превью не завершилась
                try
                {
                    _cancellationTokenSource.Cancel();
                    Task.Run(() =>
                    {
                        _previewLoading.Wait();
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
                    var preview = GetPreview(ImageSettings, _cancellationToken);
                    ImageSettings.LoadedPreviews.Add(preview);
                    PreviewBitmapImage = _imageReader.ReadAsBitmapImage(preview);
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
            });
        }

        private LoadedPreview GetPreview(ImageSettings imageSettings, CancellationToken ct)
        {
            // Превью с нужными настройками уже загружено
            var samePreview = imageSettings.LoadedPreviews.FirstOrDefault((lp) =>
                lp.HorizontalOffset == imageSettings.HorizontalOffset && 
                lp.VerticalOffset == imageSettings.VerticalOffset, null);

            if (samePreview != null)
            {
                return samePreview;
            }

            // Настройки соответствуют начальному изображению
            if (imageSettings.HorizontalOffset == 0 &&
                imageSettings.VerticalOffset == 0)
            {
                return new LoadedPreview(imageSettings.Compressed.FullPath, 0, 0);
            }

            // Превью нужно создать
            return _imageEditor.EditCompressedImage(_tempFilesDirectory, imageSettings, ct);
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
