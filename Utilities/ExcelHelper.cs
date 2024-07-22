using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Panoramas_Editor
{
    internal class ExcelHelper : ISerializer, IDeserializer
    {
        private TableDialogService _tableDialogService;
        private SaveTableDialogService _saveTableDialogService;
        /// <summary>
        /// 1 - папка;
        /// 2 - имя файла;
        /// 3 - горизонтальное смещение;
        /// 4 - вертикальное смещение
        /// </summary>
        private const int DATA_COLUMNS = 4;
        private Logger _logger => App.Current.Logger;
        private readonly double MIN_OFFSET;
        private readonly double MAX_OFFSET;

        public ExcelHelper(TableDialogService tableDialogService, 
                           SaveTableDialogService saveTableDialogService)
        {
            MIN_OFFSET = double.Parse(App.Current.Configuration["min"]);
            MAX_OFFSET = double.Parse(App.Current.Configuration["max"]);

            _tableDialogService = tableDialogService;
            _saveTableDialogService = saveTableDialogService;
        }

        public IEnumerable<ImageSettings> ReadData()
        {
            if (!_tableDialogService.OpenBrowsingDialog()) { return null; }

            var table = _tableDialogService.SelectedFile;
            var result = new List<ImageSettings>();

            using (FileStream stream = new FileStream(table.FullPath, FileMode.Open, FileAccess.Read))
            {
                _logger.Info($"Импорт данных из таблицы {table.FullPath} ...");

                IWorkbook workbook = new XSSFWorkbook(stream);
                ISheet sheet = workbook.GetSheetAt(0);

                //в 0 строке имя колонки
                for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    try
                    {
                        IRow row = sheet.GetRow(rowIndex);

                        string directory        = row.GetCell(0).ToString();
                        string file             = row.GetCell(1).ToString();
                        string horizontalOffset = row.GetCell(2).ToString();
                        string verticalOffset   = row.GetCell(3).ToString();

                        var settings = new ImageSettings(Path.Combine(directory, file));
                        settings.HorizontalOffset = TryReadValue(horizontalOffset);
                        settings.VerticalOffset   = TryReadValue(verticalOffset);

                        result.Add(settings);
                    }
                    catch (NullReferenceException) { _logger.Error($"Cтрока {rowIndex + 1}: Значение отсутствует"); }
                    catch (FormatException) { _logger.Error($"Cтрока {rowIndex + 1}: Не удалось преобразовать значение в число"); }
                    catch (Exception ex) { _logger.Error($"Cтрока {rowIndex + 1}: {ex.Message}"); }
                }
            }

            if (result.Count == 0)
            {
                _logger.Warn("В указанной таблице нет подходящих данных");
                return null;
            }
            return result;
        }

        private double TryReadValue(string value)
        {
            var decimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var result = double.Parse(value.Replace(".", decimalSeparator)
                                           .Replace(",", decimalSeparator)
                                           .Replace(" ", decimalSeparator)
                                           .Replace("'", decimalSeparator));
            if (result < MIN_OFFSET || MAX_OFFSET < result) { throw new Exception($"{result} не входит в допустимый диапазон"); }
            return result;
        }

        public void WriteData(IEnumerable<ImageSettings> data)
        {
            if (!_saveTableDialogService.OpenBrowsingDialog()) { return; }
            var newTable = _saveTableDialogService.SelectedFilePath;

            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Данные для Panoramas Editor");

            var rowsTotal = data.Count() + 1/*имена колонок*/;
            for (int rowIdx = 0; rowIdx < rowsTotal; rowIdx++)
            {
                var row = sheet.CreateRow(rowIdx);

                if (rowIdx == 0)
                {
                    var columnNameFont = workbook.CreateFont();
                    columnNameFont.IsBold = true;
                    
                    var columnNameStyle = workbook.CreateCellStyle();
                    columnNameStyle.SetFont(columnNameFont);
                    columnNameStyle.Alignment = HorizontalAlignment.Center;

                    var cell0 = row.CreateCell(0);
                    cell0.CellStyle = columnNameStyle;
                    cell0.SetCellValue("Папка");
                    
                    var cell1 = row.CreateCell(1);
                    cell1.CellStyle = columnNameStyle;
                    cell1.SetCellValue("Файл");

                    var cell2 = row.CreateCell(2);
                    cell2.CellStyle = columnNameStyle;
                    cell2.SetCellValue("Гор.смещ.");

                    var cell3 = row.CreateCell(3);
                    cell3.CellStyle = columnNameStyle;
                    cell3.SetCellValue("Вер.смещ.");
                }
                else
                {
                    var settings = data.ElementAt(rowIdx - 1);
                    row.CreateCell(0).SetCellValue(settings.Directory);
                    row.CreateCell(1).SetCellValue(settings.FileName);
                    row.CreateCell(2).SetCellValue(settings.HorizontalOffset);
                    row.CreateCell(3).SetCellValue(settings.VerticalOffset);
                }
            }

            for (int columIndex = 0; columIndex < DATA_COLUMNS; columIndex++)
            {
                sheet.AutoSizeColumn(columIndex);
            }
            sheet.CreateFreezePane(0, 1);
            
            // Сохранение файла Excel
            using (FileStream stream = new FileStream(newTable, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(stream);
            }

            if (CustomMessageBox.ShowQuestion($"Данные успешно записаны в таблицу\n{newTable}\nОткрыть таблицу?", "Экспорт данных"))
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = newTable;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
                catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
            }
        }
    }
}
