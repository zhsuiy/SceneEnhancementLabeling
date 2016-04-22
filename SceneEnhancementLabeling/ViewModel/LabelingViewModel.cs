using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using SceneEnhancementLabeling.Models;
using MessageBox = System.Windows.Forms.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace SceneEnhancementLabeling.ViewModel
{
    public class LabelingViewModel : ViewModelBase
    {
        private const string DefaultCategoryPath = "SceneEnhancementLabeling.category.json";
        public LabelingViewModel()
        {
            LoadDefaultCateogry();
        }

        private void LoadDefaultCateogry()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(DefaultCategoryPath))
            {
                if (stream == null)
                {
                    return;
                }
                using (StreamReader reader = new StreamReader(stream))
                {
                    try
                    {
                        var content = reader.ReadToEnd();
                        var list = JsonConvert.DeserializeObject<List<CategoryItem>>(content);
                        Category = new ObservableCollection<CategoryItem>(list);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

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
                                FileName = Path.GetFileNameWithoutExtension(file),
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

        #region Component Labeling
        #endregion

        #region Common

        private RelayCommand _resetCommand;

        public ICommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(ResetAll));

        private void ResetAll()
        {
            Bitmap = null;
            _images.Clear();
            _selectedIndex = -1;
            IsBrowseEnabled = true;
            CanPrevious = false;
            CanNext = false;
            Category = null;
            CategoryIndex = 0;
        }

        private RelayCommand _saveCommand;

        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(SaveAll));

        private void SaveAll()
        {
            if (!_images.Any())
            {
                return;
            }
            var currentImageFile = _images.ElementAt(_selectedIndex);
            if (currentImageFile == null)
            {
                return;
            }
            var dlg = new SaveFileDialog
            {
                FileName = currentImageFile.FileName,
                DefaultExt = ".txt",
                Filter = @"Text documents (.txt)|*.txt"
            };
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                var content = GenerateContent();
                File.WriteAllText(dlg.FileName, content);
            }
        }

        private string GenerateContent()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Furniture Color");
            foreach (var categoryItem in Category)
            {
                sb.AppendFormat("{0} = {1} {2} {3} {4} {5} {6} {7} {8} {9}", 
                    categoryItem.Name,
                    categoryItem.Color0.Color.R,
                    categoryItem.Color0.Color.G,
                    categoryItem.Color0.Color.B,
                    categoryItem.Color1.Color.R,
                    categoryItem.Color1.Color.G,
                    categoryItem.Color1.Color.B,
                    categoryItem.Color2.Color.R,
                    categoryItem.Color2.Color.G,
                    categoryItem.Color2.Color.B);
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Decorations");

            return sb.ToString();
        }

        #endregion
    }
}
