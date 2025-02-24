using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTP.Model
{
    public class DefectCode
    {
        [DisplayName("Index")]
        public string Index { get; set; }
        [DisplayName("DEFECT NAME")]
        public string DefectName { get; set; }
        public string DefectGroup { get; set; }
        public string Msg { get; set; }
        public string PrintCode { get; set; }

        public string AbRule { get; set; }
        public string Tray { get; set; }

    }
}
