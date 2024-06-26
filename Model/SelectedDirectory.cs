using System;
using System.IO;

namespace Panoramas_Editor
{
    internal class SelectedDirectory
    {
        public string FullPath { get; private set; }
        public string DirectoryName { get => Path.GetDirectoryName(FullPath); }

        public SelectedDirectory(string fullPath)
        {
            if (!Directory.Exists(fullPath))
            {
                throw new ArgumentException($"Папка не существует или путь указан неверно:\n{fullPath}");
            }
            else
            {
                FullPath = fullPath;
            }
        }
    }
}

