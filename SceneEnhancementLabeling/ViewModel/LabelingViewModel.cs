using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SceneEnhancementLabeling.Models;
using MessageBox = System.Windows.Forms.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace SceneEnhancementLabeling.ViewModel
{
    public class LabelingViewModel : ViewModelBase
    {
        #region Image

        private BitmapImage _bitmap;

        public BitmapImage Bitmap
        {
            get { return _bitmap; }
            set
            {
                _bitmap = value;
                RaisePropertyChanged();
            }
        }

        private bool _isBrowseEnabled = true;

        public bool IsBrowseEnabled
        {
            get { return _isBrowseEnabled; }
            set
            {
                _isBrowseEnabled = value;
                RaisePropertyChanged();
            }
        }

        private bool _canNext;

        public bool CanNext
        {
            get { return _canNext; }
            set
            {
                _canNext = value;
                RaisePropertyChanged();
            }
        }

        private bool _canPrevious;

        public bool CanPrevious
        {
            get { return _canPrevious; }
            set
            {
                _canPrevious = value;
                RaisePropertyChanged();
            }
        }

        private readonly List<ImageDetail> _images = new List<ImageDetail>();
        private int _selectedIndex = -1;
        private RelayCommand<ExecutedRoutedEventArgs> _openFileCommand;

        public ICommand OpenFolderCommand
        {
            get
            {
                return _openFileCommand ?? (_openFileCommand = new RelayCommand<ExecutedRoutedEventArgs>(e =>
                {
                    var dialog = new FolderBrowserDialog();
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        IsBrowseEnabled = false;
                        var path = dialog.SelectedPath;

                        string[] supportedExtensions = { ".bmp", ".jpeg", ".jpg", ".png", ".tiff" };
                        var files = Directory.GetFiles(path, "*.*").Where(s =>
                        {
                            var extension = Path.GetExtension(s);
                            return extension != null && supportedExtensions.Contains(extension.ToLower());
                        });

                        foreach (var file in files)
                        {
                            var id = new ImageDetail
                            {
                                Path = file,
                                FileName = Path.GetFileName(file),
                                Extension = Path.GetExtension(file)
                            };

                            FileInfo fileInfo = new FileInfo(file);
                            id.Size = fileInfo.Length;
                            _images.Add(id);
                        }

                        LoadNext();
                    }
                }));
            }
        }

        private RelayCommand _nextCommand;

        public ICommand NextCommand => _nextCommand ?? (_nextCommand = new RelayCommand(LoadNext));

        private RelayCommand _previousCommand;

        public ICommand PreviousCommand => _previousCommand ?? (_previousCommand = new RelayCommand(LoadPrevious));

        private void CheckNextState()
        {
            if (_images.Any())
            {
                CanNext = _selectedIndex + 1 < _images.Count - 1;
                CanPrevious = _selectedIndex + 1 > 0;
            }
            else
            {
                CanNext = false;
                CanPrevious = false;
            }
        }

        private void CheckPreviousState()
        {
            if (_images.Any())
            {
                CanNext = _selectedIndex - 1 < _images.Count - 1;
                CanPrevious = _selectedIndex - 1 > 0;
            }
            else
            {
                CanNext = false;
                CanPrevious = false;
            }
        }

        private void LoadNext()
        {
            CheckNextState();
            if (_images.Any() && _selectedIndex + 1 < _images.Count)
            {
                var file = _images[++_selectedIndex];
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(file.Path, UriKind.Absolute);
                bitmap.EndInit();
                Bitmap = bitmap;
            }
            else
            {
                _selectedIndex = _images.Count - 1;
            }
        }

        private void LoadPrevious()
        {
            CheckPreviousState();
            if (_images.Any() && _selectedIndex - 1 >= 0)
            {
                var file = _images[--_selectedIndex];
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(file.Path, UriKind.Absolute);
                bitmap.EndInit();
                Bitmap = bitmap;
            }
            else
            {
                _selectedIndex = 0;
            }
        }

        #endregion

        #region Color Labeling

        private ObservableCollection<CategoryItem> _category;

        public ObservableCollection<CategoryItem> Category
        {
            get { return _category; }
            set
            {
                _category = value;
                RaisePropertyChanged();
            }
        }

        private int _catIndex;

        public int CategoryIndex
        {
            get { return _catIndex; }
            set
            {
                _catIndex = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource _source;

        public BitmapSource Source
        {
            get { return _source; }
            set
            {
                _source = value;
                RaisePropertyChanged();
            }
        }

        private int _itr;
        private Color _selectedColor;

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                RaisePropertyChanged();
                int mod = _itr++%3;
                if (mod == 0)
                {
                    Category[CategoryIndex].Color0 = new SolidColorBrush(value);
                }
                else if (mod == 1)
                {
                    Category[CategoryIndex].Color1 = new SolidColorBrush(value);
                }
                else
                {
                    Category[CategoryIndex].Color2 = new SolidColorBrush(value);
                }
            }
        }

        #endregion
    }
}
