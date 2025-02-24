using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACO2.Model
{


    public class ProductData
    {
        [DisplayName("MODEL")]
        public string Model {  get; set; }
        [DisplayName("DATE TIME")]
        public string Date { get; set; }
        [DisplayName("BAR CODE")]
        public string Barcode { get; set; }
        [DisplayName("MACHINE NAME")]
        public string MachineName { get; set; }
        [DisplayName("PRODUCTS")]
        public int ProductOK { get; set; }
        public int ProductNG { get; set; }
    }
}
