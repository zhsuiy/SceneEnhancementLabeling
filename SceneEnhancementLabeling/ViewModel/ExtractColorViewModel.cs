using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SceneEnhancementLabeling.Models;

namespace SceneEnhancementLabeling.ViewModel
{
    public class ExtractColorViewModel : ViewModelBase
    {
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
            _channelNum = bitmap.Format.BitsPerPixel / 8;
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
                color = Color.FromArgb(Pixels[3 + 4 * x + y * _widthstride], Pixels[2 + 4 * x + y * _widthstride],
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
        private int _binS = 2;
        private int _binV = 2;

        private void DoProcess()
        {
            double stepH = 360.0 / _binH;
            double stepS = 1.0 / _binS;
            double stepV = 1.0 / _binV;

            Dictionary<Tuple<int, int, int>, List<Tuple<double, double, double>>> binmap =
                new Dictionary<Tuple<int, int, int>, List<Tuple<double, double, double>>>();
            // new Dictionary<Tuple<int, int, int>, int>();
            //int[,,] bins = new int[_binH,_binS,_binV];
            double[] hsv = new double[3];
            for (int i = 0; i < _imageWidth; i++)
            {
                for (int j = 0; j < _imageHeight; j++)
                {

                    Color color = GetColor(i, j);
                    hsv = Rgb2Hsv(color);
                    int hi = 0, si = 0, vi = 0;
                    hi = Convert.ToInt32(Math.Floor(hsv[0] / stepH));
                    hi = hi < _binH ? hi : _binH - 1;
                    si = Convert.ToInt32(Math.Floor(hsv[1] / stepS));
                    si = si < _binS ? si : _binS - 1;
                    vi = Convert.ToInt32(Math.Floor(hsv[2] / stepV));
                    vi = vi < _binV ? vi : _binV - 1;
                    var tuple = new Tuple<int, int, int>(hi, si, vi);
                    if (binmap.ContainsKey(tuple))
                    {
                        //binmap[tuple]++;
                        binmap[tuple].Add(new Tuple<double, double, double>(hsv[0], hsv[1], hsv[2]));
                    }
                    else
                    {
                        //binmap.Add(tuple, 1);
                        var list = new List<Tuple<double, double, double>>();
                        list.Add(new Tuple<double, double, double>(hsv[0], hsv[1], hsv[2]));
                        binmap.Add(tuple, list);
                    }
                }
            }

            //var ordered_binmap = binmap.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            var ordered_binmap = binmap.OrderByDescending(x => x.Value.Count).ToDictionary(x => x.Key, x => x.Value);

            // var colorlist = new List<Color>();
            var hsvlist = new List<Tuple<double, double, double>>();
            for (int i = 0; i < 5; i++)
            {
                var tmp = ordered_binmap.ElementAt(i).Value;
                hsvlist.Add(new Tuple<double, double, double>(tmp.Average(v => v.Item1), tmp.Average(v => v.Item2), tmp.Average(v => v.Item3)));
            }

            hsvlist = hsvlist.OrderBy(v => v.Item1).ToList();

            Color0 = new SolidColorBrush(Hsv2Rgb(hsvlist[0].Item1, hsvlist[0].Item2, hsvlist[0].Item3));
            Color1 = new SolidColorBrush(Hsv2Rgb(hsvlist[1].Item1, hsvlist[1].Item2, hsvlist[1].Item3));
            Color2 = new SolidColorBrush(Hsv2Rgb(hsvlist[2].Item1, hsvlist[2].Item2, hsvlist[2].Item3));
            Color3 = new SolidColorBrush(Hsv2Rgb(hsvlist[3].Item1, hsvlist[3].Item2, hsvlist[3].Item3));
            Color4 = new SolidColorBrush(Hsv2Rgb(hsvlist[4].Item1, hsvlist[4].Item2, hsvlist[4].Item3));

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
