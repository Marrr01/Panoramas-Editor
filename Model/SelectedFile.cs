﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace Panoramas_Editor
{
    internal class SelectedFile : ObservableObject
    {
        public string FullPath { get; private set; }
        public string Directory => Path.GetDirectoryName(FullPath);
        public string FileName => Path.GetFileName(FullPath);
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FullPath);
        public string Extension => Path.GetExtension(FullPath);

        public SelectedFile(string fullPath)
        {
            if (!File.Exists(fullPath))
            { 
                throw new ArgumentException($"Файл не существует или путь указан неверно: {fullPath}"); 
            }
            else 
            {
                FullPath = fullPath; 
            }
        }
    }
}
