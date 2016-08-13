using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UpdateCreator.Views
{
    /// <summary>
    /// Interaction logic for LabelTextboxUserControl.xaml
    /// </summary>
    public partial class LabelTextboxUserControl : UserControl
    {
        #region DependencyProperties

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(LabelTextboxUserControl), new PropertyMetadata(default(string)));
        public string Label
        {
            get { return (string) this.GetValue(LabelProperty); }
            set { this.SetValue(LabelProperty, value); }
        }

        //public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        //    "Text", typeof(string), typeof(LabelTextboxUserControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(LabelTextboxUserControl), new FrameworkPropertyMetadata
            (
                string.Empty,
                FrameworkPropertyMetadataOptions.Journal |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null, //new PropertyChangedCallback(TextBox.OnTextPropertyChanged),
                null, //new CoerceValueCallback(TextBox.CoerceText),
                true,
                UpdateSourceTrigger.PropertyChanged
                ));


        public string Text
        {
            get { return (string) this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof(bool), typeof(LabelTextboxUserControl), new PropertyMetadata(default(bool)));
        public bool IsReadOnly
        {
            get { return (bool) this.GetValue(IsReadOnlyProperty); }
            set { this.SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsMultilineProperty = DependencyProperty.Register(
            "IsMultiline", typeof(bool), typeof(LabelTextboxUserControl), new PropertyMetadata(default(bool)));
        public bool IsMultiline
        {
            get { return (bool) this.GetValue(IsMultilineProperty); }
            set { this.SetValue(IsMultilineProperty, value); }
        }

        #endregion DependencyProperty

        public LabelTextboxUserControl()
        {
            InitializeComponent();

            this.Loaded += (sender, args) =>
            {
                var bindingExpressionUrl = this.GetBindingExpression(TextProperty);

                if (bindingExpressionUrl?.ParentBinding.UpdateSourceTrigger != UpdateSourceTrigger.LostFocus)
                {
                    this.UpdateBinding(TextProperty);
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
