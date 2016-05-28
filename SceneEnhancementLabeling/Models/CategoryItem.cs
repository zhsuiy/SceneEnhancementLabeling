using System;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;

namespace SceneEnhancementLabeling.Models
{
    public class CategoryItem : ObservableObject
    {
        private int _id;

        [JsonProperty("id")]
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        private SolidColorBrush _color0 = new SolidColorBrush(Colors.Transparent);

        public SolidColorBrush Color0
        {
            get { return _color0; }
            set
            {
                _color0 = value;
                RaisePropertyChanged();
            }
        }

        private SolidColorBrush _color1 = new SolidColorBrush(Colors.Transparent);

        public SolidColorBrush Color1
        {
            get { return _color1; }
            set
            {
                _color1 = value;
                RaisePropertyChanged();
            }
        }

        private SolidColorBrush _color2 = new SolidColorBrush(Colors.Transparent);

        public SolidColorBrush Color2
        {
            get { return _color2; }
            set
            {
                _color2 = value;
                RaisePropertyChanged();
            }
        }

        private double _lowerValue;

        public double LowerValue
        {
            get { return _lowerValue; }
            set
            {
                if (_lowerValue != value)
                {
                    UpdatePercentage(value, HigherValue);
                }
                _lowerValue = value;
                RaisePropertyChanged();
            }
        }

        private double _higherValue;

        public double HigherValue
        {
            get { return _higherValue; }
            set
            {
                if (_higherValue != value)
                {
                    UpdatePercentage(LowerValue, value);
                }
                _higherValue = value;
                RaisePropertyChanged();
            }
        }

        public void UpdatePercentage(double low, double high)
        {
            Color0Percent = low / 100.0;
            Color1Percent = (high - low) / 100.0;
            Color2Percent = (100 - high) / 100.0;
        }


        private double _color0Percent;

        public double Color0Percent
        {
            get { return _color0Percent; }
            set
            {
                _color0Percent = value;
                RaisePropertyChanged();
            }
        }

        private double _color1Percent;

        public double Color1Percent
        {
            get { return _color1Percent; }
            set
            {
                _color1Percent = value;
                RaisePropertyChanged();
            }
        }

        private double _color2Percent;

        public double Color2Percent
        {
            get { return _color2Percent; }
            set
            {
                _color2Percent = value;
                RaisePropertyChanged();
            }
        }


        private bool _isChecked0 = true;

        public bool IsChecked0
        {
            get { return _isChecked0; }
            set
            {
                _isChecked0 = value;
                RaisePropertyChanged();
            }
        }

        private bool _isChecked1;

        public bool IsChecked1
        {
            get { return _isChecked1; }
            set
            {
                _isChecked1 = value;
                RaisePropertyChanged();
            }
        }

        private bool _isChecked2;

        public bool IsChecked2
        {
            get { return _isChecked2; }
            set
            {
                _isChecked2 = value;
                RaisePropertyChanged();
            }
        }


        private bool _isCurrentChecked;

        public bool IsCurrentChecked
        {
            get { return _isCurrentChecked; }
            set
            {
                _isCurrentChecked = value;
                RaisePropertyChanged();
            }
        }

        public bool IsColor0PercentInited { get; set; }
        public bool IsColor1PercentInited { get; set; }
        public bool IsColor2PercentInited { get; set; }
    }
}
