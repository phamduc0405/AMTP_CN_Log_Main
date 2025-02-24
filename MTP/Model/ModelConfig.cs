using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACO2.Model
{
    public class ModelConfig
    {
       public ModelName CurrentModel {  get; set; }
       public List<ModelName> ModelList { get; set; } = new List<ModelName>();
    }
    public class ModelName
    {
        public string Name { get; set; }
        public List<ModelParameter> ModelParas { get; set; } = new List<ModelParameter>();
    }
    public class ModelParameter
    {
        public bool IsSkip { get; set; }
        public string Name { get; set; }
        public bool IsUseMCR { get; set; }
        public bool IsRotary { get; set; }
        public string BarCode { get; set; } = "";
        public bool IsFirstMachine { get; set; }
        public int CountPaper { get; set; } =0;
        public bool IsThickness { get; set; } = false;
    }
}
