using APlc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACO2.Data
{
    public class TempData
    {
        private DateTime _time;
        private Dictionary<string, string> _parameter;
        private int count;

        public int Index
        { get; set; }
        public int Count
        { get { return count; } }
        public DateTime Time
        { get { return _time; } }
        public Dictionary<string, string> Parameter
        { get { return _parameter; } 
        set { _parameter = value; }
        }

        public TempData()
        {
            _parameter = new Dictionary<string, string>();
        }
        public void AddData(List<WordModel> words) 
        {
            _time = DateTime.Now;
            foreach (var w in words)
            {
                _parameter.Add(w.Comment, w.GetValue);
            }
        }

        public static TempData FromCsv(string csvLine, string[] header)
        {
            TempData tempData = new TempData();
            string[] values = csvLine.Split(',');

            // Parse the time
            if (DateTime.TryParse(values[0], out DateTime time))
            {
                tempData._time = time;
            }

            // Parse the parameter keys and values based on the header
            for (int i = 1; i < values.Length-1; i++)
            {
                tempData.Parameter.Add(header[i], values[i]);
            }

            return tempData;
        }
    }
}
