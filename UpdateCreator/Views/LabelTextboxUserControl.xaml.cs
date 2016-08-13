using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(LabelTextboxUserControl), new PropertyMetadata(default(string)));
        
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

        }
    }
}
