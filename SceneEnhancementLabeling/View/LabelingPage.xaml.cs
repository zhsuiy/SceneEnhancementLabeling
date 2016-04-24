using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SceneEnhancementLabeling.ViewModel;

namespace SceneEnhancementLabeling.View
{
    /// <summary>
    /// Interaction logic for LabelingPage.xaml
    /// </summary>
    public partial class LabelingPage : Page
    {
        public LabelingPage()
        {
            InitializeComponent();
        }
        
        private void MySaveCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as LabelingViewModel;
            vm?.SaveCommand.Execute(null);
        }

        private void MyResetCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as LabelingViewModel;
            vm?.ResetCommand.Execute(null);
        }

        private void MyOpenOutputCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as LabelingViewModel;
            vm?.OpenOutputCommand.Execute(null);
        }
    }
}
