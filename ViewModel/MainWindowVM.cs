using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Panoramas_Editor
{
    internal class MainWindowVM : ObservableObject
    {
        public UserControl ExecutionSetup { get; set; }
        private ExecutionSetupVM _executionSetupVM;
        public UserControl Execution { get; set; }
        private ExecutionVM _executionVM;
        private WpfDispatcherContext _context;
        private ISerializer _serializer;
        private IDeserializer _deserializer;
        public bool IsRunning => _executionVM.IsRunning;
        public UserControl Editor { get; set; }

        private string _memoryUsed;
        public string MemoryUsed
        {
            get => _memoryUsed;
            set
            {
                _memoryUsed = value;
                OnPropertyChanged();
            }
        }

        private Logger _logger => App.Current.Logger;
        public string LogsDirectory { get; }
        public string TempFilesDirectory { get; }
        public string Version { get; }
        public string GitHub { get; }

        public MainWindowVM(ExecutionSetupVM executionSetupVM,
                            ExecutionVM executionVM,
                            WpfDispatcherContext context,
                            ISerializer serializer,
                            IDeserializer deserializer)
        {
            LogsDirectory = App.Current.Configuration["logs"];
            TempFilesDirectory = App.Current.Configuration["temp"];
            Version = App.Current.Configuration["version"];
            GitHub = App.Current.Configuration["github"];

            Directory.CreateDirectory(LogsDirectory);
            Directory.CreateDirectory(TempFilesDirectory);

            ExecutionSetup = new ExecutionSetup();
            _executionSetupVM = executionSetupVM;

            Execution = new Execution();
            _executionVM = executionVM;
            _executionVM.IsRunningChanged += (s, e) => OnPropertyChanged(nameof(IsRunning));

            _context = context;
            _serializer = serializer;
            _deserializer = deserializer;

            Editor = new Stub();

            _executionSetupVM.SelectedSettingsChanged += (s, e) =>
            {
                if (_executionSetupVM.SelectedSettings == null)
                {
                    Editor = new Stub();
                }
                else
                {
                    Editor = new Editor();
                }
                OnPropertyChanged(nameof(Editor));
            };

            Task.Run(() =>
            {
                var process = Process.GetCurrentProcess();
                var counter = new PerformanceCounter("Process", "Working Set - Private", process.ProcessName);

                while (true)
                {
                    var taskManager = BytesToReadableString(counter.RawValue);
                    MemoryUsed = $"RAM used:{taskManager}";
                    Thread.Sleep(1500);
                }
            });

            IsSettingsOverlayShown = false;

            ImportCommand = new RelayCommand(Import);
            ExportCommand = new RelayCommand(Export);
            OpenLogsCommand = new RelayCommand(OpenLogs);
            OpenTempCommand = new RelayCommand(OpenTemp);
            //OpenManualCommand = new RelayCommand(OpenManual);
            OpenGitHubCommand = new RelayCommand(OpenGitHub);
            ShowSettingsOverlayCommand = new RelayCommand(ShowSettingsOverlay);
            HideSettingsOverlayCommand = new RelayCommand(HideSettingsOverlay);
            HandleClosingEventCommand = new RelayCommand<CancelEventArgs>(HandleClosingEvent);
            HandleClosedEventCommand = new RelayCommand(HandleClosedEvent);
        }
        
        private string BytesToReadableString(double bytes)
        {
            const double BYTES_IN_KILOBYTE = 1024;
            const double BYTES_IN_MEGABYTE = 1024 * 1024;
            const double BYTES_IN_GIGABYTE = 1024 * 1024 * 1024;

            var gb = Math.Round(bytes / BYTES_IN_GIGABYTE, 0, MidpointRounding.ToZero);
            bytes -= gb * BYTES_IN_GIGABYTE;

            var mb = Math.Round(bytes / BYTES_IN_MEGABYTE, 0, MidpointRounding.ToZero);
            bytes -= mb * BYTES_IN_MEGABYTE;

            var kb = Math.Round(bytes / BYTES_IN_KILOBYTE, 0, MidpointRounding.ToZero);
            //bytes -= kb * BYTES_IN_KILOBYTE;

            return $"{(gb > 0 ? $" {gb} GB" : string.Empty)}{(mb > 0 ? $" {mb} MB" : string.Empty)}{(kb > 0 ? $" {kb} KB" : string.Empty)}";
        }

        #region commands
        public IRelayCommand ImportCommand { get; }
        public IRelayCommand ExportCommand { get; }
        public IRelayCommand OpenLogsCommand { get; }
        public IRelayCommand OpenTempCommand { get; }
        //public IRelayCommand OpenManualCommand { get; }
        public IRelayCommand OpenGitHubCommand { get; }
        public IRelayCommand ShowSettingsOverlayCommand { get; }
        public IRelayCommand HideSettingsOverlayCommand { get; }
        public IRelayCommand <CancelEventArgs> HandleClosingEventCommand { get; }
        public IRelayCommand HandleClosedEventCommand { get; }

        public void Import()
        {
            if (_executionSetupVM.ImagesSettings.Count > 0)
            {
                if (!CustomMessageBox.ShowQuestion("Текущие настройки изображений будут удалены\nПродолжить?", "Импорт данных")) { return; }
            }
            try
            {
                var data = _deserializer.ReadData();
                if (data == null) { return; }
                _executionSetupVM.RemoveAllSettings();
                _executionSetupVM.AddImagesSettings(data);
                _logger.Info($"Количество добавленных настроек изображений: {data.Count()}");
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(ex.Message, "Импорт данных");
            }
        }

        public void Export()
        {
            try
            {
                _serializer.WriteData(_executionSetupVM.ImagesSettings);
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowError(ex.Message, "Экспорт данных");
            }
        }

        public void OpenLogs()
        {
            try { Process.Start("explorer.exe", LogsDirectory); }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public void OpenTemp()
        {
            try { Process.Start("explorer.exe", TempFilesDirectory); }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        //public void OpenManual()
        //{
        //    try
        //    {
        //        ProcessStartInfo psi = new ProcessStartInfo();
        //        psi.FileName = Manual;
        //        psi.UseShellExecute = true;
        //        Process.Start(psi);
        //    }
        //    catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        // }
        
        public void OpenGitHub()
        {
            try { Process.Start(new ProcessStartInfo(GitHub) { UseShellExecute = true }); }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public bool _isSettingsOverlayShown;
        /// <summary>
        /// Теперь это оверлей с информацией о программе
        /// </summary>
        public bool IsSettingsOverlayShown
        {
            get => _isSettingsOverlayShown;
            set
            {
                _isSettingsOverlayShown = value;
                OnPropertyChanged();
            }
        }

        public void ShowSettingsOverlay() => IsSettingsOverlayShown = true;

        public void HideSettingsOverlay() => IsSettingsOverlayShown = false;

        public void HandleClosingEvent(CancelEventArgs e)
        {
            if (IsRunning)
            {
                if (CustomMessageBox.ShowQuestion("Выполнение программы не завершено\nОтменить выполнение и закрыть?", "Закрытие"))
                {
                    e.Cancel = true;
                    _executionVM.Stop();
                    _executionVM.IsRunningChanged += (s, e) => _context.Invoke(() => App.Current.Shutdown());
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        public void HandleClosedEvent()
        {
            try
            {
                _executionSetupVM.StopCompressionTasks();
                LogManager.Shutdown();
                foreach (var settings in _executionSetupVM.ImagesSettings)
                {
                    settings.Dispose();
                }
            }
            catch { }
        }
        #endregion
    }
}
