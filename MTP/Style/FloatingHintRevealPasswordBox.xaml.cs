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
using ACO2_App.Style;

namespace ACO2_App.Style
{
    /// <summary>
    /// Interaction logic for FloatingHintRevealPasswordBox.xaml
    /// </summary>
    /// 
    public partial class FloatingHintRevealPasswordBox : UserControl
    {
        public FloatingHintRevealPasswordBox()
        {
            InitializeComponent();

        }
        public string PlaceHolder
        {
            get { return (string)GetValue(PlaceHolderProperty); }

            set { SetValue(PlaceHolderProperty, value); }

        }
        public static readonly DependencyProperty PlaceHolderProperty = DependencyProperty.Register("PlaceHolder", typeof(string), typeof(FloatingHintRevealPasswordBox));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }

            set { SetValue(TextProperty, value); }

        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FloatingHintRevealPasswordBox));

        public bool IsPassword
        {
            get { return (bool)GetValue(PasswordProperty); }

            set { SetValue(PasswordProperty, value); }

        }
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("IsPassword", typeof(bool), typeof(FloatingHintRevealPasswordBox));

        private void passbox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtPassWord.Text = passbox.Password;
        }
    }
}
