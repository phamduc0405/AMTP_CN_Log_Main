using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACO2_App._0.Model
{
    public class Alarm
    {
        public string TIME { get; set; }
        [Browsable(false)]
        public uint ALNO { get; set; }      // Alarm Number

        public uint ALID { get; set; }     // Alarm ID (User Define)
        [Browsable(false)]
        public string ALST { get; set; }   // Alarm Status 1 Set, 2 Reset

        public string ALCD { get; set; }   // Alarm Code 1 Light Alarm, 2 Serious Alarm
        [Browsable(false)]
        public string ALED { get; set; }     // Alarm Enable Disable 0 Enable, 1 Disable
        [Browsable(false)]
        public string ALARMIDCALC { get; set; }
        [Browsable(false)]
        public string EQPID { get; set; }
        [Browsable(false)]
        public string UNITID { get; set; }
        public string MODULEID { get; set; }
        public string ALTEXT { get; set; }   //  Alarm Text (User Define)

    }
}
