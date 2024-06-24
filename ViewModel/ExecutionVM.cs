using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using NLog.Fluent;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Panoramas_Editor
{
    internal class ExecutionVM : ObservableObject, ILoggerVM
    {
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                IsRunningChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }
        public event EventHandler? IsRunningChanged;

        private ExecutionSetupVM _executionSetupVM;
        private IImageEditor _imageEditor;
        private IContext _context;
        public Task Execution { get; set; }
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        public ExecutionVM(ExecutionSetupVM executionSetupVM, IImageEditor imageEditor, IContext context)
        {
            _executionSetupVM = executionSetupVM;
            _imageEditor = imageEditor;
            _context = context;

            IsRunning = false;

            RunCommand = new RelayCommand(Run);
            StopCommand = new RelayCommand(Stop);
            HandleSVLoadedEventCommand = new RelayCommand<ScrollViewer>((sv) => _logViewScrollViewer = sv);
            HandleRTBLoadedEventCommand = new RelayCommand<RichTextBox>((rtb) =>
            {
                _logView = rtb;
                _logView.Document.LineHeight = 1;
                _logView.Document.Blocks.Clear();
            });
        }

        #region logger
        private Logger _logger => App.Current.Logger;

        private ScrollViewer _logViewScrollViewer;
        private RichTextBox _logView;

        public void Add(string message, LogLevel logLevel)
        {
            switch (logLevel.Name)
            {     
                case "Warn":
                    WriteColoredMessage(message, Brushes.DarkOrange);
                    break;

                case "Error":
                    WriteColoredMessage(message, Brushes.Red);
                    break;

                case "Fatal":
                    WriteColoredMessage(message, Brushes.Red);
                    break;

                default:
                    WriteColoredMessage(message, Brushes.White);
                    break;
            }
            _context.Invoke(() => _logViewScrollViewer.ScrollToEnd());
        }

        private void WriteColoredMessage(string message, Brush brush)
        {
            _context.Invoke(() =>
            {
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(message) { Foreground = brush });
                _logView.Document.Blocks.Add(paragraph);
            });
        }
        #endregion

        #region commands
        public IRelayCommand RunCommand { get; }
        public IRelayCommand StopCommand { get; }
        public IRelayCommand<RichTextBox> HandleRTBLoadedEventCommand { get; }
        public IRelayCommand<ScrollViewer> HandleSVLoadedEventCommand { get; }

        public void Run()
        {
            try
            {
                if (_executionSetupVM.ImagesSettings.Count == 0)
                {
                    throw new ArgumentException("Перед запуском необходимо выбрать изображения");
                }
                if (_executionSetupVM.NewImagesDirectory == null)
                {
                    throw new ArgumentException("Перед запуском необходимо выбрать папку для сохранения новых изображений");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowWarning(ex.Message, "Недостаточно данных");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            Execution = Task.Run(async () =>
            {
                IsRunning = true;

                await Task.Run(() =>
                {
                    try
                    {
                        var imagesEditing = Parallel.ForEach(_executionSetupVM.ImagesSettings, (settings) =>
                        {
                            try
                            {
                                SelectedImage result;
                                if (_executionSetupVM.NewImagesExtension == ExecutionSetupVM.KEEP_OLD_EXTENSION)
                                {
                                    result = _imageEditor.EditOriginalImage(_executionSetupVM.NewImagesDirectory, settings, _cancellationToken, settings.Extension);
                                }
                                else
                                {
                                    result = _imageEditor.EditOriginalImage(_executionSetupVM.NewImagesDirectory, settings, _cancellationToken, _executionSetupVM.NewImagesExtension);
                                }
                                _logger.Info($"Изображение {settings.FullPath} сохранено с новыми настройками:\nГоризонтальное смещение: {settings.HorizontalOffset}\nВертикальное смещение: {settings.VerticalOffset}\nРезультат: {result.FullPath}");
                            }
                            catch (OperationCanceledException)
                            {
                                //_logger.Warn("Выполнение отменено пользователем");
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex.Message);
                            }
                        });
                        if (imagesEditing.IsCompleted)
                        {
                            _logger.Info("Выполнение завершено");
                        }
                        //while (true)
                        //{
                        //    if (_cancellationToken.IsCancellationRequested)
                        //    {
                        //        _cancellationToken.ThrowIfCancellationRequested();
                        //    }
                        //    _logger.Info("это запись из логгера");
                        //    Thread.Sleep(1000);
                        //}
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.Warn("Выполнение отменено пользователем");
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

                IsRunning = false;

            }, _cancellationToken);

        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
        #endregion
    }
}
