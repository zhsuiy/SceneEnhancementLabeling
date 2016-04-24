using System.Windows;
using System.Windows.Controls;

namespace SceneEnhancementLabeling.Common
{
    public class RadioButtonEx : RadioButton
    {
        private static bool _bIsChanging;

        // Using a DependencyProperty as the backing store for IsCheckedReal. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedRealProperty =
            DependencyProperty.Register("IsCheckedReal", typeof (bool?), typeof (RadioButtonEx),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Journal |
                                                     FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    IsCheckedRealChanged));

        public RadioButtonEx()
        {
            Checked += RadioButtonExtended_Checked;
            Unchecked += RadioButtonExtended_Unchecked;
        }

        public bool? IsCheckedReal
        {
            get { return (bool?) GetValue(IsCheckedRealProperty); }
            set { SetValue(IsCheckedRealProperty, value); }
        }

        private void RadioButtonExtended_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_bIsChanging)
                IsCheckedReal = false;
        }

        private void RadioButtonExtended_Checked(object sender, RoutedEventArgs e)
        {
            if (!_bIsChanging)
                IsCheckedReal = true;
        }

        public static void IsCheckedRealChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            _bIsChanging = true;
            ((RadioButtonEx) d).IsChecked = (bool) e.NewValue;
            _bIsChanging = false;
        }
    }
}