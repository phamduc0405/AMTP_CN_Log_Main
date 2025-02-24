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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ACO2_App._0.Style
{
    /// <summary>
    /// Interaction logic for IOComment.xaml
    /// </summary>
    public partial class IOComment : UserControl
    {
        public IOComment()
        {
            InitializeComponent();
        }
        private void SetEffectBasedOnFill()
        {
            if (elpOnOff.Fill != Brushes.Gray)
            {
                elpOnOff.Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 315,
                    ShadowDepth = 3,
                    Opacity = 0.5
                };
            }
            else
            {
                elpOnOff.Effect = null; // Loại bỏ hiệu ứng
            }
        }
        public void UpdateEffect()
        {
            SetEffectBasedOnFill();
        }
    }
}
