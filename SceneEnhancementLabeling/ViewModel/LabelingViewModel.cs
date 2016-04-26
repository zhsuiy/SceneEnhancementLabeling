using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private const string DefaultComponentPath = "SceneEnhancementLabeling.component.json";
        private const string DefaultGroupedComponentPath = "SceneEnhancementLabeling.grouped_components.json";
        public LabelingViewModel()
        {
            LoadDefaultCateogry();
            //LoadDefaultComponent();
            LoadDefaultGroupedCompoenent();
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

        private void LoadDefaultComponent()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(DefaultComponentPath))
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
                        var list = JsonConvert.DeserializeObject<List<ComponentItem>>(content);
                        foreach (var componentItem in list)
                        {
                            componentItem.Category = new List<CategoryItem>(Category);
                        }
                        Components = new ObservableCollection<ComponentItem>(list);
                        ComponentIndex = 0;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        private void LoadDefaultGroupedCompoenent()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(DefaultGroupedComponentPath))
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
                        var collection = JsonConvert.DeserializeObject<GroupedComponentsCollection>(content);
                        foreach (var group in collection)
                        {
                            group.Category = new List<CategoryItem>(Category);
                        }
                        ComponentsCollection = collection;
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
                        var path = dialog.SelectedPath;
                        IsBrowseEnabled = false;

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
            if (IsEditingColor || IsEditingComponent)
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
                    IsEditingComponent = false;
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
            if (IsEditingColor || IsEditingComponent)
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
                    IsEditingComponent = false;
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

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
                if (!IsColorLabelStepEnabled)
                {
                    return;
                }

                _selectedColor = value;
                RaisePropertyChanged();

                IsEditingColor = true;

                StringBuilder sb = new StringBuilder();
                if (string.IsNullOrEmpty(ColorLabelingOutput) || !ColorLabelingOutput.StartsWith("Furniture Color"))
                {
                    sb.AppendLine("Furniture Color");
                }

                var item = Category[CategoryIndex];
                var head = item.Name + " =";
                var breakLine = "\r\n";
                int startIndex = -1;
                int lineIndex = -1;
                if (string.IsNullOrEmpty(ColorLabelingOutput) || !ColorLabelingOutput.Contains(head))
                {
                    sb.AppendFormat("{0} =", item.Name);
                }
                else
                {
                    startIndex = ColorLabelingOutput.IndexOf(head, StringComparison.Ordinal) + head.Length;
                    lineIndex = ColorLabelingOutput.IndexOf(breakLine, startIndex, StringComparison.Ordinal) + breakLine.Length;
                }
                if (item.IsChecked0)
                {
                    item.Color0 = new SolidColorBrush(value);
                    sb.AppendFormat(" {0} {1} {2}", value.R, value.G, value.B);
                    if (item.Color1.Color != Colors.Transparent)
                    {
                        sb.AppendFormat(" {0} {1} {2}", item.Color1.Color.R, item.Color1.Color.G, item.Color1.Color.B);
                    }
                    if (item.Color2.Color != Colors.Transparent)
                    {
                        sb.AppendFormat(" {0} {1} {2}", item.Color2.Color.R, item.Color2.Color.G, item.Color2.Color.B);
                    }
                }
                else if (item.IsChecked1)
                {
                    if (item.Color0.Color == Colors.Transparent)
                    {
                        System.Windows.MessageBox.Show("Cannot set this color because the previous color never be set.");
                        return;
                    }
                    item.Color1 = new SolidColorBrush(value);
                    if (item.Color0.Color != Colors.Transparent)
                    {
                        sb.AppendFormat(" {0} {1} {2}", item.Color0.Color.R, item.Color0.Color.G, item.Color0.Color.B);
                    }
                    sb.AppendFormat(" {0} {1} {2}", value.R, value.G, value.B);
                    if (item.Color2.Color != Colors.Transparent)
                    {
                        sb.AppendFormat(" {0} {1} {2}", item.Color2.Color.R, item.Color2.Color.G, item.Color2.Color.B);
                    }
                }
                else if (item.IsChecked2)
                {
                    if (item.Color0.Color == Colors.Transparent || item.Color1.Color == Colors.Transparent)
                    {
                        System.Windows.MessageBox.Show("Cannot set this color because the previous color never be set.");
                        return;
                    }
                    item.Color2 = new SolidColorBrush(value);
                    if (item.Color0.Color != Colors.Transparent)
                    {
                        sb.AppendFormat(" {0} {1} {2}", item.Color0.Color.R, item.Color0.Color.G, item.Color0.Color.B);
                    }
                    if (item.Color1.Color != Colors.Transparent)
                    {
                        sb.AppendFormat(" {0} {1} {2}", item.Color1.Color.R, item.Color1.Color.G, item.Color1.Color.B);
                    }
                    sb.AppendFormat(" {0} {1} {2}", value.R, value.G, value.B);
                }
                if (lineIndex >= startIndex && lineIndex > -1 && startIndex > -1)
                {
                    ColorLabelingOutput = ColorLabelingOutput.Remove(startIndex, lineIndex - startIndex);
                }
                sb.AppendLine();
                ColorLabelingOutput += sb.ToString();

                sb.Clear();
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine();

                if (!string.IsNullOrEmpty(ComponentLabelingOutput))
                {
                    OutputContent = ColorLabelingOutput + sb.ToString() + ComponentLabelingOutput;
                }
                else
                {
                    OutputContent = ColorLabelingOutput;
                }
            }
        }

        #endregion

        #region Component Labeling
        private ObservableCollection<ComponentItem> _components;

        public ObservableCollection<ComponentItem> Components
        {
            get { return _components; }
            set
            {
                _components = value;
                RaisePropertyChanged();
            }
        }

        private int _componentIndex;

        public int ComponentIndex
        {
            get { return _componentIndex; }
            set
            {
                _componentIndex = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Output

        private bool _isColorLabelStepEnabled = true;

        public bool IsColorLabelStepEnabled
        {
            get { return _isColorLabelStepEnabled; }
            set
            {
                _isColorLabelStepEnabled = value;
                RaisePropertyChanged();
            }
        }

        private bool _isComponentLabelStepEnabled;

        public bool IsComponentLabelStepEnabled
        {
            get { return _isComponentLabelStepEnabled; }
            set
            {
                _isComponentLabelStepEnabled = value;
                RaisePropertyChanged();
            }
        }

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

        private bool _isEditingComponent;

        public bool IsEditingComponent
        {
            get { return _isEditingComponent; }
            set
            {
                _isEditingComponent = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand _openOutputCommand;

        public ICommand OpenOutputCommand => _openOutputCommand ?? (_openOutputCommand = new RelayCommand(OpenOutput));

        private void OpenOutput()
        {
            if (string.IsNullOrEmpty(OutputPath))
            {
                return;
            }
            ProcessStartInfo startInformation = new ProcessStartInfo { FileName = OutputPath };
            Process process = Process.Start(startInformation);
            if (process != null)
            {
                process.EnableRaisingEvents = true;
            }
        }

        private bool _isScrollTop;

        public bool IsScrollToTop
        {
            get { return _isScrollTop; }
            set
            {
                _isScrollTop = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand _resetCommand;

        public ICommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(ResetAll));

        private void ResetAll()
        {
            IsEditingColor = false;
            IsEditingComponent = false;
            IsColorLabelStepEnabled = true;
            IsComponentLabelStepEnabled = false;
            IsNextStepEnabled = true;
            IsPreviousStepEnabeld = false;
            Bitmap = null;
            _images.Clear();
            _selectedIndex = -1;
            IsBrowseEnabled = true;
            CanPrevious = false;
            CanNext = false;
            Category = null;
            CategoryIndex = 0;
            LoadDefaultCateogry();
            LoadDefaultComponent();
            IsScrollToTop = true;
            OutputPath = null;
            ColorLabelingOutput = null;
            ComponentLabelingOutput = null;
            OutputContent = null;
        }

        private void ResetOnlyLabeling()
        {
            LoadDefaultCateogry();
            LoadDefaultComponent();
            IsScrollToTop = true;
            IsNextStepEnabled = true;
            IsPreviousStepEnabeld = false;
            IsColorLabelStepEnabled = true;
            IsComponentLabelStepEnabled = false;
            OutputContent = null;
            ColorLabelingOutput = null;
            ComponentLabelingOutput = null;
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
                if (string.IsNullOrEmpty(OutputContent))
                {
                    return;
                }
                var buffer = Encoding.UTF8.GetBytes(OutputContent);
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
                    using (var fs = File.Open(fileName, FileMode.Truncate, FileAccess.Write))
                    {
                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
                IsEditingColor = false;
                IsEditingComponent = false;
                IsColorLabelStepEnabled = true;
                IsComponentLabelStepEnabled = false;
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
            if (Category.Any(t =>
                t.Color0.Color != Colors.Transparent || t.Color1.Color != Colors.Transparent ||
                t.Color2.Color != Colors.Transparent))
            {
                sb.AppendLine("Furniture Color");
                foreach (var categoryItem in Category)
                {
                    if (categoryItem.Color0.Color == Colors.Transparent &&
                        categoryItem.Color1.Color == Colors.Transparent &&
                        categoryItem.Color2.Color == Colors.Transparent)
                    {
                        continue;
                    }
                    sb.AppendFormat("{0} = ", categoryItem.Name);
                    if (categoryItem.Color0.Color != Colors.Transparent)
                    {
                        sb.AppendFormat("{0} {1} {2}",
                            categoryItem.Color0.Color.R,
                            categoryItem.Color0.Color.G,
                            categoryItem.Color0.Color.B);
                    }
                    if (categoryItem.Color1.Color != Colors.Transparent)
                    {
                        sb.AppendFormat(" {0} {1} {2}",
                            categoryItem.Color1.Color.R,
                            categoryItem.Color1.Color.G,
                            categoryItem.Color1.Color.B);
                    }
                    if (categoryItem.Color2.Color != Colors.Transparent)
                    {
                        sb.AppendFormat(" {0} {1} {2}",
                            categoryItem.Color2.Color.R,
                            categoryItem.Color2.Color.G,
                            categoryItem.Color2.Color.B);
                    }
                    sb.AppendLine();
                }
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine();

                if (Components.Any(t => t.SelectedCategoryIndex != -1))
                {
                    sb.AppendLine("Decorations");
                    foreach (var componentItem in Components)
                    {
                        if (componentItem.SelectedCategoryIndex == -1 &&
                            !componentItem.IsLeft &&
                            !componentItem.IsRight &&
                            !componentItem.IsFront &&
                            !componentItem.IsBack &&
                            !componentItem.IsCenter)
                        {
                            // not set
                            continue;
                        }

                        if (componentItem.SelectedCategoryIndex == -1)
                        {
                            sb.AppendFormat("{0} = NotSet |", componentItem.Name);
                        }
                        else
                        {
                            sb.AppendFormat("{0} = {1} |", componentItem.Name,
                                componentItem.Category[componentItem.SelectedCategoryIndex].Name);
                        }

                        if (!componentItem.IsLeft && !componentItem.IsRight && !componentItem.IsFront &&
                            !componentItem.IsBack && !componentItem.IsCenter)
                        {
                            sb.Append(" NotSet");
                            sb.AppendLine();
                            continue;
                        }

                        if (componentItem.IsLeft || componentItem.IsRight)
                        {
                            sb.Append(" ");
                            if (componentItem.IsLeft)
                            {
                                sb.Append("Left");
                            }
                            else if (componentItem.IsRight)
                            {
                                sb.Append("Right");
                            }
                        }

                        if (componentItem.IsFront || componentItem.IsBack)
                        {
                            sb.Append(" ");
                            if (componentItem.IsFront)
                            {
                                sb.Append("Front");
                            }
                            else if (componentItem.IsBack)
                            {
                                sb.Append("Back");
                            }
                        }

                        if (componentItem.IsCenter)
                        {
                            sb.Append(" ");
                            sb.Append("Center");
                        }
                        sb.AppendLine();
                    }
                }
            }
            return sb.ToString();
        }

        private bool _isNextStepEnabled = true;

        public bool IsNextStepEnabled
        {
            get { return _isNextStepEnabled; }
            set
            {
                _isNextStepEnabled = value;
                RaisePropertyChanged();
            }
        }

        private bool _isPreviousStepEnabeld;

        public bool IsPreviousStepEnabeld
        {
            get { return _isPreviousStepEnabeld; }
            set
            {
                _isPreviousStepEnabeld = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand _nextStepCommand;

        public ICommand NextStepCommand => _nextStepCommand ?? (_nextStepCommand = new RelayCommand(NextStep));

        private void NextStep()
        {
            if (Bitmap == null)
            {
                return;
            }
            IsColorLabelStepEnabled = false;
            IsComponentLabelStepEnabled = true;
            IsEditingComponent = true;
            IsNextStepEnabled = false;
            IsPreviousStepEnabeld = true;
            if (string.IsNullOrEmpty(ComponentLabelingOutput))
            {
                ComponentLabelingOutput = "Decorations\r\n";
            }
            else if (!ComponentLabelingOutput.StartsWith("Decorations"))
            {
                ComponentLabelingOutput = ComponentLabelingOutput.Insert(0, "Decorations\r\n");
            }
        }

        private RelayCommand _previousStepCommand;
        public ICommand PreviousStepCommand => _previousStepCommand ?? (_previousStepCommand = new RelayCommand(PreviousStep));

        private void PreviousStep()
        {
            if (Bitmap == null)
            {
                return;
            }
            IsColorLabelStepEnabled = true;
            IsComponentLabelStepEnabled = false;
            IsPopOpen = false;
            IsNextStepEnabled = true;
            IsPreviousStepEnabeld = false;
        }
        #endregion

        private bool _isPopOpen;

        public bool IsPopOpen
        {
            get { return _isPopOpen; }
            set
            {
                _isPopOpen = value;
                RaisePropertyChanged();
            }
        }

        private string _outputContent;

        public string OutputContent
        {
            get { return _outputContent; }
            set
            {
                _outputContent = value;
                RaisePropertyChanged();
            }
        }

        public string ColorLabelingOutput { get; set; }
        public string ComponentLabelingOutput { get; set; }

        private GroupedComponentsCollection _componentsCollection;

        public GroupedComponentsCollection ComponentsCollection
        {
            get { return _componentsCollection; }
            set
            {
                _componentsCollection = value;
                RaisePropertyChanged();
            }
        }

        private int _collectionIndex;

        public int CollectionIndex
        {
            get { return _collectionIndex; }
            set
            {
                _collectionIndex = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand _okCommand;
        public ICommand OkCommand => _okCommand ?? (_okCommand = new RelayCommand(DoOk));

        private void DoOk()
        {
            StringBuilder sb = new StringBuilder();
            if (ComponentsCollection.Any())
            {
                var group = ComponentsCollection[CollectionIndex];
                if (group.SelectedComponentIndex == -1 &&
                    group.SelectedCategoryIndex == -1 &&
                    !group.IsLeft &&
                    !group.IsRight &&
                    !group.IsFront &&
                    !group.IsBack &&
                    !group.IsCenter)
                {
                    // not set
                    return;
                }
                var component = group.Data[group.SelectedComponentIndex];
                if (group.SelectedCategoryIndex == -1)
                {
                    sb.AppendFormat("{0} = NotSet |", component.Name);
                }
                else
                {
                    var category = group.Category[group.SelectedCategoryIndex];
                    sb.AppendFormat("{0} = {1} |", component.Name, category.Name);
                }

                if (!group.IsLeft && !group.IsRight && !group.IsFront && !group.IsBack && !group.IsCenter)
                {
                    sb.Append(" NotSet");
                    sb.AppendLine();
                }
                else
                {
                    if (group.IsLeft || group.IsRight)
                    {
                        sb.Append(" ");
                        if (group.IsLeft)
                        {
                            sb.Append("Left");
                        }
                        else if (group.IsRight)
                        {
                            sb.Append("Right");
                        }
                    }

                    if (group.IsFront || group.IsBack)
                    {
                        sb.Append(" ");
                        if (group.IsFront)
                        {
                            sb.Append("Front");
                        }
                        else if (group.IsBack)
                        {
                            sb.Append("Back");
                        }
                    }

                    if (group.IsCenter)
                    {
                        sb.Append(" ");
                        sb.Append("Center");
                    }
                    sb.AppendLine();
                }

                ComponentLabelingOutput += sb.ToString();

                sb.Clear();
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine();

                if (!string.IsNullOrEmpty(ColorLabelingOutput))
                {
                    OutputContent = ColorLabelingOutput + sb + ComponentLabelingOutput;
                }
                else
                {
                    OutputContent = ComponentLabelingOutput;
                }

                ResetPopup();
                IsPopOpen = false;
            }
        }

        private void ResetPopup()
        {
            CollectionIndex = 0;
            foreach (var group in ComponentsCollection)
            {
                group.SelectedCategoryIndex = -1;
                group.SelectedComponentIndex = -1;
                group.ErrorMessage = "";
                group.IsLeft = false;
                group.IsRight = false;
                group.IsFront = false;
                group.IsBack = false;
                group.IsCenter = false;
                group.IsDirty = false;
            }
        }
    }
}
