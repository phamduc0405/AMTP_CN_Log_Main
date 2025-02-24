using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACO2_App._0.INIT
{
    static class DefaultData
    {
        public static string AppPath = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).ToString()).ToString();
        public static string CsvPath = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).ToString()).ToString();
        public static string LogPath = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).ToString()).ToString();
        public static void CheckFolder(string name)
        {
            if (!Directory.Exists(name))
                Directory.CreateDirectory(name);
        }
        //public static bool IsStringInEnum<T>(string str) where T : Enum
        //{
        //    return Enum.IsDefined(typeof(T), str);
        //}
        public static bool IsStringInEnum<T>(string str) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                return false;
            }
            foreach (var value in Enum.GetValues(enumType))
            {
                if (str.Equals(value.ToString()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
