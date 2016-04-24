using System.Windows;
using System.Windows.Controls;

namespace SceneEnhancementLabeling.Common
{
    public static class ScrollViewerEx
    {
        public static bool GetScrollToTop(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollToTopProperty);
        }

        public static void SetScrollToTop(DependencyObject obj, bool value)
        {
                obj.SetValue(ScrollToTopProperty, value);
        }

        public static readonly DependencyProperty ScrollToTopProperty =
            DependencyProperty.RegisterAttached("ScrollToTop", typeof(bool), typeof(ScrollViewerEx), new PropertyMetadata(false, ScrollToTopPropertyChanged));

        private static void ScrollToTopPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;
            if (scrollViewer != null && (bool)e.NewValue)
            {
                scrollViewer.ScrollToTop();
                SetScrollToTop(d, false);
            }
        }

    }
}
