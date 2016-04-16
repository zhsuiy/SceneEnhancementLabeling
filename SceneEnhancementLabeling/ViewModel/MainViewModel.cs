using System;
using System.Collections.Generic;
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

namespace SceneEnhancementLabeling.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }

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
                GetPixels(bitmap);
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
                GetPixels(bitmap);
            }
            else
            {
                _selectedIndex = 0;
            }
        }

        private void GetPixels(BitmapImage bitmap)
        {
            int stride = bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[bitmap.PixelHeight * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            Pixels = pixels;
        }

        private void GetColor(int x, out SolidColorBrush brush)
        {
            if (Pixels == null)
            {
                brush = null;
                return;
            }

            //  Get a pixel color like this.
            Color color = Colors.White;
            if (Bitmap.Format == PixelFormats.Pbgra32)
            {
                color = Color.FromArgb(Pixels[3 + 4 * x], Pixels[2 + 4 * x], Pixels[1 + 4 * x], Pixels[0 + 4 * x]);
            }
            else if (Bitmap.Format == PixelFormats.Bgr32)
            {
                color = Color.FromArgb(0xFF, Pixels[2 + 3 * x], Pixels[1 + 3 * x], Pixels[0 + 3 * x]);
            }
            brush = new SolidColorBrush(color);
        }

        private SolidColorBrush _color0;

        public SolidColorBrush Color0
        {
            get { return _color0; }
            set
            {
                _color0 = value;
                RaisePropertyChanged();
            }
        }

        private SolidColorBrush _color1;

        public SolidColorBrush Color1
        {
            get { return _color1; }
            set
            {
                _color1 = value;
                RaisePropertyChanged();
            }
        }

        private SolidColorBrush _color2;

        public SolidColorBrush Color2
        {
            get { return _color2; }
            set
            {
                _color2 = value;
                RaisePropertyChanged();
            }
        }

        private SolidColorBrush _color3;

        public SolidColorBrush Color3
        {
            get { return _color3; }
            set
            {
                _color3 = value;
                RaisePropertyChanged();
            }
        }

        private SolidColorBrush _color4;

        public SolidColorBrush Color4
        {
            get { return _color4; }
            set
            {
                _color4 = value;
                RaisePropertyChanged();
            }
        }

        public byte[] Pixels { get; set; }

        private RelayCommand _startCommand;

        public ICommand StartCommand => _startCommand ?? (_startCommand = new RelayCommand(DoProcess));

        private void DoProcess()
        {
            SolidColorBrush color0;
            GetColor(0, out color0);
            Color0 = color0;

            SolidColorBrush color1;
            GetColor(10, out color1);
            Color1 = color1;

            SolidColorBrush color2;
            GetColor(20, out color2);
            Color2 = color2;

            SolidColorBrush color3;
            GetColor(30, out color3);
            Color3 = color3;

            SolidColorBrush color4;
            GetColor(40, out color4);
            Color4 = color4;
        }
    }
}