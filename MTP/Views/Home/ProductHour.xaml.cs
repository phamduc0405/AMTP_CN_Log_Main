using ACO2_App._0.Style;
using ACO2_App._0;
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
using APlc;
using ACO2.Style;

namespace ACO2.Views.Home
{
    /// <summary>
    /// Interaction logic for ProductHour.xaml
    /// </summary>
    public partial class ProductHour : UserControl
    {
        private Controller _controller;
        public ProductHour()
        {
            InitializeComponent();
            _controller = MainWindow.Controller;
            Initial();
        }
        private void Initial()
        {
            int count = 0;
            List<WordModel> words = new List<WordModel>();
            words=_controller.PlcH.Words.Where(x => x.Area.Contains("PRODUCT_HOURS")).ToList();
                for (int i = 0; i < words.Count; i++)
                {
                    ColumnDefinition columnDefinition = new ColumnDefinition();
                grdData.ColumnDefinitions.Add(columnDefinition);
                }
            foreach (var word in words)
            {
                WordModel w = new WordModel();
                w = word;
                {
                    DataHeaderValue wordCmt = new DataHeaderValue();
                    
                    wordCmt.txtHeader.Text = string.IsNullOrEmpty(w.Comment) ? "" : w.Comment;
                    wordCmt.txtValue.Text = w.GetValue.ToString();
                    TextBlock tblValue = wordCmt.txtValue;
                    w.WordChangedEvent += () =>
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            tblValue.Text = w.GetValue.ToString();
                        }));

                    };

                    // Thêm TextBlock vào Grid tại dòng thứ i và cột thứ i
                    Grid.SetColumn(wordCmt, count);
                    Grid.SetRow(wordCmt, 0);  // Nếu bạn sử dụng hàng, hãy chỉ định hàng tương ứng

                    // Thêm TextBlock vào mainGrid
                    grdData.Children.Add(wordCmt);
                    count++;


                }
            }
        }
    }
}
