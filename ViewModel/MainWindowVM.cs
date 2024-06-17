﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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
        //private string _tempFilesDirectory { get => App.Current.Configuration["temp"]; }

        private ExecutionSetupVM _executionSetupVM;
        public UserControl ExecutionSetup { get; set; }
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
        
        public MainWindowVM(ExecutionSetupVM executionSetupVM)
        {
            Directory.CreateDirectory(_logsDirectory);
            //Directory.CreateDirectory(_tempFilesDirectory);

            ExecutionSetup = new ExecutionSetup();
            _executionSetupVM = executionSetupVM;

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

                const double BYTES_IN_KILOBYTE = 1024;
                const double BYTES_IN_MEGABYTE = 1024 * 1024;
                const double BYTES_IN_GIGABYTE = 1024 * 1024 * 1024;

                while (true)
                {
                    double bytes = counter.RawValue;

                    var gb = Math.Round(bytes / BYTES_IN_GIGABYTE, 0, MidpointRounding.ToZero);
                    bytes -= gb * BYTES_IN_GIGABYTE;

                    var mb = Math.Round(bytes / BYTES_IN_MEGABYTE, 0, MidpointRounding.ToZero);
                    bytes -= mb * BYTES_IN_MEGABYTE;

                    var kb = Math.Round(bytes / BYTES_IN_KILOBYTE, 0, MidpointRounding.ToZero);
                    //bytes -= kb * BYTES_IN_KILOBYTE;

                    MemoryUsed = $"memory used:{(gb > 0 ? $" {gb} GB" : string.Empty)}{(mb > 0 ? $" {mb} MB" : string.Empty)}{(kb > 0 ? $" {kb} KB" : string.Empty)}";
                    Thread.Sleep(3000);
                }
            });

            ImportCommand = new RelayCommand(Import);
            ExportCommand = new RelayCommand(Export);
            OpenProgramInfoCommand = new RelayCommand(OpenProgramInfo);
            OpenLogsCommand = new RelayCommand(OpenLogs);
            OpenManualCommand = new RelayCommand(OpenManual);
            HandleClosedEventCommand = new RelayCommand(HandleClosedEvent);
        }

        #region commands
        public IRelayCommand ImportCommand { get; }
        public IRelayCommand ExportCommand { get; }
        public IRelayCommand OpenProgramInfoCommand { get; }
        public IRelayCommand OpenLogsCommand { get; }
        public IRelayCommand OpenManualCommand { get; }
        public IRelayCommand HandleClosedEventCommand { get; }

        public void Import()
        {
            // При импорте нужно учесть региональные настройки разделителя для числа с плавающей точкой
            try { throw new NotImplementedException("Эта команда не реализована"); }
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

        public void HandleClosedEvent()
        {
            //try { Directory.Delete(_tempFilesDirectory, true); }
            //catch { }
        }
        #endregion
    }
}
