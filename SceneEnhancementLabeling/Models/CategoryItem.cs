using System.Windows.Data;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace SceneEnhancementLabeling.Models
{
    public class CategoryItem : ObservableObject
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        private SolidColorBrush _color0 = new SolidColorBrush(Colors.White);

        public SolidColorBrush Color0
        {
            get { return _color0; }
            set
            {
                _color0 = value;
                RaisePropertyChanged();
            }
        }

        private SolidColorBrush _color1 = new SolidColorBrush(Colors.White);

        public SolidColorBrush Color1
        {
            get { return _color1; }
            set
            {
                _color1 = value;
                RaisePropertyChanged();
            }
        }

        private SolidColorBrush _color2 = new SolidColorBrush(Colors.White);

        public SolidColorBrush Color2
        {
            get { return _color2; }
            set
            {
                _color2 = value;
                RaisePropertyChanged();
            }
        }
    }
}
