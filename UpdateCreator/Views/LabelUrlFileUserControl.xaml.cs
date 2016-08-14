using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UpdateCreator.Views
{
    /// <summary>
    /// Interaction logic for LabelUrlFileUserControl.xaml
    /// </summary>
    public partial class LabelUrlFileUserControl : UserControl
    {
        #region DependencyProperties

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(LabelUrlFileUserControl), new PropertyMetadata(default(string)));
        public string Label
        {
            get { return (string) this.GetValue(LabelProperty); }
            set { this.SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register(
            "Url", typeof(string), typeof(LabelUrlFileUserControl), new FrameworkPropertyMetadata
            (
                string.Empty,
                FrameworkPropertyMetadataOptions.Journal |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null,
                null,
                true,
                UpdateSourceTrigger.PropertyChanged
                ));
        public string Url
        {
            get { return (string) this.GetValue(UrlProperty); }
            set { this.SetValue(UrlProperty, value); }
        }

        public static readonly DependencyProperty FilenameProperty = DependencyProperty.Register(
            "Filename", typeof(string), typeof(LabelUrlFileUserControl), new PropertyMetadata(default(string)));
        public string Filename
        {
            get { return (string) this.GetValue(FilenameProperty); }
            set { this.SetValue(FilenameProperty, value); }
        }

        #endregion DependencyProperty

        public LabelUrlFileUserControl()
        {
            this.InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                var bindingExpressionUrl = this.GetBindingExpression(UrlProperty);
                var bindingExpressionFilename = this.GetBindingExpression(FilenameProperty);

                if (bindingExpressionUrl?.ParentBinding.UpdateSourceTrigger != UpdateSourceTrigger.LostFocus)
                {
                    this.UpdateBinding(UrlProperty);
                }
                if (bindingExpressionFilename?.ParentBinding.UpdateSourceTrigger != UpdateSourceTrigger.LostFocus)
                {
                    this.UpdateBinding(FilenameProperty);
                }
            };
        }

        private IEnumerable<TextBox> _textBoxes;
        private IEnumerable<TextBox> TextBoxes
        {
            get { return this._textBoxes ?? (this._textBoxes = FindVisualChildren<TextBox>(this)); }
        }

        private void UpdateBinding(DependencyProperty dependencyProperty)
        {
            foreach (var textBox in this.TextBoxes)
            {
                var bindingExpression = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
                if (bindingExpression == null)
                {
                    return;
                }
                var oldBinding = bindingExpression.ParentBinding;
                if (oldBinding.Path.Path != dependencyProperty.Name)
                {
                    return;
                }
                var newBinding = new Binding
                {
                    RelativeSource = oldBinding.RelativeSource,
                    Path = new PropertyPath(dependencyProperty.Name),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                textBox.SetBinding(TextBox.TextProperty, newBinding);
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

    }
}
