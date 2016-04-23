using System.Windows.Data;
using System.Windows.Media;
using GalaSoft.MvvmLight;
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
    }
}
