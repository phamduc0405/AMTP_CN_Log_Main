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

namespace ACO2.Style
{
    /// <summary>
    /// Interaction logic for CharacterLimitTextBox.xaml
    /// </summary>
    public partial class CharacterLimitTextBox : UserControl
    {
        private int _limit =100;

        public CharacterLimitTextBox(int limit)
        {
            InitializeComponent();
            _limit = limit; 
            UpdateCharacterCounter(0); 
        }

        public void SetCharacterLimit(int limit)
        {
            _limit = limit;
            UpdateCharacterCounter(0);
        }

        private void txtLimit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtLimit.Template.FindName("MainTextBox", txtLimit) is TextBox mainTextBox)
            {
                if (mainTextBox.Text.Length > _limit)
                {
                    mainTextBox.Text = mainTextBox.Text.Substring(0, _limit);
                    mainTextBox.SelectionStart = mainTextBox.Text.Length; 
                }

                UpdateCharacterCounter(mainTextBox.Text.Length);
            }
        }

        private void UpdateCharacterCounter(int currentLength)
        {
            if (txtLimit.Template.FindName("CharacterCounter", txtLimit) is TextBlock characterCounter)
            {
                characterCounter.Text = $"{currentLength}/{_limit}";
            }
        }
    }
}
