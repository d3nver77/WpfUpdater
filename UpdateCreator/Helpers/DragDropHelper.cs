using System.Windows;

namespace UpdateCreator.Helpers
{
    public static class DragDropHelper
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source", typeof(object), typeof(DragDropHelper), new PropertyMetadata(default(object), SourcePropertyChangedCallback));
        private static void SourcePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            DragDropManager.Instance.DragDropSource = (FrameworkElement)dependencyObject;
        }
        public static void SetSource(DependencyObject element, object value)
        {
            element.SetValue(SourceProperty, value);
        }
        public static object GetSource(DependencyObject element)
        {
            return (object)element.GetValue(SourceProperty);
        }

        public static readonly DependencyProperty TargetProperty = DependencyProperty.RegisterAttached(
            "Target", typeof(object), typeof(DragDropHelper), new PropertyMetadata(default(object), TargetPropertyChangedCallback));
        private static void TargetPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            DragDropManager.Instance.DragDropTarget = (FrameworkElement)dependencyObject;
        }
        public static void SetTarget(DependencyObject element, object value)
        {
            element.SetValue(TargetProperty, value);
        }
        public static object GetTarget(DependencyObject element)
        {
            return (object) element.GetValue(TargetProperty);
        }
    }
}