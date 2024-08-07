﻿using System;
using System.IO;

namespace Panoramas_Editor
{
    internal class SelectedDirectory
    {
        public string FullPath { get; private set; }
        public string DirectoryName => Path.GetDirectoryName(FullPath);

        public SelectedDirectory(string fullPath)
        {
            if (!Directory.Exists(fullPath))
            {
                throw new ArgumentException($"Папка не существует или путь указан неверно: {fullPath}");
            }
            else
            {
                FullPath = fullPath;
            }
        }
    }
}

