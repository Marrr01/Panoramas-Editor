using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        public Task Execution { get; set; }
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        public ExecutionVM()
        {
            IsRunning = false;
            RunCommand = new RelayCommand(Run);
            StopCommand = new RelayCommand(Stop);

            _logger = LogManager.GetLogger("logger");
        }

        private static Logger _logger;

        private string _log;
        public string Log
        {
            get => _log;
            set
            {
                _log = value;
                OnPropertyChanged();
            }
        }
        public void Add(string message, NLog.LogLevel logLevel) => Log = $"{Log}{message}\n";

        public IRelayCommand RunCommand { get; }
        public IRelayCommand StopCommand { get; }

        public void Run()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            Execution = Task.Run(async () =>
            {
                IsRunning = true;

                await Task.Run(() =>
                {
                    try
                    {
                        while (true)
                        {
                            if (_cancellationToken.IsCancellationRequested)
                            {
                                _cancellationToken.ThrowIfCancellationRequested();
                            }
                            _logger.Info("это запись из логгера");
                            Thread.Sleep(3000);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        
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


    }
}
