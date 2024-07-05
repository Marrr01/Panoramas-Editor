using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
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
                OnPropertyChanged(nameof(IsStopButtonEnabled));
            }
        }
        public event EventHandler? IsRunningChanged;

        private bool _isCancellationInProgress;
        public bool IsCancellationInProgress
        {
            get => _isCancellationInProgress;
            set
            {
                _isCancellationInProgress = value;
                OnPropertyChanged(nameof(IsStopButtonEnabled));
            }
        }

        public bool IsStopButtonEnabled
        {
            get => !IsCancellationInProgress && IsRunning;
        }

        private ExecutionSetupVM _executionSetupVM;
        private IImageEditor _imageEditor;
        private WpfDispatcherContext _context;
        private ProgressBarController _pbController;
        public double PBPercents { get => _pbController.Percents; }
        public Task Execution { get; set; }
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        public ExecutionVM(ExecutionSetupVM executionSetupVM,
                           IImageEditor imageEditor,
                           WpfDispatcherContext context,
                           ProgressBarController pbController)
        {
            _executionSetupVM = executionSetupVM;
            _imageEditor = imageEditor;
            _context = context;
            _pbController = pbController;

            IsRunning = false;
            IsCancellationInProgress = false;

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

            _pbController.Initialize(_executionSetupVM.ShareData ? _executionSetupVM.ImagesSettings.Count + 1 : _executionSetupVM.ImagesSettings.Count);
            OnPropertyChanged(nameof(PBPercents));

            Execution = Task.Run(() =>
            {
                IsRunning = true;

                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;

                var parallelOptions = new ParallelOptions();
                parallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount;

                Parallel.Invoke(parallelOptions,
                    () =>
                    {
                        if (!_executionSetupVM.ShareData) { return; }
                        try
                        {
                            // логику отправки данных в БД писать сюда

                            _cancellationToken.ThrowIfCancellationRequested();
                            Thread.Sleep(2000);

                            _cancellationToken.ThrowIfCancellationRequested();
                            Thread.Sleep(2000);

                            _cancellationToken.ThrowIfCancellationRequested();
                            Thread.Sleep(2000);

                            _logger.Info("Данные отправлены в БД");
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.Warn("Отправка данных в БД отменена");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Не удалось отправить данные в БД: {ex.Message}");
                        }
                        finally
                        {
                            _pbController.Tick();
                            OnPropertyChanged(nameof(PBPercents));
                        }
                    },
                    () =>
                    {
                        Parallel.ForEach(_executionSetupVM.ImagesSettings, parallelOptions, (settings) =>
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
                                _logger.Info($"Изображение {result.FullPath} сохранено. Оригинал: {settings.FullPath}");
                            }
                            catch (OperationCanceledException)
                            {
                                //_logger.Warn("Загрузка изображения отменена");
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"Не удалось создать новое изображение: {ex.Message}. Оригинал: {settings.FullPath}");
                            }
                            finally
                            {
                                _pbController.Tick();
                                OnPropertyChanged(nameof(PBPercents));
                            }
                        });
                    });

                _logger.Info("Выполнение завершено");
                _pbController.Percents = 100;
                OnPropertyChanged(nameof(PBPercents));
                IsRunning = false;
                IsCancellationInProgress = false;
                _cancellationTokenSource.Dispose();
            });
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _logger.Warn("Выполнение отменено пользователем");
            IsCancellationInProgress = true;
        }
        #endregion
    }
}
