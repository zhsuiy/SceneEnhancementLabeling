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
using SceneEnhancementLabeling.Common;
using SceneEnhancementLabeling.Models;
using SceneEnhancementLabeling.ViewModel;
using Xceed.Wpf.Toolkit;
using WindowStartupLocation = System.Windows.WindowStartupLocation;

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
            ShowMagnifier();
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

        //private void PreviousStepBtn_OnClick(object sender, RoutedEventArgs e)
        //{
        //    ShowMagnifer();
        //}

        //private void NextStepBtn_OnClick(object sender, RoutedEventArgs e)
        //{
        //    HideMagnifer();
        //}

        private void ShowMagnifier()
        {
            var magnifier = MagnifierManager.GetMagnifier(Image);
            if (magnifier == null)
            {
                MagnifierManager.SetMagnifier(Image, new Magnifier
                {
                    Radius = 70,
                    BorderBrush = new SolidColorBrush(Colors.Red),
                    BorderThickness = new Thickness(4),
                    ZoomFactor = 0.4
                });
            }
            else
            {
                magnifier.Visibility = Visibility.Visible;
            }
        }

        private void HideMagnifer()
        {
            var magnifier = MagnifierManager.GetMagnifier(Image);
            if (magnifier != null)
            {
                magnifier.Visibility = Visibility.Collapsed;
            }
        }

        private void ColorPlatte_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as LabelingViewModel;
            if (vm == null)
            {
                return;
            }

            var radio = sender as RadioButtonEx;
            var model = radio?.DataContext as CategoryItem;
            if (model == null)
            {
                return;
            }
            if (radio.Tag.ToString() == "0" && model.IsChecked0 && model.Color0.Color != Colors.Transparent)
            {
                vm.SelectedColor = model.Color0.Color;
                ShowToolWindow();
            }
            if (radio.Tag.ToString() == "1" && model.IsChecked1 && model.Color1.Color != Colors.Transparent)
            {
                vm.SelectedColor = model.Color1.Color;
                ShowToolWindow();
            }
            if (radio.Tag.ToString() == "2" && model.IsChecked2 && model.Color2.Color != Colors.Transparent)
            {
                vm.SelectedColor = model.Color2.Color;
                ShowToolWindow();
            }
        }

        private Window _child;
        private void ShowToolWindow()
        {
            var vm = DataContext as LabelingViewModel;
            if (vm == null)
            {
                return;
            }
            var isOpen = Helper.IsWindowOpen<Window>("ColorCanvas");
            if (isOpen)
            {
                if (_child != null && !_child.IsActive)
                {
                    _child.Activate();
                }
            }
            else
            {
                var colorCanvas = new ColorCanvas
                {
                    Background = new SolidColorBrush(Colors.White),
                    BorderThickness = new Thickness(0)
                };
                var binding = new Binding
                {
                    Source = vm,
                    Path = new PropertyPath("SelectedColor"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding(colorCanvas, ColorCanvas.SelectedColorProperty, binding);

                var button = new Button
                {
                    Content = "Add to Preference Colors",
                    Height = 30,
                    Margin = new Thickness(12, 6, 12, 0),
                    Command = vm.AddPreferenceColorCommand
                };
                var stackPanel = new StackPanel();
                stackPanel.Children.Add(colorCanvas);
                stackPanel.Children.Add(button);

                _child = new Window
                {
                    Name = "ColorCanvas",
                    Title = "Palette",
                    WindowStyle = WindowStyle.ToolWindow,
                    ShowInTaskbar = false,
                    Width = 250,
                    Height = 360,
                    Content = stackPanel,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Top = 150,
                    Left = ActualWidth - 250
                };
                _child.Show();
            }
        }

        private void PreferenceColor_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as LabelingViewModel;
            if (vm == null)
            {
                return;
            }
            var frameworkElement = sender as FrameworkElement;
            var context = frameworkElement?.DataContext as SolidColorBrush;
            if (context != null)
            {
                vm.SelectedColor = context.Color;
            }
        }

        private void Image_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var magnifier = MagnifierManager.GetMagnifier(Image);
            if (magnifier != null)
            {
                bool handle = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
                if (handle)
                {
                    var zoom = magnifier.ZoomFactor;
                    zoom -= e.Delta*0.001;
                    if (zoom < 0.2)
                    {
                        magnifier.ZoomFactor = 0.2;
                    }
                    else if (zoom > 0.8)
                    {
                        magnifier.ZoomFactor = 0.8;
                    }
                    else
                    {
                        magnifier.ZoomFactor = zoom;
                    }
                    return;
                }


                var radius = magnifier.Radius;
                radius += e.Delta * 0.1;
                if (radius < 30)
                {
                    magnifier.Radius = 30;
                }
                else if (radius > 300)
                {
                    magnifier.Radius = 300;
                }
                else
                {
                    magnifier.Radius = radius;
                }
            }
        }
    }
}
