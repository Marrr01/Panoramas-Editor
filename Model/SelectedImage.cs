using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Panoramas_Editor
{
    internal class SelectedImage : SelectedFile
    {
        #region static
        // Список стандартных расширений, которые должны обрабатываться на любой машине:
        // https://learn.microsoft.com/ru-ru/windows/win32/wic/-wic-about-windows-imaging-codec?redirectedfrom=MSDN
        public static List<string> ValidExtensions = new List<string>
        {
            ".bmp",
            //".gif",
            //".ico", Нет предустановленного кодировщика
            ".jpeg",
            ".jpe",
            ".jpg",
            ".jxr",
            ".png",
            ".tiff",
            ".tif"
            //".wdp",
            //".dds"
        };
        public static string ValidExtensionsAsString()
        {
            var sb = new StringBuilder();
            foreach (var extension in ValidExtensions)
            {
                sb.Append($"{extension};");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
        #endregion

        public SelectedImage(string fullPath) : base(fullPath)
        {
            if (!ValidExtensions.Contains(Extension))
            {
                throw new ArgumentException($"Файлы с расширением {Extension} не поддерживаются: {fullPath}");
            }
        }
    }
}
