using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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
            "Url", typeof(string), typeof(LabelUrlFileUserControl), new PropertyMetadata(default(string)));
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
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.Label = "Design Label Text";
                this.Url = "Design Text";
            }
        }
    }
}
