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

        private int _widthstride = 0;
        private int _imageWidth;
        private int _imageHeight;
        private int _channelNum = 3;
        private void GetPixels(BitmapImage bitmap)
        {
            _imageWidth = bitmap.PixelWidth;
            _imageHeight = bitmap.PixelHeight;
            _channelNum = bitmap.Format.BitsPerPixel/8;
            _widthstride = _imageWidth * _channelNum;
            byte[] pixels = new byte[bitmap.PixelHeight * _widthstride];
            bitmap.CopyPixels(pixels, _widthstride, 0);
            Pixels = pixels;
        }

        private Color GetColor(int x, int y)
        {
            if (Pixels == null)
            {
               // brush = null;
                return Colors.White;
            }

            //  Get a pixel color like this.
            Color color = Colors.White;
            if (Bitmap.Format == PixelFormats.Pbgra32)
            {
                color = Color.FromArgb(Pixels[3 + 4 * x + y*_widthstride], Pixels[2 + 4 * x + y * _widthstride],
                    Pixels[1 + 4 * x + y * _widthstride], Pixels[0 + 4 * x + y * _widthstride]);
            }
            else if (Bitmap.Format == PixelFormats.Bgr32)
            {
                color = Color.FromArgb(0xFF, Pixels[2 + _channelNum * x + y * _widthstride], Pixels[1 + _channelNum * x + y * _widthstride], 
                    Pixels[0 + _channelNum * x + y * _widthstride]);
            }
            //brush = new SolidColorBrush(color);
            return color;
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

        private int _binH = 10;
        private int _binS = 10;
        private int _binV = 10;

        private void DoProcess()
        {
            double stepH = 360.0/_binH;
            double stepS = 1.0/_binS;
            double stepV = 1.0/_binV;

            Dictionary<Tuple<int,int,int>, int> binmap = new Dictionary<Tuple<int, int, int>, int>();
            //int[,,] bins = new int[_binH,_binS,_binV];
            double[] hsv = new double[3];
            for (int i = 0; i < _imageWidth; i++)
            {
                for (int j = 0; j < _imageHeight; j++)
                {
                    
                    Color color = GetColor(i, j);
                    hsv = Rgb2Hsv(color);
                    int hi=0, si=0, vi=0;
                    hi = Convert.ToInt32(Math.Floor(hsv[0] / stepH));
                    hi = hi < _binH ? hi : _binH - 1;
                    si = Convert.ToInt32(Math.Floor(hsv[1]/stepS));
                    si = si < _binS ? si : _binS - 1;
                    vi = Convert.ToInt32(Math.Floor(hsv[2] / stepV));
                    vi = vi < _binV ? vi : _binV - 1;
                    var tuple = new Tuple<int, int, int>(hi, si, vi);
                    if (binmap.ContainsKey(tuple))
                    {
                        binmap[tuple]++;
                    }
                    else
                    {
                        binmap.Add(tuple, 1);
                    }
                }
            }

            var ordered_binmap = binmap.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            var hsvindex = ordered_binmap.ElementAt(0).Key;
            Color0  = new SolidColorBrush(Hsv2Rgb(hsvindex.Item1 * stepH + 0.5 * stepH, hsvindex.Item2 * stepS + 0.5 * stepS,
                hsvindex.Item3 * stepV + 0.5 * stepV));
            hsvindex = ordered_binmap.ElementAt(1).Key;
            Color1 = new SolidColorBrush(Hsv2Rgb(hsvindex.Item1 * stepH + 0.5 * stepH, hsvindex.Item2 * stepS + 0.5 * stepS,
                hsvindex.Item3 * stepV + 0.5 * stepV));
            hsvindex = ordered_binmap.ElementAt(2).Key;
            Color2 = new SolidColorBrush(Hsv2Rgb(hsvindex.Item1 * stepH + 0.5 * stepH, hsvindex.Item2 * stepS + 0.5 * stepS,
                hsvindex.Item3 * stepV + 0.5 * stepV));
            hsvindex = ordered_binmap.ElementAt(3).Key;
            Color3 = new SolidColorBrush(Hsv2Rgb(hsvindex.Item1 * stepH + 0.5 * stepH, hsvindex.Item2 * stepS + 0.5 * stepS,
                hsvindex.Item3 * stepV + 0.5 * stepV));
            hsvindex = ordered_binmap.ElementAt(4).Key;
            Color4 = new SolidColorBrush(Hsv2Rgb(hsvindex.Item1 * stepH + 0.5 * stepH, hsvindex.Item2 * stepS + 0.5 * stepS,
                hsvindex.Item3 * stepV + 0.5 * stepV));



            //SolidColorBrush color0;
            //GetColor(0,0, out color0);
            //Color0 = color0;

            //SolidColorBrush color1;
            //GetColor(499, 20,out color1);
            //Color1 = color1;

            //SolidColorBrush color2;

            //Color color = GetColor(0,400);
            //Color2 = new SolidColorBrush(color);

            //SolidColorBrush color3;
            //GetColor(400,400, out color3);
            //Color3 = color3;

            //SolidColorBrush color4;
            //GetColor(40,0, out color4);
            //Color4 = color4;
        }

        private double[] Rgb2Hsv(Color color)
        {
            double[] hsv = new double[3];
            double r = color.R;
            double g = color.G;
            double b = color.B;
            double max = Math.Max(Math.Max(r, g), b);
            double min = Math.Min(Math.Min(r, g), b);
            
            if (max == min)
            {
                hsv[0] = -1; // undefined
            }
            else
            {
                if (r == max)
                    hsv[0] = (g - b) / (max - min);
                if (g == max)
                    hsv[0] = 2 + (b - r) / (max - min);
                if (b == max)
                    hsv[0] = 4 + (r - g) / (max - min);

                hsv[0] = hsv[0] * 60;
                if (hsv[0] < 0)
                    hsv[0] = hsv[0] + 360;
            }
            hsv[1] = max == 0 ? 0 : (max - min) / max;
            hsv[2] = max / 255f;
            return hsv;
        }

        private Color Hsv2Rgb(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);

        }
    }
}