using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Panoramas_Editor
{
    // TODO:
    // Импорт
    // Экспорт
    internal class MainWindowVM : ObservableObject
    {
        private string _version { get => App.Current.Configuration["version"]; }
        private string _manual { get => App.Current.Configuration["manual"]; }
        private string _logsDirectory { get => App.Current.Configuration["logs"]; }
        private string _tempFilesDirectory { get => App.Current.Configuration["temp"]; }

        public UserControl ExecutionSetup { get; set; }
        private ExecutionSetupVM _executionSetupVM;
        public UserControl Execution { get; set; }
        private ExecutionVM _executionVM;
        public bool IsRunning { get => _executionVM.IsRunning; }
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
        
        public MainWindowVM(ExecutionSetupVM executionSetupVM,
                            ExecutionVM executionVM)
        {
            Directory.CreateDirectory(_logsDirectory);
            Directory.CreateDirectory(_tempFilesDirectory);

            ExecutionSetup = new ExecutionSetup();
            _executionSetupVM = executionSetupVM;

            Execution = new Execution();
            _executionVM = executionVM;
            _executionVM.IsRunningChanged += (s, e) => OnPropertyChanged(nameof(IsRunning));

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
                    //var heap = BytesToReadableString(GC.GetTotalMemory(true));
                    //MemoryUsed = $"task manager:{taskManager} | heap:{heap}";
                    MemoryUsed = $"RAM used:{taskManager}";
                    Thread.Sleep(3000);
                }
            });

            IsSettingsOverlayShown = false;

            ImportCommand = new RelayCommand(Import);
            ExportCommand = new RelayCommand(Export);
            OpenProgramInfoCommand = new RelayCommand(OpenProgramInfo);
            OpenLogsCommand = new RelayCommand(OpenLogs);
            OpenTempCommand = new RelayCommand(OpenTemp);
            OpenManualCommand = new RelayCommand(OpenManual);
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
        public IRelayCommand OpenProgramInfoCommand { get; }
        public IRelayCommand OpenLogsCommand { get; }
        public IRelayCommand OpenTempCommand { get; }
        public IRelayCommand OpenManualCommand { get; }
        public IRelayCommand ShowSettingsOverlayCommand { get; }
        public IRelayCommand HideSettingsOverlayCommand { get; }
        public IRelayCommand <CancelEventArgs> HandleClosingEventCommand { get; }
        public IRelayCommand HandleClosedEventCommand { get; }
        public void Import()
        {
            // При импорте нужно учесть региональные настройки разделителя для числа с плавающей точкой
            // decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            try 
            {
                if (_executionSetupVM.ImagesSettings.Count > 0)
                {
                    if (CustomMessageBox.ShowQuestion("Текущие настройки изображений будут удалены\nПродолжить?", "Импорт"))
                    {
                        _executionSetupVM.RemoveAllSettings();
                    }
                    else
                    {
                        return;
                    }
                }
                throw new NotImplementedException("Эта команда не реализована"); 
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public void Export()
        {
            try { throw new NotImplementedException("Эта команда не реализована"); }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public void OpenProgramInfo()
        {
            CustomMessageBox.ShowInfo($"Версия: {_version}", "О программе");
        }

        public void OpenLogs()
        {
            try { Process.Start("explorer.exe", _logsDirectory); }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public void OpenTemp()
        {
            try { Process.Start("explorer.exe", _tempFilesDirectory); }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public void OpenManual()
        {
            try
            {
                if (File.Exists(_manual))
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = _manual;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
                else
                {
                    CustomMessageBox.ShowError("Файл с руководством не найден");
                }
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public bool _isSettingsOverlayShown;
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
                    _executionVM.Stop();
                    _executionVM.Execution.Wait();
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
