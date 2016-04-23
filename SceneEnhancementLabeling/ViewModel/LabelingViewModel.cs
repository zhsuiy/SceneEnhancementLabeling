﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
using SceneEnhancementLabeling.Common;
using SceneEnhancementLabeling.Models;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace SceneEnhancementLabeling.ViewModel
{
    public class LabelingViewModel : ViewModelBase
    {
        #region Default
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
                        CategoryIndex = 0;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        private string _outputPath;

        public string OutputPath
        {
            get { return _outputPath; }
            set
            {
                _outputPath = value;
                RaisePropertyChanged();
            }
        }

        #endregion

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

                        OutputPath = path + "\\Output";

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
            if (IsEditingColor)
            {
                var result = System.Windows.Forms.MessageBox.Show(
                    @"You are labeling current image without save. Do you like to save before loading next ?",
                    @"Warning",
                    MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    SaveAll();
                }
                else if (result == DialogResult.No)
                {
                    IsEditingColor = false;
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

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

                ResetOnlyLabeling();
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

                ResetOnlyLabeling();
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
        
        private Color _selectedColor;

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                RaisePropertyChanged();

                IsEditingColor = true;
                var item = Category[CategoryIndex];
                if (item.IsChecked0)
                {
                    item.Color0 = new SolidColorBrush(value);
                }
                else if (item.IsChecked1)
                {
                    if (item.Color0.Color == Colors.Transparent)
                    {
                        System.Windows.MessageBox.Show("Cannot set this color because the previous color never be set.");
                        return;
                    }
                    item.Color1 = new SolidColorBrush(value);
                }
                else if (item.IsChecked2)
                {
                    if (item.Color0.Color == Colors.Transparent || item.Color1.Color == Colors.Transparent)
                    {
                        System.Windows.MessageBox.Show("Cannot set this color because the previous color never be set.");
                        return;
                    }
                    item.Color2 = new SolidColorBrush(value);
                }
            }
        }

        #endregion

        #region Component Labeling
        #endregion

        #region Output

        private bool _isEditingColor;

        public bool IsEditingColor
        {
            get { return _isEditingColor; }
            set
            {
                _isEditingColor = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand _resetCommand;

        public ICommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(ResetAll));

        private void ResetAll()
        {
            IsEditingColor = false;
            Bitmap = null;
            _images.Clear();
            _selectedIndex = -1;
            IsBrowseEnabled = true;
            CanPrevious = false;
            CanNext = false;
            Category = null;
            CategoryIndex = 0;
            LoadDefaultCateogry();
            OutputPath = null;
        }

        private void ResetOnlyLabeling()
        {
            LoadDefaultCateogry();
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
            if (string.IsNullOrEmpty(OutputPath))
            {
                return;
            }
            //var dlg = new SaveFileDialog
            //{
            //    FileName = currentImageFile.FileName,
            //    DefaultExt = ".txt",
            //    Filter = @"Text documents (.txt)|*.txt"
            //};
            //var result = dlg.ShowDialog();
            //if (result == DialogResult.OK || result == DialogResult.Yes)
            //{
            //    var content = GenerateContent();
            //    File.WriteAllText(dlg.FileName, content);
            //}

            try
            {
                var content = GenerateContent();
                var buffer = Encoding.UTF8.GetBytes(content);
                if (!Directory.Exists(OutputPath))
                {
                    Directory.CreateDirectory(OutputPath);
                }
                var fileName = Path.Combine(OutputPath, currentImageFile.FileName + ".txt");
                if (!File.Exists(fileName))
                {
                    using (var fs = File.Create(fileName))
                    {
                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    using (var fs = File.OpenWrite(fileName))
                    {
                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
                IsEditingColor = false;
                AutoClosingMessageBox.Show("Save successfully.", "Labeling", 2000);
            }
            catch (Exception)
            {
                AutoClosingMessageBox.Show("Save failed.", "Labeling", 2000);
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
