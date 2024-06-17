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
        //private string _tempFilesDirectory { get => App.Current.Configuration["temp"]; }
        private DirDialogService _dirDialogService;
        private ImageDialogService _imageDialogService;
        private IImageCompressor _imageCompressor;
        //private IContext _context;

        public ExecutionSetupVM(DirDialogService dirDialogService, 
                                ImageDialogService imageDialogService, 
                                IImageCompressor imageCompressor
                                /*IContext context*/)
        {
            _dirDialogService = dirDialogService;
            _imageDialogService = imageDialogService;
            _imageCompressor = imageCompressor;
            //_context = context;

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

            NewFilesExtension = NewFilesExtensions.First();
            ShareData = true;

            SelectFilesCommand = new RelayCommand(SelectFiles);
            SelectFilesFromDirectoryCommand = new RelayCommand(SelectFilesFromDirectory);
            DeleteCommand = new RelayCommand(RemoveMarkedSettings);
            SelectNewFilesDirectoryCommand = new RelayCommand(SelectNewFilesDirectory);
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
        public IRelayCommand SelectFilesCommand { get; }
        public IRelayCommand SelectFilesFromDirectoryCommand { get; }
        public IRelayCommand DeleteCommand { get; }
        public IRelayCommand SelectNewFilesDirectoryCommand { get; }

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

        public List<string> NewFilesExtensions
        {
            get
            {
                var result = new List<string>() { "Не изменять" };
                result.AddRange(ImageSettings.ValidExtensions);
                return result;
            }
        }

        public string NewFilesExtension { get; set; }

        private SelectedFile _newFilesDirectory;
        public SelectedFile NewFilesDirectory
        {
            get => _newFilesDirectory;
            set
            {
                _newFilesDirectory = value;
                OnPropertyChanged();
            }
        }
        public void SelectNewFilesDirectory()
        {
            try
            {
                if (_dirDialogService.OpenBrowsingDialog() == true)
                {
                    NewFilesDirectory = _dirDialogService.SelectedFiles.First();
                }
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public void AddImagesSettings(IEnumerable<SelectedFile> newSelectedFiles)
        {
            var newImagesSettings = from sf in newSelectedFiles
                                    select new ImageSettings(sf.FullPath);
            AddImagesSettings(newImagesSettings);
        }

        public void AddImagesSettings(IEnumerable<ImageSettings> newImagesSettings)
        {
            foreach (var imageSettings in newImagesSettings)
            {
                if (!ImagesSettings.Contains(imageSettings))
                {
                    ImagesSettings.Add(imageSettings);
                    Task.Run(() =>
                    {
                        try
                        {
                            //throw new Exception("test");
                            imageSettings.CompressedBitmapImage = _imageCompressor.GetCompressedBitmapImage(imageSettings.FullPath);
                        }
                        catch (Exception ex)
                        {
                            CustomMessageBox.ShowError($"Не удалось сжать изображение:\n{imageSettings.FullPath}\nПодробности:\n{ex.Message}");
                        }
                    });
                }
            }
        }

        public void SelectFiles()
        {
            try
            {
                if (_imageDialogService.OpenBrowsingDialog() == true)
                {
                    AddImagesSettings(_imageDialogService.SelectedFiles);
                }
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }

        public void SelectFilesFromDirectory()
        {
            try 
            { 
                if (_dirDialogService.OpenBrowsingDialog() == true)
                {
                    var directory = _dirDialogService.SelectedFiles.First();
                    var files = Directory.GetFiles(directory.FullPath, "*", SearchOption.AllDirectories);
                    var result = from file in files
                                 where ImageSettings.ValidExtensions.Contains(Path.GetExtension(file))
                                 select new SelectedFile(file);
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
                    ImagesSettings.Remove(settings);
                }
            }
            catch (Exception ex) { CustomMessageBox.ShowError(ex.Message); }
        }
        #endregion

        public void RemoveAllSettings()
        {
            ImagesSettings.Clear();
        }
    }
}
