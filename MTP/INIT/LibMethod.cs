using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ACO2_App._0.INIT
{
    internal class LibMethod
    {
        public enum extension
        {
            excel,
            txt,
            doc,
            image,

        }
        /// <summary>
        /// Excel,Txt,Doc,Image
        /// </summary>
        /// <param name="Excel"></param>
        /// <param name="txt"></param>
        public static void SelectFile(extension exten, TextBox txt)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            switch (exten)
            {
                case extension.excel:
                    dlg.DefaultExt = ".xlsx";
                    dlg.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm|All Files (*.*)|*.*";
                    break;
                case extension.txt:
                    dlg.DefaultExt = ".txt";
                    dlg.Filter = "Excel Files|*.txt|All Files (*.*)|*.*";
                    break;
                case extension.doc:
                    dlg.DefaultExt = ".docx";
                    dlg.Filter = "Text Files (.txt)|*.txt|Word Documents (.docx)|*.docx|Word Template (.dotx)|*.dotx|All Files (*.*)|*.*";
                    break;
                case extension.image:
                    dlg.Filter =
"Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" +
"All files (*.*)|*.*";
                    break;
                default:
                    break;
            }
            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                txt.Text = filename;
            }
        }

        /// <summary>
        /// Check Window is Opened
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsWindowOpen(string name = "")
        {
            return Application.Current.Windows.Cast<System.Windows.Window>().Any(x => x.ToString().Contains(name));

        }
        public static Window GetWindow(string name = "")
        {
            var window = Application.Current.Windows.Cast<System.Windows.Window>().First(x => x.ToString().Contains(name));
            return window;
        }

        public static void SetIfNotNull<T>(T value, Action<T> setter)
        {
            if (setter != null && value != null)
            {
                setter(value);
            }
        }
    }
}
