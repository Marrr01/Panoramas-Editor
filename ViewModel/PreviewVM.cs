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

        public ImageSettings ImageSettings
        {
            get => _executionSetupVM.SelectedSettings;
        }

        public BitmapImage ResultPreview
        {
            get => ImageSettings.ResultPreview;
            set
            {
                ImageSettings.ResultPreview = value;
                OnPropertyChanged();
            }
        }

        private Task _imageEditing;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        public PreviewVM(ExecutionSetupVM executionSetupVM, IImageEditor imageEditor)
        {
            _executionSetupVM = executionSetupVM;
            //ImageSettings.ResultPreviewChanged += (s, e) => OnPropertyChanged(nameof(ResultPreview));
            _imageEditor = imageEditor;

            if (ResultPreview == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;

                _imageEditing = Task.Run(() =>
                {
                    try
                    {
                        // В некоторых случаях ImageSettings может измениться ПОСЛЕ вызова
                        // конструктора этого класса, поэтому здесь костыль в виде микрозадержки
                        //Thread.Sleep(50);


                        ResultPreview = _imageEditor.EditCompressedBitmapImage(ImageSettings, _cancellationToken);

                    }
                    catch (OperationCanceledException)
                    {
                        CustomMessageBox.ShowMessage("Загрузка предпросмотра отменена");
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
