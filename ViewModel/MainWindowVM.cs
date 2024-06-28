using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Panoramas_Editor
{
    // TODO:
    // Импорт
    // Экспорт
    internal class MainWindowVM : ObservableObject
    {
        public string Manual { get => App.Current.Configuration["manual"]; }
        public string LogsDirectory { get => App.Current.Configuration["logs"]; }
        public string TempFilesDirectory { get => App.Current.Configuration["temp"]; }
        public string Version { get => App.Current.Configuration["version"]; }
        public string GitHub { get => App.Current.Configuration["github"]; }public UserControl ExecutionSetup { get; set; }
        private ExecutionSetupVM _executionSetupVM;
        public UserControl Execution { get; set; }
        private ExecutionVM _executionVM;
        private WpfDispatcherContext _context;
        private DirTableFileDialogService _dirTableFileDialogService;
        private TableFileDialogService _tableFileDialogService;
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

        public string NewTableDirectory { get; set; }

        public SelectedFile NewSelectedFile { get; set; }
        
        public MainWindowVM(ExecutionSetupVM executionSetupVM,
                            ExecutionVM executionVM,
                            WpfDispatcherContext context)
        {
            Directory.CreateDirectory(LogsDirectory);
            Directory.CreateDirectory(TempFilesDirectory);

            _dirTableFileDialogService = new DirTableFileDialogService();
            _tableFileDialogService = new TableFileDialogService();
            ExecutionSetup = new ExecutionSetup();
            _executionSetupVM = executionSetupVM;

            Execution = new Execution();
            _executionVM = executionVM;
            _executionVM.IsRunningChanged += (s, e) => OnPropertyChanged(nameof(IsRunning));

            _context = context;

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
                    Thread.Sleep(1000);
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
            SelectTableFileDirectoryCommand = new RelayCommand(selectTableDirectory);
            SelectTableFileCommand = new RelayCommand(SelectTableFilePath);
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
        public IRelayCommand SelectTableFileDirectoryCommand { get; }
        public IRelayCommand SelectTableFileCommand { get; }
        public void Import()
        {
           SelectTableFilePath();
           var dataFromTable = getDataFromTable();
           if (_executionSetupVM.ImagesSettings.Count > 0)
           {
                    if (CustomMessageBox.ShowQuestion("Текущие настройки изображений будут удалены\nПродолжить?", "Импорт") && dataFromTable.Count > 0)
                    {
                        _executionSetupVM.RemoveAllSettings();
                    }
                    else
                    {
                        return;
                    }
           }
           _executionSetupVM.AddImagesSettings(dataFromTable); 
        }

        private List<ImageSettings> getDataFromTable()
        {
            var dataFormTable = new List<ImageSettings>();
            
            string pathFileTable = NewSelectedFile.FullPath;
            using (FileStream file = new FileStream(pathFileTable, FileMode.Open, FileAccess.Read))
            {
                int numberDataInRow = 4;
                int listIndex = 0;
                
                IWorkbook workbook = new XSSFWorkbook(file);
                ISheet sheet = workbook.GetSheetAt(listIndex);
                
                for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    IRow row = sheet.GetRow(rowIndex);
                    if (row == null)
                    {
                        continue;
                    }
                    for (int cellIndex = 0; cellIndex < numberDataInRow; cellIndex++)
                    {
                        var cell = row.GetCell(cellIndex);
                        try
                        {
                            switch (cellIndex)
                            {
                                case 0:
                                    string pathToImage = Path.Combine(cell.ToString(), row.GetCell(cellIndex + 1).ToString());
                                    dataFormTable.Add(new ImageSettings(pathToImage));
                                    break;
                                case 2:
                                    double HorizontalOffset = CustomConvertToDouble(cell.ToString());
                                    if (!isInBorders(HorizontalOffset))
                                    {
                                        throw new ArithmeticException("У изображения неверные горизонтальные границы");
                                    }
                                    dataFormTable.Last().HorizontalOffset = HorizontalOffset;
                                    break;
                                case 3:
                                    double VerticalOffset = CustomConvertToDouble(cell.ToString());
                                    if (!isInBorders(VerticalOffset))
                                    {
                                        throw new ArithmeticException("У изображения неверные вертикальные границы");
                                    }
                                    dataFormTable.Last().VerticalOffset = VerticalOffset;
                                    break;
                            }
                        } 
                        catch (ArithmeticException aEx) { App.Current.Logger.Warn(aEx.Message); }
                        catch (Exception ex) { CustomMessageBox.ShowError("Не удалось сжать изображение " + ex.Message); } 
                    }
                }
            }
            return dataFormTable;
        }

        private bool isInBorders(double offset)
        {
            return offset is >= -1 and <= 1;
        }

        private static double CustomConvertToDouble(string value)
        {
            var decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            value.Replace(".", decimalSeparator).Replace(",", decimalSeparator);
            return Convert.ToDouble(value);
        }

        public void Export()
        {
            selectTableDirectory(); 
            writeDataInTableFile();
        }

        private void writeDataInTableFile()
        {
            string pathToTable = NewTableDirectory;
            saveDataInTableFile(pathToTable);
        }

        private void saveDataInTableFile(string pathToTable)
        {
            if (_executionSetupVM.ImagesSettings.Count > 0)
            {
                workWithTableFile(pathToTable);
                // можно заменить на ShowQuestion, где будет открываться папка с файлом или сам файл
                CustomMessageBox.ShowInfo("Данные сохраненны в таблицу по пути:\n" + pathToTable);
            }
            else
            {
                CustomMessageBox.ShowWarning("Отсуствуют изображения");
            }
        }

        private void workWithTableFile(string pathToTable)
        {
            int numberDataInRow = 4;
            var imagesData = _executionSetupVM.ImagesSettings;

            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("data");
            int rowIndex = 0;
            foreach (var image in imagesData)
            {
                IRow row = sheet.CreateRow(rowIndex++);
                for (int columIndex = 0; columIndex < numberDataInRow; columIndex++)
                {
                    addImage(image, row, columIndex);
                }
            }
            // Сохранение файла Excel
            using (FileStream fileStream = new FileStream(pathToTable, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fileStream);
            }
        }

        private void addImage(ImageSettings image, IRow row, int cellIndex)
        {
            switch (cellIndex)
            {
                case 0:
                    row.CreateCell(cellIndex).SetCellValue(image.Directory);
                    break;
                case 1:
                    row.CreateCell(cellIndex).SetCellValue(image.FileName); 
                    break;
                case 2:
                    row.CreateCell(cellIndex).SetCellValue(image.HorizontalOffset); 
                    break;
                case 3:
                    row.CreateCell(cellIndex).SetCellValue(image.VerticalOffset); 
                    break;
              
            }
        }
        private void selectTableDirectory() 
        {
            try
            {
                if (_dirTableFileDialogService.OpenBrowsingDialog() == true)
                {
                    NewTableDirectory = _dirTableFileDialogService.SelectedFilePath;
                }
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        private void SelectTableFilePath()
        {
            {
                try
                {
                    if (_tableFileDialogService.OpenBrowsingDialog() == true)
                    {
                        NewSelectedFile = _tableFileDialogService.selectedFile;
                    }
                }
                catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
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

        public void OpenManual()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Manual;
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
         }
        
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
                if (CustomMessageBox.ShowQuestion("Выполнение программы не завершено\n\nОтменить выполнение и закрыть?", "Закрытие"))
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
