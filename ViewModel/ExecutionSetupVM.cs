using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Panoramas_Editor
{
    internal class ExecutionSetupVM : ObservableObject
    {
        public FullyObservableCollection<ImageSettings> ImagesSettings { get; private set; }
        private SelectedDirectory _tempFilesDirectory { get => new SelectedDirectory(App.Current.Configuration["temp"]); }
        private DirDialogService _dirDialogService;
        private ImageDialogService _imagesDialogService;
        private IImageCompressor _imageCompressor;
        private IImageReader _imageReader;

        public const string KEEP_OLD_EXTENSION = "Не изменять";
        public ExecutionSetupVM(DirDialogService dirDialogService,
                                ImageDialogService imagesDialogService, 
                                IImageCompressor imageCompressor,
                                IImageReader imageReader)
        {
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
        }

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
                    Task.Run(() =>
                    {
                        var parallelOptions = new ParallelOptions();
                        parallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount;

                        Parallel.Invoke(parallelOptions,
                        () => // загрузка миниатюры
                        {
                            try
                            {
                                newImageSettings.Thumbnail = _imageCompressor.CompressImageToThumbnail(_tempFilesDirectory, newImageSettings);
                                newImageSettings.ThumbnailBitmapImage = _imageReader.ReadAsBitmapImage(newImageSettings.Thumbnail);
                            }
                            catch (Exception ex)
                            {
                                CustomMessageBox.ShowError($"Не удалось сжать изображение:\n{newImageSettings.FullPath}\nПодробности:\n{ex.Message}");
                            }
                        },
                        () => // загрузка сжатого изображения
                        {
                            try
                            {
                                newImageSettings.Compressed = _imageCompressor.CompressImage(_tempFilesDirectory, newImageSettings);
                            }
                            catch (Exception ex)
                            {
                                CustomMessageBox.ShowError($"Не удалось сжать изображение:\n{newImageSettings.FullPath}\nПодробности:\n{ex.Message}");
                            }
                        }
                        );
                    });
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

        public void SelectImagesFromDirectory()
        {
            try 
            { 
                if (_dirDialogService.OpenBrowsingDialog() == true)
                {
                    var directory = _dirDialogService.SelectedDirectory;
                    var files = Directory.GetFiles(directory.FullPath, "*", SearchOption.AllDirectories);
                    var result = from file in files
                                 where SelectedImage.ValidExtensions.Contains(Path.GetExtension(file))
                                 select new SelectedImage(file);
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
