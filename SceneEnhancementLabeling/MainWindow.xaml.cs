using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SceneEnhancementLabeling
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Frame.NavigationService.Navigate(new Uri("View/LabelingPage.xaml", UriKind.Relative));
        }

        private void ExtractColor_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.NavigationService.Navigate(new Uri("View/ExtractColorPage.xaml", UriKind.Relative));
        }

        private void Labeling_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.NavigationService.Navigate(new Uri("View/LabelingPage.xaml", UriKind.Relative));
        }
    }
}
