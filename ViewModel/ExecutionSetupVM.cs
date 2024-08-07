﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Panoramas_Editor
{
    internal class ExecutionSetupVM : ObservableObject
    {
        public FullyObservableCollection<ImageSettings> ImagesSettings { get; private set; }

        public const string KEEP_OLD_EXTENSION = "Не изменять";

        private DirDialogService _dirDialogService;
        private ImageDialogService _imagesDialogService;
        private IImageCompressor _imageCompressor;
        private IImageReader _imageReader;

        private Logger _logger => App.Current.Logger;
        private SelectedDirectory _tempFilesDirectory => new SelectedDirectory(App.Current.Configuration["temp"]);
        private readonly double CENTER;

        public ExecutionSetupVM(DirDialogService dirDialogService,
                                ImageDialogService imagesDialogService, 
                                IImageCompressor imageCompressor,
                                IImageReader imageReader)
        {
            CENTER = double.Parse(App.Current.Configuration["center"]);

            _dirDialogService = dirDialogService;
            _imagesDialogService = imagesDialogService;
            _imageCompressor = imageCompressor;
            _imageReader = imageReader;

            ImagesSettings = new FullyObservableCollection<ImageSettings>();
            ImagesSettings.ItemPropertyChanged += (s, e) =>
            { 
                OnPropertyChanged(nameof(IsEverythingMarked));
                OnPropertyChanged(nameof(MarkedSettings));
            };
            ImagesSettings.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IsEverythingMarked));
                OnPropertyChanged(nameof(MarkedSettings));
            };

            NewImagesExtension = NewImagesExtensions.First();
            ShareData = false;

            SelectImagesCommand = new RelayCommand(SelectImages);
            SelectImagesFromDirectoryCommand = new RelayCommand(SelectImagesFromDirectory);
            DeleteCommand = new RelayCommand(RemoveMarkedSettings);
            SelectNewImagesDirectoryCommand = new RelayCommand(SelectNewImagesDirectory);
            OpenNewImagesDirectoryCommand = new RelayCommand(OpenNewImagesDirectory);
            SetHorizontalOffsetToDefaultCommand = new RelayCommand(SetHorizontalOffsetToDefault);
            SetVerticalOffsetToDefaultCommand = new RelayCommand(SetVerticalOffsetToDefault);
            HandleDropEventCommand = new RelayCommand<DragEventArgs>(args => HandleDropEvent(args));

            CompressionTasks = new List<Task>();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        private ImageSettings _selectedSettings;
        public ImageSettings SelectedSettings
        {
            get => _selectedSettings;
            set
            {
                _selectedSettings = value;
                SelectedSettingsChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }
        public event EventHandler? SelectedSettingsChanged;

        #region чекбоксы
        private bool _isEverythingMarked;
        public bool IsEverythingMarked
        {
            get
            {
                _isEverythingMarked = true;
                if (ImagesSettings.Count == 0)
                {
                    _isEverythingMarked = false;
                }
                foreach (var settings in ImagesSettings)
                {
                    if (!settings.IsMarked)
                    {
                        _isEverythingMarked = false;
                        break;
                    }
                }
                return _isEverythingMarked;
            }
            set
            {
                if (value == true && _isEverythingMarked == false)
                {
                    foreach (var settings in ImagesSettings)
                    {
                        settings.IsMarked = true;
                    }
                }
                if (value == false && _isEverythingMarked == true)
                {
                    foreach (var settings in ImagesSettings)
                    {
                        settings.IsMarked = false;
                    }
                }
            }
        }

        public IEnumerable<ImageSettings> MarkedSettings
        {
            get
            {
                return from settings in ImagesSettings
                       where settings.IsMarked == true
                       select settings;
            }
        }
        #endregion

        #region commands
        public IRelayCommand SelectImagesCommand { get; }
        public IRelayCommand SelectImagesFromDirectoryCommand { get; }
        public IRelayCommand DeleteCommand { get; }
        public IRelayCommand SelectNewImagesDirectoryCommand { get; }
        public IRelayCommand OpenNewImagesDirectoryCommand { get; }
        public IRelayCommand SetHorizontalOffsetToDefaultCommand { get; }
        public IRelayCommand SetVerticalOffsetToDefaultCommand { get; }
        public IRelayCommand<DragEventArgs> HandleDropEventCommand { get; }

        public void HandleDropEvent(DragEventArgs args)
        {
            var result = new List<SelectedImage>();
            var newFilesPaths = (string[]) args.Data.GetData(DataFormats.FileDrop, false);
            foreach (var path in newFilesPaths)
            {
                try
                {
                    var attr = File.GetAttributes(path);
                    if (attr == FileAttributes.Directory)
                    {
                        result.AddRange(GetImagesFromDirectory(path));
                    }
                    else
                    {
                        result.Add(new SelectedImage(path));
                    }
                }
                catch { }
            }
            try
            {
                AddImagesSettings(result);
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public void OpenNewImagesDirectory()
        {
            if (Directory.Exists(NewImagesDirectory.FullPath))
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = NewImagesDirectory.FullPath;
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            else
            {
                CustomMessageBox.ShowWarning($"Такой папки не существует:\n{NewImagesDirectory.FullPath}");
            }
        }

        public void SetHorizontalOffsetToDefault()
        {
            foreach (var settings in MarkedSettings)
            {
                settings.HorizontalOffset = CENTER;
            }
        }

        public void SetVerticalOffsetToDefault()
        {
            foreach (var settings in MarkedSettings)
            {
                settings.VerticalOffset = CENTER;
            }
        }

        private SelectedDirectory _newImagesDirectory;
        public SelectedDirectory NewImagesDirectory
        {
            get => _newImagesDirectory;
            set
            {
                _newImagesDirectory = value;
                OnPropertyChanged();
            }
        }

        public void SelectNewImagesDirectory()
        {
            try
            {
                if (_dirDialogService.OpenBrowsingDialog() == true)
                {
                    NewImagesDirectory = _dirDialogService.SelectedDirectory;
                }
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public List<Task> CompressionTasks;
        public void StopCompressionTasks()
        {
            _cancellationTokenSource.Cancel();
            Task.WhenAll(CompressionTasks).Wait();
            _cancellationTokenSource.Dispose();
        }

        public void AddImagesSettings(IEnumerable<SelectedImage> newSelectedImages)
        {
            var newImagesSettings = from si in newSelectedImages
                                    select new ImageSettings(si.FullPath);
            AddImagesSettings(newImagesSettings);
        }

        public void AddImagesSettings(IEnumerable<ImageSettings> newImagesSettings)
        {
            foreach (var newImageSettings in newImagesSettings)
            {
                var sameImageSettings = ImagesSettings.FirstOrDefault((s) => s.FullPath == newImageSettings.FullPath, null);
                if (sameImageSettings == null)
                {
                    ImagesSettings.Add(newImageSettings);

                    // загрузка миниатюры
                    CompressionTasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            _cancellationToken.ThrowIfCancellationRequested();
                            newImageSettings.Thumbnail = _imageCompressor.CompressImageToThumbnail(_tempFilesDirectory, newImageSettings);
                            newImageSettings.ThumbnailBitmap = _imageReader.ReadAsBitmapSource(newImageSettings.Thumbnail);
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            _logger.Error($"Не удалось создать миниатюру {newImageSettings.FullPath}: {ex.Message}");
                        }
                    }));

                    // загрузка сжатого изображения
                    CompressionTasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            _cancellationToken.ThrowIfCancellationRequested();
                            newImageSettings.Compressed = _imageCompressor.CompressImage(_tempFilesDirectory, newImageSettings);
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            _logger.Error($"Не удалось создать сжатое изображение {newImageSettings.FullPath}: {ex.Message}");
                        }
                    }));
                }
            }
        }

        public void SelectImages()
        {
            try
            {
                if (_imagesDialogService.OpenBrowsingDialog() == true)
                {
                    AddImagesSettings(_imagesDialogService.SelectedImages);
                }
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        private IEnumerable<SelectedImage> GetImagesFromDirectory(string directory)
        {
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            return from file in files
                   where SelectedImage.ValidExtensions.Contains(Path.GetExtension(file))
                   select new SelectedImage(file);
        }

        public void SelectImagesFromDirectory()
        {
            try
            {
                if (_dirDialogService.OpenBrowsingDialog() == true)
                {
                    var result = GetImagesFromDirectory(_dirDialogService.SelectedDirectory.FullPath);
                    AddImagesSettings(result);
                }
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public void RemoveMarkedSettings()
        {
            try 
            {
                var toRemove = new List<ImageSettings>(MarkedSettings);
                foreach (var settings in toRemove)
                {
                    settings.Dispose();
                    ImagesSettings.Remove(settings);
                }
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }
        #endregion

        private bool _shareData;
        public bool ShareData
        {
            get => _shareData;
            set
            {
                _shareData = value;
                OnPropertyChanged();
            }
        }

        public List<string> NewImagesExtensions
        {
            get
            {
                var result = new List<string>() { KEEP_OLD_EXTENSION };
                result.AddRange(SelectedImage.ValidExtensions);
                return result;
            }
        }

        public string NewImagesExtension { get; set; }

        public void RemoveAllSettings()
        {
            foreach(var settings in ImagesSettings)
            {
                settings.Dispose();
            }
            ImagesSettings.Clear();
        }
    }
}
