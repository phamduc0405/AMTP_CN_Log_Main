using ACO2_App._0.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTP.Model
{
   public class CellDataQueueAction
    {
        public enum ActionType
        {
            None,
           Delete,
           Add,
           Modify,
           ModifyAndSave,
           DeleteAll,
        }
        public CellData CellData { get; set; } = new CellData();
        public ActionType Action { get; set; } = ActionType.None;
    }
}
